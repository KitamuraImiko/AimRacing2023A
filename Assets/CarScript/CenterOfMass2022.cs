//---------------------------------------------
// 作成者：20CU0232 松下和樹
// 作成日：2022/07/30
//
//---------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
    public class CenterOfMass2022 : MonoBehaviour
    {
        private GameObject m_vehicle;               //車のobject
        private Rigidbody m_vehicleRigitbody;
        [SerializeField] Transform m_centerOfMass;    // 重心位置

        private bool m_showCOM = true;
        private float m_radius = 0.1f;

        // Start is called before the first frame update
        void Start()
        {
            m_vehicle = GameObject.Find("bmw2022");
            m_vehicleRigitbody = m_vehicle.GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            //m_vehicleRigitbody.centerOfMass = m_centerOfMass.localPosition;    // 重心位置を変更
        }

        private void OnDrawGizmosSelected()
        {
            if (m_showCOM && m_vehicleRigitbody != null)
            {
                try
                {
                    m_radius = GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 30f;
                }
                catch { }

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(m_vehicleRigitbody.transform.TransformPoint(m_vehicleRigitbody.centerOfMass), m_radius);
            }
        }
    }
}
