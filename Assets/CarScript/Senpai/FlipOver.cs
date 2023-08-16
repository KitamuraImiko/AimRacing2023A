using UnityEngine;
using System.Collections;

namespace AIM
{
    [System.Serializable]
    public class FlipOver
    {
        public bool enabled = true;
        public bool manual = false;

        public float timeout = 3f;
        public float allowedAngle = 70f;
        public float maxDetectionSpeed = 1f;
        public float rotationSpeed = 80f;

        [HideInInspector]
        public bool flippedOver = false;

        private bool wasFlippedOver = false;
        private float timeSinceFlip = 0f;
        private float timeAfterRecovery = 0f;
        private VehicleController vc;
        private float direction = 0;
        private bool manualFlipoverInProgress = false;

        public void Initialize(VehicleController vc)
        {
            this.vc = vc;
        }

        public void Update()
        {
            DetectFlipOver();

            if (!enabled) return;

            // 手動
            if (manual)
            {
                if (vc.input.flipOver)
                {
                    manualFlipoverInProgress = true;
                    vc.input.flipOver = false;
                }
            }

            // 復帰処理
            if ((flippedOver && !manual) || (flippedOver && manual && manualFlipoverInProgress))
            {
                if (direction == 0)
                {
                    direction = Mathf.Sign(Vector3.SignedAngle(vc.transform.up, -Physics.gravity.normalized, vc.transform.forward) - 180f);
                }

                vc.vehicleRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                Quaternion yRotation = Quaternion.AngleAxis(rotationSpeed * Time.fixedDeltaTime, vc.transform.InverseTransformDirection(vc.transform.forward));
                vc.vehicleRigidbody.MoveRotation(vc.transform.rotation * yRotation);
                vc.ResetInactivityTimer();
            }
            else if (wasFlippedOver && !flippedOver)
            {
                vc.vehicleRigidbody.constraints = RigidbodyConstraints.None;
                manualFlipoverInProgress = false;
            }

            wasFlippedOver = flippedOver;
        }

        void DetectFlipOver()
        {
            int wheelsOnGround = 0;
            foreach (AxleWheel wheel in vc.Wheels)
            {
                if (wheel.IsGrounded)
                {
                    wheelsOnGround++;
                }
            }

            if (vc.Speed < maxDetectionSpeed && VehicleAngle() > allowedAngle && wheelsOnGround <= vc.Wheels.Count / 2f)
            {
                timeSinceFlip += Time.fixedDeltaTime;

                if (timeSinceFlip > timeout)
                {
                    flippedOver = true;
                }
            }
            else
            {
                timeAfterRecovery += Time.fixedDeltaTime;

                if (timeAfterRecovery > 1f || VehicleAngle() < 45f)
                {
                    flippedOver = false;
                    timeSinceFlip = 0f;
                    timeAfterRecovery = 0f;
                    direction = 0;
                }
            }
        }

        float VehicleAngle()
        {
            return Vector3.Angle(vc.transform.up, -Physics.gravity.normalized);
        }
    }
}