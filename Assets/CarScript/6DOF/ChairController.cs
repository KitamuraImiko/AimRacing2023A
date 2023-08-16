/*------------------------------------------------------------------
* ファイル名：ChairController.cs
* 概要：椅子をコントロール用のクラス
* 担当者：ゴコケン
* 作成日：2022/07/15
-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;  //シーン遷移用

public class ChairController : MonoBehaviour
{
#if UNITY_EDITOR
	public bool showGraph = true;   // エディタ用
#endif
	//━━━━━━━━━━━━━━━━━━━━━━━━━━
	//              　　　プロパティ
	//━━━━━━━━━━━━━━━━━━━━━━━━━━
	#region 参照するクラス
	private Rigidbody rootRigidbody;                        // 車の鋼体
	public BalancePoint balancePoint { get; private set; }  // 重心のクラス
	public WIZMOController controller { get; private set; } // 椅子を制御するクラス
	private GoalJudgment goal;                              // ゴールしたかを取得用
	private GameObject transform1;                          // 車の座標を取得用
	private AIM.WheelInput input;                           // 入力取得用
	#endregion

	#region 椅子の動きを調整する用
	public float farwordCoefficient = 1;                    // 前後の動きを調整用の係数
	public float rightCoefficient = 1;                      // 左右の動きを調整用の係数
	public float upCoefficient = 1;                         // 上下の動きを調整用の係数

	public float accelCoefficient = 1;
	public float brakeCoefficient = 0.5f;
	public float shiftUpCoefficient = 1;
	public float shiftDownCoefficient = 1;

	public Vector3 speedCoefficient = new Vector3(1, 1, 1);

	public float maxRotate = 1;                             // 椅子の最大回転量
	public Vector2 impactCoefficient = new Vector2(1, 1);   // ぶつかった後の衝撃を調整用の係数
	public float OverallTravelFactor = 0.5f;                // 全体の移動量を調整用の係数
	public float OverallSpeedFactor = 1;                      // 速度を調整用の係数
	#endregion

	#region デバッグ用
	public bool bIsConnected { get; private set; } = false;         // 椅子に接続したかのフラグ
	private int reconnectCnt = 0;                                   // 再接続までのカウンター
	public bool bEmergencyShutdown { get; private set; } = false;   // 緊急停止中かどうか
	#endregion

	#region フレームをカウント用
	public int zAxisCnt { get; private set; }   // ｚ軸の移動したフレーム数を保存用
	public int zAxisThreshold = 3;              // ｚ軸の移動したフレーム数の閾値
	#endregion

	#region 補間用
	public bool bBalancePointMoved_x { get; private set; }     // ｘ軸移動したかのフラグ
	public bool bBalancePointMoved_y { get; private set; }     // ｙ軸移動したかのフラグ
	public bool bBalancePointMoved_z { get; private set; }    // ｚ軸移動したかのフラグ
	public Vector3 movePercent = new Vector3(70, 70, 70);     // 毎回移動するときのパーセント
	public Vector3 returnPercent = new Vector3(20, 20, 20); // 戻るときのパーセント
	private Vector3 targetPos = Vector3.zero;                 // 移動先の「座標」
#if UNITY_EDITOR
	public Vector3 _targetPos { get { return targetPos; } }   // エディタに表示する用
#endif
	#endregion

	#region 振動
	public float RollingRandomRange = 0.05f;                  // 振動のランダム範囲
	public int RollingCnt { get; private set; } = 0;          // 振動の間隔をカウントする用
	public AnimationCurve rollingCurve;                       // 振動の間隔を決めるグラフ
	public Vector3 RollingCoefficient = new Vector3(1, 1, 1); // 振動の激しさを調整する用の係数
	#endregion

	#region ぶつかった後の処理用
	public bool bGotHit { get; private set; } = false;          // ヒットしたかのフラグ
	public int hitCnt { get; private set; } = 0;                // 処理の時間を計算用カウンター
	public int hitCntThreshold = 40;                            // 処理にかけるフレーム数を決める変数
	public float hitSpeed { get; private set; }                 // ヒットした後椅子の速度を保存用
	public float pitchAfterHit { get; private set; }            // ヒットした後ピッチの値を保存用
	public float rollAfterHit { get; private set; }             // ヒットした後ロールの値を保存用
	public float yawAfterHit { get; private set; }              // ヒットした後ヨーの値を保存用
	public AnimationCurve hitImpactCurve;                       // ヒットする時の衝撃を決めるグラフ
	public Vector2 returnPercentAfterHit = new Vector2(50, 50); // 戻の位置に戻すパーセント
	public float accelerationAfterHit = 0.8f;                       // ヒットした後椅子の加速度
	public float maxSpeedAfterHit = 0.6f;                          // ヒットした後椅子の最大速度
	#endregion

	string SceneName = "Title";

	//━━━━━━━━━━━━━━━━━━━━━━━━━━
	//              　　　メソッド
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
		// 接続のチェック
		#region if(!CheckChairConnect()) return;
#if !UNITY_EDITOR
		// 元の処理
		if(!CheckChairConnect()) return;
#else
		// デバッグ用
		CheckChairConnect();
#endif
		#endregion

		// 緊急停止キー
		if (!EmergencyShutdown()) return;

		//シーン別の処理を呼び出す
		SceneCollFunc(SceneName);

	}

	//━━━━━━━━━━━━━━━━━━━━━
	// オブジェクトが消される時に呼ばれる関数
	//━━━━━━━━━━━━━━━━━━━━━
	private void OnDestroy()
	{
		// 椅子の位置をリセット
		ResetChair();
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 初期化用の関数
	//━━━━━━━━━━━━━━━━━━━━━
	public void Init()
	{/*
		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += GetSceneName;*/

		// 各フラグのリセット
		bEmergencyShutdown = false;
		bBalancePointMoved_x = false;
		bBalancePointMoved_y = false;
		bBalancePointMoved_z = false;

		// 参照するクラスを取得しておく
		balancePoint = transform.root.gameObject.transform.Find("carbody").GetComponent<BalancePoint>();
		controller = transform.gameObject.GetComponent<WIZMOController>();
		rootRigidbody = transform.root.GetComponent<Rigidbody>();
		transform1 = transform.root.transform.Find("carbody").Find("transform1").gameObject;
		goal = GameObject.Find("GoalGate").GetComponent<GoalJudgment>();
		input = transform.root.GetComponent<AIM.VehicleController>().wheelInput;

		// 椅子をリセット
		ResetChair();

		// 値の初期化
		targetPos = Vector3.zero;
		zAxisCnt = 0;

		// グラフの初期化
		// 振動の間隔
		rollingCurve.AddKey(0.5f, 30);
		rollingCurve.AddKey(3, 24);
		rollingCurve.AddKey(8, 24);
		rollingCurve.AddKey(35, 26);
		rollingCurve.AddKey(60, 30);
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 椅子をリセット用の関数
	//━━━━━━━━━━━━━━━━━━━━━
	public void ResetChair()
	{
		// 速度
		controller.speedAxis123 = 0.4f;

		// 加速度
		controller.accelAxis123 = 0.25f;

		// 前後
		controller.pitch = 0;

		// 左右
		controller.roll = 0;
		controller.yaw = 0;

		// 上下
		controller.heave = 0;

		// ほかの使っていない変数
		controller.sway = 0;
		controller.surge = 0;
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 椅子との接続をチェックする関数
	// 戻り値：使っていい場合はtrue、
	// 　　　　それ以外はfalse
	//━━━━━━━━━━━━━━━━━━━━━
	private bool CheckChairConnect()
	{
		// コントローラーが見つけない場合はこの後の処理をしない
		if (controller == null) return false;

		// 椅子が正常に動いているかをチェック
		bIsConnected = controller.isRunning();
		if (!bIsConnected)
		{
			// もし接続していない場合、
			// 600フレーム毎に接続してみる
			if (reconnectCnt++ >= 600)
			{
				// カウンターをリセット
				reconnectCnt = 0;

				// 再接続
				controller.OpenSIMVR();
				return false;
			}
			else return false;
		}
		return true;
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 椅子の緊急停止の関数
	// 戻り値：使っていい場合はtrue、
	// 　　　　それ以外はfalse
	//━━━━━━━━━━━━━━━━━━━━━
	public bool EmergencyShutdown()
	{
		// 通常状態
		if (!bEmergencyShutdown)
		{
			// キーが押されたら、緊急停止します
			if (Input.GetKeyDown(KeyCode.F7))
			{
				bEmergencyShutdown = true;

				// 値を戻す
				ResetChair();

				controller.CloseSIMVR();
				return false;
			}
			return true;
		}
		// 緊急停止中
		else
		{
			// もう一度押すと元に戻す
			if (Input.GetKeyDown(KeyCode.F7))
			{
				bEmergencyShutdown = false;

				// 値を戻す
				controller.OpenSIMVR();
			}

			return false;
		}
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 椅子の値を更新する関数
	//━━━━━━━━━━━━━━━━━━━━━
	private void UpdateValueOfChair()
	{
		//━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
		// 優先順位で処理を分ける（１が一番高い）
		// １：ぶつかった後の処理
		// ２：シフトチェンジ・前後の処理　の最初のフレームの処理
		// ３：左右の処理（戻る処理を除く）
		// ４：シフトチェンジ・前後の処理　の続き（戻る処理を除く）
		// ５：前後・左右　の戻る処理
		// ６：振動・上下　の処理
		// 
		// ※５番と６番は毎フレームやる
		//━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
		#region １：ぶつかった後の処理

		// ヒットした場合のみやる
		if (bGotHit)
		{
			// カウントをインクリメント
			++hitCnt;
			//
			//float temp = DampedSinWave(hitCnt, hitCntThreshold / 2, 
			//	(_t) => { return Mathf.Pow(0.3f, Mathf.Sqrt(_t)); });
			//
			if (hitCnt < hitCntThreshold / 3)
			{
				controller.pitch = pitchAfterHit;
				controller.roll = rollAfterHit;
				controller.yaw = yawAfterHit;

				controller.accelAxis123 = 0.8f;
				controller.speedAxis123 = Mathf.Clamp(hitSpeed * OverallSpeedFactor, 0, maxSpeedAfterHit);
			}
			// 戻すの処理
			else
			{
				// 前後の処理
				// アクセル踏んでいる場合
				if (input.inputAxel >= 0.01f ||
					Input.GetKey(KeyCode.UpArrow) ||
					Input.GetKey(KeyCode.W))
				{
					if (Mathf.Abs(controller.pitch - 0.2f) > 0.05f)
					{
						controller.pitch = ((controller.pitch - 0.2f) * (100 - returnPercentAfterHit.y) / 100) + 0.2f;
					}
					else controller.pitch = 0.2f;
				}
				// 踏んでいない場合は普通に０に戻す
				else
				{
					if (Mathf.Abs(controller.pitch) > 0.05f) controller.pitch *= (100 - returnPercentAfterHit.y) / 100;
					else controller.pitch = 0;
				}

				// 左右の処理
				if (Mathf.Abs(controller.roll) > 0.05f) controller.roll *= (100 - returnPercentAfterHit.x) / 100;
				else controller.roll = 0;
				if (Mathf.Abs(controller.yaw) > 0.05f) controller.yaw *= (100 - returnPercentAfterHit.x) / 100;
				else controller.yaw = 0;

				// スピードをセットする
				controller.speedAxis123 = 0.25f;
			}

			if (hitCnt > hitCntThreshold)
			{
				// 値やフラグのリセット
				bGotHit = false;
				hitCnt = 0;
				controller.speedAxis123 = 0.4f;
				controller.accelAxis123 = 0.25f;
			}
			return;
		}

		#endregion

		#region ２：シフトチェンジ・前後の処理　の最初のフレームの処理

		//else if (bBalancePointMoved_z && zAxisCnt <= zAxisThreshold)
		//{
		//	++zAxisCnt;
		//	float temp = (targetPos.z - controller.pitch) * movePercent.z / 100;

		//	// スピードをセットする
		//	controller.speedAxis123 = Mathf.Clamp(Mathf.Abs(temp) * speedCoefficient.z * OverallSpeedFactor, 0.1f, 1);

		//	// 移動させる
		//	controller.pitch += temp;

		//	// ついたかをチェック
		//	if (Mathf.Abs(controller.pitch - targetPos.z) < 0.1f)
		//	{
		//		controller.pitch = targetPos.z;
		//		bBalancePointMoved_z = false;
		//	}
		//}

		#endregion

		#region ３：左右の処理（戻る処理を除く）

		else if (bBalancePointMoved_x && balancePoint.forwardSpeed > 5)
		{
			bBalancePointMoved_z = false;

			float temp = (targetPos.x - controller.roll) * movePercent.x / 100;

			// 速度を変える
			controller.speedAxis123 = Mathf.Clamp(Mathf.Abs(temp) * speedCoefficient.x * OverallSpeedFactor, 0.1f, 1);

			// 移動させる
			controller.roll += temp;
			controller.yaw -= temp;

			// ついたかをチェック
			if (Mathf.Abs(controller.roll - targetPos.x) < 0.1f)
			{
				controller.roll = targetPos.x;
				controller.yaw = -targetPos.x;
				bBalancePointMoved_x = false;
			}
		}

		#endregion

		#region ４：シフトチェンジ・前後の処理　の続き（戻る処理を除く）

		else if (bBalancePointMoved_z)
		{
			float temp = (targetPos.z - controller.pitch) * movePercent.z / 100;

			// スピードをセットする
			controller.speedAxis123 = Mathf.Clamp(Mathf.Abs(temp) * speedCoefficient.z * OverallSpeedFactor, 0.1f, 1);

			// 移動させる
			controller.pitch += temp;

			// ついたかをチェック
			if (Mathf.Abs(controller.pitch - targetPos.z) < 0.1f)
			{
				controller.pitch = targetPos.z;
				bBalancePointMoved_z = false;
			}
		}

		#endregion

		#region 他：１、２、３以外の場合

		else
		{
			// 速度と加速度をリセット
			controller.speedAxis123 = 0.4f;
			controller.accelAxis123 = 0.25f;
		}

		#endregion

		#region ５：前後・左右　の戻る処理

		// 値を戻す
		// 左右
		if (!bBalancePointMoved_x)
		{
			if (Mathf.Abs(controller.roll) > 0.05f) controller.roll *= (100 - returnPercent.x) / 100;
			else controller.roll = 0;

			if (Mathf.Abs(controller.yaw) > 0.05f) controller.yaw *= (100 - returnPercent.x) / 100;
			else controller.yaw = 0;
		}

		// 前後
		if (input.inputBrake > 0.01f ||
				Input.GetKey(KeyCode.S) ||
				Input.GetKey(KeyCode.DownArrow))
		{
			// 速度が低い場合は真ん中の位置に戻す
			if (Vector3.Dot(rootRigidbody.velocity, transform1.transform.forward) < 5)
			{
				controller.pitch = 0;
				controller.speedAxis123 = 0.5f;
				controller.accelAxis123 = 0.4f;
			}
		}
		else if (!bBalancePointMoved_z)
		{
			// アクセルを踏んでいる時は０じゃなくで
			// ０．２ｆに戻す
			if (input.inputAxel >= 0.01f ||
				Input.GetKey(KeyCode.UpArrow) ||
				Input.GetKey(KeyCode.W))
			{
				if (Mathf.Abs(controller.pitch - 0.2f) > 0.05f)
				{
					controller.pitch = ((controller.pitch - 0.2f) * (100 - returnPercent.z) / 100) + 0.2f;
				}
				else controller.pitch = 0.2f;
			}
			// 踏んでいない場合は普通に０に戻す
			else
			{
				if (Mathf.Abs(controller.pitch) > 0.05f) controller.pitch *= (100 - returnPercent.z) / 100;
				else controller.pitch = 0;
			}
		}

		#endregion

		#region ６：振動・上下　の処理

		// 振動
		Rolling(balancePoint.forwardSpeed);

		// 上下
		if (bBalancePointMoved_y)
		{
			controller.heave += (targetPos.y - controller.heave) * movePercent.y / 100;
			if (Mathf.Abs(controller.heave - targetPos.y) < 0.1f)
			{
				controller.heave = targetPos.y;
				bBalancePointMoved_y = false;
			}
		}

		#endregion

		#region 他：範囲を超えないようにする

		controller.roll = Mathf.Clamp(controller.roll, -maxRotate, maxRotate);
		controller.pitch = Mathf.Clamp(controller.pitch, -maxRotate, maxRotate);
		controller.heave = Mathf.Clamp(controller.heave, -maxRotate, maxRotate);
		controller.yaw = Mathf.Clamp(controller.yaw, -maxRotate, maxRotate);
		controller.speedAxis123 = Mathf.Clamp(controller.speedAxis123, 0, 1);
		controller.accelAxis123 = Mathf.Clamp(controller.accelAxis123, 0, 1);

		#endregion

	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 速度が変えた後の処理
	// 引数１：重心のｚ軸値
	//━━━━━━━━━━━━━━━━━━━━━
	public void Accel(float _z)
	{
		bool bAccel = false;
		bool bBrake = false;

		// ブレーキとアクセルの値を調整
		if (input.inputBrake > 0.01f || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			_z *= brakeCoefficient;
			bBrake = true;
		}
		else if (input.inputAxel > 0.01f || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			_z *= accelCoefficient;
			bAccel = true;
		}

		float temp = Mathf.Clamp(_z * farwordCoefficient / 2.5f * OverallTravelFactor, -maxRotate, maxRotate);

		if (bBalancePointMoved_z)
		{
			if (targetPos.z > 0 && temp > 0 && temp < targetPos.z && bAccel)
			{
				return;
			}
			else if (targetPos.z < 0 && temp < 0 && temp > targetPos.z && bBrake)
			{
				if (targetPos.z > 0)
				{
					targetPos.z = -0.1f;
				}
				return;
			}
		}

		// カウンターをリセット
		zAxisCnt = 0;

		// フラグを立てる
		bBalancePointMoved_z = true;

		// 移動した後の値を計算する
		targetPos.z = temp;

		// アクセルを踏んでいるかをチェック
		if (bAccel)
		{
			// アクセルを踏んでいるのに、椅子が前に傾けるのを防ぐ
			if (targetPos.z < 0.2f) targetPos.z = 0.2f;

			// 車が動き始める時の傾ける量を大きくする
			if (Vector3.Dot(rootRigidbody.velocity, transform1.transform.forward) < 5)
			{
				targetPos.z = Mathf.Clamp(targetPos.z * 1.5f, -maxRotate, maxRotate);
			}
		}
		else if (bBrake)
		{
			if (targetPos.z > 0)
			{
				targetPos.z = -0.1f;
			}
		}
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 車が曲がるときの処理
	// 引数１：重心のｘ軸値
	//━━━━━━━━━━━━━━━━━━━━━
	public void Steering(float _x)
	{
		// フラグを立てる
		bBalancePointMoved_x = true;

		// 移動した後の値を計算する
		targetPos.x = Mathf.Clamp(_x / 2.5f * rightCoefficient * OverallTravelFactor, -maxRotate, maxRotate);
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 車上下の振動
	// 引数１：重心のｙ軸値
	//━━━━━━━━━━━━━━━━━━━━━
	public void Vibration(float _y)
	{
		// フラグを立てる
		bBalancePointMoved_y = true;

		// 移動した後の値を計算する
		targetPos.y = Mathf.Clamp(_y / 5 * upCoefficient * OverallTravelFactor, -maxRotate, maxRotate);
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// シフトをチェンジするときの処理
	//━━━━━━━━━━━━━━━━━━━━━
	public void ShiftUp()
	{
		if (Vector3.Dot(rootRigidbody.velocity, transform1.transform.forward) < 5) return;

		bBalancePointMoved_z = true;

		// 前へ
		controller.pitch -= 0.3f * shiftUpCoefficient * speedCoefficient.z * OverallTravelFactor;
	}
	public void ShiftDown()
	{
		if (Vector3.Dot(rootRigidbody.velocity, transform1.transform.forward) < 5) return;

		bBalancePointMoved_z = true;

		// 前への速度の計算
		float speed = Vector3.Dot(rootRigidbody.velocity, transform1.transform.forward);

		float temp = (int)(Mathf.Abs(speed + 5) / 10);

		// 速度が低いほど前への力が強くなる
		controller.pitch -= 0.3f / temp * shiftDownCoefficient * speedCoefficient.z * OverallTravelFactor;
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 壁に当たった後に呼び出されるの処理
	// 引数１：右壁に当たった→true
	// 　　　　左壁に当たった→false
	//━━━━━━━━━━━━━━━━━━━━━
	public void Hit(bool _right)
	{
		// すでに処理している場合は後の処理をパスします
		if (bGotHit) return;

		// 前への速度の計算
		float speed = Vector3.Dot(rootRigidbody.velocity, transform1.transform.forward);

		Vector2 temp = new Vector2(-1, 1);
		if (_right) temp.x = -temp.x;

		// 当たった後各軸の値を計算し、一定の範囲の中にした後保存しておく
		pitchAfterHit = Mathf.Clamp(controller.pitch - temp.y * speed / 60 * impactCoefficient.y, -maxRotate, maxRotate);
		rollAfterHit = Mathf.Clamp(controller.roll - temp.x * speed / 60 * impactCoefficient.x, -maxRotate, maxRotate);
		yawAfterHit = Mathf.Clamp(controller.yaw + temp.x * speed / 60 * impactCoefficient.x, -maxRotate, maxRotate);

		// 計算結果を各軸に代入
		controller.pitch = pitchAfterHit;
		controller.roll = rollAfterHit;
		controller.yaw = yawAfterHit;

		// フラグを立てる
		bGotHit = true;

		// カウンターをリセット
		hitCnt = 0;

		// 椅子の速度を計算し、保存する
		hitSpeed = Mathf.Clamp(speed * 0.4f, 0, maxSpeedAfterHit);

		// 加速度を変える
		controller.accelAxis123 = accelerationAfterHit;
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 車の振動
	// 引数１：今車の速度
	//━━━━━━━━━━━━━━━━━━━━━
	private void Rolling(float _nowSpeed)
	{
		// 速度で振動の間隔を取得し、保存しておく
		float RollingCntThreshold = (int)rollingCurve.Evaluate(_nowSpeed);

		// もし間隔が、30以上になっていたら、後の処理はしない
		if (RollingCntThreshold >= 30) return;

		// カウンターで間隔を計算
		if (++RollingCnt < RollingCntThreshold) return;

		// カウンターのリセット
		RollingCnt = 0;

		// 振動をランダムで計算し、各軸に代入する
		// 前後
		controller.pitch += Mathf.Clamp((UnityEngine.Random.value - 0.5f) * 2 / RollingRandomRange * 10 / _nowSpeed * RollingCoefficient.z * OverallTravelFactor, -RollingRandomRange, RollingRandomRange);

		// 上下
		controller.heave += Mathf.Clamp((UnityEngine.Random.value - 0.5f) * 2 / RollingRandomRange * 10 / _nowSpeed * RollingCoefficient.y * OverallTravelFactor, -RollingRandomRange, RollingRandomRange);

		// 左右
		float temp = Mathf.Clamp((UnityEngine.Random.value - 0.5f) * 2 / RollingRandomRange * 10 / _nowSpeed * RollingCoefficient.x * OverallTravelFactor, -RollingRandomRange, RollingRandomRange);
		controller.roll += temp;
		controller.yaw -= temp;
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 減衰する Sin波
	// 引数１：時間
	// 引数２：波長
	// 引数３：減衰の関数
	// 戻り値：時間をグラフに代入した後の解
	//━━━━━━━━━━━━━━━━━━━━━
	private float DampedSinWave(float _time, float _wavelength, Func<float, float> _func)
	{
		return Mathf.Sin(_time / _wavelength * 2 * Mathf.PI) * _func(_time);
	}
	//━━━━━━━━━━━━━━━━━━━━━
	// 減衰する Sin波
	// 引数１：時間
	// 引数２：波長
	// 引数３：減衰のグラフ
	// 戻り値：時間をグラフに代入した後の解
	//━━━━━━━━━━━━━━━━━━━━━
	private float DampedSinWave(float _time, float _wavelength, AnimationCurve _curve)
	{
		return Mathf.Sin(_time / _wavelength * 2 * Mathf.PI) * _curve.Evaluate(_time);
	}

	//━━━━━━━━━━━━━━━━━━━━━
	// 減衰する Cos波
	// 引数１：時間
	// 引数２：波長
	// 引数３：減衰の関数
	// 戻り値：時間をグラフに代入した後の解
	//━━━━━━━━━━━━━━━━━━━━━
	private float DampedCosWave(float _time, float _wavelength, Func<float, float> _func)
	{
		return Mathf.Cos(_time / _wavelength * 2 * Mathf.PI) * _func(_time);
	}
	//━━━━━━━━━━━━━━━━━━━━━
	// 減衰する Cos波
	// 引数１：時間
	// 引数２：波長
	// 引数３：減衰のグラフ
	// 戻り値：時間をグラフに代入した後の解
	//━━━━━━━━━━━━━━━━━━━━━
	private float DampedCosWave(float _time, float _wavelength, AnimationCurve _curve)
	{
		return Mathf.Cos(_time / _wavelength * 2 * Mathf.PI) * _curve.Evaluate(_time);
	}

	private void SceneCollFunc(string scenename)
	{
		switch (scenename)
		{
			case "Title":
				controller.heave = 1;
				break;

			case "Start_Scene":
				break;

			case "NewGarage":
				controller.heave = 0;
				break;

			case "Scene_Map":

				// ゴールのチェック
				if (goal.getGoalFlag())
				{
					// 椅子をリセットする
					ResetChair();
					return;
				}

				// 値の更新
				UpdateValueOfChair();
				break;

			case "Result_Scene":
				controller.heave = 1;
				break;
		}
	}

	private void GetSceneName(Scene next, LoadSceneMode mode)
	{
		SceneName = SceneManager.GetActiveScene().name;
	}
}