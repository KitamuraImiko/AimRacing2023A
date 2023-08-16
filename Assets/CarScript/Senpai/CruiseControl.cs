using UnityEngine;

namespace AIM
{
    /// <summary>
    /// 一定速度を維持して走行させる機能
    /// </summary>
    [System.Serializable]
    public class CruiseControl : DrivingAssists.DrivingAid
    {
        // 指定速度
        public float targetSpeed = 0f;

        // 入力がない時にこの機能を有効化するなら
        public bool deactivateOnVerticalInput = true;

        // ブレーキ
        public bool useBrakesOnOverspeed = true;
        public float overspeedBrakeIntensity = 0.3f;

        private float correction;
        private float prevVerticalInput;
        private float speedDiff;
        private float vertical;

        private const float inputCoeff = 10.0f;

        public void Update(VehicleController vc)
        {
            // 入力がない時に、この機能をスキップする
            if (deactivateOnVerticalInput && (vc.input.Vertical < 0.1f || vc.input.Vertical > 0.1f))
            {
                return;
            }

            // 設定した速度以上になった場合、アクセルの入力を弱める（速度を維持する）
            speedDiff = vc.SpeedKPH - targetSpeed;
            correction = -Mathf.Sign(speedDiff) * Mathf.Pow(speedDiff, 2f) * 0.5f;

            // 速度維持の為の入力補正
            if (useBrakesOnOverspeed)
            {
                vertical = Mathf.Clamp(correction, -1f, 0f) * overspeedBrakeIntensity + Mathf.Clamp(correction, 0f, 1f);
            }
            else
            {
                vertical = Mathf.Clamp(correction, 0f, 1f);
            }

            vc.input.Vertical = Mathf.Lerp(prevVerticalInput, vertical, Time.fixedDeltaTime * inputCoeff);

            prevVerticalInput = vc.input.Vertical;
        }
    }
}