/*------------------------------------------------------------------
* ファイル名：FrontCollider.cs
* 概要：壁にぶつかった後の処理をするクラス
* 担当者：ゴコケン
* 作成日：2022/05/16
-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIM;

public class FrontCollider : MonoBehaviour
{
//━━━━━━━━━━━━━━━━━━━━━━━━━━
//              エディタに表示する
//━━━━━━━━━━━━━━━━━━━━━━━━━━
    #region エディタに表示する値
    [SerializeField, Range(0, 50), Header("閾値（小さい方）"), Tooltip("ぶつかる角度がこれより小さいと処理しない")] 
        private float Threshold1 = 15;              // 角度補正の閾値

    [Space(10)]
    [SerializeField, Range(50, 120), Header("閾値（大きい方）"), Tooltip("ぶつかる角度がこれより大きいと無理やり回転し、速度を落とす")]
        private float Threshold2 = 135;              // 角度補正の閾値

    [Space(10)]
    [SerializeField, Header("処理を何フレームに分割するか"), Tooltip("回転速度などにも影響あり")] 
        private int NumberOfFramesToBeSplit = 30;   // 分割するフレーム数

    [Space(10)]
    [SerializeField] private bool SpeedDown = false;
	
	[Space(10)]
    [SerializeField, Header("角度が小さすぎるときの角度補正")] 
	private bool bAngleCorrection = false;
	
	[Space(10)]
    [SerializeField, Range(1, 100),Header("減速率(角度が小さい時)"), Tooltip("角度がThreshold1より小さい時の減速率")]
    private float DecelerationRate1 = /*20*/10;   // 減速率

    [Space(10)]
    [SerializeField, Range(1, 100), Header("減速率(角度が大きい時)"), Tooltip("角度がThreshold1より大きい時の減速率")]
    private float DecelerationRate2 = /*80*/40;   // 減速率

    [Space(10)]
    [SerializeField, Range(1, 100), Header("減速率(速度が速い時)"), Tooltip("一定速度以上でぶつかった時の減速率")]
    private float DecelerationRate3 = /*80*/40;   // 減速率

    [Space(10)]
    [SerializeField, Range(1, 100), Header("減速率(角度が大きすぎる時)"), Tooltip("角度がThreshold2より大きい時の減速率")]
    private float DecelerationRate4 = /*100*/ 50;   // 減速率

    [Space(10)]
    [SerializeField, Header("回転速度を決めるグラフ")] 
        private AnimationCurve rotateCurve = new AnimationCurve();
    #endregion

