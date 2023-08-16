using UnityEngine;
using System;
using System.Text;

namespace AIM
{
	/// <summary>
	/// LogicoolSDKを使った、ハンドルコントローラーでの入力を管理する
	/// </summary>
	[Serializable]
	public class WheelInput
	{
		VehicleController vc;

		public bool isWheelConected;
		public bool isG29;
		public int wheelNum;
		public string wheelName;

		[Space]
		// 各種入力
		public float inputHandle;
		public float inputAxel;
		public float inputBrake;
		// ------------------入力値の取得用(0809船渡)---------------------
		public float GetInputAxel
		{
			get { return inputAxel; } // アクセルの入力量
		}

		public float GetInputBreke
		{
			get { return inputBrake; } // ブレーキの入力量
		}
		// -------------------------------------------------------------
		public float inputClutch;
		[Space]

		public float deadzoneZero_handlePer;
		public float deadzoneZero_handle;
		public float deadzoneZero_pedalPer;
		public float deadzoneZero_pedal;

		// 取得できる数と、実際の数に違いがある為比較する
		private int controllerCount = 0;
		private int realControllerCount = 0;
		private string[] controllerNames;

		/// <summary>
		/// ハンドルコントローラーの揺れやLEDなどを管理する
		/// </summary>
		[Serializable]
		public class WheelEffect
		{
			[Header("Spring")]
			public bool isSpring;
			[Range(-100, 100)] public int sp_center;
			[Range(0, 100)] public int sp_satPer;
			[Range(0, 100)] public int sp_coefPer;

			[Header("Constant")]
			public bool isConstant;
			[Range(-100, 100)] public int co_magPer;

			[Header("Damper")]
			public bool isDamper;
			[Range(-100, 100)] public int da_coefPer;

			[Header("SideCol")]
			public bool isSideCol;
			[Range(-100, 100)] public int sc_magPer;

			[Header("FrontCol")]
			public bool isFrontCol;
			[Range(-0, 100)] public int fc_magPer;

			[Header("DirtRoad")]
			public bool isDirtRoad;
			[Range(0, 100)] public int dr_magPer;

			[Header("BumpyRoad")]
			public bool isBumpyRoad;
			[Range(0, 100)] public int br_magPer;

			[Header("SlipperyRoad")]
			public bool isSlipperyRoad;
			[Range(0, 100)] public int sr_magPer;

			[Header("Surface")]
			public bool isSurface;
			[Tooltip("Sine:0   Square:1   Triangle:2")] [Range(0, 2)] public int su_type;
			[Range(0, 100)] public int su_magPer;
			[Range(0, 1500)] public int su_period;

			[Header("Airborne")]
			public bool isAirborne;

			[Header("Softstop")]
			public bool isSoftstop;
			[Range(0, 100)] public int ss_rangePer;

			[Header("LED")]
			public int led_RPMcurrent;
			public int led_RPMthreshold;
			public int led_RPMmax;

