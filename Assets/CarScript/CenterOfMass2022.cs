//---------------------------------------------
// �쐬�ҁF20CU0232 �����a��
// �쐬���F2022/07/30
//
//---------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
    public class CenterOfMass2022 : MonoBehaviour
    {
        private GameObject m_vehicle;               //�Ԃ�object
        private Rigidbody m_vehicleRigitbody;
        [SerializeField] Transform m_centerOfMass;    // �d�S�ʒu

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
            //m_vehicleRigitbody.centerOfMass = m_centerOfMass.localPosition;    // �d�S�ʒu��ύX
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
