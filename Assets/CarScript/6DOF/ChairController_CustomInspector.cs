/*------------------------------------------------------------------
* �t�@�C�����FBalancePoint_CustomInspector.cs
* �T�v�FChairController�N���X�̃C���X�y�N�^�[�̃J�X�^��
* �S���ҁF�S�R�P��
* �쐬���F2022/07/15
-------------------------------------------------------------------*/
#if UNITY_EDITOR   // exe�t�@�C���������o���Ƃ��ɏ��O����悤��
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(ChairController)), CanEditMultipleObjects]
public class ChairController_CustomInspector : Editor
{
    ChairController chairController = null;
    BalancePoint balancePoint = null;
    float screenWidth = 200;

    public void OnEnable()
    {
        chairController = target as ChairController;
        balancePoint = chairController.transform.root.gameObject.transform.Find("carbody").GetComponent<BalancePoint>();
    }

    //������������������������������������������
    // �ϐ����G�f�B�^���ɕ\��������֐�
    //������������������������������������������
    public override void OnInspectorGUI()
    {
        #region �d�S�̐}��`��
        chairController.showGraph = EditorGUILayout.BeginFoldoutHeaderGroup(chairController.showGraph, "�d�S�iReadOnly�j");
        if (chairController.showGraph)
        {
            screenWidth = EditorGUILayout.Slider("�O���t�̃T�C�Y", screenWidth, 50, 400);

            // �l�p�`�̃G���A���m��
            Rect area = GUILayoutUtility.GetRect(screenWidth + 60, screenWidth);

            // ���S���W���擾
            Vector2 center = area.center;
            center.x -= 30;

            // �l�p�`�̂�\��
            Vector2 temp = center;
            temp.x -= screenWidth / 2;
            temp.y -= screenWidth / 2;
            Rect rect = new Rect(temp, new Vector2(screenWidth, screenWidth));
            Handles.DrawSolidRectangleWithOutline(rect, Color.gray, Color.black);

            // �F�����ɂ��A��������
            if (balancePoint._balancePoint.z == 0) Handles.color = new Color(0, 0.8f, 0);
            else Handles.color = new Color(0, 0.3f, 0);
            Handles.DrawLine(new Vector3(center.x - screenWidth / 2, center.y),
                             new Vector3(center.x + screenWidth / 2, center.y));

            if (balancePoint._balancePoint.x == 0) Handles.color = new Color(0, 0.8f, 0);
            else Handles.color = new Color(0, 0.3f, 0);
            Handles.DrawLine(new Vector3(center.x, center.y - screenWidth / 2),
                             new Vector3(center.x, center.y + screenWidth / 2));

            // �d�S�̈ʒu��\���~�̈ʒu���v�Z���A�Ԃŕ`��
            Vector2 pos = center;
            pos.x += balancePoint._balancePoint.x / 5 * screenWidth / 2;
            pos.y -= balancePoint._balancePoint.z / 5 * screenWidth / 2;
            Handles.color = Color.green;
            Handles.DrawSolidDisc(pos, Vector3.forward, 10f);

            center.x += screenWidth / 2 + 10 + 25;

            Handles.color = Color.white;

            // �l�p�`�̂�\��
            temp = center;
            temp.x -= 25;
            temp.y -= screenWidth / 2;
            rect = new Rect(temp, new Vector2(50, screenWidth));
            Handles.DrawSolidRectangleWithOutline(rect, Color.gray, Color.black);

            // �F��΂ɂ��A��������
            if (balancePoint._balancePoint.y == 0) Handles.color = new Color(0, 0.8f, 0);
            else Handles.color = new Color(0, 0.3f, 0);
            Handles.DrawLine(new Vector3(center.x - 25, center.y),
                             new Vector3(center.x + 25, center.y));

            // �d�S�̈ʒu��\���~�̈ʒu���v�Z���A�Ԃŕ`��
            pos = center;
            pos.y -= balancePoint._balancePoint.y / 5 * screenWidth / 2;
            Handles.color = Color.green;
            Handles.DrawSolidDisc(pos, Vector3.forward, 10f);

            EditorGUILayout.Space(5);
            EditorGUILayout.Vector3Field("�d�S", balancePoint._balancePoint);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region �f�o�b�O�p�l��\��
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "�f�o�b�O�p(ReadOnly)");
        EditorGUILayout.Toggle("�֎q�ɂȂ�����", chairController.bIsConnected);
        EditorGUILayout.Toggle("�ً}��~��", chairController.bEmergencyShutdown);
        
        EditorGUILayout.Space(10);

        EditorGUILayout.Slider("�ڕW�l�i���j", chairController._targetPos.x, -1, 1);
        EditorGUILayout.Slider("�ڕW�l�i���j", chairController._targetPos.y, -1, 1);
        EditorGUILayout.Slider("�ڕW�l�i���j", chairController._targetPos.z, -1, 1);
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        #endregion

        #region �֎q�ɓ����Ă���l��\��
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "�֎q�ɓ����Ă���l(ReadOnly)");
        if (chairController.controller)
        {
            EditorGUILayout.Slider("�O��", chairController.controller.pitch, -1, 1);
            EditorGUILayout.Slider("���E", chairController.controller.roll, -1, 1);
            EditorGUILayout.Slider("�㉺", chairController.controller.heave, -1, 1);
        }
        else
        {
            EditorGUILayout.Slider("�O��", 0, -1, 1);
            EditorGUILayout.Slider("���E", 0, -1, 1);
            EditorGUILayout.Slider("�㉺", 0, -1, 1);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);
        #endregion

