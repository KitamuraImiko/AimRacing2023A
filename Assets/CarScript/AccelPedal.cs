/*------------------------------------------------------------------
* �t�@�C�����FAxelPedal.cs
* �T�v�F�y�_���̊֌W����̃N���X
* �S���ҁF�L�^����
* �쐬���F2023/06/02
-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelPedal : MonoBehaviour
{
    private AIM.WheelController wc = new AIM.WheelController(); //
    private AIM.WheelInput input;                               // ���͎擾�p
    [SerializeField] private float AccelDeskTopSee;
    // Start is called before the first frame update
    void Start()
    {
        // �g���ꏊ��I��
        input = transform.root.GetComponent<AIM.VehicleController>().wheelInput;
    }

    // Update is called once per frame
    void Update()
    {
        // �A�N�Z���̓��݋��8���𒴂���Ƃ�
        AccelPower();
    }

    private void AccelPower()
    {
        AccelDeskTopSee = input.inputAxel;
        // �A�N�Z���̓��݋�� 0.8f �𒴂���Ƃ�
        if (input.inputAxel >= 0.8f)
        {

        }
    }
}