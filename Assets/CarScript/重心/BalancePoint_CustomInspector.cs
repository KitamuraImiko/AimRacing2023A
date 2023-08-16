/*------------------------------------------------------------------
* �t�@�C�����FBalancePoint_CustomInspector.cs
* �T�v�F�d�S�N���X�̃C���X�y�N�^�[�̃J�X�^��
* �S���ҁF�S�R�P��
* �쐬���F2022/06/24
-------------------------------------------------------------------*/
#if UNITY_EDITOR    // exe�t�@�C���������o���Ƃ��ɏ��O����悤��
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(BalancePoint)), CanEditMultipleObjects]
public class BalancePoint_CustomInspector : Editor
{
    BalancePoint balancePoint = null;
    float screenWidth = 200;

    public void OnEnable()
    {
        balancePoint = target as BalancePoint;
    }

    //������������������������������������������
    // �ϐ����G�f�B�^���ɕ\��������֐�
    //������������������������������������������
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space(5);

        #region ���x�E�ړ�����
        //balancePoint.showParam[0] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[0], "���x�E�ړ������iReadOnly�j");
        //if (balancePoint.showParam[0])
        {
            EditorGUILayout.FloatField("�O�ւ̑��x(�O��)", balancePoint.prevForwardSpeed);
            EditorGUILayout.FloatField("�O�ւ̑��x", balancePoint.forwardSpeed);
            EditorGUILayout.Vector3Field("�ړ�����(�O��)", balancePoint.prevDirection);
            EditorGUILayout.Vector3Field("�ړ�����", balancePoint.direction);
            EditorGUILayout.Slider("�㏸�ʁi�O��j", balancePoint.prevAscend, -2, 2);
            EditorGUILayout.Slider("�㏸��", balancePoint.ascend, -2, 2);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region �����x�E�d�SZ
        //balancePoint.showParam[1] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[1], "�����x�E�d�SZ�iReadOnly�j");
        //if (balancePoint.showParam[1])
        {
            EditorGUILayout.Slider("�����x(�O��)", balancePoint.prevAcceleration, -5, 5);
            EditorGUILayout.Slider("�����x", balancePoint.acceleration, -5, 5);
            EditorGUILayout.Slider("�d�SZ(�}�C�i�X)", balancePoint.balancePoint_z, -5, 5);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region �萔
        //balancePoint.showParam[2] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[2], "�萔");
        //if (balancePoint.showParam[2])
        {
            balancePoint.framesBetweenUpdate = EditorGUILayout.Slider("�l���X�V����Ԋu", balancePoint.framesBetweenUpdate, 1, 60);
            balancePoint.movePercent_z = EditorGUILayout.Slider("�d�S�̈ړ��p�[�Z���g�i���j", balancePoint.movePercent_z, 0, 100);
            balancePoint.movePercent_x = EditorGUILayout.Slider("�d�S�̈ړ��p�[�Z���g�i���j", balancePoint.movePercent_x, 0, 100);
            balancePoint.speedBalancePointReposition.y = EditorGUILayout.Slider("�d�S���ʂ̑��x(z)", balancePoint.speedBalancePointReposition.y, 0, 1);
            balancePoint.speedBalancePointReposition.x = EditorGUILayout.Slider("�d�S���ʂ̑��x(x)", balancePoint.speedBalancePointReposition.x, 0, 1);
            balancePoint.speedBalancePointRepositionWhenBreak.y = EditorGUILayout.Slider("�Ԃ��~�܂�Ƃ��A�d�S���ʂ̑��x(z)", balancePoint.speedBalancePointRepositionWhenBreak.y, 0, 1);
            balancePoint.speedBalancePointRepositionWhenBreak.x = EditorGUILayout.Slider("�Ԃ��~�܂�Ƃ��A�d�S���ʂ̑��x(x)", balancePoint.speedBalancePointRepositionWhenBreak.x, 0, 1);
            balancePoint.ascendThreshold = EditorGUILayout.Slider("�d�S�㉺�ړ���臒l", balancePoint.ascendThreshold, 0, 1);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region �W��
        //balancePoint.showParam[3] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[3], "�W��");
        //if (balancePoint.showParam[3])
        {
            balancePoint.farwordCoefficient = EditorGUILayout.Slider("�O��", balancePoint.farwordCoefficient, 0, 10);
            balancePoint.rightCoefficient = EditorGUILayout.Slider("���E", balancePoint.rightCoefficient, 0, 10);
            balancePoint.upCoefficient = EditorGUILayout.Slider("�㉺", balancePoint.upCoefficient, 0, 10);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region �d�S�̍X�V�p(Z)
        //balancePoint.showParam[4] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[4], "�d�S�̍X�V�p(Z)�iReadOnly�j");
        //if (balancePoint.showParam[4])
        {
            EditorGUILayout.Toggle("�d�S���ړ����Ă���", balancePoint.bIsAccelerationChange);
            EditorGUILayout.Slider("�ő�l", balancePoint.extremeValue_z, -5, 5);
            EditorGUILayout.Slider("�ő�l�܂ł̍�", balancePoint.differenceExtremeValue_z, 0, 10);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region �d�S�̍X�V�p(X)
        //balancePoint.showParam[5] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[5], "�d�S�̍X�V�p(X)�iReadOnly�j");
        //if (balancePoint.showParam[5])
        {
            EditorGUILayout.Toggle("�d�S���ړ����Ă���", balancePoint.bIsBalancePointXChange);
            EditorGUILayout.Slider("�ő�l", balancePoint.extremeValue_x, -5, 5);
            EditorGUILayout.Slider("�ő�l�܂ł̍�", balancePoint.differenceExtremeValue_x, 0, 10);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region �d�S�̃O���t
        //balancePoint.showParam[6] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[6], "�d�S�iReadOnly�j");
        //if (balancePoint.showParam[6])
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
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region �Ԃ̉�]��ړ�
        //balancePoint.showParam[7] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[7], "�Ԃ̉�]");
        //if (balancePoint.showParam[7])
        {
            balancePoint.rotateMax.y = EditorGUILayout.Slider("��]�p�x�̏��(z)", balancePoint.rotateMax.y, 0, 10);
            balancePoint.rotateMax.x = EditorGUILayout.Slider("��]�p�x�̏��(x)", balancePoint.rotateMax.x, 0, 10);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        EditorUtility.SetDirty(balancePoint);
    }
}
#endif