        #region �֎q�̓����𒲐�����p
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "�֎q�̓����𒲐�����W��");
        chairController.farwordCoefficient = EditorGUILayout.Slider("�O��", chairController.farwordCoefficient, 0, 5);
        chairController.rightCoefficient = EditorGUILayout.Slider("���E", chairController.rightCoefficient, 0, 5);
        chairController.upCoefficient = EditorGUILayout.Slider("�㉺", chairController.upCoefficient, 0, 5);
        EditorGUILayout.Space(10);

        chairController.accelCoefficient = EditorGUILayout.Slider("�A�N�Z��", chairController.accelCoefficient, 0, 5);
        chairController.brakeCoefficient = EditorGUILayout.Slider("�u���[�L", chairController.brakeCoefficient, 0, 5);
        chairController.shiftUpCoefficient = EditorGUILayout.Slider("�V�t�g�A�b�v", chairController.shiftUpCoefficient, 0, 5);
        chairController.shiftDownCoefficient = EditorGUILayout.Slider("�V�t�g�_�E��", chairController.shiftDownCoefficient, 0, 5);
        EditorGUILayout.Space(10);

        chairController.speedCoefficient.z = EditorGUILayout.Slider("���x�i�O��j", chairController.speedCoefficient.z, 0, 5);
        chairController.speedCoefficient.x = EditorGUILayout.Slider("���x�i���E�j", chairController.speedCoefficient.x, 0, 5);
        chairController.speedCoefficient.y = EditorGUILayout.Slider("���x�i�㉺�j", chairController.speedCoefficient.y, 0, 5);
        EditorGUILayout.Space(10);
        
        chairController.maxRotate = EditorGUILayout.Slider("��]�p�x�̏��", chairController.maxRotate, 0, 1);
        chairController.impactCoefficient.y = EditorGUILayout.Slider("�Ռ��̌W���i�O��j", chairController.impactCoefficient.y, 0, 5);
        chairController.impactCoefficient.x = EditorGUILayout.Slider("�Ռ��̌W���i���E�j", chairController.impactCoefficient.x, 0, 5);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("��������������������������������������������������������������������������������������������������������");
        chairController.OverallTravelFactor = EditorGUILayout.Slider("�֎q�S�̂̈ړ��ʂ̌W��", chairController.OverallTravelFactor, 0, 5);
        chairController.OverallSpeedFactor = EditorGUILayout.Slider("�֎q�S�̂̑��x�̌W��", chairController.OverallSpeedFactor, 0, 5);
        EditorGUILayout.LabelField("��������������������������������������������������������������������������������������������������������");
        EditorGUILayout.Space(10);

        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        #endregion

        #region �ق��̃p�����[�^
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "�ق��̃p�����[�^");
        chairController.zAxisThreshold = EditorGUILayout.IntSlider("Z���̃t���[������臒l", chairController.zAxisThreshold, 0, 10);
        chairController.RollingRandomRange = EditorGUILayout.Slider("�U���̃����_���͈�", chairController.RollingRandomRange, 0, 1);
        chairController.movePercent.x = EditorGUILayout.Slider("�ړ��̃p�[�Z���e�[�W(x)", chairController.movePercent.x, 0, 100);
        chairController.movePercent.y = EditorGUILayout.Slider("�ړ��̃p�[�Z���e�[�W(y)", chairController.movePercent.y, 0, 100);
        chairController.movePercent.z = EditorGUILayout.Slider("�ړ��̃p�[�Z���e�[�W(z)", chairController.movePercent.z, 0, 100);
        chairController.returnPercent.x = EditorGUILayout.Slider("�߂�̃p�[�Z���e�[�W(x)", chairController.returnPercent.x, 0, 100);
        chairController.returnPercent.y = EditorGUILayout.Slider("�߂�̃p�[�Z���e�[�W(y)", chairController.returnPercent.y, 0, 100);
        chairController.returnPercent.z = EditorGUILayout.Slider("�߂�̃p�[�Z���e�[�W(z)", chairController.returnPercent.z, 0, 100);
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);

        chairController.rollingCurve = EditorGUILayout.CurveField("�U���̕p�x", chairController.rollingCurve);
        #endregion

        #region �Ԃ�������̏����p
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "�Ԃ�������̏����p");
        EditorGUILayout.Toggle("�Ԃ�����", chairController.bGotHit);
        EditorGUILayout.IntSlider("�J�E���^�[", chairController.hitCnt, 0, chairController.hitCntThreshold);
        chairController.hitCntThreshold = EditorGUILayout.IntSlider("�Ԃ�������̏����̎���", chairController.hitCntThreshold, 1, 60);
        EditorGUILayout.Slider("�q�b�g�������̑��x", chairController.hitSpeed, 0, 65);
        chairController.hitImpactCurve = EditorGUILayout.CurveField("�Ԃ�������֎q�̑��x", chairController.hitImpactCurve);
        chairController.accelerationAfterHit = EditorGUILayout.Slider("�Ԃ�������֎q�̉����x", chairController.accelerationAfterHit, 0, 1);
        chairController.maxSpeedAfterHit = EditorGUILayout.Slider("�Ԃ�������֎q�̍ő呬�x", chairController.maxSpeedAfterHit, 0, 1);

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(10);
        #endregion

        EditorUtility.SetDirty(chairController);
    }
}
#endif