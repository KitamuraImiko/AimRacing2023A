/*------------------------------------------------------------------
* �t�@�C�����FUtility
* �T�v�F���[�e�B���e�B�N���X
* �S���ҁF�ʈ�Ȃ�
* �쐬���F2022/5/13
-------------------------------------------------------------------*/
//�X�V����
/*
* 
*/

//-----------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace AIM
{
    public static class Utility
    {
        //Logotech SDK Setting�Ŏg�p
        // �f�b�h�]�[���v�Z
        public static bool IsDeadzoneZero(this float value, float round)
        {
            return (value > -round && value < round);
        }

		/// <summary>
		/// Avoid using at runtime.
		/// Run it in Start or Awake and cache the result.
		/// </summary>
		public static VehicleController FindRootVehicle(Transform transform)
		{
			if (transform == null) return null;

			if (transform.GetComponent<VehicleController>())
			{
				return transform.GetComponent<VehicleController>();
			}
			else if (transform.parent != null)
			{
				return FindRootVehicle(transform.parent);
			}
			else
			{
				return null;
			}
		}

		public static bool IsDeadzoneZero(this float value)
		{
			return (value > -0.0001f && value < 0.0001f);
		}

		public static float Round(this float value, int digit)
		{
			if (digit < 0 && digit >= 7) { return value; }

			value *= Mathf.Pow(10.0f, (float)digit);
			return Mathf.Round(value) / Mathf.Pow(10.0f, (float)digit);
		}
	}
}
