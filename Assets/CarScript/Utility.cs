/*------------------------------------------------------------------
* ファイル名：Utility
* 概要：ユーティリティクラス
* 担当者：玉井なの
* 作成日：2022/5/13
-------------------------------------------------------------------*/
//更新履歴
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
        //Logotech SDK Settingで使用
        // デッドゾーン計算
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
