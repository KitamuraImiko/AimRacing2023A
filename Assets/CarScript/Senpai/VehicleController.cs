using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace AIM
{
	/// <summary>
	/// 車に関する機能をまとめているクラス
	/// </summary>
	[DisallowMultipleComponent]
	[DefaultExecutionOrder(1)]
	[RequireComponent(typeof(Rigidbody))]
	//[RequireComponent(typeof(CenterOfMass))]
	public partial class VehicleController : MonoBehaviour
	{
		//2023/7/6/河合奏追加-----------------------------------------------------
		//先輩の処理をオンオフ
		public bool BSenpai = true;
		//デバック用の力の大きさを見やすいように制御するためのプロパティ
		[SerializeField]
		public float DebugFoceSize;

		[SerializeField] public int Type = 0;
		[SerializeField] public int SteeringAngle = 0;
		[SerializeField] public int SteaEase = 0;
		[SerializeField] public int SteaPower = 0;
		//------------------------------------------------------------------------

		public static float zeroDeadzone = 0.025f;

		[SerializeField] private bool active = true;
		[HideInInspector] public bool isStart = false;

		//--------------------------
		// 各機能変数
		[HideInInspector] public InputStates input = new InputStates();
		//[SerializeField] public Effects effects = new Effects();
		[SerializeField] public Steering steering = new Steering();
		[SerializeField] public Engine engine = new Engine();
		[SerializeField] public Transmission transmission = new Transmission();
		[SerializeField] public List<Axle> axles = new List<Axle>(2);
		[SerializeField] public Brakes brakes = new Brakes();
		[SerializeField] public DriftManager drift = new DriftManager();

		[HideInInspector] public EngineSound es;
		private GoalJudgment goalJudgment;
		private List<AxleWheel> wheels = new List<AxleWheel>();

		[HideInInspector] [SerializeField] public GroundDetection groundDetection;
		[SerializeField] public DrivingAssists drivingAssists = new DrivingAssists();
		[SerializeField] public Fuel fuel = new Fuel();
		[SerializeField] public FlipOver flipOver = new FlipOver();
		[HideInInspector] public Metrics metrics = new Metrics();

		[HideInInspector] public float forwardSlipThreshold = 0.35f;
		public float sideSlipThreshold = 0.1f;
		[HideInInspector] public float speedLimiter = 0f;
		[HideInInspector] public bool freezeWhenStill = true;
		[HideInInspector] public bool freezeWhenInactive = true;
		[HideInInspector] public bool switchToSingleRayWhenInactive = true;
		//[SerializeField] private Logitech_test logiCon;
		//[SerializeField] private IntroductionManager intro;
		[SerializeField] private List<Transform> wheelPos = new List<Transform>(4);
		//[HideInInspector] public CarsRespawn carsRespawn;
		[HideInInspector] public Load loadCom;

		[SerializeField]
		public WheelInput wheelInput = new WheelInput();
		// --------クラス情報の取得(2023/08/09船渡)---------
		public WheelInput GetWheelInput
		{
			get { return wheelInput; }
		}
		//------------------------------------------------
		//--------------------------

		private bool frozen;
		private bool wasFrozen;
		private float forwardVelocity;
		private float forwardAcceleration;
		private float load;
		private Vector3 velocity;
		private Vector3 prevVelocity;
		private Vector3 acceleration;
		private bool wheelSpin;
		private bool wheelSkid;

		public Rigidbody rb;

		private float inactivityTimer = 0;
		private const float InactiveTimeout = 2f;

		// 衝突判定
		// 一条
		private bool crash;
		private bool rub;
		private bool leave;

		public bool getRub => rub;
		public bool getCrash => crash;
		public bool getLeave => leave;

		[SerializeField]
		[Range(0.0f, 10.0f)]
		private float normalDrag = 0.01f;

		//壁にぶつかったときの抵抗
		[SerializeField]
		[Range(0.0f, 10.0f)]
		private float wallDrag = 1.2f;

		[SerializeField] private float[] holdInput;
		[SerializeField] private AnimationCurve[] dragCurve;

		//追加シフトアップフラグ
		[HideInInspector] public Rigidbody vehicleRigidbody;
		public Logitech_test logi;
		public bool isShiftUp;

		public int gear1_maxRPM	 = 7000;

		public enum VehicleCollisionState
		{
			Enter,
			Stay,
			None
		}

		private VehicleCollisionState collisionState = VehicleCollisionState.None;
		private Collision collisionInfo;

		private bool initialized = false;

		public bool Active
		{
			get => active;
			set
			{
				if (active == false && value == true)
					Activate();
				else if (active == true && value == false)
					Suspend();

				active = value;
			}
		}

		public float ForwardVelocity => forwardVelocity;

		public float Speed => Mathf.Abs(ForwardVelocity);

		public float SpeedKPH => Speed * 5.0f;

		public float SpeedMPH => Speed * 2.237f;

		public float Load => load;

		public float WheelSpeed
		{
			get
			{
				float rpmSum = 0;
				int poweredAxleCount = 0;
				float radiusSum = 0;
				foreach (Axle axle in axles)
				{
					if (axle.IsPowered)
					{
						poweredAxleCount++;
						rpmSum += Mathf.Abs(axle.RPM);
						radiusSum += axle.leftWheel.Radius;
						radiusSum += axle.rightWheel.Radius;
					}
				}

				if (poweredAxleCount != 0)
				{
					float avgRadius = radiusSum / (poweredAxleCount * 2);
					float avgRpm = rpmSum / poweredAxleCount;
					return avgRadius * avgRpm * 0.10472f;
				}
				else
				{
					return Mathf.Abs(ForwardVelocity);
				}
			}
		}

		public float ForwardAcceleration => forwardAcceleration;

		public Vector3 Acceleration => acceleration;

		public float Direction
		{
			get
			{
				float velZ = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z;

				if (velZ > 0)
					return 1;
				else if (velZ < 0)
					return -1;
				else
					return 0;
			}
		}

		public List<AxleWheel> Wheels => wheels;

		public bool WheelSpin => wheelSpin;

		public bool WheelSkid => wheelSkid;

		private bool InactivityTimerOverflow => inactivityTimer > InactiveTimeout;

		private float lastCollisionTime;

		public float dAIntensity;
		public AnimationCurve horizontalToDACurve;

		//==================================================================================
		private void Start()
		{
			#region AdjustDeltaTime

			if (Time.fixedDeltaTime > 0.017f)
			{
				Debug.LogWarning("フレーム更新時間が0.017以上です。0.015以下に設定すれば、車はスムーズになる");
				Time.fixedDeltaTime = 0.015f;
			}

			#endregion

			// 各種初期化
			Initialize();

			//es = GameObject.Find("EngineSound").GetComponent<EngineSound>();
			goalJudgment = GameObject.Find("GoalGate").GetComponent<GoalJudgment>();
			rb = GetComponent<Rigidbody>();
			//logiCon = GameObject.Find("VehicleManager").GetComponent<Logitech_test>();
			//intro = GameObject.Find("IntroductionManager").GetComponent<IntroductionManager>();
			//carsRespawn = GetComponent<CarsRespawn>();
			loadCom = GetComponent<Load>();

			// トランスミッション設定
			if (TwoChoiceNew.GetTransmisson() == 1)
			{
				transmission.transmissionType = Transmission.TransmissionType.Automatic;
			}
			else if (TwoChoiceNew.GetTransmisson() == 2)
			{
				transmission.transmissionType = Transmission.TransmissionType.Manual;
			}
			

			// LED設定
			wheelInput.wheelEffect.led_RPMmax = (int)transmission.targetShiftUpRPM;
			wheelInput.wheelEffect.led_RPMthreshold = 3000;

            wheelInput.wheelEffect.AllStop();

            Cursor.visible = false;

		}
		//==================================================================================
		private void Initialize()
		{
			vehicleRigidbody = GetComponent<Rigidbody>();
			vehicleRigidbody.maxAngularVelocity = 10f;

			// コンポーネント初期化
			if (groundDetection == null)
			{
				groundDetection = FindGroundDetectionComponent();
			}

			steering.Initialize(this);
			foreach (Axle axle in axles) axle.Initialize(this);

			wheels = GetAllWheels();

			engine.Initialize(this);
			transmission.Initialize(this);
			//effects.Initialize(this);
			fuel.Initialize(this);

			drift.Initialize(this);

			if (input == null)
				input = new InputStates();
			input.Initialize(this);

			dAIntensity = drivingAssists.driftAssist.intensity;
			drivingAssists.Initialize(this);
			flipOver.Initialize(this);
			metrics.Initialize(this);

			wheelInput.Initialized(this);

			// サスペンションフラグ
			if (!active)
				Suspend();
			else if (active)
				Activate();

			initialized = true;

			LogitechGSDK.LogiStopSpringForce(0);

			LogitechGSDK.LogiPlaySpringForce(0, SteeringAngle, SteaEase, SteaPower);
		}
		//==================================================================================
		private void Update()
		{
			//デバッグモード：ミッションの切り替え
			if (Input.GetKeyDown(KeyCode.F9))
			{
				if (!transmission.isAuto)
				{
					transmission.transmissionType = Transmission.TransmissionType.Automatic;
					transmission.isAuto = true;
				}
				else if (transmission.isAuto)
				{
					transmission.transmissionType = Transmission.TransmissionType.Manual;
					transmission.isAuto = false;
				}
			}

            LogitechGSDK.LogiPlaySpringForce(0, SteeringAngle, SteaEase, SteaPower);
        }

		private void FixedUpdate()
		{
			//エントリーで選択したトランスミッションの更新
			if (!initialized) Initialize();

			//前回更新時からの経過時間を加算
			inactivityTimer += Time.fixedDeltaTime;

			//変更前の速度を保存？
			prevVelocity = velocity;
			velocity = transform.InverseTransformDirection(vehicleRigidbody.velocity);
			acceleration = (velocity - prevVelocity) / Time.fixedDeltaTime;

			forwardVelocity = velocity.z;
			forwardAcceleration = acceleration.z;

			//稼働している場合、inactivityTimerをリセット
			//速度が0.1以上ある場合リセット
			if (velocity.magnitude > 0.1f) ResetInactivityTimer();
			//
			if (!input.Horizontal.IsDeadzoneZero() || !input.Vertical.IsDeadzoneZero()) ResetInactivityTimer();

			////エンジン処理の更新
			//if (engine.Starting || engine.Stopping)
			//	engine.Update();

			//ホイール処理の更新
			foreach (AxleWheel wheel in wheels)
			{
				wheel.Update();
			}

			//ブレーキ処理の更新
			brakes.Update(this);
			//ドリフト処理の更新
			drift.Update();

			if (active)
			{
				//負荷？
				load = GetLoad();

				// タイヤの滑り
				DetectWheelSkid();
				DetectWheelSpin();

				// 車軸処理の更新
				foreach (Axle axle in axles) axle.Update();

				if (groundDetection != null)
				{
					foreach (AxleWheel wheel in Wheels)
					{
						GroundDetection.GroundEntity groundEntity =
							groundDetection.GetCurrentGroundEntity(wheel.WheelController);

						if (groundEntity != null)
						{
							wheel.WheelController.SetActiveFrictionPreset(groundEntity.frictionPresetEnum);
						}
					}
				}
				drivingAssists.Update();
				engine.Update();
				transmission.Update();

				// トルク配分
				transmission.TorqueSplit(
					transmission.TransmitTorque(engine.Torque),
					transmission.TransmitRPM(engine.RPM)
				);

				if (transmission.Gear == 0)
				{
					if (engine.RPM >= gear1_maxRPM)
					{
						input.Vertical = 0f;
						engine.power = 0;
					}
				}

				steering.Steer();
				steering.AdjustGeometry();

				metrics.Update();
				fuel.Update();
			}

			// スピード制限
			if (speedLimiter != 0)
			{
				float regulatorThreshold = 0.8f;
				if (Speed > speedLimiter * regulatorThreshold)
				{
					float powerReduction = Mathf.Clamp01((Speed - (speedLimiter * regulatorThreshold)) /
														 (speedLimiter * (1f - regulatorThreshold)));
					engine.TcsPowerReduction = powerReduction * powerReduction;
				}
			}
			// 引き返す処理
			flipOver.Update();

			// 固定化
			// 固定続けるか
			if (freezeWhenStill && !flipOver.flippedOver)
			{
				bool wheelsTurning = false;
				foreach (AxleWheel wheel in Wheels)
				{
					if (wheel.RPM < -3 || wheel.RPM > 3)
					{
						wheelsTurning = true;
						break;
					}
				}

				if (((active && InactivityTimerOverflow) || (!active && freezeWhenInactive && InactivityTimerOverflow))
					&& !wheelsTurning)
				{
					frozen = true;
				}
				else
				{
					frozen = false;
				}
			}

			if (frozen && !wasFrozen)
			{
				vehicleRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
				vehicleRigidbody.isKinematic = true;
			}
			else if (!frozen && wasFrozen)
			{
				vehicleRigidbody.isKinematic = false;
				vehicleRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}

			wheelInput.wheelEffect.led_RPMcurrent = (int)engine.rpm;
			wheelInput.wheelEffect.EffectLEDUpdate();

			wasFrozen = frozen;
			DragUpdate();

			wheelInput.BumpyRoadUpdate();
			wheelInput.SlipCounterHandle();
		}

		//public void LateUpdate()
		//{
		//	effects.Update();
		//}

		private List<AxleWheel> GetWheels(int from, int to)
		{
			List<AxleWheel> whs = new List<AxleWheel>();
			for (int i = from; i <= to; i++)
			{
				whs.Add(axles[i].leftWheel);
				whs.Add(axles[i].rightWheel);
			}

			return whs;
		}

		private List<AxleWheel> GetAllWheels()
		{
			List<AxleWheel> whs = new List<AxleWheel>();
			foreach (Axle axle in axles)
			{
				whs.Add(axle.leftWheel);
				whs.Add(axle.rightWheel);
			}
			return whs;
		}

		private float GetLoad()
		{
			float rpmCoeff = engine.RPMPercent;
			float powerCoeff = engine.Power / engine.maxPower;

			float load = Mathf.Clamp01(powerCoeff * 0.6f + rpmCoeff * 0.4f);
			return load;
		}

		public float GetAverageWheelRPM()
		{
			float allWheelRpmSum = 0;
			int wheelCount = 0;
			foreach (AxleWheel wheel in Wheels)
			{
				allWheelRpmSum += wheel.SmoothRPM;
				wheelCount++;
			}

			float averageRpm = 0;
			if (wheelCount > 0)
			{
				averageRpm = allWheelRpmSum / wheelCount;
			}
			return averageRpm;
		}

		public float GetRawAverageWheelRPM()
		{
			float allWheelRpmSum = 0;
			int wheelCount = 0;
			foreach (AxleWheel wheel in Wheels)
			{
				allWheelRpmSum += wheel.RPM;
				wheelCount++;
			}

			float averageRpm = 0;
			if (wheelCount > 0)
			{
				averageRpm = allWheelRpmSum / wheelCount;
			}

			return averageRpm;
		}

		public float GetCorrectWheelRpm(AxleWheel wheel)
		{
			float wheelCircumfence = 2 * Mathf.PI * wheel.Radius;
			return (Speed / wheelCircumfence) * 60f;
		}

		public bool DetectWheelSpin()
		{
			foreach (Axle a in axles)
			{
				if (a.IsPowered && a.WheelSpin)
				{
					return wheelSpin = true;
				}
			}

			return wheelSpin = false;
		}

		public bool DetectWheelSkid()
		{
			foreach (AxleWheel wheel in Wheels)
			{
				if (wheel.HasSideSlip)
				{
					return wheelSkid = true;
				}
			}

			return wheelSkid = false;
		}

		private void Suspend()
		{
			active = false;
			if (engine.stopOnDisable && engine.IsRunning) engine.Stop();
			//effects.lights.enabled = false;
			foreach (AxleWheel wheel in Wheels)
			{
				wheel.SetBrakeIntensity(0.2f);
				wheel.Suspend();
			}
		}

		private void Activate()
		{
			active = true;
			if (engine.runOnEnable && !engine.IsRunning) engine.Start();
			//effects.lights.enabled = true;
			foreach (AxleWheel wheel in Wheels)
			{
				wheel.ResetBrakes(0);
				wheel.Activate();
			}
		}

		public VehicleCollisionState GetCollisionState()
		{
			return collisionState;
		}

		private void OnCollisionEnter(Collision collision)
		{
			collisionState = VehicleCollisionState.Enter;
			collisionInfo = collision;

			if (Time.realtimeSinceStartup < lastCollisionTime + 0.5f) return;

			// 壁との衝突開始
			if (collision.gameObject.tag == "RightWall" || collision.gameObject.tag == "LeftWall")
			{
				crash = true;
			}
		}

		private void OnCollisionStay(Collision collision)
		{
			collisionState = VehicleCollisionState.Stay;
			collisionInfo = collision;

			// 壁との衝突継続
			if (collision.gameObject.tag == "RightWall" || collision.gameObject.tag == "LeftWall")
			{
				rub = true;
				leave = false;
			}
		}

		private void OnCollisionExit(Collision collision)
		{
			collisionState = VehicleCollisionState.None;
			collisionInfo = null;

			// 壁との衝突終了
			if (collision.gameObject.tag == "RightWall" || collision.gameObject.tag == "LeftWall")
			{
				rub = false;
				crash = false;
				leave = true;
			}
		}

		//追加
		//速度減衰の変化
		private void DragUpdate()
		{
			if (getRub)
			{
				rb.drag = wallDrag;
			}
			else
			{
				// エンジンブレーキを疑似的に実装
				if (transmission.Gear > 0 && input.Vertical > 0.0f && input.Vertical <= holdInput[transmission.Gear - 1])
				{
					rb.drag = dragCurve[transmission.Gear - 1].Evaluate(SpeedKPH);
				}
				else
				{
					if (input.Vertical.IsDeadzoneZero(0.05f) && transmission.Gear > 0)
					{
                        rb.drag = engine.EngineBrake(transmission.Gear - 1);
                        Debug.LogError("エンジンブレーキ（疑似的に）");
					}
					else
					{
						rb.drag = normalDrag;
					}
				}
			}
		}
		public void Reset()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			groundDetection = FindGroundDetectionComponent();

			if (axles != null && axles.Count == 0)
			{
				axles = GetAxles();

				if (axles == null || axles.Count == 0)
				{
					Debug.LogWarning("車軸は設定されていない");
					return;
				}

				if (axles != null && axles.Count != 0)
				{
					for (int i = 0; i < axles.Count; i++)
					{
						Axle.Geometry g = new Axle.Geometry();
						if (i == 0)
						{
							g.steerCoefficient = 1f;
							g.antiRollBarForce = axles[i].leftWheel.WheelController.springMaximumForce / 2f;
							axles[i].geometry = g;
						}
						else if (i == axles.Count - 1)
						{
							g.antiRollBarForce = axles[i].leftWheel.WheelController.springMaximumForce / 2.5f;
							axles[i].geometry = g;
							axles[i].handbrakeCoefficient = 1;
						}

						axles[i].powerCoefficient = 1f;
					}
				}
				foreach (Axle axle in axles)
				{
					axle.Initialize(this);
				}
			}

			//try
			//{
			//	ResetCOM();
			//}
			//catch
			//{
			//	Debug.LogWarning("重心設定できない。");
			//}

			if (Application.isPlaying)
			{
				Initialize();
			}
		}

		private GroundDetection FindGroundDetectionComponent()
		{
			var groundDetectionObjects = FindObjectsOfType(typeof(GroundDetection));
			if (groundDetectionObjects.Length > 0)
			{
				if (groundDetectionObjects.Length > 1)
					Debug.LogWarning(
						"GroundDetectionは多数存在している");

				return (GroundDetection)groundDetectionObjects[0];
			}
			else
			{
				Debug.LogWarning("GroundDetectionがないため、摩擦は反映されない");

				return null;
			}
		}

		//重心のリセット
		//public void ResetCOM()
		//{
		//	vehicleRigidbody = GetComponent<Rigidbody>();
		//	CenterOfMass com = GetComponent<CenterOfMass>();
		//	Vector3 centerPoint = Vector3.zero;
		//	Vector3 pointSum = Vector3.zero;
		//	int count = 0;
		//	foreach (AxleWheel wheel in GetAllWheels())
		//	{
		//		pointSum += transform.InverseTransformPoint(wheel.WheelController.transform.position);
		//		count++;
		//	}

		//	if (count == 0) return;

		//	centerPoint = pointSum / count;
		//	centerPoint -= GetAllWheels()[0].WheelController.springLength * 0.45f * transform.up;
		//	vehicleRigidbody.ResetCenterOfMass();
		//	com.centerOfMassOffset = centerPoint - vehicleRigidbody.centerOfMass;
		//}

		//車軸設置
		public List<Axle> GetAxles()
		{
			List<Axle> axles = new List<Axle>();
			List<WheelController> wcs = transform.GetComponentsInChildren<WheelController>().ToList();

			if (wcs.Count > 0)
			{
				List<WheelController> leftWheels = new List<WheelController>();
				List<WheelController> rightWheels = new List<WheelController>();

				foreach (WheelController wc in wcs)
				{
					if (wc.VehicleSide == WheelController.LRSide.Left)
						leftWheels.Add(wc);
					else if (wc.VehicleSide == WheelController.LRSide.Right)
						rightWheels.Add(wc);
				}

				if (leftWheels.Count != rightWheels.Count) return null;
				if ((leftWheels.Count + rightWheels.Count) % 2 != 0) return null;
				if (leftWheels.Count == 0 && rightWheels.Count == 0 && wcs.Count != 0) return null;

				leftWheels = leftWheels.OrderByDescending(x => GetWheelZPosition(x, this)).ToList();
				rightWheels = rightWheels.OrderByDescending(x => GetWheelZPosition(x, this)).ToList();

				for (int i = 0; i < leftWheels.Count; i++)
				{
					Axle axle = new Axle();
					axle.leftWheel = new AxleWheel(leftWheels[i], this);
					axle.rightWheel = new AxleWheel(rightWheels[i], this);
					axles.Add(axle);
				}

				return axles;
			}
			else
			{
				Debug.LogWarning("WheelControllers設置していない");
				return null;
			}
		}

		private float GetWheelZPosition(WheelController wc, VehicleController vc)
		{
			return transform.InverseTransformPoint(wc.transform.position).z;
		}

		public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
		{
			return Mathf.Atan2(
					   Vector3.Dot(n, Vector3.Cross(v1, v2)),
					   Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
		}

		public static float GetMouseHorizontal()
		{
			float percent = Mathf.Clamp(Input.mousePosition.x / Screen.width, -1f, 1f);
			if (percent < 0.5f)
				return -(0.5f - percent) * 2.0f;
			return (percent - 0.5f) * 2.0f;
		}

		public static float GetMouseVertical()
		{
			float percent = Mathf.Clamp(Input.mousePosition.y / Screen.height, -1f, 1f);
			if (percent < 0.5f)
				return -(0.5f - percent) * 2.0f;
			return (percent - 0.5f) * 2.0f;
		}

		public void ResetInactivityTimer()
		{
			inactivityTimer = 0;
		}

		private bool isSpeedZero()
		{
			if ((Input.GetAxisRaw("R_Trigger") == 1) && SpeedKPH <= 1)
			{
				return true;
			}
			return false;
		}
		//受け取ったRPM分上下する
		IEnumerator RPMCoroutine(int maxRpm_enum)
		{
			if (engine.rpm >= maxRpm_enum + 300)
				engine.isRpm_enum = true;
			else if (engine.rpm <= maxRpm_enum + 50)
				engine.isRpm_enum = false;

			if (!engine.isRpm_enum)
				engine.rpm++;
			else
				engine.rpm--;

			yield return new WaitForSeconds(0.001f);
		}
		// 終了時にすべての機能をオフにする
		// 機能が有効になったまま終了すると、終了後も有効のままになってしまう
		void OnApplicationQuit()
		{
			Debug.Log("WheelEffects終了");
			wheelInput.wheelEffect.AllStop();
			LogitechGSDK.LogiStopSpringForce(0);
		}
	}
}