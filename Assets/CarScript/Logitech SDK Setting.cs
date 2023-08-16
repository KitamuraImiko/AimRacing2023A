/*------------------------------------------------------------------
* �t�@�C�����FLogitech SDK Setting
* �T�v�FFreeAsset�uLogicoolSDK�v���g�����A�n���h���R���g���[���[�ł̓��͂��Ǘ�����
* �S���ҁF�ʈ�Ȃ�
* �쐬���F2022/5/13
-------------------------------------------------------------------*/
//�X�V����
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
		// �ŏ��̃t���[���X�V�̑O�ɌĂяo�����
		void Start()
		{
			//�������֐�
			LogitechGSDK.LogiSteeringInitialize(false);
		}

		// ���l�␳
		public bool InputUpdate()
		{
			if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
			{
				LogitechGSDK.DIJOYSTATE2ENGINES rec;
				rec = LogitechGSDK.LogiGetStateUnity(0);

				// �n���h��
				// ���̐��l��-32767�`32767
				// �����ȓ��͒l��0�ɕ␳���A-1�`1�͈̔͂ɐݒ�
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

				// �y�_��
				// ���̐��l��-32767�`32767
				// �n���h���ƈႢ�A0�`1�͈̔͂ň��������̂ŁA
				// �܂�-1�`1�ɕ␳���A1�𑫂���0�`2�ɂ���B
				// ���̌�A�����ɂ��鎖��0�`1�ɂ���B
				inputAxel = ((float)rec.lY / (float)-Int16.MaxValue + 1.0f) * 0.5f;
				inputBrake = ((float)rec.lRz / (float)-Int16.MaxValue + 1.0f) * 0.5f;
				inputClutch = ((float)rec.rglSlider[0] / (float)-Int16.MaxValue + 1.0f) * 0.5f;

				// �����ȓ��͒l��0�ɕ␳
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
		// 1�t���[����1��Ăяo�����
		void Update()
		{
			//�R���g���[���[�ڑ��`�F�b�N
			//�R���g���[���[�ڑ����ŐV�ɐݒ�
			if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
			{
				// �����O(2022/5/13)
				Debug.Log("Connection Success Logitech SDK.");
				Debug.Log("inputHandle:" + inputHandle + "\n" + "inputAxel:" + inputAxel 
					+ "\n" + "inputBrake:" + inputBrake + "\n" + "inputClutch:" + inputClutch);
			}
			//���ڑ��̏ꍇ�̓f�o�b�O���O��\��
			else
			{
				Debug.Log("Not Connected Logitech SDK.");
			}
		}

		// �A�v���P�[�V�������I������O�ɌĂяo�����
		private void OnApplicationQuit()
		{
			// SDK ���V���b�g�_�E�����A�R���g���[���I�u�W�F�N�g��j��
			LogitechGSDK.LogiSteeringShutdown();
		}
	}
}
