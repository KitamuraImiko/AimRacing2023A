using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
    [System.Serializable]
    public class AxleWheel
    {
        public AxleWheel() { }

        public AxleWheel(WheelController wc, VehicleController vc)
        {
            this.wheelController = wc;
            this.vc = vc;
        }

        [HideInInspector]
        public float brakeCoefficient = 1f;

        [SerializeField]
        public WheelController wheelController;
        private VehicleController vc;

        private float smoothRPM;
        private float prevSmoothRPM;
        public float bias;
        private float damage;
        private float damageSteerDirection;
        private bool singleRayByDefault;
        private float prevSideSlip;
        private float prevForwardSlip;
        private float smoothForwardSlip;
        private float smoothSideSlip;

        public void Update()
        {
            prevSmoothRPM = smoothRPM;
            float a = (smoothRPM - prevSmoothRPM) / Time.fixedDeltaTime;
            smoothRPM = Mathf.Lerp(smoothRPM, wheelController.rpm, Time.fixedDeltaTime * 20f)
                                + (smoothRPM - prevSmoothRPM) * 50 + a * Time.fixedDeltaTime * Time.fixedDeltaTime;

            smoothForwardSlip = Mathf.Lerp(smoothForwardSlip, ForwardSlip, Time.fixedDeltaTime * 5f);

            smoothSideSlip = Mathf.Lerp(smoothSideSlip, SideSlip, Time.fixedDeltaTime * 5f);
        }

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

        public float ForwardSlip
        {
            get
            {
                return Mathf.Clamp01(Mathf.Abs(wheelController.forwardFriction.slip));
            }
        }

        public float SideSlip
        {
            get
            {
                return Mathf.Clamp01(Mathf.Abs(wheelController.sideFriction.slip));
            }
        }

        public float SmoothForwardSlip
        {
            get
            {
                return smoothForwardSlip;
            }
        }

        public float SmoothSideSlip
        {
            get
            {
                return smoothSideSlip;
            }
        }

        public float ForwardSlipPercent
        {
            get
            {
                return Mathf.Clamp01(ForwardSlip / vc.forwardSlipThreshold);
            }
        }
        public float SideSlipPercent
        {
            get
            {
                return Mathf.Clamp01(SideSlip / vc.sideSlipThreshold);
            }
        }

        public bool HasForwardSlip
        {
            get
            {
                if (Mathf.Abs(ForwardSlip) > vc.forwardSlipThreshold && Mathf.Abs(wheelController.rpm) > 6f)
                    return true;
                else
                    return false;
            }
        }

        public GroundDetection.GroundEntity CurrentGroundEntity
        {
            get
            {
                return vc.groundDetection.GetCurrentGroundEntity(this.wheelController);
            }
        }

        public string CurrentGroundEntityName
        {
            get
            {
                return vc.groundDetection.GetCurrentGroundEntity(this.wheelController).name;
            }
        }

        public bool HasSideSlip
        {
            get
            {
                if (Mathf.Abs(SideSlip) > vc.sideSlipThreshold)
                    return true;
                else
                    return false;
            }
        }

        public float Damage
        {
            get { return damage; }
            set { damage = value; }
        }

        public float RPM
        {
            get
            {
                return wheelController.rpm;
            }
        }

        public Transform ControllerTransform
        {
            get
            {
                return wheelController.transform;
            }
        }

        public bool IsGrounded
        {
            get
            {
                return wheelController.isGrounded;
            }
        }

        public float SpringTravel
        {
            get
            {
                return wheelController.springCompression * wheelController.springLength;
            }
        }

        public float MotorTorque
        {
            get
            {
                return wheelController.motorTorque;
            }
            set
            {
                wheelController.motorTorque = value;
            }
        }

        public float BrakeTorque
        {
            get
            {
                return wheelController.brakeTorque;
            }
            set
            {
                wheelController.brakeTorque = value;
            }
        }

        public Transform VisualTransform
        {
            get
            {
                return wheelController.Visual.transform;
            }
        }

        public float Radius
        {
            get
            {
                return wheelController.tireRadius;
            }
        }

        public float Width
        {
            get
            {
                return wheelController.tireWidth;
            }
        }

        public GameObject ControllerGO
        {
            get
            {
                return wheelController.gameObject;
            }
        }

        public float SteerAngle
        {
            get
            {
                return wheelController.steerAngle;
            }
            set
            {
                wheelController.steerAngle = value;
            }
        }

        public float SmoothRPM
        {
            get
            {
                return smoothRPM;
            }
        }

        public float NoSlipRPM
        {
            get
            {
                return wheelController.forwardFriction.speed / (6.28f * Radius);
            }
        }

        public WheelController WheelController
        {
            get
            {
                return wheelController;
            }
        }

        public float DamageSteerDirection
        {
            get
            {
                return damageSteerDirection;
            }
        }

        /// <summary>
        /// ブレーキ力の設定
        /// </summary>
        public void AddBrakeTorque(float torque)
        {
            torque *= brakeCoefficient;
            if (torque < 0)
                wheelController.brakeTorque += 0f;
            else
                wheelController.brakeTorque += torque;

            if (wheelController.brakeTorque > vc.brakes.maxTorque)
                wheelController.brakeTorque = vc.brakes.maxTorque;

            if (wheelController.brakeTorque < 0)
                wheelController.brakeTorque = 0;

            vc.brakes.Active = true;
        }

        // ハンドブレーキ時に、ブレーキ係数を無視し、上限をハンドブレーキ用の値を使用する
        public void AddHandBrakeTorque(float torque)
        {
            if (torque < 0)
                wheelController.brakeTorque += 0f;
            else
                wheelController.brakeTorque += torque;

            if (wheelController.brakeTorque > vc.brakes.handBrakeTorque)
                wheelController.brakeTorque = vc.brakes.handBrakeTorque;

            if (wheelController.brakeTorque < 0)
                wheelController.brakeTorque = 0;

            vc.brakes.Active = true;
        }

        public void Lockup()
        {
            wheelController.brakeTorque = 1000000f;
        }

        public void Initialize(VehicleController vc)
        {
            this.vc = vc;
            damageSteerDirection = Random.Range(-1f, 1f);
            singleRayByDefault = WheelController.singleRay;
        }

        public void SetBrakeIntensity(float percent)
        {
            AddBrakeTorque(Mathf.Abs(vc.brakes.maxTorque * Mathf.Clamp01(Mathf.Abs(percent))));
        }

        public void ResetBrakes(float value)
        {
            wheelController.brakeTorque = Mathf.Abs(value);
        }

        public void Activate()
        {
            if (!singleRayByDefault && vc.switchToSingleRayWhenInactive)
                wheelController.singleRay = false;
        }

        public void Suspend()
        {
            if (vc.switchToSingleRayWhenInactive)
            {
                wheelController.singleRay = true;
            }
        }
    }
}

