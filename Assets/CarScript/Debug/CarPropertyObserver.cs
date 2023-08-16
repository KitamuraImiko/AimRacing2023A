/*------------------------------------------------------------------
* �t�@�C�����FCarPropertyObserver.cs
* �T�v�F�Ԃ̊e��l���Ď��A�擾����N���X
* �S���ҁF�D�n�ʔT
* �쐬���F2023/08/08
-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIM;

public class CarPropertyObserver : MonoBehaviour
{
    #region �G�f�B�^�ɕ\������l
    [Header("ShowFlag")]
    // �Ԃ̑��x�����邩�ǂ���
    [SerializeField] private bool bShowCarSpeed = true;

    // �A�N�Z���̓��͗ʂ����邩�ǂ���
    [SerializeField] private bool bShowAxelPower = true;

    // �u���[�L�̓��͗ʂ����邩�ǂ���
    [SerializeField] private bool bShowBrakePower = true;

    // �Ԃ̓����蔻������邩�ǂ���
    [SerializeField] private bool bShowCarWallHit = true;

    [Space(10)]
    [Header("Check")]
    // �l�̊m�F�Ԋu
    [SerializeField] private int CheckFrequency = 0;

    // �Q�Ƃ���I�u�W�F�N�g
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject FCollider;

    #endregion

    #region �l��ۑ�����ϐ�
    // �l�̃J�E���g
    int CheakCount = 0;

    // �Ԃ̑��x
    Vector3 CarSpeed;

    Vector3 GetCarSpeed
    {
        get { return CarSpeed; }
    }

    // �A�N�Z���̓��͗�
    float AxelInput;
    float GetAxelInput
    {
        get { return AxelInput; }
    }

    // �u���[�L�̓��͗�
    float BrakeInput;
    float GetBrakeInput
    {
        get { return BrakeInput; }
    }

    // �E�̕ǂɓ����������̃t���O
    bool RightWallHit;
    bool GetRightWallHit
    {
        get { return RightWallHit; }
    }

    // ���̕ǂɓ����������̃t���O
    bool LeftWallHit;
    bool GetLeftWallHit
    {
        get { return LeftWallHit; }
    }
    #endregion

    #region �Q�Ƃ���N���X
    private Rigidbody CarRigidBody;
    private FrontCollider FrontCollider;
    private WheelInput WheelInput;
    private VehicleController VehicleController;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        // ������
        CarRigidBody = root.GetComponent<Rigidbody>();
        FrontCollider = FCollider.GetComponent<FrontCollider>();
        VehicleController = root.GetComponent<VehicleController>();
        WheelInput = VehicleController.GetWheelInput;
    }

    // Update is called once per frame
    void Update()
    {
        // �J�E���^�[���`�F�b�N�p�x�𒴂��Ă�����
        if (CheakCount >= CheckFrequency)
        {
            // �J�E���^�[�����Z�b�g����
            CheakCount = 0;

            // �e��l���擾����
            CarSpeed = CarRigidBody.velocity;
            AxelInput = WheelInput.GetInputAxel;
            BrakeInput = WheelInput.GetInputBreke;
            RightWallHit = FrontCollider.GetRightHit;
            LeftWallHit = FrontCollider.GetleftHit;

            // �e��l��\������
            if (bShowCarSpeed) Debug.Log($"OB_CarSpeed : {CarSpeed}");
            if (bShowAxelPower) Debug.Log($"OB_AxelInput : {AxelInput}");
            if (bShowBrakePower) Debug.Log($"OB_BrakeInput : {BrakeInput}");
            if (bShowCarWallHit)
            {
                Debug.Log($"OB_RightWallHit : {RightWallHit}");

                Debug.Log($"OB_LeftWallHit : {LeftWallHit}");
            }
        }
    }
}
