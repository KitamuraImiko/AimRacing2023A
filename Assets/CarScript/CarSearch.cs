// ��y���̃R�[�h CarSearch.cs ���Q�l

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSearch : MonoBehaviour
{
	
	[SerializeField] string carName;					// �����A�Z�b�g��
	[SerializeField] string tagName;					// �����^�O��

	public GameObject carObject;						// �ԏ����i�[
	public AIM.VehicleController vc;					// VehicleController �����i�[
	public bool bIsCar;									// �Ԃ��i�[����Ă��邩
	
	public bool bIsDesktopInputManager;                 // DesktopInputManager�p�ڑ��Ǘ��t���O

	// Update is called once per frame
	void Update()
	{
		// �Ԃ����łɊi�[����Ă���ꍇ
		if (bIsCar)
        {
			// ���������Ȃ�
			return;
		}

		// �A�Z�b�g���Ō���
		if (GameObject.Find(carName))
		{
			// �Ԃ̊e�����i�[
			carObject = GameObject.Find(carName);
			vc = carObject.GetComponent<AIM.VehicleController>();
			bIsCar = true;
		}
		else
		{
			// �^�O���Ō���
			if (GameObject.FindGameObjectWithTag(tagName))
			{
				// �Ԃ̊e�����i�[
				carObject = GameObject.FindGameObjectWithTag(tagName);
				vc = carObject.GetComponent<AIM.VehicleController>();
				bIsCar = true;
			}
			else
			{
				// ������Ȃ��ꍇ���O���o��
				Debug.Log("�Ԃ�������܂���B");
				return;
			}
		}
	}
}
