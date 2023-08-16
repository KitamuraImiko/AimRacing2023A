using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AIM
{
    [RequireComponent(typeof(VehicleController))]
    public class Downforce : MonoBehaviour
    {
        [System.Serializable]
        public class DownforcePoint
        {
            public Vector3 position;
            public float maxForce;

            public float force;
        }
        public List<DownforcePoint> downforcePoints = new List<DownforcePoint>();

        public float maxDownforceSpeed = 50f;
        private VehicleController vc;

        private void Start()
        {
            vc = GetComponent<VehicleController>();
        }

        void Update()
        {
            float speedPercent = vc.Speed / maxDownforceSpeed;
            float forceCoeff = 1f - (1f - Mathf.Pow(speedPercent, 2f));

            foreach (DownforcePoint dp in downforcePoints)
            {
                dp.force = forceCoeff * dp.maxForce;
                // 車の前と後ろに、力を加える
                // 速ければ速いほど、強く力を加える
                vc.vehicleRigidbody.AddForceAtPosition(dp.force * -transform.up, transform.TransformPoint(dp.position));
            }
        }

        private void OnDrawGizmosSelected()
        {
            // ギズモを追加、色や形を変更
            foreach (DownforcePoint dp in downforcePoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.TransformPoint(dp.position), 0.1f);
            }
        }
    }
}

