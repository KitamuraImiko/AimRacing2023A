using System;
using UnityEngine;

namespace AIM
{
    /// <summary>
    /// ドリフト時に滑りすぎないように補正する機能
    /// </summary>
    [System.Serializable]
    public class DriftAssist : DrivingAssists.DrivingAid
    {
        private const float speedCoeff = 0.066f;

        public void Update(VehicleController vc)
        {
            Vector3 normVel = vc.vehicleRigidbody.velocity.normalized;
            Vector3 vehicleDir = vc.transform.forward;

            // 回転している角度の計算
            float driftAngle = VehicleController.AngleSigned(normVel, vehicleDir, vc.transform.up);
            driftAngle = Mathf.Sign(driftAngle) * Mathf.Clamp(Mathf.Abs(Mathf.Clamp(driftAngle, -90f, 90f)), 0f, Mathf.Infinity);

            if (vc.axles.Count > 0)
            {
                // 後輪軸の中心座標取得
                Axle a = vc.axles[vc.axles.Count - 1];
                Vector3 center = (a.leftWheel.ControllerTransform.position + a.rightWheel.ControllerTransform.position) / 2.0f;

                // ドリフト角度に応じて、方向の設定
                float forceMag = driftAngle * Mathf.Lerp(0f, vc.vehicleRigidbody.mass, vc.Speed * speedCoeff) * intensity;
                Vector3 force = vc.transform.right * forceMag;

                // 後輪軸の中心から、内輪側へ力を加える
                //vc.vehicleRigidbody.AddForceAtPosition(force, center);
            }
        }
    }
}