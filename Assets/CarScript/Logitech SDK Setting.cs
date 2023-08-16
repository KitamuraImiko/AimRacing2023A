/*------------------------------------------------------------------
* ファイル名：Logitech SDK Setting
* 概要：FreeAsset「LogicoolSDK」を使った、ハンドルコントローラーでの入力を管理する
* 担当者：玉井なの
* 作成日：2022/5/13
-------------------------------------------------------------------*/
//更新履歴
/*
* 
*/

//-----------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Text;

namespace AIM
{
	public class LogitechSDKSetting : MonoBehaviour
	{
		[Space]
		public float inputHandle;
		public float inputAxel;
		public float inputBrake;
		public float inputClutch;
		[Space]
		public float deadzoneZero_handlePer;
		public float deadzoneZero_handle;
		public float deadzoneZero_pedalPer;
		public float deadzoneZero_pedal;

		// Start is called before the first frame update
		// 最初のフレーム更新の前に呼び出される
		void Start()
		{
			//初期化関数
			LogitechGSDK.LogiSteeringInitialize(false);
		}

		// 数値補正
		public bool InputUpdate()
		{
			if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
			{
				LogitechGSDK.DIJOYSTATE2ENGINES rec;
				rec = LogitechGSDK.LogiGetStateUnity(0);

				// ハンドル
				// 元の数値は-32767～32767
				// 微少な入力値を0に補正しつつ、-1～1の範囲に設定
				if (rec.lX > 0)
				{
					inputHandle = ((float)rec.lX - deadzoneZero_handle) / ((float)Int16.MaxValue - deadzoneZero_handle);
					inputHandle = Mathf.Clamp(inputHandle, 0.0f, 1.0f);
				}
				else if (rec.lX < 0)
				{
					inputHandle = ((float)rec.lX + deadzoneZero_handle) / ((float)Int16.MaxValue - deadzoneZero_handle);
					inputHandle = Mathf.Clamp(inputHandle, -1.0f, 0.0f);
				}
				else
				{
					inputHandle = 0.0f;
				}

				// ペダル
				// 元の数値は-32767～32767
				// ハンドルと違い、0～1の範囲で扱いたいので、
				// まず-1～1に補正し、1を足して0～2にする。
				// その後、半分にする事で0～1にする。
				inputAxel = ((float)rec.lY / (float)-Int16.MaxValue + 1.0f) * 0.5f;
				inputBrake = ((float)rec.lRz / (float)-Int16.MaxValue + 1.0f) * 0.5f;
				inputClutch = ((float)rec.rglSlider[0] / (float)-Int16.MaxValue + 1.0f) * 0.5f;

				// 微少な入力値を0に補正
				if (inputAxel.IsDeadzoneZero(deadzoneZero_pedalPer)) { inputAxel = 0.0f; }
				else { inputAxel = Mathf.Clamp(inputAxel, 0.0f, 1.0f); }

				if (inputBrake.IsDeadzoneZero(deadzoneZero_pedalPer)) { inputBrake = 0.0f; }
				else { inputBrake = Mathf.Clamp(inputBrake, 0.0f, 1.0f); }

				if (inputClutch.IsDeadzoneZero(deadzoneZero_pedalPer)) { inputClutch = 0.0f; }
				else { inputClutch = Mathf.Clamp(inputClutch, 0.0f, 1.0f); }

				return true;
			}
			else
			{
				return false;
			}
		}

		// Update is called once per frame
		// 1フレームに1回呼び出される
		void Update()
		{
			//コントローラー接続チェック
			//コントローラー接続を最新に設定
			if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
			{
				// 仮ログ(2022/5/13)
				Debug.Log("Connection Success Logitech SDK.");
				Debug.Log("inputHandle:" + inputHandle + "\n" + "inputAxel:" + inputAxel 
					+ "\n" + "inputBrake:" + inputBrake + "\n" + "inputClutch:" + inputClutch);
			}
			//未接続の場合はデバッグログを表示
			else
			{
				Debug.Log("Not Connected Logitech SDK.");
			}
		}

		// アプリケーションが終了する前に呼び出される
		private void OnApplicationQuit()
		{
			// SDK をシャットダウンし、コントローラオブジェクトを破棄
			LogitechGSDK.LogiSteeringShutdown();
		}
	}
}
