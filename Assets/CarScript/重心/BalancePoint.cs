/*------------------------------------------------------------------
* ファイル名：BalancePoint.cs
* 概要：車の重心を計算するクラス
* 担当者：ゴコケン
* 作成日：2022/06/24
-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BalancePoint : MonoBehaviour
{
// エディタのカスタマイズ用
#if UNITY_EDITOR
    public bool[] showParam = new bool[8]; // インスペクター用の変数
#endif
//━━━━━━━━━━━━━━━━━━━━━━━━━━
//           エディタに表示するプロパティ
//━━━━━━━━━━━━━━━━━━━━━━━━━━
    #region 速度・移動速度
    public Vector3 prevPos { get; private set; } = Vector3.zero;             // 前回記録するときの座標
    public float prevForwardSpeed { get; private set; } = 0;                 // 前のフレームの前への速度
    public float forwardSpeed { get; private set; } = 0;                     // 今前方への速度
    public float averageSpeed { get; private set; } = 0;                     // 平均速度 
    public Vector3 prevDirection { get; private set; } = Vector3.zero;       // 全フレームの移動ベクトル
    public Vector3 direction { get; private set; } = Vector3.zero;           // 移動ベクトル
    public float prevAscend { get; private set; } = 0;                       // 前回記録するときの上昇量
    public float ascend { get; private set; } = 0;                           // 上昇量
    #endregion

    #region 加速度・移動方向
    public float prevAcceleration { get; private set; } = 0;                 // 前のフレームの加速度
    public float acceleration { get; private set; } = 0;                     // 今の加速度
    public float balancePoint_z { get; private set; } = 0;                   // 重心のz軸部分を一時的に保存する用
    #endregion

    #region 定数
    public float framesBetweenUpdate = 1;             // 値を更新する間隔
    public float movePercent_z = 3.5f;
    public float movePercent_x = 75;
    public Vector2 speedBalancePointReposition = new Vector2(0.7f, 0.7f); // 重心の戻る速度
    public Vector2 speedBalancePointRepositionWhenBreak = new Vector2(0.2f, 0.9f);  // ブレーキを踏むときに重心の戻る速度
    public float ascendThreshold = 0.2f;               // 上昇量の閾値
    #endregion

    #region 係数
    public float farwordCoefficient = 1;
    public float rightCoefficient = 10;
    public float upCoefficient = 1;
    public float BrakeCorrection = 1;       // ブレーキ時の補正値(2023/08/09 船渡)
    #endregion

    #region 重心更新用（ｚ）
    public bool bIsAccelerationChange { get; private set; } = false;         // 加速度が更新したかを記録する用
    public float extremeValue_z { get; private set; } = 0;                   // 最大値/最小値を一時的に記録する変数
    public float differenceExtremeValue_z { get; private set; } = 0;         // 重心のz軸部分とextremeValue_zの間の差
    #endregion

    #region 重心更新用（ｘ）
    public bool bIsBalancePointXChange { get; private set; } = false;        // 遠心力が更新したかを記録する用
    public float extremeValue_x { get; private set; } = 0;                   // 最大値/最小値を一時的に記録する変数
    public float differenceExtremeValue_x { get; private set; } = 0;         // 重心のx軸部分とextremeValue_zの間の差
    #endregion

    #region 重心
    public Vector3 prevBalancePoint { get; private set; } = Vector3.zero;
    private Vector3 balancePoint = Vector3.zero;        // 重心
    public Vector3 _balancePoint { get { return balancePoint; } }
    #endregion

    #region 車本体の回転
    public Vector2 rotateMax = new Vector2(0, 0); // 車体の回転角度の上限
    private Vector2 rotateCnt = Vector2.zero;          // すでに回転した角度
    #endregion

//━━━━━━━━━━━━━━━━━━━━━━━━━━
//           エディタに表示しないプロパティ
//━━━━━━━━━━━━━━━━━━━━━━━━━━
    #region 参照するオブジェクト
    private GameObject root;                          // 親
    private Rigidbody rootRigidbody;                  // 親の剛体
    private GameObject carbody;                       // 車体
    private GameObject wheelCollideres;               // タイヤの当たり判定
    private ChairController chair;                    // 椅子を制御するクラス
    private AIM.Transmission transmission;            // ギアを取得用
    private AIM.VehicleController VehicleController;  // ブレーキ入力量を保存するための呼び水(2023/08/09 船渡)
    private AIM.WheelInput WheelInput;                // ブレーキ入力量の取得用(2023/08/09 船渡)
    #endregion

    #region 初期数値を保存する変数
    private float carbodyWidth;         // 車の幅
    private float carbodyHeight;        // 車の高さ
    private Vector3 carbodyLocalPos;    // 車体のローカル座標
    #endregion

    #region 時間をカウントする用
    private float prevTime = 0.0f;                     // 前回記録するときの時間
    private int cnt = 0;                               // フレームのカウント
    #endregion

    private float BrakeInput = 0.0f;     // ブレーキ入力量の保存(0809船渡)

//━━━━━━━━━━━━━━━━━━━━━━━━━━
//              　　　メソッド
//━━━━━━━━━━━━━━━━━━━━━━━━━━

    //━━━━━━━━━━━━━━━━━━━━━
    // ゲームの最初に呼ばれる関数
    //━━━━━━━━━━━━━━━━━━━━━
    void Start()
    {
        // 初期化
        Init();
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 毎フレームに呼ばれる関数
    //━━━━━━━━━━━━━━━━━━━━━
    void Update()
    {
        // 重心を計算する関数を呼び出し
        CalcBalancePoint();

        //---------------------------------------
        //2023/06/25 河合追記
        Vector3 BP;
        BP = balancePoint.x / 50.0f * rootRigidbody.transform.right;
        BP += /*balancePoint.y / 10000 */0.0f * rootRigidbody.transform.up;
        BP += (balancePoint.z / 10.0f) * rootRigidbody.transform.forward;

        rootRigidbody.centerOfMass = BP;      //重心を剛体に反映
        //---------------------------------------
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 初期化用の関数
    //━━━━━━━━━━━━━━━━━━━━━
    public void Init()
    {
        // 参照するクラスを取得しておく
        root = transform.root.gameObject;
        transmission = root.GetComponent<AIM.VehicleController>().transmission;
        rootRigidbody = root.GetComponent<Rigidbody>();
        carbody = root.transform.Find("carbody").gameObject;
        wheelCollideres = root.transform.Find("WheelColliders").gameObject;
        chair = root.transform.Find("WIZMO").GetComponent<ChairController>();

        // ----------ブレーキに関するクラスを取得(0809船渡)---------------
        VehicleController = root.GetComponent<AIM.VehicleController>();
        WheelInput = VehicleController.GetWheelInput;
        // ---------------------------------------------------------------

        // 前回の座標を記録する
        prevPos = root.transform.position;

        // 車の初期値を取得しておく
        carbodyWidth = carbody.GetComponent<MeshRenderer>().bounds.extents.x;
        carbodyHeight = carbody.GetComponent<MeshRenderer>().bounds.extents.y;
        carbodyLocalPos = carbody.transform.localPosition;

        //2023 河合奏追記---------------------------

        rootRigidbody.centerOfMass = Vector3.zero;
        //------------------------------------------
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 重心を計算する関数
    //━━━━━━━━━━━━━━━━━━━━━
    public void CalcBalancePoint()
    {
        // 全フレームの重心座標を保存しておく
        prevBalancePoint = balancePoint;

        // ブレーキ入力量を保存しておく(2023/08/09 船渡)
        BrakeInput = WheelInput.GetInputBreke + BrakeCorrection;

        // 経ったフレーム数をカウントする
        ++cnt;
        //Debug.Log("0------------------------------------------------------------------0_");
        // 一定フレーム数たったら、速度や加速度を更新する
        if (cnt > framesBetweenUpdate)
        {
            // カウンターをリセット
            cnt = 0;

            // 変数の更新
            UpdateParameter();

            // 重心の移動を計算する
            CalcBalancePointZ();
            CalcBalancePointX();
            CalcBalancePointY();
        }
        else
        {
            // 重心毎フレームの更新
            BalancePointUpdateZ();
            BalancePointUpdateX();

        }
        // 重心により、車体の回転や移動
        //CarMeshTransform();
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 変数の値を更新
    //━━━━━━━━━━━━━━━━━━━━━
    private void UpdateParameter()
    {
        // 前への速度を保存しておく
        prevForwardSpeed = forwardSpeed;

        // 新しい速度を計算する
        forwardSpeed = Vector3.Dot(rootRigidbody.velocity, root.transform.forward);

        // 値が小さすぎると０にする
        if (Mathf.Abs(forwardSpeed) < 0.0005f) forwardSpeed = 0;

        // 前の加速度を保存しておく
        prevAcceleration = acceleration;
        //Debug.Log($"0_acceleration,{acceleration}");

        // 新しい加速度を計算し、一定範囲の中にする
        acceleration = Mathf.Clamp((forwardSpeed - prevForwardSpeed) / (Time.time - prevTime), -5, 5);
        //Debug.Log($"0_ClampAcceleration,{acceleration}");

        // 新しい時間を記録する
        prevTime = Time.time;

        // 平均速度
        averageSpeed = (prevForwardSpeed + forwardSpeed) / 2;

        // 移動のベクトルを保存
        prevDirection = direction;
        direction = rootRigidbody.velocity.normalized;

        // 上昇量を計算
        prevAscend = ascend;
        if (Mathf.Abs(forwardSpeed) <= 0.001f) ascend = 0;
        else ascend = Vector3.Dot(rootRigidbody.velocity, root.transform.up);
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 重心の前後移動を計算する
    //━━━━━━━━━━━━━━━━━━━━━
    private void CalcBalancePointZ()
    {
        if (Mathf.Abs(forwardSpeed) < 1)
        {
            Debug.Log(extremeValue_z);
            if (extremeValue_z != 0)
            {
                extremeValue_z = 0;
                bIsAccelerationChange = true;
            }
            return;
        }

        else if (forwardSpeed > transmission.GetMaxSpeedForGear(transmission.Gear) - 1)
        {
            return;
        }

        if ((acceleration < -4 && prevAcceleration < -4) ||
            (acceleration > 4 && prevAcceleration > 4))
        {
            extremeValue_z = ((int)(acceleration * farwordCoefficient * 4.0f)) / 4.0f;
            chair.Accel((balancePoint.z + extremeValue_z) / 2);
           // Debug.Log("0_ChairUpdate_if");
           // Debug.Log($"0_extremeValue_z : {extremeValue_z}");
            //Debug.Log($"0_extremeValue_z : {(balancePoint.z + extremeValue_z) / 2}");
        }
        // 前の加速度と今の加速度の差が閾値を超えたら、
        // 重心を新しい加速度に近づくようにする
        else if ((Mathf.Abs(acceleration - prevAcceleration) / 5 )>= 0.1f)
        {
            // 重心がすでに移動している場合
            if (bIsAccelerationChange)
            {
                // 新しい加速度とすでに保存しておいた最大値の差が閾値を超えたら、
                // 新しいのに更新する
                float temp = ((int)(acceleration * farwordCoefficient * 4.0f)) / 4.0f;
                if (Mathf.Abs(temp - extremeValue_z) >= 0.25f)
                {
                    extremeValue_z = temp;

                    chair.Accel((balancePoint.z + extremeValue_z) / 2);
                    //Debug.Log("0_ChairUpdate_Accel_elif");
                    //Debug.Log($"0_extremeValue_z : {extremeValue_z}");
                    //Debug.Log($"0_extremeValue_z : {(balancePoint.z + extremeValue_z) / 2}");
                }
            }
            else
            {
                // 加速度が更新したと記録する
                bIsAccelerationChange = true;

                // 値を保存する
                extremeValue_z = ((int)(acceleration * farwordCoefficient * 4.0f)) / 4.0f;
                
                chair.Accel((balancePoint.z + extremeValue_z) / 2);
                //Debug.Log("0_ChairUpdate_Accel_else");
                //Debug.Log($"0_extremeValue_z : {extremeValue_z}");
                //Debug.Log($"0_extremeValue_z : {(balancePoint.z + extremeValue_z) / 2}");
            }
        }
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 重心の左右移動を計算する
    //━━━━━━━━━━━━━━━━━━━━━
    private void CalcBalancePointX()
    {
        // 移動していない、もしくはステアリングしていない場合は、
        // 最大値を０にし、
        // そのあとの処理をしないように
        if (forwardSpeed == 0 ||
            Mathf.Abs(direction.x - prevDirection.x) < 0.001f ||
            Mathf.Abs(direction.z - prevDirection.z) < 0.001f)
        {
            if (balancePoint.x != 0)
            {
                extremeValue_x = 0;
                bIsBalancePointXChange = true;
            }
            return;
        }

        // 回転する量を計算
        float rotate = Mathf.Abs(direction.x - prevDirection.x) + Mathf.Abs(direction.z - prevDirection.z);
        
        // 方向を決める
        float sign;
        if (Vector3.Cross(direction, prevDirection).y > 0)
            sign = 1;
        else
            sign = -1;

        // ｘ軸の計算
        var temp = averageSpeed * rotate * sign / 2 * rightCoefficient;

        // 新しい値で更新する
        extremeValue_x = Mathf.Clamp(temp, -5, 5);
        bIsBalancePointXChange = true;
        chair.Steering(extremeValue_x);
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 重心の上下移動を計算する
    //━━━━━━━━━━━━━━━━━━━━━
    private void CalcBalancePointY()
    {
        // 車が動いてない場合はリセットする
        if (Mathf.Abs(forwardSpeed) <= 0.0001f)
        {
            balancePoint.y = 0;
            return;
        }

        // 上昇量のさが閾値を超えたら、
        // 重心のy軸の部分を更新する
        if (Mathf.Abs(ascend - prevAscend) > ascendThreshold)
        {
            balancePoint.y = (ascend - prevAscend) * upCoefficient;
            chair.Vibration(balancePoint.y);
        }

        // 超えてない場合は０にする
        else balancePoint.y = 0;
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 重心毎フレームの更新(前後)
    //━━━━━━━━━━━━━━━━━━━━━
    private void BalancePointUpdateZ()
    {
        // 加速度が更新した場合
        if (bIsAccelerationChange)
        {
            //
            if(BrakeInput > 1.0f)
            {
                // 車の現在の速度によって重心の移動量に補正をかける
                if (forwardSpeed < 10.0f)
                {
                    movePercent_z = 0.4f;
                }

                else if (forwardSpeed < 20.0f)
                {
                    movePercent_z = 0.8f;
                }

                else
                {
                    movePercent_z = 1.0f;
                }

            }
            // 重心のz軸の部分を保存しておいた最大値に近づくようにする(ブレーキ入力分の補正を追加 : 2023/08/10)
            // 重心のZ軸 += (最大値 - 重心のZ軸) * 移動率(?) / 100(0807船渡)
            balancePoint_z += (extremeValue_z - balancePoint_z) * (movePercent_z * BrakeInput) / 100;

            // ↓元の式
            //balancePoint_z += (extremeValue_z - balancePoint_z) * movePercent_z / 100;

            differenceExtremeValue_z = Mathf.Abs(balancePoint_z - extremeValue_z);

            // 重心のz軸の部分と保存しておいた最大値の差が
            // 0.01よりも小さい場合は、
            // 重心のz軸の部分を保存しておいた最大値にする
            if (differenceExtremeValue_z <= 0.01f)
            {
                balancePoint_z = extremeValue_z;
                bIsAccelerationChange = false;
            }
            // 車が止まる時の処理
            if (forwardSpeed <= 0.75f || balancePoint_z <= -4.0f)
            {
               balancePoint_z *= speedBalancePointRepositionWhenBreak.y;
                Debug.Log("2_Call");
            }
        }
        else
        {
            differenceExtremeValue_z = 0;
            // 重心をだんだん中心に戻るようにする
            balancePoint_z *= speedBalancePointReposition.y;
        }

        // 重心を更新する
        balancePoint.z = -balancePoint_z;
        Debug.Log($"2_balancePoint.z : {balancePoint.z}");
        Debug.Log($"2_balancePoint_z : {balancePoint_z}");
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 重心毎フレームの更新(左右)
    //━━━━━━━━━━━━━━━━━━━━━
    private void BalancePointUpdateX()
    {
        // 遠心力が更新した場合
        if (bIsBalancePointXChange)
        {
            // 最大値と今の値の差を計算
            differenceExtremeValue_x = extremeValue_x - balancePoint.x;

            // その差が0.1よりも小さい場合
            if (Mathf.Abs(differenceExtremeValue_x) < 0.1f)
            {
                // 重心のx軸の部分を直接に保存しておいた最大値にする
                balancePoint.x = extremeValue_x;
                extremeValue_x = 0;
                differenceExtremeValue_x = 0;
                bIsBalancePointXChange = false;
            }
            else
            {
                // 最大値が小さすぎる場合は
                // 中心に戻るようにする
                if (Mathf.Abs(extremeValue_x) < 0.025f)
                {
                    extremeValue_x = 0;
                    balancePoint.x *= 0.9f;
                    differenceExtremeValue_x = extremeValue_x - balancePoint.x;
                }
                else
                {
                    balancePoint.x += differenceExtremeValue_x * movePercent_x / 100;
                    differenceExtremeValue_x = extremeValue_x - balancePoint.x;
                }
            }
        }
    }

    //━━━━━━━━━━━━━━━━━━━━━
    // 車の重心により、車体の回転や移動
    //━━━━━━━━━━━━━━━━━━━━━
    private void CarMeshTransform()
    {
        // 前後
        var temp = balancePoint.z / 5 * rotateMax.y;
        float angle = temp - rotateCnt.y;
        rotateCnt.y = temp;

        carbody.transform.RotateAround(root.transform.position, root.transform.right, angle);

        // 左右
        temp = balancePoint.x / 5 * rotateMax.x;
        angle = temp - rotateCnt.x;

        Vector3 pos = wheelCollideres.transform.position;
        pos -= root.transform.up * (carbodyHeight / 32);

        // 左へ
        if (angle < -0.01f)
        {
            if (rotateCnt.x > 0.1f)
            {
                if (temp < 0)
                {
                    pos += root.transform.right * (carbodyWidth / 2);
                    carbody.transform.RotateAround(pos, root.transform.forward, rotateCnt.x);
                    pos -= root.transform.right * (carbodyWidth);
                    carbody.transform.RotateAround(pos, root.transform.forward, -angle - rotateCnt.x);
                }
                else
                {
                    pos += root.transform.right * (carbodyWidth / 2);
                    carbody.transform.RotateAround(pos, root.transform.forward, -angle);
                }
            }
            else 
            {
                pos -= root.transform.right * (carbodyWidth / 2);
                carbody.transform.RotateAround(pos, root.transform.forward, -angle);
            }
        }
        // 右へ
        else if (angle > 0.01f)
        {
            if (rotateCnt.x < -0.1f)
            {
                if(temp > 0)
                {
                    pos -= root.transform.right * (carbodyWidth / 2);
                    carbody.transform.RotateAround(pos, root.transform.forward, rotateCnt.x);
                    pos += root.transform.right * (carbodyWidth);
                    carbody.transform.RotateAround(pos, root.transform.forward, -angle - rotateCnt.x);
                }
                else
                {
                    pos -= root.transform.right * (carbodyWidth / 2);
                    carbody.transform.RotateAround(pos, root.transform.forward, -angle);
                }
            }
            else
            {
                pos += root.transform.right * (carbodyWidth / 2);
                carbody.transform.RotateAround(pos, root.transform.forward, -angle);
            }
        }
        else
        {
            if(rotateCnt.x == 0)
            {
                carbodyLocalPos.x = carbody.transform.localPosition.x;
                carbody.transform.localPosition = carbodyLocalPos;
            }
            return;
        }
        rotateCnt.x = temp;
    }
}
