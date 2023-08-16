using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
	/// <summary>
	/// タイヤの軸クラス
	/// </summary>
	[System.Serializable]
	public class Axle
	{
		public AxleWheel leftWheel = new AxleWheel();
		public AxleWheel rightWheel = new AxleWheel();

		//河合変更---------------------------------
		// アッカーマン曲線
		public AnimationCurve AckermanCurve = null;
		//-----------------------------------------

		private float bias;

		[System.Serializable]
		public class Geometry
		{
			// ハンドル反映度
			[Range(-1f, 1f)]
			public float steerCoefficient;

			// タイヤ角度
			[Range(-8f, 8f)]
			public float toeAngle = 0;
			[Range(-8f, 8f)]
			public float casterAngle = 0;
			[Range(-10f, 10f)]
			public float camberAtTop = 0;
			[Range(-10f, 10f)]
			public float camberAtBottom = 0;

			public bool isSolid = false;

			/// <summary>
			///  車両のロールを減らすためのもの
			///  値は最大バネ力未満で設定する
			/// </summary>
			public float antiRollBarForce;
		}
		[SerializeField]
		public Geometry geometry = new Geometry();

		// アクセル・ブレーキ反映度
		public float powerCoefficient = 1f;
		public float brakeCoefficient = 1f;
		public int handbrakeCoefficient;

		/// <summary>
		/// トルクを分配する設定
		/// Equal - 同じ値
		/// Open - RPMの割合で決定
		/// Limited Slip - RPMの割合で決まるものの、差を少なくする
		/// Locking - 差を大きくする
		/// </summary>
		public enum DifferentialType { Equal, Open, LimitedSlip, Locking }

		// LimitedSlipとLockingで使用する、強さ
		[Range(0f, 1f)]
		public float differentialStrength;

		public DifferentialType differentialType = DifferentialType.LimitedSlip;

		public bool isDriftReverseAxle;

		private VehicleController vc;

		//河合追記---------------------------------
		public AnimationCurve AnimCur
		{
			get
            {
				return AckermanCurve;
			}
			set
			{
				AckermanCurve = value;
			}
		}
		//-----------------------------------------

		public float Bias
		{
			get
			{
				return bias;
			}
			set
			{
				bias = Mathf.Clamp01(value);
			}
		}

		public bool IsPowered
		{
			get
			{
				return powerCoefficient > 0 ? true : false;
			}
		}

		public float RPM
		{
			get
			{
				return (leftWheel.RPM + rightWheel.RPM) / 2f;
			}
		}

		public float SmoothRPM
		{
			get
			{
				return (leftWheel.SmoothRPM + rightWheel.SmoothRPM) / 2f;
			}
		}

		public float NoSlipRPM
		{
			get
			{
				return (leftWheel.NoSlipRPM + rightWheel.NoSlipRPM) / 2f;
			}
		}

		public bool WheelSpin
		{
			get
			{
				return (leftWheel.HasForwardSlip || rightWheel.HasForwardSlip) && IsPowered;
			}
		}

		public void Initialize(VehicleController vc)
		{
			this.vc = vc;
			leftWheel.Initialize(vc);
			rightWheel.Initialize(vc);

			leftWheel.brakeCoefficient = brakeCoefficient;
			rightWheel.brakeCoefficient = brakeCoefficient;

			//河合追記---------------------------------
			AnimCur = GenerateDefaultSpringCurve();
			//-----------------------------------------
		}

		public void Update()
		{
			// キャンバー設定
			if (!(geometry.isSolid || (geometry.camberAtBottom == 0 && geometry.camberAtTop == 0)))
			{
				leftWheel.WheelController.SetCamber(geometry.camberAtTop, geometry.camberAtBottom);
				rightWheel.WheelController.SetCamber(geometry.camberAtTop, geometry.camberAtBottom);
			}

			// ロール設定
			if (geometry.antiRollBarForce != 0)
			{
				float leftTravel = leftWheel.SpringTravel;
				float rightTravel = rightWheel.SpringTravel;

				float arf = (leftTravel - rightTravel) * geometry.antiRollBarForce;

				if (leftWheel.IsGrounded && rightWheel.IsGrounded)
				{
                    vc.vehicleRigidbody.AddForceAtPosition(leftWheel.ControllerTransform.up * -arf, leftWheel.ControllerTransform.position);
                    vc.vehicleRigidbody.AddForceAtPosition(rightWheel.ControllerTransform.up * arf, rightWheel.ControllerTransform.position);
                }
			}

			if (geometry.isSolid)
			{
				Vector3 position = (leftWheel.WheelController.springTravelPoint + rightWheel.WheelController.springTravelPoint) / 2f;
				Vector3 direction = position - leftWheel.WheelController.springTravelPoint;

				float camberAngle = VehicleController.AngleSigned(vc.transform.right, direction, vc.transform.forward);
				camberAngle = Mathf.Clamp(camberAngle, -25f, 25f);

				leftWheel.WheelController.SetCamber(camberAngle);
				rightWheel.WheelController.SetCamber(-camberAngle);
				geometry.camberAtBottom = geometry.camberAtTop = camberAngle;
			}
		}

		// トルク分配
		public void TorqueSplit(float torque, float topRPM)
		{
			float leftRPM = Mathf.Abs(leftWheel.SmoothRPM);
			float rightRPM = Mathf.Abs(rightWheel.SmoothRPM);

			float rpmSum = Mathf.Abs(leftRPM) + Mathf.Abs(rightRPM);

			if (rpmSum != 0)
			{
				if (differentialType == DifferentialType.Equal)
				{
					leftWheel.Bias = rightWheel.Bias = 0.5f;
				}
				else if (differentialType == DifferentialType.Open)
				{
					leftWheel.Bias = (leftRPM / rpmSum);
					rightWheel.Bias = (rightRPM / rpmSum);
				}
				else if (differentialType == DifferentialType.LimitedSlip || differentialType == DifferentialType.Locking)
				{
					leftWheel.Bias = Mathf.Pow(1f - (leftRPM / rpmSum), 2f);
					rightWheel.Bias = Mathf.Pow(1f - (rightRPM / rpmSum), 2f);

					if (differentialType == DifferentialType.Locking)
					{
						leftWheel.Bias = Mathf.Pow(leftWheel.Bias, 6f);
						rightWheel.Bias = Mathf.Pow(rightWheel.Bias, 6f);
					}

					float biasSum = leftWheel.Bias + rightWheel.Bias;
					leftWheel.Bias = leftWheel.Bias / biasSum;
					rightWheel.Bias = rightWheel.Bias / biasSum;

					if (leftWheel.Bias < rightWheel.Bias)
					{
						leftWheel.Bias = Mathf.Lerp(leftWheel.Bias, 1f - leftWheel.Bias, 1f - differentialStrength);
						rightWheel.Bias = 1f - leftWheel.Bias;
					}
					else
					{
						rightWheel.Bias = Mathf.Lerp(rightWheel.Bias, 1f - rightWheel.Bias, 1f - differentialStrength);
						leftWheel.Bias = 1f - rightWheel.Bias;
					}
				}
			}
			else
			{
				leftWheel.Bias = rightWheel.Bias = 0.5f;
			}

			if (SmoothRPM > topRPM * 2.2f)
			{
				leftWheel.Bias = rightWheel.Bias = 0;
			}

			leftWheel.MotorTorque = torque * leftWheel.Bias;
			rightWheel.MotorTorque = torque * rightWheel.Bias;
		}

		//河合追記---------------------------------------------------
		//アッカーマン理論曲線
		//参考
		//http://www.enjoy.ne.jp/~k-ichikawa/car_Ackerman.html
		private AnimationCurve GenerateDefaultSpringCurve()
		{
			AnimationCurve ackermanCurve = new AnimationCurve();
			ackermanCurve.AddKey(0.0f, 0.0f);
			ackermanCurve.AddKey(0.1f, AckermanCalculate(0.1f));
			ackermanCurve.AddKey(0.2f, AckermanCalculate(0.2f));
			ackermanCurve.AddKey(0.3f, AckermanCalculate(0.3f));
			ackermanCurve.AddKey(0.4f, AckermanCalculate(0.4f));
			ackermanCurve.AddKey(0.5f, AckermanCalculate(0.5f));
			ackermanCurve.AddKey(0.6f, AckermanCalculate(0.6f));
			ackermanCurve.AddKey(0.8f, AckermanCalculate(0.8f));
			ackermanCurve.AddKey(0.9f, AckermanCalculate(0.9f));
			ackermanCurve.AddKey(1.0f, AckermanCalculate(1.0f));
			return ackermanCurve;
		}

		//アッカーマン理論曲線計算用メソッド
		private float AckermanCalculate(float frame)
        {
			float rad = frame * 100.0f;

			float result  =  0.45f + (1.0f / Mathf.Tan(rad * Mathf.Deg2Rad));
			result = 1.0f / result;
			return Mathf.Atan(result) * Mathf.Rad2Deg;
		}
		//-----------------------------------------------------------
	}
}