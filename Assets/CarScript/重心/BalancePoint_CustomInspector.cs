/*------------------------------------------------------------------
* ファイル名：BalancePoint_CustomInspector.cs
* 概要：重心クラスのインスペクターのカスタム
* 担当者：ゴコケン
* 作成日：2022/06/24
-------------------------------------------------------------------*/
#if UNITY_EDITOR    // exeファイルを書き出すときに除外するように
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

    //━━━━━━━━━━━━━━━━━━━━━
    // 変数をエディタ側に表示させる関数
    //━━━━━━━━━━━━━━━━━━━━━
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space(5);

        #region 速度・移動方向
        //balancePoint.showParam[0] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[0], "速度・移動方向（ReadOnly）");
        //if (balancePoint.showParam[0])
        {
            EditorGUILayout.FloatField("前への速度(前回)", balancePoint.prevForwardSpeed);
            EditorGUILayout.FloatField("前への速度", balancePoint.forwardSpeed);
            EditorGUILayout.Vector3Field("移動方向(前回)", balancePoint.prevDirection);
            EditorGUILayout.Vector3Field("移動方向", balancePoint.direction);
            EditorGUILayout.Slider("上昇量（前回）", balancePoint.prevAscend, -2, 2);
            EditorGUILayout.Slider("上昇量", balancePoint.ascend, -2, 2);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 加速度・重心Z
        //balancePoint.showParam[1] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[1], "加速度・重心Z（ReadOnly）");
        //if (balancePoint.showParam[1])
        {
            EditorGUILayout.Slider("加速度(前回)", balancePoint.prevAcceleration, -5, 5);
            EditorGUILayout.Slider("加速度", balancePoint.acceleration, -5, 5);
            EditorGUILayout.Slider("重心Z(マイナス)", balancePoint.balancePoint_z, -5, 5);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 定数
        //balancePoint.showParam[2] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[2], "定数");
        //if (balancePoint.showParam[2])
        {
            balancePoint.framesBetweenUpdate = EditorGUILayout.Slider("値を更新する間隔", balancePoint.framesBetweenUpdate, 1, 60);
            balancePoint.movePercent_z = EditorGUILayout.Slider("重心の移動パーセント（ｚ）", balancePoint.movePercent_z, 0, 100);
            balancePoint.movePercent_x = EditorGUILayout.Slider("重心の移動パーセント（ｘ）", balancePoint.movePercent_x, 0, 100);
            balancePoint.speedBalancePointReposition.y = EditorGUILayout.Slider("重心復位の速度(z)", balancePoint.speedBalancePointReposition.y, 0, 1);
            balancePoint.speedBalancePointReposition.x = EditorGUILayout.Slider("重心復位の速度(x)", balancePoint.speedBalancePointReposition.x, 0, 1);
            balancePoint.speedBalancePointRepositionWhenBreak.y = EditorGUILayout.Slider("車が止まるとき、重心復位の速度(z)", balancePoint.speedBalancePointRepositionWhenBreak.y, 0, 1);
            balancePoint.speedBalancePointRepositionWhenBreak.x = EditorGUILayout.Slider("車が止まるとき、重心復位の速度(x)", balancePoint.speedBalancePointRepositionWhenBreak.x, 0, 1);
            balancePoint.ascendThreshold = EditorGUILayout.Slider("重心上下移動の閾値", balancePoint.ascendThreshold, 0, 1);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 係数
        //balancePoint.showParam[3] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[3], "係数");
        //if (balancePoint.showParam[3])
        {
            balancePoint.farwordCoefficient = EditorGUILayout.Slider("前後", balancePoint.farwordCoefficient, 0, 10);
            balancePoint.rightCoefficient = EditorGUILayout.Slider("左右", balancePoint.rightCoefficient, 0, 10);
            balancePoint.upCoefficient = EditorGUILayout.Slider("上下", balancePoint.upCoefficient, 0, 10);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 重心の更新用(Z)
        //balancePoint.showParam[4] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[4], "重心の更新用(Z)（ReadOnly）");
        //if (balancePoint.showParam[4])
        {
            EditorGUILayout.Toggle("重心が移動している", balancePoint.bIsAccelerationChange);
            EditorGUILayout.Slider("最大値", balancePoint.extremeValue_z, -5, 5);
            EditorGUILayout.Slider("最大値までの差", balancePoint.differenceExtremeValue_z, 0, 10);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 重心の更新用(X)
        //balancePoint.showParam[5] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[5], "重心の更新用(X)（ReadOnly）");
        //if (balancePoint.showParam[5])
        {
            EditorGUILayout.Toggle("重心が移動している", balancePoint.bIsBalancePointXChange);
            EditorGUILayout.Slider("最大値", balancePoint.extremeValue_x, -5, 5);
            EditorGUILayout.Slider("最大値までの差", balancePoint.differenceExtremeValue_x, 0, 10);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 重心のグラフ
        //balancePoint.showParam[6] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[6], "重心（ReadOnly）");
        //if (balancePoint.showParam[6])
        {
            screenWidth = EditorGUILayout.Slider("グラフのサイズ", screenWidth, 50, 400);

            // 四角形のエリアを確保
            Rect area = GUILayoutUtility.GetRect(screenWidth + 60, screenWidth);

            // 中心座標を取得
            Vector2 center = area.center;
            center.x -= 30;

            // 四角形のを表示
            Vector2 temp = center;
            temp.x -= screenWidth / 2;
            temp.y -= screenWidth / 2;
            Rect rect = new Rect(temp, new Vector2(screenWidth, screenWidth));
            Handles.DrawSolidRectangleWithOutline(rect, Color.gray, Color.black);

            // 色を黒にし、線を引く
            if (balancePoint._balancePoint.z == 0) Handles.color = new Color(0, 0.8f, 0);
            else Handles.color = new Color(0, 0.3f, 0);
            Handles.DrawLine(new Vector3(center.x - screenWidth / 2, center.y),
                             new Vector3(center.x + screenWidth / 2, center.y));

            if (balancePoint._balancePoint.x == 0) Handles.color = new Color(0, 0.8f, 0);
            else Handles.color = new Color(0, 0.3f, 0);
            Handles.DrawLine(new Vector3(center.x, center.y - screenWidth / 2),
                             new Vector3(center.x, center.y + screenWidth / 2));

            // 重心の位置を表す円の位置を計算し、赤で描く
            Vector2 pos = center;
            pos.x += balancePoint._balancePoint.x / 5 * screenWidth / 2;
            pos.y -= balancePoint._balancePoint.z / 5 * screenWidth / 2;
            Handles.color = Color.green;
            Handles.DrawSolidDisc(pos, Vector3.forward, 10f);

            center.x += screenWidth / 2 + 10 + 25;

            Handles.color = Color.white;

            // 四角形のを表示
            temp = center;
            temp.x -= 25;
            temp.y -= screenWidth / 2;
            rect = new Rect(temp, new Vector2(50, screenWidth));
            Handles.DrawSolidRectangleWithOutline(rect, Color.gray, Color.black);

            // 色を緑にし、線を引く
            if (balancePoint._balancePoint.y == 0) Handles.color = new Color(0, 0.8f, 0);
            else Handles.color = new Color(0, 0.3f, 0);
            Handles.DrawLine(new Vector3(center.x - 25, center.y),
                             new Vector3(center.x + 25, center.y));

            // 重心の位置を表す円の位置を計算し、赤で描く
            pos = center;
            pos.y -= balancePoint._balancePoint.y / 5 * screenWidth / 2;
            Handles.color = Color.green;
            Handles.DrawSolidDisc(pos, Vector3.forward, 10f);

            EditorGUILayout.Space(5);
            EditorGUILayout.Vector3Field("重心", balancePoint._balancePoint);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region 車の回転や移動
        //balancePoint.showParam[7] = EditorGUILayout.BeginFoldoutHeaderGroup(balancePoint.showParam[7], "車の回転");
        //if (balancePoint.showParam[7])
        {
            balancePoint.rotateMax.y = EditorGUILayout.Slider("回転角度の上限(z)", balancePoint.rotateMax.y, 0, 10);
            balancePoint.rotateMax.x = EditorGUILayout.Slider("回転角度の上限(x)", balancePoint.rotateMax.x, 0, 10);
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        EditorUtility.SetDirty(balancePoint);
    }
}
#endif