			//============================================
			// Spring
			//============================================
			public bool EffectSpringOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlaySpringForce(_wheelNum, sp_center, sp_satPer, sp_coefPer);
			}
			public bool EffectSpringStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopSpringForce(_wheelNum);
			}
			//============================================
			// Constant
			//============================================
			public bool EffectConstantOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlayConstantForce(_wheelNum, co_magPer);
			}
			public bool EffectConstantStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopConstantForce(_wheelNum);
			}
			//============================================
			// Damper
			//============================================
			public bool EffectDamperOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlayDamperForce(_wheelNum, da_coefPer);
			}
			public bool EffectDamperStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopDamperForce(_wheelNum);
			}
			//============================================
			// SideCol
			//============================================
			public bool EffectSideCol(int _wheelNum = 0)
			{
				if (sc_magPer == 0)
				{
					isSideCol = false;
					return false;
				}
				else
				{
					isSideCol = true;
					return LogitechGSDK.LogiPlaySideCollisionForce(_wheelNum, sc_magPer);
				}
			}
			//============================================
			// FrontCol
			//============================================
			public bool EffectFrontCol(int _wheelNum = 0)
			{
				if (fc_magPer == 0)
				{
					isFrontCol = false;
					return false;
				}
				else
				{
					isFrontCol = true;
					return LogitechGSDK.LogiPlayFrontalCollisionForce(_wheelNum, fc_magPer);
				}
			}
			//============================================
			// DirtRoad
			//============================================
			public bool EffectDirtRoadOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlayDirtRoadEffect(_wheelNum, dr_magPer);
			}
			public bool EffectDirtRoadStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopDirtRoadEffect(_wheelNum);
			}
			//============================================
			// BumpyRoad
			//============================================
			public bool EffectBumpyRoadOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlayBumpyRoadEffect(_wheelNum, br_magPer);
			}
			public bool EffectBumpyRoadStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopBumpyRoadEffect(_wheelNum);
			}
			//============================================
			// SlipperyRoad
			//============================================
			public bool EffectSlipperyRoadOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlaySlipperyRoadEffect(_wheelNum, sr_magPer);
			}
			public bool EffectSlipperyRoadStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopSlipperyRoadEffect(_wheelNum);
			}
			//============================================
			// Surface
			//============================================
			public bool EffectSurfaceOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlaySurfaceEffect(_wheelNum, su_type, su_magPer, su_period);
			}
			public bool EffectSurfaceStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopSurfaceEffect(_wheelNum);
			}
			//============================================
			// Airborne
			//============================================
			public bool EffectAirborneOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlayCarAirborne(_wheelNum);
			}
			public bool EffectAirborneStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopCarAirborne(_wheelNum);
			}
			//============================================
			// Softstop
			//============================================
			public bool EffectSoftstopOn(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlaySoftstopForce(_wheelNum, ss_rangePer);
			}
			public bool EffectSoftstopStop(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiStopSoftstopForce(_wheelNum);
			}
			//============================================
			// LED
			//============================================
			public bool EffectLEDUpdate(int _wheelNum = 0)
			{
				return LogitechGSDK.LogiPlayLeds(_wheelNum, led_RPMcurrent, led_RPMthreshold, led_RPMmax);
			}
			//============================================
			// 各種機能が有効かどうかチェック
			//============================================
			public void ActiveEffectUpdate(int _wheelNum = 0)
			{
				isSpring = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_SPRING);
				isConstant = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_CONSTANT);
				isDamper = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_DAMPER);

				isDirtRoad = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_DIRT_ROAD);
				isBumpyRoad = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_BUMPY_ROAD);
				isSlipperyRoad = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_SLIPPERY_ROAD);

				isSurface = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_SURFACE_EFFECT);
				isAirborne = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_CAR_AIRBORNE);
				isSoftstop = LogitechGSDK.LogiIsPlaying(_wheelNum, LogitechGSDK.LOGI_FORCE_SOFTSTOP);
			}
			//============================================
			// 全機能を停止する
			//============================================
			public void AllStop()
			{
				EffectAirborneStop();
				EffectBumpyRoadStop();
				EffectConstantStop();
				EffectDamperStop();
				EffectDirtRoadStop();
				EffectSlipperyRoadStop();
				EffectSoftstopStop();
				EffectSpringStop();
				EffectSurfaceStop();
			}
		}

		[SerializeField]
		public WheelEffect wheelEffect;

		const int RUB_COEFF = 30;
		const int ROAD_SHAKE_COEFF = 15;
		const int CRASH_COEFF = 100;
		const float SLIP_COEFF = 10.0f;
		bool isCrashStart = true;

		public bool isSpringShake;

		//============================================
		/// <summary>
		/// ハンドルの機能を使う際は、必ず実行すること
		/// </summary>
		public bool Initialized(VehicleController _vc)
		{
			vc = _vc;

			deadzoneZero_handle = deadzoneZero_handlePer * (float)Int16.MaxValue;
			deadzoneZero_pedal = deadzoneZero_pedalPer * (float)Int16.MaxValue;

			return LogitechGSDK.LogiSteeringInitialize(false);
		}
		//============================================
		public bool InputUpdate()
		{
			if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(wheelNum))
			{
				wheelEffect.ActiveEffectUpdate(wheelNum);

				LogitechGSDK.DIJOYSTATE2ENGINES rec;
				rec = LogitechGSDK.LogiGetStateUnity(wheelNum);

				// 元の数値は-32767～32767
				// 微少な入力値を0に補正しつつ、-1～1の範囲に設定
				if (rec.lX > 0)
				{
					inputHandle = ((float)rec.lX - deadzoneZero_handle) / (((float)Int16.MaxValue * ( 2.0f / 5.0f) ) - deadzoneZero_handle);
					inputHandle = Mathf.Clamp(inputHandle, 0.0f, 1.0f);
				}
				else if (rec.lX < 0)
				{
					inputHandle = ((float)rec.lX + deadzoneZero_handle) / (((float)Int16.MaxValue * (2.0f / 5.0f)) - deadzoneZero_handle);
					inputHandle = Mathf.Clamp(inputHandle, -1.0f, 0.0f);
				}
				else
				{
					inputHandle = 0.0f;
				}

				// 元の数値は-32767～32767
				// ハンドルと違い、0～1の範囲で扱いたいので、
				// まず-1～1に補正し、1を足して0～2にする。
				// その後、半分にする事で0～1にする。
				inputAxel = ((float)rec.lY / (float)-Int16.MaxValue + 1.0f) * 0.5f;
				inputBrake = ((float)rec.lRz / (float)-Int16.MaxValue + 1.0f) * 0.5f;
				inputClutch = ((float)rec.rglSlider[0] / (float)-Int16.MaxValue + 1.0f) * 0.5f;

				// 微少な入力値を0に補正
				if (inputAxel.IsDeadzoneZero(deadzoneZero_pedalPer)) { inputAxel = 0.0f; }
				else { inputAxel = Mathf.Clamp(inputAxel, 0.0f, 1.0f); }

				if (inputBrake.IsDeadzoneZero(deadzoneZero_pedalPer)) { inputBrake = 0.0f; }
				else { inputBrake = Mathf.Clamp(inputBrake, 0.0f, 1.0f); }

				if (inputClutch.IsDeadzoneZero(deadzoneZero_pedalPer)) { inputClutch = 0.0f; }
				else { inputClutch = Mathf.Clamp(inputClutch, 0.0f, 1.0f); }

				return true;
			}
			else
			{
				return false;
			}
		}
		//==========================================================================================
		private bool WheelConnectCheck(in string[] _controllerNames)
		{
			for (int i = 0; i < _controllerNames.Length; ++i)
			{
				// デバイスの種別がWHEELかどうか調べる
				if (LogitechGSDK.LogiIsDeviceConnected(i, LogitechGSDK.LOGI_DEVICE_TYPE_WHEEL))
				{
					StringBuilder deviceName = new StringBuilder(256);
					LogitechGSDK.LogiGetFriendlyProductName(0, deviceName, 256);

					wheelName = "Name:" + deviceName;
					wheelNum = i;

					if (wheelName.Contains("G29")) { isG29 = true; }
					else { isG29 = false; }

					return true;
				}
			}
			return false;
		}
		//==========================================================================================
		/// <summary>
		/// 定期的に実行して、ハンドルが接続され続けているか確認する
		/// </summary>
		public void ControllerConnectCheck()
		{
			// 接続されていなくても配列が確保されているので、それを除いた実際の値を取得する
			controllerNames = Input.GetJoystickNames();
			realControllerCount = 0;
			for (int i = 0; i < controllerNames.Length; ++i)
			{
				// 名前があるかどうかで接続チェック
				if (controllerNames[i] != "")
				{
					++realControllerCount;
				}
			}

			// 数を比較して、違った場合のみ接続チェックを行う
			if (realControllerCount != controllerCount)
			{
				controllerCount = realControllerCount;
				isWheelConected = WheelConnectCheck(in controllerNames);
			}
		}
		//==========================================================================================
		/// <summary>
		/// 壁に衝突時に、ハンドルを揺らす
		/// </summary>
		bool RubWallHandleShake()
		{
			wheelEffect.fc_magPer = CRASH_COEFF;

			if (vc.getLeave)
			{
				isCrashStart = true;
			}

			if (vc.getCrash && isCrashStart)
			{
				isCrashStart = false;
				wheelEffect.EffectFrontCol(wheelNum);
			}

			if (vc.getRub || vc.getCrash)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		//==========================================================================================
		/// <summary>
		/// 滑っている時にカウンターを行う
		/// </summary>
		public void SlipCounterHandle()
		{
			//wheelEffect.sc_magPer = (int)Mathf.Clamp((vc.axles[1].geometry.steerCoefficient * vc.input.Horizontal * SLIP_COEFF), -100.0f, 100.0f);
			//wheelEffect.EffectSideCol(wheelNum);
		}
		//==========================================================================================
		bool RoadShake()
		{
			return isSpringShake;
		}
		//==========================================================================================
		/// <summary>
		/// 優先度の高い揺れから判定して、ハンドルを揺らす
		/// </summary>
		public void BumpyRoadUpdate()
		{
			if (!vc.isStart)
			{
				return;
			}

			// 壁に衝突時の揺れ
			if (RubWallHandleShake())
			{
				//wheelEffect.br_magPer = RUB_COEFF;
				//wheelEffect.EffectBumpyRoadOn(wheelNum);
				wheelEffect.EffectBumpyRoadStop(wheelNum);
			}
			// 車体の傾きによる揺れ
			else if (RoadShake())
			{
				wheelEffect.br_magPer = ROAD_SHAKE_COEFF;
				wheelEffect.EffectBumpyRoadOn(wheelNum);
			}
			// 揺れ停止処理
			else
			{
				wheelEffect.EffectBumpyRoadStop(wheelNum);
			}
		}
	}
}