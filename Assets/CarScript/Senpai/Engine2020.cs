/*------------------------------------------------------------------
* ファイル名：Engine2020.cs
* 概要：エンジンのクラス
* 担当者：不明
* 作成日：不明
-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AIM
{
    [System.Serializable]
    public class Engine
    {
        public bool runOnStartup = false;  //起動時に実行
        public bool runOnEnable = true;    //有効時に実行
        public bool stopOnDisable = true;  //停止時無効化

        private bool starting = false;      //起動準備中か
        private bool stopping = false;      //停止準備中か

        public float minRPM = 600;          // 最低回転数
        public float maxRPM = 5000;         // 最高回転数

        public float maxPower = 90;         // 最大パワー
        public float[] maxPowerList;        // パワーをリスト化

        public float maxRpmChange = 10000;  // 回転最大値
        public float[] maxRpmChangeList;    // 回転最大値をリスト化7個(0~6)

        public int ChackGearNum = 0;        // エラーが出たときの確認変数
        public float ChackGearNum2 = 0;        // エラーが出たときの確認変数

        [Range(0.0f, 1.0f)]
        public float throttleSmoothing = 0.2f; // スロットルの滑らかにするもの（？）

        public AnimationCurve powerCurve = new AnimationCurve(new Keyframe[3] {
                new Keyframe(0f, 0f),
                new Keyframe(0.75f, 1f),
                new Keyframe(1f, 0.92f)
            });

        [SerializeField]
        private bool isRunning = true;      //走っているか
        private bool wasRunning = false;　　//走っていたか

        //リザルト時のフラグ（車を止める）
        public bool brakeFrag = false;

        [SerializeField]
        public List<float> acceleOnLimit = new List<float>();

        public float rpm;                   // RPM

        private float prevRpm;              // 前へのRPM
        private float rpmPercent;           // rpmのパーセント
        private float rpmOverflow;          // rpmのオーバーフロー

        public float power;                 // 力

        private float throttle = 0f;        // スロットル
        private float prevThrottle = 0f;    // 前へのスロットル
        private float throttleVelocity = 0f;// スロットルの速度

        private float startDuration = 1f;   //起動ラグ
        private float stopDuration = 1f;    //停止ラグ
        private float startedTime = -1;     //開始時間
        private float stoppedTime = -1;

        private float fuelCutoffStart;
        private float fuelCutoffDuration = 0.01f;

        public float[] engineBrake;

        private float prevMaxRPM;

        public ForcedInduction forcedInduction = new ForcedInduction();
        private VehicleController vc;

        public bool isRpm_enum;

        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

        public float StartingPercent
        {
            get
            {
                if (startedTime >= 0)
                    return Mathf.Clamp01((Time.realtimeSinceStartup - startedTime) / startDuration);
                else
                    return 1;
            }
        }

        public float StoppingPercent
        {
            get
            {
                if (stoppedTime >= 0)
                    return Mathf.Clamp01((Time.realtimeSinceStartup - stoppedTime) / stopDuration);
                else
                    return 1;
            }
        }

        public bool FuelCutoff
        {
            get
            {
                if (Time.realtimeSinceStartup < fuelCutoffStart + fuelCutoffDuration)
                    return true;
                else
                    return false;
            }
        }


        public float RPM
        {
            get
            {
                float r = 0;
                if (isRunning && !starting && !stopping)
                {
                    r = Mathf.Clamp(rpm, minRPM, maxRPM);
                }
                else
                {
                    if (starting)
                    {
                        r = StartingPercent * minRPM;
                    }
                    else if (stopping)
                    {
                        r = (1f - StoppingPercent) * minRPM;
                    }
                }
                return r;
            }
        }

        public float RpmOverflow
        {
            get { return rpmOverflow; }
        }

        // ブレーキと強制誘導に使用
        public float RPMPercent
        {
            get
            {
                // （回転数ー最低値）/（最高値ー最低値）
                return Mathf.Clamp01((RPM - minRPM) / (maxRPM - minRPM));
            }
        }

        public float Power
        {
            get
            {
                return power;
            }
        }

        public float PowerInHP
        {
            get
            {
                // PowerはkW
                // kW * 1.341 = HP
                return Power * 1.341f;
            }
        }

        public float TcsPowerReduction
        {
            get
            {
                return vc.drivingAssists.tcs.powerReduction;
            }
            set
            {
                vc.drivingAssists.tcs.powerReduction = Mathf.Clamp01(value);
            }
        }

        public float TotalPowerReduction
        {
            get
            {
                return Mathf.Clamp01(TcsPowerReduction);
            }
        }

        // はやさをいじる場合ここをいじっていく
        public float Torque
        {
            get
            {
                if (RPM > 0)
                {
                    return (9548f * Power) / RPM;
                }
                return 0;
            }
        }

        public float ApproxMaxTorque
        {
            get
            {
                // 回転数が動いている時
                if (RPM > 0)
                {
                    // 根拠不明の値をかけて値を返す。
                    return (9548f * maxPower) / (maxRPM * 0.6f);
                }
                return 0;
            }
        }
        //エンジン処理の更新の条件に使われている。
        public bool Starting
        {
            get { return starting; }
            set { starting = value; }
        }
        //エンジン処理の更新の条件に使われている。
        public bool Stopping
        {
            get { return stopping; }
            set { stopping = value; }
        }

        public void Start()
        {
            if (!vc.fuel.HasFuel)
            {
                isRunning = false;
            }
            else
            {
                wasRunning = isRunning ? true : false;
                isRunning = true;

                if (!wasRunning)
                {
                    startedTime = Time.realtimeSinceStartup;
                }
            }
        }

        public void Stop()
        {
            //走っていたのかの確認
            wasRunning = isRunning ? true : false;
            //走っていない
            isRunning = false;
            //止まっている
            stopping = true;
            //止まってる時間を計測
            stoppedTime = Time.realtimeSinceStartup;
        }

        public void Toggle()
        {
            if (isRunning)
                Stop();
            else
                Start();
        }

        public void Initialize(VehicleController vc)
        {
            this.vc = vc;
            forcedInduction.Initialize(vc);

            starting = false;
            stopping = false;
            wasRunning = false;
            isRunning = false;

            if (!isRunning && runOnStartup)
            {
                Start();
            }
        }

        //毎フレーム更新される処理
        public void Update()
        {
            //走り出しのタイミングのみ処理する
            if (isRunning == true && wasRunning == false)
            {
                //開始時の時刻を保存
                startedTime = Time.realtimeSinceStartup;
                wasRunning = true;
                //エンジン起動準備
                starting = true;
                stopping = false;
            }

            //走り出した時間から1秒だけ処理しない
            if (starting == true && Time.realtimeSinceStartup > startedTime + startDuration)
            {
                //起動準備を終える
                starting = false;
            }

            //止まり出した時間から1秒だけ処理しない
            if (stopping == true && Time.realtimeSinceStartup > stoppedTime + stopDuration)
            {
                //停止準備を終える
                stopping = false;
            }

            //走っているか
            if (isRunning)
            {
                float allowedRpmChange;
                // 変更場所（北村翔太）
                // 2023/07/13
                if (vc.transmission.Gear >= 0 && vc.transmission.Gear <= 6)
                {
                    allowedRpmChange = maxRpmChangeList[vc.transmission.Gear] * Time.fixedDeltaTime * powerCurve.Evaluate(rpm / maxRPM);
                    ChackGearNum = vc.transmission.Gear;
                }
                else
                {
                    allowedRpmChange = maxRpmChange * Time.fixedDeltaTime * powerCurve.Evaluate(rpm / maxRPM);
                }

                forcedInduction.Update();

                // 代入パート
                prevRpm = rpm;

                rpmOverflow = 0;

                vc.transmission.UpdateClutch();

                // RPMレブ
                if (rpm > maxRPM)
                {
                    rpmOverflow = rpm - maxRPM;
                    rpm = maxRPM;
                    StartFuelCutoff();
                }
                else if (rpm < minRPM)
                {
                    rpmOverflow = rpm - minRPM;
                    rpm = minRPM;
                }

                // シフトチェンジ中以外
                if (vc.transmission.Gear != 0 && !vc.transmission.Shifting)
                {
                    // Transmissionから値を取得（ギア比）
                    rpm = vc.transmission.ReverseRPM;

                    if (!vc.input.Vertical.IsDeadzoneZero()) { rpm += vc.transmission.AddedClutchRPM; }

                    if (rpm > (prevRpm + allowedRpmChange))
                    {
                        rpm = prevRpm + allowedRpmChange;
                    }
                    else if (rpm < (prevRpm - allowedRpmChange))
                    {
                        rpm = prevRpm - allowedRpmChange;
                    }

                    if (rpm < minRPM)
                    {
                        rpm = minRPM;
                    }
                    else if (rpm > maxRPM)
                    {
                        rpm = maxRPM;
                    }
                }
                // N、もしくはシフトチェンジ中
                else
                {
                    float userInput = Mathf.Clamp01(vc.input.Vertical - 0.025f);
                    if (vc.transmission.Shifting || userInput == 0 || FuelCutoff)
                        userInput = -1;

                    rpm += allowedRpmChange * userInput;
                }
                if (vc.transmission.Gear != 0 && vc.Speed > vc.transmission.GetMaxSpeedForGear(vc.transmission.Gear))
                {
                    StartFuelCutoff();
                }
            }
            // それ以外
            else
            {
                rpmOverflow = 0;
                rpm = 0;
                prevRpm = 0;
            }

            prevThrottle = throttle;  // スロットルを前スロットルに入れる
            power = 0;

            //  if (!starting && !stopping && IsRunning && !FuelCutoff && !vc.transmission.Shifting)
            // 始まってる最中ではなく、止まってるわけでもおらず、今現在進行形で走ってて、燃料切れて無くて、シフト変更を行っていない時。
            // このif文がない場合キーボード操作が効かない
            if (false==starting && false == stopping && IsRunning && false == FuelCutoff && false == vc.transmission.Shifting)
            {
                // スロットルに垂直の値を渡す。
                throttle = vc.input.Vertical;

                //他スクリプトからギアの値をもらう。
                int gear = vc.transmission.Gear;
                //Debug.LogError("なにこれ");
                // ギアごとのスロットル制限が入っている時
                if (vc.transmission.useThrottleLimiting)
                {
                    if (gear < 0)
                    {
                        float limit = vc.transmission.reverseThrottleLimit;
                        throttle = Mathf.Clamp(throttle, -limit, limit);
                    }
                    else if (gear > 0)
                    {
                        int index = gear - 1;
                        if (index < vc.transmission.forwardThrottleLimits.Count)
                        {
                            float limit = vc.transmission.forwardThrottleLimits[index];
                            throttle = Mathf.Clamp(throttle, -limit, limit);
                        }
                    }
                }

                if (throttle > prevThrottle)
                {
                    throttle = Mathf.SmoothDamp(prevThrottle, throttle, ref throttleVelocity, throttleSmoothing);
                }
                else
                {
                    throttleVelocity = 0;
                }

                float directionInversion = vc.transmission.transmissionType == Transmission.TransmissionType.Manual ? Mathf.Sign(vc.transmission.Gear) : 1f;
                float userInput = Mathf.Clamp01(throttle * Mathf.Sign(vc.transmission.Gear) * directionInversion);

                // 変更場所（北村翔太）
                // 2023/07/13
                if (vc.transmission.Gear >= 0 && vc.transmission.Gear <= 6)
                {
                    power = Mathf.Abs(powerCurve.Evaluate(RPM / maxRPM) * maxPowerList[vc.transmission.Gear] * userInput)
                            * (1f - TotalPowerReduction) * forcedInduction.PowerGainMultiplier;
                }
                else
                {
                    power = Mathf.Abs(powerCurve.Evaluate(RPM / maxRPM) * maxPower * userInput)
                            * (1f - TotalPowerReduction) * forcedInduction.PowerGainMultiplier;
                }
            }
            else
            {
                // スロットルを0に
                throttle = 0;
                // パワーを0に
                power = 0;
            }
        }

        //スタート燃料カットオフ
        private void StartFuelCutoff()
        {
            fuelCutoffStart = Time.realtimeSinceStartup;
        }

        //藤森追加
        // 内容　エンジンを止めるフラグを立てるため
        public bool isEngineStopFlag()
        {
            // ギアが0以上の時とギアが８（acceleOnLimit.Count）より下の時
            if (vc.transmission.Gear >= 0 && vc.transmission.Gear < acceleOnLimit.Count)
            {
                // 前に進む力を5倍したものより各アクセル限界値の方が値が高かったら。
                if (vc.SpeedKPH < acceleOnLimit[vc.transmission.Gear])
                {
                    Debug.LogError("藤森");
                    // YES
                    return true;
                }
            }
            return false;
        }

        private bool isEngineStop()
        {
            return false;
        }

        // 使われてないのでカット
        //===============================================
/*        // イントロ用
        public void IntroSetMaxRPM()
        {
            maxRPM = prevMaxRPM;
        }

        public void IntroSetPrevRPM(float max)
        {
            prevMaxRPM = max;
            maxRPM = 7300.0f;
        }*/
        //===============================================

        // エンジンブレーキについての処理
        // いる？
        public float EngineBrake(int gear)
        {
            return engineBrake[gear];
        }
    }
}