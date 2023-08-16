/*------------------------------------------------------------------
* ファイル名：CarPropertyObserver.cs
* 概要：車の各種値を監視、取得するクラス
* 担当者：船渡彩乃
* 作成日：2023/08/08
-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIM;

public class CarPropertyObserver : MonoBehaviour
{
    #region エディタに表示する値
    [Header("ShowFlag")]
    // 車の速度を見るかどうか
    [SerializeField] private bool bShowCarSpeed = true;

    // アクセルの入力量を見るかどうか
    [SerializeField] private bool bShowAxelPower = true;

    // ブレーキの入力量を見るかどうか
    [SerializeField] private bool bShowBrakePower = true;

    // 車の当たり判定を見るかどうか
    [SerializeField] private bool bShowCarWallHit = true;

    [Space(10)]
    [Header("Check")]
    // 値の確認間隔
    [SerializeField] private int CheckFrequency = 0;

    // 参照するオブジェクト
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject FCollider;

    #endregion

    #region 値を保存する変数
    // 値のカウント
    int CheakCount = 0;

    // 車の速度
    Vector3 CarSpeed;

    Vector3 GetCarSpeed
    {
        get { return CarSpeed; }
    }

    // アクセルの入力量
    float AxelInput;
    float GetAxelInput
    {
        get { return AxelInput; }
    }

    // ブレーキの入力量
    float BrakeInput;
    float GetBrakeInput
    {
        get { return BrakeInput; }
    }

    // 右の壁に当たったかのフラグ
    bool RightWallHit;
    bool GetRightWallHit
    {
        get { return RightWallHit; }
    }

    // 左の壁に当たったかのフラグ
    bool LeftWallHit;
    bool GetLeftWallHit
    {
        get { return LeftWallHit; }
    }
    #endregion

    #region 参照するクラス
    private Rigidbody CarRigidBody;
    private FrontCollider FrontCollider;
    private WheelInput WheelInput;
    private VehicleController VehicleController;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        // 初期化
        CarRigidBody = root.GetComponent<Rigidbody>();
        FrontCollider = FCollider.GetComponent<FrontCollider>();
        VehicleController = root.GetComponent<VehicleController>();
        WheelInput = VehicleController.GetWheelInput;
    }

    // Update is called once per frame
    void Update()
    {
        // カウンターがチェック頻度を超えていたら
        if (CheakCount >= CheckFrequency)
        {
            // カウンターをリセットして
            CheakCount = 0;

            // 各種値を取得する
            CarSpeed = CarRigidBody.velocity;
            AxelInput = WheelInput.GetInputAxel;
            BrakeInput = WheelInput.GetInputBreke;
            RightWallHit = FrontCollider.GetRightHit;
            LeftWallHit = FrontCollider.GetleftHit;

            // 各種値を表示する
            if (bShowCarSpeed) Debug.Log($"OB_CarSpeed : {CarSpeed}");
            if (bShowAxelPower) Debug.Log($"OB_AxelInput : {AxelInput}");
            if (bShowBrakePower) Debug.Log($"OB_BrakeInput : {BrakeInput}");
            if (bShowCarWallHit)
            {
                Debug.Log($"OB_RightWallHit : {RightWallHit}");

                Debug.Log($"OB_LeftWallHit : {LeftWallHit}");
            }
        }
    }
}