//━━━━━━━━━━━━━━━━━━━━━━━━━━
//              エディタに表示しない
//━━━━━━━━━━━━━━━━━━━━━━━━━━
    #region 参照するクラス
    private GameObject root;                    // 親
    private Rigidbody m_rootRigidbody;          // 親の鋼体
    private ChairController chair;
    #endregion

    #region 当たったかのフラグ
    private bool bRightHit = false;             // 右の壁に当たったか
    private bool bLeftHit = false;              // 左の壁に当たったか
    // ------------------フラグの取得用(0809船渡)---------------------
    // 常に壁に当たっているかどうかを監視する変数
    private bool bRigheHitWall = false; // 右の壁
    private bool bLeftHitWall = false; // 左の壁

    private int R_Count = 0;          // 値リセットのカウンター(右)
    private bool R_ResetFlag = false; // リセット監視フラグ(右)

    private int L_Count = 0;          // 値リセットのカウンター(左)
    private bool L_ResetFlag = false; // リセット監視フラグ(左)

    public bool GetRightHit
    {
        get { return bRigheHitWall; } // 右の壁に当たったか
    }

    public bool GetleftHit
    {
        get { return bLeftHitWall; } // 左の壁に当たったか
    }

    // --------------------------------------------------------------
    #endregion

    #region 角度補正用
    private int AngleCorrectionCnt;             // 角度補正用カウンター
    private float CorrectionAnglePerFlame = 0;  // フレームごとの補正角度
    #endregion

    #region 初期値を保存する変数
    private Vector3 startPos;
    private Vector3 startRotate;
    #endregion

    private void Start()
    {
        Init();
    }

    //━━━━━━━━━━━━━━━━━━━━━━━
    // 初期化用の関数
    //━━━━━━━━━━━━━━━━━━━━━━━
    public void Init()
    {
        AngleCorrectionCnt = NumberOfFramesToBeSplit + 1;

        root = transform.root.gameObject;
        m_rootRigidbody = root.GetComponent<Rigidbody>();

        startPos = this.transform.localPosition;
        startRotate = this.transform.localEulerAngles;

        if (rotateCurve.length == 0)
        {
            rotateCurve.AddKey(0, 2);
            rotateCurve.AddKey(1, 0);
        }

        //chair = root.transform.Find("WIZMO").GetComponent<ChairController>();
    }

    //━━━━━━━━━━━━━━━━━━━━━━━
    // 何かに当たるときに勝手に呼び出されるメソッド
    // 引数１：当たったオブジェクト
    //━━━━━━━━━━━━━━━━━━━━━━━
    private void OnCollisionEnter(Collision collision)
    {
        // 当たったもののtagを取得しておく
        var tag = collision.collider.tag;

        // 右壁
        if (tag == "RightWall")
        {
            if (AngleCorrectionCnt <= NumberOfFramesToBeSplit / 2 && bRightHit) return;

            //chair.Hit(true);

            // -----------壁に当たっているかどうかの監視(2023/08/09船渡)----------------
            bRigheHitWall = true;
            // ----------------------------------------------------------------------

            #region 角度の計算
            // 当たっている壁の法線を取得する
            Vector3 WallNormal = collision.contacts[0].normal;

            var WallVector = Quaternion.Euler(0, 90, 0) * WallNormal;

            // 車の方向
            Vector3 CarVector = Vector3.ProjectOnPlane(root.transform.forward, Vector3.up);

            // 車の方向を壁の法線に沿って反射した後のベクトル
            Vector3 VectorAfterReflection = CarVector + -Vector3.ProjectOnPlane(CarVector, WallVector) * 2;

            // 正規化
            VectorAfterReflection.Normalize();

            // 反射後のベクトルと壁の法線の間の角度を計算
            float angleBetweenWallNormalAndVectorAfterReflection =
                Mathf.Acos(VectorAfterReflection.x * WallVector.x + VectorAfterReflection.z * WallVector.z) * Mathf.Rad2Deg;
            #endregion

            #region 角度が小さいときの処理
            // 角度が小さすぎると処理しない
            if (angleBetweenWallNormalAndVectorAfterReflection < Threshold1)
            {
                if (SpeedDown) m_rootRigidbody.velocity *= (100.0f - DecelerationRate1) / 100.0f;
				if (bAngleCorrection) 
				{
					//Debug.LogError(Time.time + "右");
					CorrectionAnglePerFlame = (-angleBetweenWallNormalAndVectorAfterReflection - 5) /
											  (float)NumberOfFramesToBeSplit;

					if (Vector3.Dot(m_rootRigidbody.velocity, root.transform.forward) > 40)
						CorrectionAnglePerFlame *= (100.0f - DecelerationRate3) / 100.0f;

					AngleCorrectionCnt = 0;
					transform.Rotate(new Vector3(0, CorrectionAnglePerFlame * rotateCurve.Evaluate((float)AngleCorrectionCnt / (float)NumberOfFramesToBeSplit), 0));
				}
					
                return;
            }
            #endregion

            #region 角度が大きすぎるときの処理
            // 角度が大きすぎると車の角度を無理やりに戻し、速度を落とす
            else if (angleBetweenWallNormalAndVectorAfterReflection > Threshold2)
            {
                bRightHit = true;
                root.GetComponent<Rigidbody>().angularVelocity *= 100.0f - DecelerationRate4;
                if (SpeedDown) m_rootRigidbody.velocity = Vector3.zero;
                CorrectionAnglePerFlame = Vector3.SignedAngle(CarVector, Quaternion.Euler(0, -10, 0) * WallVector, Vector3.up) / (float)NumberOfFramesToBeSplit;
                AngleCorrectionCnt = 0;
                transform.Rotate(new Vector3(0, CorrectionAnglePerFlame * rotateCurve.Evaluate((float)AngleCorrectionCnt / (float)NumberOfFramesToBeSplit), 0));
            }
            #endregion

            #region それ以外の時の処理
            else
            {
                root.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                if (SpeedDown)m_rootRigidbody.velocity *= (100.0f - DecelerationRate2) / 100.0f;

                // VectorAfterReflectionを回転し、保存する
                Vector3 NewVector;
                if (angleBetweenWallNormalAndVectorAfterReflection < 30)
                {
                    NewVector = Quaternion.Euler(0, angleBetweenWallNormalAndVectorAfterReflection, 0) * VectorAfterReflection;
                }
                else
                {
                    NewVector = Quaternion.Euler(0, angleBetweenWallNormalAndVectorAfterReflection / 2f, 0) * VectorAfterReflection;
                }

                bRightHit = true;
                CorrectionAnglePerFlame = Vector3.SignedAngle(CarVector, NewVector, Vector3.up) / (float)NumberOfFramesToBeSplit;

                if (Vector3.Dot(m_rootRigidbody.velocity, root.transform.forward) > 40)
                    CorrectionAnglePerFlame *= (100.0f - DecelerationRate3) / 100.0f;

                AngleCorrectionCnt = 0;
                transform.Rotate(new Vector3(0, CorrectionAnglePerFlame * rotateCurve.Evaluate((float)AngleCorrectionCnt / (float)NumberOfFramesToBeSplit), 0));

                #region デバッグ用
#if true
                Debug.DrawRay(collision.GetContact(0).point, WallNormal, new Color(1, 0, 0), 30);
                Debug.DrawRay(collision.GetContact(0).point, WallVector, new Color(1, 0, 0), 30);
                Debug.DrawRay(collision.GetContact(0).point, CarVector, new Color(1, 1, 1), 30);
                Debug.DrawRay(collision.GetContact(0).point, VectorAfterReflection, new Color(0, 0, 1), 30);
                Debug.DrawRay(collision.GetContact(0).point, NewVector, new Color(0, 1, 0), 30);
               //Debug.Log("角度:" + angleBetweenWallNormalAndVectorAfterReflection);
               //Debug.Log("新しいベクトル：" + NewVector);
               //Debug.Log("CorrectionAnglePerFlame：" + CorrectionAnglePerFlame);
#endif
                #endregion
            }
            #endregion
        }
        // 左壁
        else if (tag == "LeftWall")
        {
            if (AngleCorrectionCnt <= NumberOfFramesToBeSplit / 2 && bLeftHit) return;

            //chair.Hit(false);

            // -----------壁に当たっているかどうかの監視(2023/08/09船渡)----------------
            bLeftHitWall = true;
            // ----------------------------------------------------------------------

            #region 角度の計算
            // 当たっている壁の法線を取得する
            Vector3 WallNormal = collision.contacts[0].normal;

            var WallVector = Quaternion.Euler(0, -90, 0) * WallNormal;

            // 車の方向
            Vector3 CarVector = Vector3.ProjectOnPlane(root.transform.forward, Vector3.up);

            // 車の方向を壁の法線に沿って反射した後のベクトル
            Vector3 VectorAfterReflection = CarVector + -Vector3.ProjectOnPlane(CarVector, WallVector) * 2;

            // 正規化
            VectorAfterReflection.Normalize();

            // 反射後のベクトルと壁の法線の間の角度を計算
            float angleBetweenWallNormalAndVectorAfterReflection =
                Mathf.Acos(VectorAfterReflection.x * WallVector.x + VectorAfterReflection.z * WallVector.z) * Mathf.Rad2Deg;
            #endregion

            #region 角度が小さいときの処理
            // 角度が小さすぎると処理しない
            if (angleBetweenWallNormalAndVectorAfterReflection < Threshold1)
            {
                if (SpeedDown) m_rootRigidbody.velocity *= (100.0f - DecelerationRate1) / 100.0f;
				if (bAngleCorrection)
				{
					//Debug.LogError(Time.time + "左");
					CorrectionAnglePerFlame = (angleBetweenWallNormalAndVectorAfterReflection + 5) /
											  (float)NumberOfFramesToBeSplit;

					if (Vector3.Dot(m_rootRigidbody.velocity, root.transform.forward) > 40)
						CorrectionAnglePerFlame *= (100.0f - DecelerationRate3) / 100.0f;

					AngleCorrectionCnt = 0;
					transform.Rotate(new Vector3(0, CorrectionAnglePerFlame * rotateCurve.Evaluate((float)AngleCorrectionCnt / (float)NumberOfFramesToBeSplit), 0));
				}
				return;
            }
            #endregion

            #region 角度が大きい過ぎる時の処理
            // 角度が大きすぎると車の角度を無理やりに戻し、速度を落とす
            else if (angleBetweenWallNormalAndVectorAfterReflection > Threshold2)
            {
                bLeftHit = true;
                if (SpeedDown) m_rootRigidbody.velocity *= (100.0f - DecelerationRate4) / 100.0f;
                CorrectionAnglePerFlame = Vector3.SignedAngle(CarVector, Quaternion.Euler(0, +10, 0) * WallVector, Vector3.up) / (float)NumberOfFramesToBeSplit;
                AngleCorrectionCnt = 0;
                transform.Rotate(new Vector3(0, CorrectionAnglePerFlame * rotateCurve.Evaluate((float)AngleCorrectionCnt / (float)NumberOfFramesToBeSplit), 0));
            }
            #endregion

            #region それ以外の処理
            else
            {
                root.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                if (SpeedDown) m_rootRigidbody.velocity *= (100.0f - DecelerationRate2) / 100.0f;

                // VectorAfterReflectionを回転し、保存する
                Vector3 NewVector;
                if (angleBetweenWallNormalAndVectorAfterReflection < 30)
                {
                    NewVector = Quaternion.Euler(0, -angleBetweenWallNormalAndVectorAfterReflection, 0) * VectorAfterReflection;
                }
                else
                {
                    NewVector = Quaternion.Euler(0, -angleBetweenWallNormalAndVectorAfterReflection / 2f, 0) * VectorAfterReflection;
                }

                bLeftHit = true;
                CorrectionAnglePerFlame = Vector3.SignedAngle(CarVector, NewVector, Vector3.up) / (float)NumberOfFramesToBeSplit;

                if (Vector3.Dot(m_rootRigidbody.velocity, root.transform.forward) > 40)
                    CorrectionAnglePerFlame *= (100.0f - DecelerationRate3) / 100.0f;

                AngleCorrectionCnt = 0;
                transform.Rotate(new Vector3(0, CorrectionAnglePerFlame * rotateCurve.Evaluate((float)AngleCorrectionCnt / (float)NumberOfFramesToBeSplit), 0));

                #region デバッグ用
#if true
                Debug.DrawRay(collision.GetContact(0).point, WallNormal, new Color(1, 0, 0), 30);
                Debug.DrawRay(collision.GetContact(0).point, WallVector, new Color(1, 0, 0), 30);
                Debug.DrawRay(collision.GetContact(0).point, CarVector, new Color(1, 1, 1), 30);
                Debug.DrawRay(collision.GetContact(0).point, VectorAfterReflection, new Color(0, 0, 1), 30);
                Debug.DrawRay(collision.GetContact(0).point, NewVector, new Color(0, 1, 0), 30);
               //Debug.Log("角度:" + angleBetweenWallNormalAndVectorAfterReflection);
               //Debug.Log("新しいベクトル：" + NewVector);
               //Debug.Log("CorrectionAnglePerFlame：" + CorrectionAnglePerFlame);
#endif
                #endregion
            }
            #endregion
        }

    }

    //━━━━━━━━━━━━━━━━━━━━━━━
    // 何かに当たり続けるときに勝手に呼び出されるメソッド
    // 引数１：当たったオブジェクト
    //━━━━━━━━━━━━━━━━━━━━━━━
    private void OnCollisionStay(Collision collision)
    {
        //// 当たったもののtagを取得しておく
        var tag = collision.collider.tag;
        
        // 右壁
        if (tag == "RightWall")
        {
			root.transform.Rotate(new Vector3(0, -0.25f, 0));
        }
        else if (tag == "LeftWall")
        {
			root.transform.Rotate(new Vector3(0, 0.25f, 0));
		}
    }

    //━━━━━━━━━━━━━━━━━━━━━━━
    // 椅子の角度などの更新
    //━━━━━━━━━━━━━━━━━━━━━━━
    private void RotationUpdate()
    {
        // 回転中の処理
        if (AngleCorrectionCnt++ < NumberOfFramesToBeSplit)
        {
            root.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            m_rootRigidbody.angularVelocity = Vector3.zero;
            root.transform.Rotate(new Vector3(0, CorrectionAnglePerFlame * rotateCurve.Evaluate((float)AngleCorrectionCnt / (float)NumberOfFramesToBeSplit), 0));
        }
        // リセット
        else if(AngleCorrectionCnt == NumberOfFramesToBeSplit)
        {
            bRightHit = false;
            bLeftHit = false;
        }
        // コリジョンボックスの角度を固定
        transform.localEulerAngles = startRotate;
    }

    private void Update()
    {
        // 車の回転
        RotationUpdate();

        // コリジョンボックスの位置を固定
        transform.localPosition = startPos;

        // -----------壁に当たっているかどうかの監視(2023/08/09船渡)----------------
        // ----------------------右壁--------------------------
        // 壁に当たっている場合
        if (bRigheHitWall)
        {
            // カウントを増加させる
            ++R_Count;

            // 1フレーム経過したら
            if (R_Count > 2)
            {
                Debug.Log("R_CallHitResetCount");
                // リセットフラグをオンにして
                R_ResetFlag = true;
                // カウンターをリセットする
                R_Count = 0;
            }
        }

        // リセットフラグがオンの場合
        if (R_ResetFlag)
        {
            Debug.Log("R_CallHitReset");
            // 右の壁に当たっているフラグがオンの場合
            if (bRigheHitWall)
            {
                // フラグをオフにする
                bRigheHitWall = false;
            }

            // リセットフラグをオフにする
            R_ResetFlag = false;
        }
        //--------------------------------------------------------

        // ----------------------左壁--------------------------
        // 壁に当たっている場合
        if (bLeftHitWall)
        {
            // カウントを増加させる
            ++L_Count;

            // 1フレーム経過したら
            if (L_Count > 2)
            {
                Debug.Log("L_CallHitResetCount");
                // リセットフラグをオンにして
                L_ResetFlag = true;
                // カウンターをリセットする
                L_Count = 0;
            }
        }

        // リセットフラグがオンの場合
        if (L_ResetFlag)
        {
            Debug.Log("L_CallHitReset");
            // 右の壁に当たっているフラグがオンの場合
            if (bLeftHitWall)
            {
                // フラグをオフにする
                bLeftHitWall = false;
            }

            // リセットフラグをオフにする
            L_ResetFlag = false;
        }


        // ----------------------------------------------------------------------
    }
}