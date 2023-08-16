using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AIM
{
    [DisallowMultipleComponent]
    public class DesktopInputManager : MonoBehaviour
    {
        [SerializeField] CarSearch carSearch;

        //public VehicleChanger vehicleChanger;
        private VehicleController vc;

        private float vertical = 0f;
        private float horizontal = 0f;
        [HideInInspector] public float brakeValue; // 0905一條

        [Space]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float goalBrakeValue = 0;

        [SerializeField] public bool isControl = true;
        //[SerializeField] private CarAI carAI;
        //[SerializeField] private GameObject rayObject;

        [Header("入力された値の小数点の桁数")]
        [SerializeField] private int inputDigit = 3;

        [Header("ハンドルカーブ")]
        public AnimationCurve handleCurve;

        [Header("ペダルカーブ")]
        [SerializeField] private AnimationCurve brakeCurve;
        [SerializeField] private AnimationCurve accelCurve;

        [Header("入力デバイス用変数")]
        [SerializeField] private float connectCheckTime = 0.0f;
        [SerializeField] private string[] hadleName = new string[] { "Driving", "GT", "G25" };
        private bool isStartHandleCheck = false;
        private bool isConnectHandle = false;
        private bool isG29 = false;

        [Header("コントローラー用変数")]
        public float contHori_AccelPer;
        public float contHori_DecelPer;
        public float contHori_changeMax;
        private float controllerHorizontal;
        public bool isConectController;
        public AnimationCurve controllerHandleCurve;

        [SerializeField]
        public PlayStartFlag playStart;

        // ━━━━━━━━━━━ ５追加 ━━━━━━━━━━━
        private float keyboardInputCnt = 0;
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━

        //=================================================================================
        private bool TryGetButtonDown(string buttonName, KeyCode altKey)
        {
            //try  {return Input.GetButtonDown(buttonName);}
            //catch{return Input.GetKeyDown(altKey);}

            if (Input.GetButtonDown(buttonName))
            {
                return Input.GetButtonDown(buttonName);
            }
            else
            {
                return Input.GetKeyDown(altKey);
            }
        }

        private bool TryGetButton(string buttonName, KeyCode altKey)
        {
            try
            {
                return Input.GetButton(buttonName);
            }
            catch
            {
                return Input.GetKey(altKey);
            }
        }
        //====================================================
        private void Start()
        {
            InputHandleController.inputDigit = inputDigit;
        }
        //====================================================
        void Update()
        {
            if (!carSearch.bIsCar)
                return;

            if (!carSearch.bIsDesktopInputManager)
            {
                vc = carSearch.vc;
                //carAI = GetComponent<CarAI>();
                //rayObject = GameObject.Find("RayObject");
                carSearch.bIsDesktopInputManager = true;
            }

            //if (vehicleChanger != null)
            //{
            //    vc = vehicleChanger.ActiveVehicleController;
            //}

            if (vc == null) return;

            try
            {
                if (playStart.bStart)
                {
                    if (isControl)
                    {
                        // コルーチンで定期的にコントローラーのチェック（起動は一度のみ）
                        if (!isStartHandleCheck)
                        {
                            isStartHandleCheck = true;
                            StartCoroutine(LoopConnectHandle());

                            for (int i = 0; i < hadleName.Length; ++i)
                            {
                                hadleName[i] = hadleName[i].ToLower();
                            }
                        }
                        // StaticClassの値更新　東樹追加
                        InputHandleController.isConnectHandle = isConnectHandle;

                        // LogicoolSDKの更新
                        vc.wheelInput.ControllerConnectCheck();
                        vc.wheelInput.InputUpdate();
                        isConectController = ConnectController();

                        //carAI.enabled = false;
                        //rayObject.SetActive(false);

                        // 入力の初期化
                        vertical = 0f;
                        horizontal = 0f;

                        if (vc == null) return;

                        // マニュアル時のシフトアップ（ハンドル裏のパドル）
                        if (isConnectHandle)
                        {
                            if (TryGetButtonDown("ShiftUp", KeyCode.R))
                            {
                                vc.input.ShiftUp = true;
                            }
                            if (TryGetButtonDown("ShiftDown", KeyCode.F))
                            {
                                vc.input.ShiftDown = true;
                            }
                            if (TryGetButtonDown("FifthGear", KeyCode.Alpha5))
                            {
                                vc.input.ShiftUp = true;
                            }
                            if (TryGetButtonDown("SixthGear", KeyCode.Alpha6))
                            {
                                vc.input.ShiftDown = true;
                            }
                        }
                        // コントローラー
                        else if (isConectController)
                        {
                            if (TryGetButtonDown("ShiftUp_Pad", KeyCode.R))
                            {
                                vc.input.ShiftUp = true;
                            }
                            if (TryGetButtonDown("ShiftDown_Pad", KeyCode.F))
                            {
                                vc.input.ShiftDown = true;
                            }
                        }
                        else
                        {
                            if (TryGetButtonDown("ShiftUp", KeyCode.N))
                            {
                                vc.input.ShiftUp = true;
                            }
                            if (TryGetButtonDown("ShiftDown", KeyCode.B))
                            {
                                vc.input.ShiftDown = true;
                            }
                        }
                        // シフトチェンジ(シフトレバー)
                        if (vc.transmission.transmissionType == Transmission.TransmissionType.Manual)
                        {
                            InputShiftLever();
                        }

                        // 各種入力の更新
                        // 入力はおかしくなりやすいので、ドライバの確認、再起動など試すこと
                        // 特にAxisは取得できないがButtonは取得できる状態になりやすいので注意
                        InputHorizontal();
                        InputPedal();
                        InputHandBrake();

                        // ライトの更新
                        // 実装してません
                        //InputLight();

                        // ホーン処理
                        // 実装してません
                        //vc.input.horn = TryGetButton("Horn", KeyCode.H);
                    }
                    //入力外の処理
                    else
                    {
                        //carAI.enabled = true;
                        //rayObject.SetActive(true);
                        vc.transmission.transmissionType = Transmission.TransmissionType.Manual;
                        vc.transmission.Gear = 1;
                        //vc.input.Vertical = carAI.accel;
                        //vc.input.Horizontal = carAI.steer;
                    }
                }
                
                if (vc.engine.brakeFrag)
                {
                    vc.input.Vertical = -goalBrakeValue;
                }

                // エンジンスタート・ストップ
                //if (TryGetButtonDown("EngineStartStop", KeyCode.Alpha0))
                //{
                //	Debug.Log("EngineStop");
                //	vc.engine.Toggle();
                //}
                if (vc.engine.isEngineStopFlag() && (Input.GetAxis("R_Trigger").Round(inputDigit) > 0.0 || Input.GetAxisRaw("Vertical").Round(inputDigit) > 0.0 || AIM.InputHandleController.GetHandleAccelCheck() > 0.0f))
                {
                    vc.engine.Stop();
                    vc.input.Vertical = 0f;
                }
                else
                {
                    vc.engine.Start();
                }
                // 手動でひっくり返った状態から復帰
                if (vc.flipOver.manual)
                {
                    if (Input.GetButtonDown("FlipOver") && vc.flipOver.flippedOver)
                        vc.input.flipOver = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }
        // ハンドルの計算--------------------------------------------------------------------------------------------------------------------
        void InputHorizontal()
        {
            // ハンドル使用
            if (isConnectHandle)
            {
                // LogicoolSDK使用
                if (vc.wheelInput.isG29)
                {
                    // +-両方を同じカーブにする事が難しいため、スクリプト側で対応
                    if (vc.wheelInput.inputHandle >= 0)
                        horizontal = 1 * handleCurve.Evaluate(Mathf.Abs(vc.wheelInput.inputHandle.Round(inputDigit)));
                    else
                        horizontal = -1 * handleCurve.Evaluate(Mathf.Abs(vc.wheelInput.inputHandle.Round(inputDigit)));
                }
                else
                {
                    if (Input.GetAxis("Horizontal") >= 0)
                        horizontal = 1 * handleCurve.Evaluate(Mathf.Abs(Input.GetAxis("Horizontal").Round(inputDigit)));
                    else
                        horizontal = -1 * handleCurve.Evaluate(Mathf.Abs(Input.GetAxis("Horizontal").Round(inputDigit)));

                   //Debug.Log(Input.GetAxis("Horizontal"));
                }
            }
            // コントローラー使用
            else if (isConectController)
            {
                if (Input.GetAxis("JoyStick_Horizontal") >= 0)
                    horizontal =  1 * controllerHandleCurve.Evaluate(Mathf.Abs(ControllerHorizontal()));
                else
                    horizontal = -1 * controllerHandleCurve.Evaluate(Mathf.Abs(ControllerHorizontal()));
              
                if (Input.GetAxis("Horizontal") >= 0)
                    horizontal =  1 * controllerHandleCurve.Evaluate(Mathf.Abs(Input.GetAxis("Horizontal")));
                else
                    horizontal = -1 * controllerHandleCurve.Evaluate(Mathf.Abs(Input.GetAxis("Horizontal")));
            }
            else
            {
                //━━━━━━━━━ 元の部分 ━━━━━━━━

                //if (Input.GetAxisRaw("KeyboardHorizontal") > 0)
                //    horizontal = Input.GetAxis("KeyboardHorizontal") /*1 * controllerHandleCurve.Evaluate(Mathf.Abs(Input.GetAxis("KeyboardHorizontal")))*/;
                //else if (Input.GetAxisRaw("KeyboardHorizontal") < 0)
                //    horizontal = -1 * controllerHandleCurve.Evaluate(Mathf.Abs(Input.GetAxis("KeyboardHorizontal")));
                //else
                //    horizontal = 0;

                //━━━ ５が変更した部分 （2022/11/16）━━━

                // 押している方向を決める
                int dir = 0;
                if      (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir =  1;
                else if (Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A)) dir = -1;
                
                // ←キー か →キー が押されている場合の処理
                if (dir != 0)
                {
                    // 値が小さすぎるときの処理
                    if (Mathf.Abs(keyboardInputCnt) < 0.01f)
                    {
                        keyboardInputCnt = 0.5f * dir;
                    }
                    // 逆方向の場合の処理
                    else if (keyboardInputCnt * dir < 0)
                    {
                        keyboardInputCnt *= Mathf.Pow(Mathf.Pow(0.2f, 30), Time.deltaTime);
                    }
                    // 普通の処理
                    else
                    {
                        float temp = Mathf.Abs(keyboardInputCnt);
                        keyboardInputCnt += ((225 - temp * temp) / 225) * 10 * Time.deltaTime * dir;
                    }
                }
                // 押していない場合の処理
                else
                {
                    // だんだん０に戻るようにする
                    keyboardInputCnt *= Mathf.Pow(Mathf.Pow(0.83f, 30), Time.deltaTime);

                    if (Mathf.Abs(keyboardInputCnt) < 0.01f) keyboardInputCnt = 0;
                }

                #region backup
                /*
                // 右
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    if (Mathf.Abs(keyboardInputCnt) < 0.01f)
                    {
                        keyboardInputCnt = 0.5f;
                    }
                    else if (keyboardInputCnt < 0)
                    {
                        keyboardInputCnt *= 10.0f * Time.deltaTime / 100.0f;
                    }
                    else
                    {
                        float temp = Mathf.Abs(keyboardInputCnt);
                        keyboardInputCnt += ((225 - temp * temp) / 225) * 10 * Time.deltaTime;
                    }
                }
                // 左
                else if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                {
                    if (Mathf.Abs(keyboardInputCnt) < 0.01f)
                    {
                        keyboardInputCnt = -0.5f;
                    }
                    else if (keyboardInputCnt > 0)
                    {
                        keyboardInputCnt *= 10.0f * Time.deltaTime / 100.0f;
                    }
                    else
                    {
                        float temp = Mathf.Abs(keyboardInputCnt);
                        keyboardInputCnt -= ((225 - temp * temp) / 225) * 10 * Time.deltaTime;
                    }
                }
                // 押してない
                else
                {
                    var temp = 25 * Time.deltaTime;
                    if (temp > 1) temp = 1; 
                    if (Mathf.Abs(keyboardInputCnt *= temp) < 0.01f) keyboardInputCnt = 0;
                }
                */
                #endregion

                // 代入
                horizontal += keyboardInputCnt * 0.05f;

                #if false
                Debug.LogError(Time.time + "    DeltaTime = " + Time.deltaTime); 
                Debug.LogError(Time.time + "    keyboardInputCnt = " + keyboardInputCnt); 
                Debug.LogError(Time.time + "    horizontal = " + horizontal); 
                #endif
                //━━━━━━━━━━━━━━━━━━━━━━
            }
            vc.input.Horizontal = horizontal;
        }
        // アクセル・ブレーキ・クラッチの計算--------------------------------------------------------------------------------------------------------------------
        void InputPedal()
        {
            // アクセルとブレーキに振り分け
            float newAccel = 0;
            float newBrake = 0;

            // ハンドル使用
            if (isConnectHandle)
            {
                // LogicoolSDK使用
                if (vc.wheelInput.isG29)
                {
                    newAccel = vc.wheelInput.inputAxel;
                    newBrake = vc.wheelInput.inputBrake;
                }
                else
                {
                    AIM.InputHandleController.GetHandleAccelAndBrake(ref newAccel, ref newBrake);
                }
                newAccel = accelCurve.Evaluate(newAccel).Round(inputDigit);
                newBrake = brakeCurve.Evaluate(newBrake).Round(inputDigit);
            }
            // コントローラー使用
            else if (isConectController)
            {
                newBrake = Input.GetAxis("L_Trigger").Round(inputDigit);
                newAccel = Input.GetAxis("R_Trigger").Round(inputDigit);
            }
            // キーボード使用
            else
            {
                newAccel = Input.GetAxis("KeyboardAccel").Round(inputDigit);
                newBrake = Input.GetAxis("KeyboardBrake").Round(inputDigit);
            }
            // クラッチ
            if (!vc.transmission.automaticClutch)
            {
                try
                {
                    vc.input.Clutch = Input.GetAxis("Clutch").Round(inputDigit);
                }
                catch
                {
                    vc.transmission.automaticClutch = true;
                    vc.input.Clutch = 0.0f;
                }
            }

            // ゴール後ブレーキ強制入力
            if (GoalJudgment.isGoal)
            {
                newAccel = 0;
                newBrake = 1;
            }

            // 統合
            vc.input.Vertical = newAccel - newBrake;
            brakeValue = newBrake;
        }
        // ハンドブレーキの計算--------------------------------------------------------------------------------------------------------------------
        void InputHandBrake()
        {
            try
            {
                vc.input.Handbrake = Input.GetAxis("Handbrake").Round(inputDigit);
            }
            catch
            {
                vc.input.Handbrake = Input.GetKey(KeyCode.Space) ? 1f : 0f;
            }
        }
        // ライトの管理--------------------------------------------------------------------------------------------------------------------
        void InputLight()
        {
            if (TryGetButtonDown("LeftBlinker", KeyCode.Z))
            {
                vc.input.leftBlinker = !vc.input.leftBlinker;
                if (vc.input.leftBlinker) vc.input.rightBlinker = false;
            }
            if (TryGetButtonDown("RightBlinker", KeyCode.X))
            {
                vc.input.rightBlinker = !vc.input.rightBlinker;
                if (vc.input.rightBlinker) vc.input.leftBlinker = false;
            }
            if (TryGetButtonDown("Lights", KeyCode.L)) vc.input.lowBeamLights = !vc.input.lowBeamLights;
            if (TryGetButtonDown("FullBeamLights", KeyCode.K)) vc.input.fullBeamLights = !vc.input.fullBeamLights;
            if (TryGetButtonDown("HazardLights", KeyCode.J))
            {
                vc.input.hazardLights = !vc.input.hazardLights;
                vc.input.leftBlinker = false;
                vc.input.rightBlinker = false;
            }
        }
        //四捨五入メソッド
        float Round(float value, int digit)
        {
            float digits = Mathf.Pow(10.0f, digit);
            float value_ = value * digits;
            float round = value_ - Mathf.Floor(value_);

            //切り捨て切り上げ処理
            if (round >= 0.5f)
                return Mathf.Ceil(value_) / digits;
            else
                return Mathf.Floor(value_) / digits;
        }
        //====================================================
        bool ConnectController()
        {
            var controllerNames = Input.GetJoystickNames();

            for (int i = 0; i < controllerNames.Length; ++i)
            {
                //Debug.Log(controllerNames[i]);
                if (controllerNames[i] != "")
                    return true;
            }
            return false;
        }
        //====================================================
        bool ConnectHandle()
        {
            var controllerNames = Input.GetJoystickNames();
            //====================================================
            for (int i = 0; i < controllerNames.Length; ++i)
            {
                controllerNames[i] = controllerNames[i].ToLower();
                //Debug.Log(controllerNames[i]);

                for (int j = 0; j < hadleName.Length; ++j)
                {
                    //Debug.Log(controllerNames[i]);
                    if (controllerNames[i].Contains(hadleName[j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //====================================================
        // 毎フレームハンドルの接続チェックを行う必要はないので、間隔をあける
        IEnumerator LoopConnectHandle()
        {
            while (true)
            {
                isConnectHandle = ConnectHandle();

                //Debug.Log("ハンドル入力：" + isConnectHandle);
                yield return new WaitForSeconds(connectCheckTime);
            }
        }
        //====================================================
        private void OnApplicationFocus(bool focus)
        {
            InputHandleController.isFocus = focus;
        }
        //====================================================
        // シフトレバーでのシフトチェンジ処理
        private void InputShiftLever()
        {
            if (vc.wheelInput.isG29)
            {
                if (TryGetButtonDown("FirstGearG29", KeyCode.Alpha1)) { vc.transmission.ShiftInto(1); }
                else if (TryGetButtonDown("SecondGearG29", KeyCode.Alpha2)) { vc.transmission.ShiftInto(2); }
                else if (TryGetButtonDown("ThirdGearG29", KeyCode.Alpha3)) { vc.transmission.ShiftInto(3); }
                else if (TryGetButtonDown("FourthGearG29", KeyCode.Alpha4)) { vc.transmission.ShiftInto(4); }
                else if (TryGetButtonDown("FifthGearG29", KeyCode.Alpha5)) { vc.transmission.ShiftInto(5); }
                else if (TryGetButtonDown("SixthGearG29", KeyCode.Alpha6)) { vc.transmission.ShiftInto(6); }
                //else if (TryGetButtonDown("Neutral", KeyCode.Alpha0)) { vc.transmission.ShiftInto(0); }
                //else if (TryGetButtonDown("ReverseG29", KeyCode.Minus)) { vc.transmission.ShiftInto(-1); }

                // ニュートラル処理(シフトレバーのみ)
                else if (Input.GetButtonUp("FirstGearG29")) { vc.transmission.ShiftInto(0); }
                else if (Input.GetButtonUp("SecondGearG29")) { vc.transmission.ShiftInto(0); }
                else if (Input.GetButtonUp("ThirdGearG29")) { vc.transmission.ShiftInto(0); }
                else if (Input.GetButtonUp("FourthGearG29")) { vc.transmission.ShiftInto(0); }
                else if (Input.GetButtonUp("FifthGearG29")) { vc.transmission.ShiftInto(0); }
                else if (Input.GetButtonUp("SixthGearG29")) { vc.transmission.ShiftInto(0); }
                //else if (Input.GetButtonUp("ReverseG29")) { vc.transmission.ShiftInto(0); }
            }
            //else
            //{
            //    if (TryGetButtonDown("FirstGear", KeyCode.Alpha1)) { vc.transmission.ShiftInto(1); }
            //    else if (TryGetButtonDown("SecondGear", KeyCode.Alpha2)) { vc.transmission.ShiftInto(2); }
            //    else if (TryGetButtonDown("ThirdGear", KeyCode.Alpha3)) { vc.transmission.ShiftInto(3); }
            //    else if (TryGetButtonDown("FourthGear", KeyCode.Alpha4)) { vc.transmission.ShiftInto(4); }
            //    else if (TryGetButtonDown("FifthGear", KeyCode.Alpha5)) { vc.transmission.ShiftInto(5); }
            //    else if (TryGetButtonDown("SixthGear", KeyCode.Alpha6)) { vc.transmission.ShiftInto(6); }
            //    //else if (TryGetButtonDown("Neutral", KeyCode.Alpha0)) { vc.transmission.ShiftInto(0); }
            //    //else if (TryGetButtonDown("Reverse", KeyCode.Minus)) { vc.transmission.ShiftInto(-1); }

            //    // ニュートラル処理(シフトレバーのみ)
            //    else if (Input.GetButtonUp("FirstGear")) { vc.transmission.ShiftInto(0); }
            //    else if (Input.GetButtonUp("SecondGear")) { vc.transmission.ShiftInto(0); }
            //    else if (Input.GetButtonUp("ThirdGear")) { vc.transmission.ShiftInto(0); }
            //    else if (Input.GetButtonUp("FourthGear")) { vc.transmission.ShiftInto(0); }
            //    else if (Input.GetButtonUp("FifthGear")) { vc.transmission.ShiftInto(0); }
            //    else if (Input.GetButtonUp("SixthGear")) { vc.transmission.ShiftInto(0); }
            //    //else if (Input.GetButtonUp("Reverse")) { vc.transmission.ShiftInto(0); }
            //}
        }
        //====================================================
        // コントローラーでハンドル操作を行うのが難しい為、補正する
        // スティックを倒した分だけ入力してしまうと、急にハンドルを切る事になる為、
        // フレーム毎の上限や、遅れて反映させる事で入力を簡単にする。
        // Timeを使って遅延を考慮しないのは、フレームが飛んだ際に入力を続けていた場合にコントロールが困難になるから
        private float ControllerHorizontal()
        {
            float input = Input.GetAxis("JoyStick_Horizontal");

            if (input.IsDeadzoneZero())
            {
                controllerHorizontal = Mathf.Lerp(controllerHorizontal, input, contHori_DecelPer);
            }
            // 0に向かう速度は速く、1、-1に向かう速度は遅く設定する
            else if (input > 0.0f)
            {
                if (controllerHorizontal >= 0.0f)
                {
                    // 現在の値より入力された値が大きいなら
                    if (controllerHorizontal <= input)
                    {
                        controllerHorizontal = Mathf.Clamp(Mathf.Lerp(controllerHorizontal, input, contHori_AccelPer), controllerHorizontal - contHori_changeMax, controllerHorizontal + contHori_changeMax);
                    }
                    // 現在の値より入力された値が小さいなら
                    else
                    {
                        controllerHorizontal = Mathf.Lerp(controllerHorizontal, input, contHori_DecelPer);
                    }
                }
                // 入力が0を跨いだ場合、0に修正
                else
                {
                    controllerHorizontal = 0.0f;
                }
            }
            else
            {
                if (controllerHorizontal <= 0.0f)
                {
                    // 現在の値より入力された値が小さいなら
                    if (controllerHorizontal >= input)
                    {
                        controllerHorizontal = Mathf.Clamp(Mathf.Lerp(controllerHorizontal, input, contHori_AccelPer), controllerHorizontal - contHori_changeMax, controllerHorizontal + contHori_changeMax);
                    }
                    // 現在の値より入力された値が大きいなら
                    else
                    {
                        controllerHorizontal = Mathf.Lerp(controllerHorizontal, input, contHori_DecelPer);
                    }
                }
                // 入力が0を跨いだ場合、0に修正
                else
                {
                    controllerHorizontal = 0.0f;
                }
            }
            controllerHorizontal = Mathf.Clamp(controllerHorizontal.Round(3), -1.0f, 1.0f);
            return controllerHorizontal;
        }
    }
    //====================================================
    // ハンコン入力の補正クラス
    //====================================================
    public static class InputHandleController
    {
        public static bool isConnectHandle = false;
        public static bool isG29 = false;
        public static bool isFocus = true;
        public static int inputDigit;
        private static float prevAccel;
        private static float prevBrake;
        private static float deadzoneZero = 0.05f;

        //====================================================
        public static void GetHandleAccelAndBrake(ref float _accel, ref float _brake)
        {
            if (isConnectHandle)
            {
                // 機種によって、入力する軸が異なるので、実際に入力されているものを選択する
                // また、ロジクール製品では、ドライバを立ち上げていないと入力を受け付けない軸がある為、
                // それを考慮して選択する
                float accel_1 = GetHandleAccel();
                float accel_2 = -Input.GetAxis("Pedal").Round(inputDigit);

                float accel = 0.0f;
                float brake = 0.0f;

                if (accel_2 > 0.0f)
                {
                    accel = accel_2;
                    brake = 0.0f;
                }
                else
                {
                    accel = 0.0f;
                    brake = -accel_2;
                }
                // アクセルとブレーキが0、つまり"Pedal"での入力ができなかった場合かつ、もう一方で入力が行えているなら
                if (accel.IsDeadzoneZero(deadzoneZero) && brake.IsDeadzoneZero(deadzoneZero))
                {
                    //if ((prevAccel != 0.0f && GetHandleAccel() != 0.0f) && (prevBrake != 0.0f && GetHandleBrake() != 0.0f))
                    //{
                        accel = accel_1;
                        brake = GetHandleBrake();
                    //}
                }
                // 入力更新
                _accel = accel;
                _brake = brake;

                prevAccel = GetHandleAccel();
                prevBrake = GetHandleBrake();
            }
        }
        //====================================================
        public static float GetHandleAccel()
        {
            if (isConnectHandle)
            {
                return ZeroToOneCorrection("Accel");
            }
            return 0.0f;
        }
        //====================================================
        public static float GetHandleBrake()
        {
            if (isConnectHandle)
            {
                return ZeroToOneCorrection("Brake");
            }
            return 0.0f;
        }
        //====================================================
        private static float ZeroToOneCorrection(string axisName)
        {
            // 未入力：1
            // 入力  ：-1
            float value = Input.GetAxis(axisName).Round(inputDigit);

            // 画面が有効でない場合、入力が入らず0になり、補正後に0.5になってしまう
            // その為、0を返す
            if (!isFocus) { return 0.0f; }

            // 1 ~ -1を、2 ~ 0にして、1 ~ 0に直す
            // その後、0 ~ 1にする
            value = (value + 1.0f) * 0.5f;
            value = Mathf.Clamp01(Mathf.Abs(value - 1));

            return value;
        }
        //====================================================
        // チェック用の入力を返す
        public static float GetHandleAccelCheck()
        {
            if (isConnectHandle)
            {
                float accel_1 = GetHandleAccel();
                float accel_2 = -Input.GetAxis("Pedal").Round(inputDigit);

                float returnAccel = 0.0f;

                if (accel_2 > 0.0f)
                {
                    returnAccel = accel_2;
                }
                if (returnAccel.IsDeadzoneZero() || isG29)
                {
                    returnAccel = accel_1;
                }
                return returnAccel;
            }
            return 0;
        }
    }

    public class InputController
    {
        public static bool isConnectHandle = false;
        public static bool isG29 = false;
        public static bool isFocus = true;
        public static int inputDigit;
        private static float prevAccel;
        private static float prevBrake;
        private static float deadzoneZero = 0.0001f;

        //====================================================
        public static void GetHandleAccelAndBrake(ref float _accel, ref float _brake)
        {
            if (isConnectHandle)
            {
                // 機種によって、入力する軸が異なるので、実際に入力されているものを選択する
                // また、ロジクール製品では、ドライバを立ち上げていないと入力を受け付けない軸がある為、
                // それを考慮して選択する
                float accel_1 = GetHandleAccel();
                float accel_2 = -Input.GetAxis("Pedal").Round(inputDigit);

                float accel = 0.0f;
                float brake = 0.0f;

                if (accel_2 > 0.0f)
                {
                    accel = accel_2;
                    brake = 0.0f;
                }
                else
                {
                    accel = 0.0f;
                    brake = -accel_2;
                }
                // アクセルとブレーキが0、つまり"Pedal"での入力ができなかった場合かつ、もう一方で入力が行えているなら
                if (accel.IsDeadzoneZero(deadzoneZero) && brake.IsDeadzoneZero(deadzoneZero))
                {
                    //if ((prevAccel != 0.0f && GetHandleAccel() != 0.0f) && (prevBrake != 0.0f && GetHandleBrake() != 0.0f))
                    //{
                    accel = accel_1;
                    brake = GetHandleBrake();
                    //}
                }
                // 入力更新
                _accel = accel;
                _brake = brake;

                prevAccel = GetHandleAccel();
                prevBrake = GetHandleBrake();
            }
        }
        //====================================================
        public static float GetHandleAccel()
        {
            if (isConnectHandle)
            {
                return ZeroToOneCorrection("Accel");
            }
            return 0.0f;
        }
        //====================================================
        public static float GetHandleBrake()
        {
            if (isConnectHandle)
            {
                return ZeroToOneCorrection("Brake");
            }
            return 0.0f;
        }
        //====================================================
        private static float ZeroToOneCorrection(string axisName)
        {
            // 未入力：1
            // 入力  ：-1
            float value = Input.GetAxis(axisName).Round(inputDigit);

            // 画面が有効でない場合、入力が入らず0になり、補正後に0.5になってしまう
            // その為、0を返す
            if (!isFocus) { return 0.0f; }

            // 1 ~ -1を、2 ~ 0にして、1 ~ 0に直す
            // その後、0 ~ 1にする
            value = (value + 1.0f) * 0.5f;
            value = Mathf.Clamp01(Mathf.Abs(value - 1));

            return value;
        }
        //====================================================
        // チェック用の入力を返す
        public static float GetHandleAccelCheck()
        {
            if (isConnectHandle)
            {
                float accel_1 = GetHandleAccel();
                float accel_2 = -Input.GetAxis("Pedal").Round(inputDigit);

                float returnAccel = 0.0f;

                if (accel_2 > 0.0f)
                {
                    returnAccel = accel_2;
                }
                if (returnAccel.IsDeadzoneZero() || isG29)
                {
                    returnAccel = accel_1;
                }
                return returnAccel;
            }
            return 0;
        }
    }
}