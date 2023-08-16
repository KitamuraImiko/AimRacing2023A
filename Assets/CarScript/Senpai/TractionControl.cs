using System;
using UnityEngine;

namespace AIM
{
    /// <summary>
    /// タイヤの空転防止装置
    /// </summary>
    [System.Serializable]
    public class TractionControl : DrivingAssists.DrivingAid
    {
        // パワーの低減量
        [HideInInspector]
        public float powerReduction;

        [HideInInspector]
        public float prevPowerReduction;


        public void Update(VehicleController vc)
        {
            active = false;
            prevPowerReduction = vc.engine.TcsPowerReduction;
            vc.engine.TcsPowerReduction = 0f;

            // 加速中に処理
            if (vc.Speed > 0.5f && !vc.brakes.Active)
            {
                foreach (AxleWheel wheel in vc.Wheels)
                {
                    float maxTorque = wheel.WheelController.MaxPutDownForce * wheel.Radius * (1f + (0.5f - intensity * 0.5f));

                    if (wheel.MotorTorque > maxTorque)
                    {
                        wheel.MotorTorque = maxTorque;
                        active = true;
                    }
                    else if (wheel.MotorTorque < -maxTorque)
                    {
                        wheel.MotorTorque = -maxTorque;
                        active = true;
                    }
                }
            }
        }
    }
}