using System;
using UnityEngine;

namespace AIM
{
    /// <summary>
    /// ブレーキの踏みすぎ防止機能
    /// </summary>
    [System.Serializable]
    public class ABS : DrivingAssists.DrivingAid
    {
        public void Update(VehicleController vc)
        {
            active = false;

            // ブレーキ中に処理
            if (vc.brakes.Active && vc.engine.RpmOverflow <= 0f && !vc.engine.FuelCutoff)
            {
                foreach (AxleWheel wheel in vc.Wheels)
                {
                    float maxTorque = wheel.WheelController.MaxPutDownForce * wheel.Radius * (1f + (0.5f - intensity * 0.5f));

                    if (wheel.BrakeTorque > maxTorque)
                    {
                        wheel.BrakeTorque = maxTorque;
                        active = true;
                    }
                    else if (wheel.BrakeTorque < -maxTorque)
                    {
                        wheel.BrakeTorque = -maxTorque;
                        active = true;
                    }
                }
            }
        }
    }
}
