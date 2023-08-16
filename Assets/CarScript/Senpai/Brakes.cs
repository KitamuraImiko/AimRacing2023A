using UnityEngine;
using System.Collections;

namespace AIM
{
    [System.Serializable]
    public class Brakes
    {
        public float maxTorque = 5000f;

        public float handBrakeTorque = 5000f;

        public float frictionTorque = 120f;

        [Range(0f, 5f)]
        public float smoothing = 0.9f;

        public bool brakeWhileIdle = true;
        public bool airBrakes = false;

        [Range(0.05f, 2f)]
        public float reverseDirectionBrakeVelocityThreshold = 0.3f;

        [HideInInspector]
        public float airBrakePressure;

        private float intensity;
        private float intensityVelocity;
        private bool active = false;

        public float brakeThreshold = 0.3f;
        public float angularDrag_brake;
        public float angularDrag_def;
        public float angularDrag_lerp;

        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }

        public void Update(VehicleController vc)
        {
            if (vc.input == null) return;

            // ブレーキリセット
            foreach (AxleWheel wheel in vc.Wheels)
                wheel.ResetBrakes(0);

            // 摩擦
            if (vc.input.Vertical.IsDeadzoneZero())
                foreach (AxleWheel wheel in vc.Wheels)
                    wheel.AddBrakeTorque(frictionTorque);

            float brakingIntensityModifier = Mathf.Clamp(vc.Speed * 50f, 0.25f, 1f);

            float axleRpmSum = 0f;
            foreach (Axle axle in vc.axles)
            {
                axleRpmSum += axle.NoSlipRPM;
            }

            float maxAllowedSpeed = vc.transmission.GetMaxSpeedForGear(vc.transmission.Gear);
            if (vc.Speed > maxAllowedSpeed
                && vc.transmission.transmissionType == Transmission.TransmissionType.Manual
                && vc.transmission.Gear != 0)
            {
                foreach (Axle axle in vc.axles)
                {
                    // エンジンブレーキなので、エンジンにつながっている軸のみブレーキ
                    if (axle.IsPowered)
                    {
                        axle.leftWheel.AddBrakeTorque(axle.leftWheel.WheelController.MaxPutDownForce * 1.1f * axle.leftWheel.Radius * axle.powerCoefficient * brakingIntensityModifier);
                        axle.rightWheel.AddBrakeTorque(axle.rightWheel.WheelController.MaxPutDownForce * 1.1f * axle.rightWheel.Radius * axle.powerCoefficient * brakingIntensityModifier);
                    }
                }
            }

            if (vc.input.Vertical.IsDeadzoneZero() && vc.transmission.Gear != 0 && !vc.transmission.Shifting)
            {
                float bTorque = vc.transmission.TransmitTorque(vc.engine.ApproxMaxTorque * vc.engine.RPMPercent * vc.engine.RPMPercent * 0.15f);
                foreach (Axle axle in vc.axles)
                {
                    // エンジンブレーキなので、エンジンにつながっている軸のみブレーキ
                    if (axle.IsPowered)
                    {
                        axle.leftWheel.AddBrakeTorque(bTorque);
                        axle.rightWheel.AddBrakeTorque(bTorque);
                    }
                }
            }

            // サイドブレーキ処理
            if (vc.input.Handbrake != 0 && vc.Active)
            {
                foreach (Axle axle in vc.axles)
                {
                    if (axle.handbrakeCoefficient != 0)
                    {
                        axle.leftWheel.AddHandBrakeTorque(handBrakeTorque);
                        axle.rightWheel.AddHandBrakeTorque(handBrakeTorque);
                    }
                }
            }

            active = false;

            if (!vc.Active || (vc.Active && brakeWhileIdle && vc.input.Vertical.IsDeadzoneZero() && vc.transmission.Gear == 0 && vc.Speed < 0.1f))
            {
                foreach (Axle axle in vc.axles)
                {
                    axle.leftWheel.SetBrakeIntensity(1);
                    axle.rightWheel.SetBrakeIntensity(1);
                }
            }

            // オート
            if (vc.transmission.transmissionType == Transmission.TransmissionType.Automatic || vc.transmission.transmissionType == Transmission.TransmissionType.AutomaticSequential)
            {
                bool velocityMatchesInput = Mathf.Sign(vc.ForwardVelocity + Mathf.Sign(vc.input.Vertical) * reverseDirectionBrakeVelocityThreshold) == Mathf.Sign(vc.input.Vertical);
                bool gearMatchesInput = Mathf.Sign(vc.transmission.Gear) == Mathf.Sign(vc.input.Vertical);
                bool inputActive = vc.input.Vertical > 0.05f || vc.input.Vertical < -0.05f;

                if (inputActive && (!gearMatchesInput || !velocityMatchesInput))
                {
                    foreach (AxleWheel wheel in vc.Wheels)
                    {
                        intensity = Mathf.SmoothDamp(intensity, Mathf.Abs(vc.input.Vertical), ref intensityVelocity, smoothing);
                        wheel.SetBrakeIntensity(intensity * brakingIntensityModifier);
                    }
                }
            }
            // マニュアル
            else if (vc.transmission.transmissionType == Transmission.TransmissionType.Manual)
            {
                if (vc.input.Vertical < 0
                    || Mathf.Sign(vc.input.Vertical) != Mathf.Sign((vc.ForwardVelocity + 0.1f) * vc.transmission.GearRatio))
                {
                    foreach (AxleWheel wheel in vc.Wheels)
                    {
                        intensity = Mathf.SmoothDamp(intensity, Mathf.Abs(vc.input.Vertical), ref intensityVelocity, smoothing);
                        wheel.SetBrakeIntensity(intensity * brakingIntensityModifier);
                    }
                }
            }

            // ブレーキ時の回転減衰
            // ブレーキ時にスリップするのを解消するためのごり押し
            if (vc.input.Vertical > -brakeThreshold || Mathf.Abs(vc.input.Horizontal) > 0.3f)
            {
                vc.rb.angularDrag = Mathf.Lerp(vc.rb.angularDrag, angularDrag_def, angularDrag_lerp * Time.deltaTime);
            }
            else
            {
                vc.rb.angularDrag = Mathf.Lerp(vc.rb.angularDrag, angularDrag_brake, angularDrag_lerp * Time.deltaTime);
            }

            if (airBrakes)
            {
                airBrakePressure += Time.fixedDeltaTime * 1f;
                airBrakePressure = Mathf.Clamp(airBrakePressure, 0f, 3f);
            }
        }
    }
}

