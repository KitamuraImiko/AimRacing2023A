using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AIM
{
	/// <summary>
	/// ギア周りの動作を行うクラス
	/// </summary>
	[System.Serializable]
	public class Transmission
	{
		public enum TransmissionType
		{
			Manual, Automatic, AutomaticSequential
		}
		public TransmissionType transmissionType = TransmissionType.AutomaticSequential;

		public enum ReverseType
		{
			Auto, DoubleTap
		}
		public ReverseType reverseType = ReverseType.Auto;

		public enum DifferentialType
		{
			Equal, Open, LimitedSlip, Locking
		}
		public DifferentialType differentialType = DifferentialType.Equal;

		[Header("Gears")]
		[SerializeField]
		private List<float> forwardGears = new List<float>() { 8f, 5.5f, 4f, 3f, 2.2f, 1.7f, 1.3f };

		[SerializeField]
		private float[] shiftDown_speeds = new float[7];

		[SerializeField]
		private List<float> reverseGears = new List<float>() { -5f };

		public float gearMultiplier = 1;

		[Header("Per-Gear Throttle Limiting")]
		public bool useThrottleLimiting = false;

		public List<float> forwardThrottleLimits = new List<float> { 0.6f, 0.8f, 1f, 1f, 1f, 1f, 1f };

		[Range(0.1f, 1f)]
		public float reverseThrottleLimit = 0.6f;

		[Header("Shifting")]
		public float targetShiftUpRPM = 3600;

		public float targetShiftDownRPM = 1400;

		public bool dynamicShiftPoint = true;

		[Range(0f, 0.2f)]
		public float shiftPointRandomness = 0.05f;

		public float shiftDuration = 0.2f;

		[Range(0f, 0.5f)]
		public float shiftDurationRandomness = 0.2f;

		public float postShiftBan = 0.5f;

		public float shiftDownRPMLimiting;

		[Header("Clutch")]
		public bool automaticClutch = true;

		[Range(0, 1)]
		public float clutchPedalPressedPercent;

		public AnimationCurve clutchEngagementCurve = new AnimationCurve(new Keyframe[2] {
				new Keyframe(0f, 1f),
				new Keyframe(1f, 0f)
			});

		[Header("Automatic Clutch")]
		public float targetClutchRPM = 1500;

		[HideInInspector]
		private float lastShiftTime;

		private float addedClutchRPM;

		private List<float> gears = new List<float>();
		[SerializeField]
		private List<float> shiftUpLimit = new List<float>();

		private VehicleController vc;
		private ChairController chair;

		private float initialShiftDuration;
		private float randomShiftDownPointAddition;
		private float randomShiftUpPointAddition;

		private float adjustedShiftUpRpm;
		private float adjustedShiftDownRpm;

		private float smoothedVerticalInput;
		private float verticalInputChangeVelocity;
		private int gear = 0;

		private bool allowedToReverse;
		private bool reverseHasBeenPressed = false;

		//シフトの押せない時間
		[SerializeField]
		private float derayTime = 0.5f;
		private float derayTime_shift = 0;
		private bool derayTime_flag = false;
		public bool canNot_Shift = false;
		public bool shift_N_1 = false;

		public bool isAuto = false;
		public void Initialize(VehicleController vc)
		{
			this.vc = vc;
			ReconstructGearList();
			initialShiftDuration = shiftDuration;
			UpdateRandomShiftDuration();
			UpdateRandomShiftPointAddition();

			chair = GameObject.Find("WIZMO").GetComponent<ChairController>();


            Gear = 0;
        }

		public void Update()
		{
			if (transmissionType == TransmissionType.Manual)
			{
				ManualShift();
			}
			else if (transmissionType == TransmissionType.Automatic || transmissionType == TransmissionType.AutomaticSequential)
			{
				AutomaticShift();
			}


			if (Input.GetKeyDown(KeyCode.L))
			{
				Debug.Log("GEAR" + vc.transmission.Gear);
			}
		}

		public void UpdateClutch()
		{
			addedClutchRPM = 0f;
			if (automaticClutch)
			{
				clutchPedalPressedPercent = 0;

				if ((vc.transmission.transmissionType == TransmissionType.Automatic) && (Gear == -1 && vc.input.Vertical < 0)
					|| (vc.transmission.transmissionType == TransmissionType.Manual) && (Gear == -1 && vc.input.Vertical > 0)
					|| (Gear == 1 && vc.input.Vertical > 0))
				{
					clutchPedalPressedPercent = Mathf.Clamp01((targetClutchRPM - Mathf.Abs(ReverseRPM)) / targetClutchRPM) * Mathf.Abs(vc.input.Vertical);
				}
				addedClutchRPM = (1f - GetClutchEngagementAtPedalPosition(clutchPedalPressedPercent)) * targetClutchRPM;
			}
			else
			{
				clutchPedalPressedPercent = vc.input.Clutch;
				addedClutchRPM = (1f - GetClutchEngagementAtPedalPosition(clutchPedalPressedPercent)) * (vc.engine.maxRPM - vc.engine.minRPM) * Mathf.Abs(vc.input.Vertical);
			}
		}

		public float ClutchPercent
		{
			get
			{
				return clutchPedalPressedPercent;
			}
		}

		public float AddedClutchRPM
		{
			get
			{
				return addedClutchRPM;
			}
		}

		public int ForwardGearCount
		{
			get
			{
				return gears.Count - 1 - reverseGears.Count;
			}
		}

		public int ReverseGearCount
		{
			get
			{
				return reverseGears.Count;
			}
		}

		public List<float> ForwardGears
		{
			get
			{
				return forwardGears;
			}
			set
			{
				forwardGears = value;
				ReconstructGearList();
			}
		}

		public List<float> ReverseGears
		{
			get
			{
				return reverseGears;
			}
			set
			{
				reverseGears = value;
				ReconstructGearList();
			}
		}

		// ギア毎の最高速度を設定する関数
		public float GetMaxSpeedForGear(int g)
		{
			if (Gear == 0) return Mathf.Infinity;

			float wheelRadiusSum = 0;
			int wheelCount = 0;

			foreach (AxleWheel wheel in vc.Wheels)
			{
				wheelRadiusSum += wheel.Radius;
				wheelCount++;
			}

			if (wheelCount > 0)
			{
				float avgWheelRadius = wheelRadiusSum / wheelCount;

				float maxRpmForGear = TransmitRPM(vc.engine.maxRPM);

				float maxSpeed = avgWheelRadius * maxRpmForGear * 0.105f;
				return maxSpeed;
			}
			return 0;
		}

		public float AdjustedShiftUpRpm
		{
			get
			{
				return adjustedShiftUpRpm;
			}
		}

		public float AdjustedShiftDownRpm
		{
			get
			{
				return adjustedShiftDownRpm;
			}
		}

		public int Gear
		{
			get
			{
				if (gear < 0 - reverseGears.Count)
					return gear = 0 - reverseGears.Count;
				else if (gear >= gears.Count - reverseGears.Count - 1)
					return gear = gears.Count - reverseGears.Count - 1;
				else
					return gear;
			}
			set
			{
				if (value < 0 - reverseGears.Count)
					gear = 0 - reverseGears.Count;
				else if (value >= gears.Count - reverseGears.Count - 1)
					gear = ForwardGearCount;
				else
					gear = value;
			}
		}

		public string GearName
		{
			get
			{
				if (Gear == 0)
				{
					return "N";
				}
				else if (Gear > 0)
				{
					return Gear.ToString();
				}
				else
				{
					if (reverseGears.Count > 1)
					{
						return "R" + Mathf.Abs(Gear).ToString();
					}
					else
					{
						return "R";
					}
				}

			}
		}

		public List<float> Gears
		{
			get
			{
				return gears;
			}
		}

		public float GearRatio
		{
			get
			{
				return gears[GearToIndex(gear)] * gearMultiplier;
			}
		}

		public float GetGearRatio(int g)
		{
			return gears[GearToIndex(g)] * gearMultiplier;
		}

		public float GetClutchEngagementAtPedalPosition(float clutchPercent)
		{
			return clutchEngagementCurve.Evaluate(clutchPercent);
		}

		public float RPM
		{
			get
			{
				float rpmSum = 0;
				foreach (Axle axle in vc.axles)
					rpmSum += axle.RPM;
				return rpmSum / vc.axles.Count;
			}
		}

		public float ReverseRPM
		{
			get
			{
				return ReverseTransmitRPM(RPM);
			}
		}

		public void ReconstructGearList()
		{
			List<float> reversedReverseGears = reverseGears;
			reversedReverseGears.Reverse();
			gears.Clear();

			gears.AddRange(reversedReverseGears);
			gears.Add(0);
			gears.AddRange(forwardGears);

			//====================================================
			// バックギア ＝＞ ニュートラル ＝＞ ギア1～6
			// の順番でリストに追加されていく

			// 呼ばれるのは、forwardGearsか、reversedReverseGearsの値を変更したときか、
			// Initialize（）が呼ばれたときである
			//====================================================
		}

		public void TorqueSplit(float torque, float topRPM)
		{
			if (transmissionType == TransmissionType.Automatic || transmissionType == TransmissionType.AutomaticSequential)
			{
				if ((Gear < 0 && vc.input.Vertical > 0) || (Gear >= 0 && vc.input.Vertical < 0) || Gear == 0)
				{
					torque = 0;
				}
			}
			else if (transmissionType == TransmissionType.Manual)
			{
				if (vc.input.Vertical < 0)
					torque = 0;
			}

			// クラッチ計算
			float torqueClutchMod = 1f;
			if (!automaticClutch)
			{
				torqueClutchMod = GetClutchEngagementAtPedalPosition(clutchPedalPressedPercent);
			}

			// トルク計算
			torque = torque * Mathf.Sign(Gear) * torqueClutchMod;

			float powerCoefficientSum = 0;
			int poweredAxleCount = 0;

			foreach (Axle axle in vc.axles)
			{
				powerCoefficientSum += axle.powerCoefficient;
				if (axle.IsPowered) poweredAxleCount++;
			}

			foreach (Axle axle in vc.axles)
			{
				axle.Bias = 0;
			}

			if (differentialType == DifferentialType.LimitedSlip || differentialType == DifferentialType.Locking)
			{
				float rpmSum = 0;
				float biasSum = 0;

				foreach (Axle axle in vc.axles)
					rpmSum += axle.SmoothRPM;

				foreach (Axle axle in vc.axles)
				{
					axle.Bias = rpmSum == 0 ? 0 : 1f - (axle.SmoothRPM / rpmSum);
					if (differentialType == DifferentialType.Locking)
						axle.Bias = Mathf.Pow(axle.Bias, 8f);
					biasSum += axle.Bias * axle.powerCoefficient;
				}

				foreach (Axle axle in vc.axles)
					axle.Bias = biasSum == 0 ? 0 : (axle.Bias * axle.powerCoefficient) / biasSum;
			}
			else if (differentialType == DifferentialType.Open)
			{
				float rpmSum = 0;

				foreach (Axle axle in vc.axles)
				{
					rpmSum += axle.SmoothRPM * axle.powerCoefficient;
				}

				foreach (Axle axle in vc.axles)
				{
					axle.Bias = rpmSum == 0 ? 0 : (axle.SmoothRPM * axle.powerCoefficient) / rpmSum;
				}
			}
			else if (differentialType == DifferentialType.Equal)
			{
				foreach (Axle axle in vc.axles)
				{
					axle.Bias = powerCoefficientSum == 0 ? 0 : axle.powerCoefficient / powerCoefficientSum;
				}
			}

			foreach (Axle axle in vc.axles)
			{
				if (axle.IsPowered)
				{
					axle.TorqueSplit(torque * axle.Bias, topRPM);
				}
			}
		}

		public void ShiftInto(int g, bool startTimer = true)
		{
			if (CanShift)
			{
				int prevGear = Gear;
				Gear = g;
				if (Gear != 0 && Gear != prevGear)
				{
					UpdateRandomShiftDuration();
					UpdateRandomShiftPointAddition();
					if (startTimer) StartTimer();
				}
				if (gear > prevGear) chair.ShiftUp();
				else chair.ShiftDown();
			}
		}

		public bool CanShift
		{
			get
			{
				if (Time.realtimeSinceStartup > lastShiftTime + shiftDuration + postShiftBan)
				{
					return true;
				}
				return false;
			}
		}

		public bool Shifting
		{
			get
			{
				if (Time.realtimeSinceStartup < lastShiftTime + shiftDuration)
				{
					return true;
				}
				return false;
			}
		}

		public float TransmitTorque(float inputTorque)
		{
			float gearRatio = GearRatio;
			return gearRatio == 0 ? 0 : Mathf.Abs(inputTorque * gearRatio);
		}

		public float ReverseTransmitTorque(float inputTorque)
		{
			float gearRatio = GearRatio;
			return gearRatio == 0 ? 0 : Mathf.Abs(inputTorque / GearRatio);
		}

		public float TransmitRPM(float inputRPM)
		{
			if (GearRatio != 0)
				return Mathf.Abs(inputRPM / GearRatio);
			return 0;
		}

		public float ReverseTransmitRPM(float inputRPM)
		{
			return Mathf.Abs(inputRPM * GearRatio);
		}

		public float ReverseTransmitRPM(float inputRPM, int g)
		{
			return Mathf.Abs(inputRPM * gears[GearToIndex(g)] * gearMultiplier);
		}

		private float GetMaxGearRatio()
		{
			float max = 0;
			for (int i = 0; i < forwardGears.Count; i++)
			{
				if (forwardGears[i] > max) max = forwardGears[i];
			}
			return max;
		}

		private float GetMinGearRatio()
		{
			float min = Mathf.Infinity;
			for (int i = 0; i < forwardGears.Count; i++)
			{
				if (forwardGears[i] < min) min = forwardGears[i];
			}
			return min;
		}

		private float GetGearRatioRange()
		{
			return GetMaxGearRatio() - GetMinGearRatio();
		}

		private void StartTimer()
		{
			lastShiftTime = Time.realtimeSinceStartup;
		}

		private int GearToIndex(int g)
		{
			return g + reverseGears.Count;
		}

		private void ManualShift()
		{
			if (isShift())
			{
				if (vc.input.ShiftUp)
				{
					if (!(Gear == 0 && vc.Speed < -2f))
					{
						Gear++;
						derayTime_flag = true;
					}
				}
				if (vc.input.ShiftDown)
				{
					if (!(Gear == 0 && vc.Speed > 2f))
					{
						if (!(vc.engine.RPM > shiftDownRPMLimiting) || Gear == 1)
						{
							Gear--;
							derayTime_flag = true;
						}
						else
						{
							canNot_Shift = true;
							Debug.Log("CanNotShift");
						}
					}
				}
			}
			vc.input.ShiftUp = false;
			vc.input.ShiftDown = false;

			if (Gear >= 7)
			{
				Gear = 6;
			}
		}

		//シフトが上がらなくなるタイミングの前で表示ORRPMがいくらか下がったら
		public bool isShiftDown()
		{
			if (Gear == 2)
			{
				if (vc.SpeedKPH <= vc.engine.acceleOnLimit[Gear] + 5)
				{
					return true;
				}
			}
			else if (Gear >= 3)
			{
				if (vc.SpeedKPH <= vc.engine.acceleOnLimit[Gear] + 15)
				{
					return true;
				}
			}
			return false;
		}

		private void AutomaticShift()
		{
			if(Countdown.GameStart)
            {
				if (reverseType == ReverseType.Auto)
				{
					allowedToReverse = true;
				}
				else if (reverseType == ReverseType.DoubleTap)
				{
					allowedToReverse = reverseHasBeenPressed ? false : true;

					if (vc.input.Vertical < -0.05f)
					{
						reverseHasBeenPressed = true;
					}
					else
					{
						reverseHasBeenPressed = false;
					}
				}
				float damping = Mathf.Abs(vc.input.Vertical) > smoothedVerticalInput ? 0.3f : 5f;
				smoothedVerticalInput = Mathf.SmoothDamp(smoothedVerticalInput, Mathf.Abs(vc.input.Vertical), ref verticalInputChangeVelocity, damping);

				adjustedShiftDownRpm = targetShiftDownRPM + randomShiftDownPointAddition;
				adjustedShiftUpRpm = targetShiftUpRPM + randomShiftUpPointAddition;

				if (dynamicShiftPoint)
				{
					adjustedShiftDownRpm = targetShiftDownRPM + (-0.5f + smoothedVerticalInput) * vc.engine.maxRPM * 0.4f;
					adjustedShiftUpRpm = targetShiftUpRPM + (-0.5f + smoothedVerticalInput) * vc.engine.maxRPM * 0.4f;

					float inclineModifier = Vector3.Dot(vc.transform.forward, Vector3.up);
					adjustedShiftDownRpm += vc.engine.maxRPM * inclineModifier;
					adjustedShiftUpRpm += vc.engine.maxRPM * inclineModifier;

					adjustedShiftUpRpm = Mathf.Clamp(adjustedShiftUpRpm, targetShiftUpRPM, vc.engine.maxRPM * 0.95f);
					adjustedShiftDownRpm = Mathf.Clamp(adjustedShiftDownRpm, vc.engine.minRPM * 1.2f, adjustedShiftUpRpm * 0.7f);
				}

				if (Gear == 0)
				{
					if (vc.input.Vertical > 0f && vc.ForwardVelocity >= -vc.brakes.reverseDirectionBrakeVelocityThreshold)
					{
						Gear = 1;
					}
					else if (vc.input.Vertical < 0f && vc.ForwardVelocity <= 1f && allowedToReverse)
					{
						Gear = -1;
					}
				}
				else if (Gear < 0)
				{
					if (vc.input.Vertical > 0f && vc.Speed < 1f)
					{
						ShiftInto(1, false);
					}
					else if (vc.input.Vertical == 0f && vc.Speed < 1f)
					{
						ShiftInto(0, false);
					}

					if (vc.engine.RPM > AdjustedShiftUpRpm)
					{
						ShiftInto(Gear - 1);
					}
					else if (vc.engine.RPM < adjustedShiftDownRpm)
					{
						if (Gear == -1 && (vc.input.Vertical == 0 || vc.engine.RpmOverflow < -(vc.engine.minRPM / 5f)))
						{
							ShiftInto(0);
						}
						else if (Gear < -1)
						{
							ShiftInto(Gear + 1);
						}
					}
				}
				else
				{
					if (vc.ForwardVelocity > 0.2f)
					{
						if (vc.engine.RPM > adjustedShiftUpRpm && vc.input.Vertical >= 0 && !vc.WheelSpin)
						{
							bool grounded = true;
							foreach (AxleWheel wheel in vc.Wheels)
							{
								if (!wheel.IsGrounded)
								{
									grounded = false;
									break;
								}
							}

							if (grounded)
							{
								if (transmissionType == TransmissionType.Automatic)
								{
									if (vc.input.Vertical > 0.6f)
									{
										ShiftInto(Gear + 1);
									}
									else
									{
										int g = Gear;
										while (g < forwardGears.Count - 1)
										{
											float wouldBeEngineRpm = ReverseTransmitRPM(RPM, g);
											if (wouldBeEngineRpm > adjustedShiftDownRpm && wouldBeEngineRpm < (adjustedShiftDownRpm + adjustedShiftUpRpm) / 2f)
											{
												break;
											}

											g++;
											//wouldBeEngineRpm = ReverseTransmitRPM(RPM, g);
											if (wouldBeEngineRpm < adjustedShiftDownRpm)
											{
												g--;
												break;
											}
										}
										if (g != Gear)
										{
											ShiftInto(g);
										}
									}
								}
								else
								{
									ShiftInto(Gear + 1);
								}

							}
						}
						else if (vc.engine.RPM < adjustedShiftDownRpm)
						{
							if (transmissionType == TransmissionType.Automatic)
							{
								if (Gear != 1)
								{
									int g = Gear;
									while (g > 1)
									{
										g--;
										float wouldBeEngineRpm = ReverseTransmitRPM(RPM, g);
										if (wouldBeEngineRpm > adjustedShiftUpRpm)
										{
											g++;
											break;
										}
									}
									if (g != Gear)
									{
										ShiftInto(g);
									}
								}
								else if (vc.Speed < 0.8f && Mathf.Abs(vc.input.Vertical) < 0.05f)
								{
									ShiftInto(0);
								}
							}
							else
							{
								if (Gear != 1)
								{
									ShiftInto(Gear - 1);
								}
								else if (vc.Speed < 0.8f && Mathf.Abs(vc.input.Vertical) < 0.05f)
								{
									ShiftInto(0);
								}
							}
						}
					}
					else if (vc.ForwardVelocity <= 0.2f && vc.input.Vertical < -0.05f && allowedToReverse)
					{
						ShiftInto(-1, false);
					}
					else if (vc.input.Vertical == 0 || (!allowedToReverse && vc.input.Vertical < 0) || vc.engine.RpmOverflow < -(vc.engine.minRPM / 5f))
					{
						ShiftInto(0);
					}
				}
			}
		}

		private void UpdateRandomShiftDuration()
		{
			shiftDuration = initialShiftDuration + Random.Range(-shiftDurationRandomness * initialShiftDuration, shiftDurationRandomness * initialShiftDuration);
		}

		private void UpdateRandomShiftPointAddition()
		{
			randomShiftDownPointAddition = Random.Range(-shiftPointRandomness * targetShiftDownRPM, shiftPointRandomness * targetShiftDownRPM);
			randomShiftUpPointAddition = Random.Range(-shiftPointRandomness * targetShiftUpRPM, shiftPointRandomness * targetShiftUpRPM);
		}

		bool IsShiftDown()
		{
			if (Gear < shiftDown_speeds.Length)
			{

			}
			return true;
		}

		bool isShift()
		{
			if (derayTime_flag)
			{
				derayTime_shift += Time.deltaTime;
				if (derayTime_shift > derayTime)
				{
					derayTime_shift = 0;
					derayTime_flag = false;
				}
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}