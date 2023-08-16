using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
    /// <summary>
    /// 車体の重心を計算・表示するクラス
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [ExecuteInEditMode]
    public class CenterOfMass : MonoBehaviour
    {
        public Vector3 centerOfMassOffset = Vector3.zero;

        public bool showCOM = true;

        private Vector3 centerOfMass;
        private Vector3 prevOffset = Vector3.zero;
        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            centerOfMass = rb.centerOfMass;
        }

        void Update()
        {
            if (centerOfMassOffset != prevOffset)
            {
                rb.centerOfMass = centerOfMass + centerOfMassOffset;
            }
            prevOffset = centerOfMassOffset;
        }

        private void OnDrawGizmosSelected()
        {
            if (showCOM && rb != null)
            {
                float radius = 0.1f;
                try
                {
                    radius = GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 30f;
                }
                catch { }

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(rb.transform.TransformPoint(rb.centerOfMass), radius);
                Gizmos.DrawCube(rb.transform.TransformPoint(centerOfMassOffset), new Vector3(0.2f, 0.2f, 0.2f));
            }
        }
    }
}