using System;
using UnityEngine;

namespace AIM
{
    /// <summary>
    /// スリップ防止機能
    /// </summary>
    [System.Serializable]
    public class Stability : DrivingAssists.DrivingAid
    {
        private const float speedCoeff = 0.166f;

        public void Update(VehicleController vc)
        {
            foreach (Axle axle in vc.axles)
            {
                foreach (AxleWheel wheel in vc.Wheels)
                {
                    if (wheel.WheelController.isGrounded)
                    {
                        float forceMag = 0;
                        Vector3 force;

                        //if (vc.input.Vertical > 0)
                        //{
                        //    forceMag = wheel.WheelController.sideFriction.force * Mathf.Clamp01(Mathf.Abs(wheel.WheelController.sideFriction.slip)) * intensity * (vc.Speed * speedCoeff) /50;
                        //    force = wheel.WheelController.wheelHit.sidewaysDir * -forceMag;

                        //   //Debug.Log(forceMag);
                        //   //Debug.Log(force);


                        //    vc.vehicleRigidbody.AddForceAtPosition(force, wheel.WheelController.wheelHit.groundPoint);
                        //}
                        /*else */if (vc.input.Vertical < 0 && vc.BSenpai)
                        {
                            forceMag = wheel.WheelController.sideFriction.force * Mathf.Clamp01(Mathf.Abs(wheel.WheelController.sideFriction.slip)) * intensity * (vc.Speed * speedCoeff);
                            force = wheel.WheelController.wheelHit.sidewaysDir * -forceMag;

                            //Debug.Log(forceMag);
                            //Debug.Log(force);


                            vc.vehicleRigidbody.AddForceAtPosition(force, wheel.WheelController.wheelHit.groundPoint);
                        }
                        //else
                        //{
                        //    forceMag = wheel.WheelController.sideFriction.force * Mathf.Clamp01(Mathf.Abs(wheel.WheelController.sideFriction.slip)) * intensity * (vc.Speed * speedCoeff) /50;
                        //    force = wheel.WheelController.wheelHit.sidewaysDir * -forceMag;


                        //   //Debug.Log(forceMag);
                        //   //Debug.Log(force);


                        //    vc.vehicleRigidbody.AddForceAtPosition(force, wheel.WheelController.wheelHit.groundPoint);
                        //}
                    }
                }
            }
        }
    }
}