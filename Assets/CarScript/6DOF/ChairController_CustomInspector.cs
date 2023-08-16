/*------------------------------------------------------------------
* ファイル名：BalancePoint_CustomInspector.cs
* 概要：ChairControllerクラスのインスペクターのカスタム
* 担当者：ゴコケン
* 作成日：2022/07/15
-------------------------------------------------------------------*/
#if UNITY_EDITOR   // exeファイルを書き出すときに除外するように
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

    //━━━━━━━━━━━━━━━━━━━━━
    // 変数をエディタ側に表示させる関数
    //━━━━━━━━━━━━━━━━━━━━━
    public override void OnInspectorGUI()
    {
        #region 重心の図を描画
        chairController.showGraph = EditorGUILayout.BeginFoldoutHeaderGroup(chairController.showGraph, "重心（ReadOnly）");
        if (chairController.showGraph)
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
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        #region デバッグ用値を表示
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "デバッグ用(ReadOnly)");
        EditorGUILayout.Toggle("椅子につながった", chairController.bIsConnected);
        EditorGUILayout.Toggle("緊急停止中", chairController.bEmergencyShutdown);
        
        EditorGUILayout.Space(10);

        EditorGUILayout.Slider("目標値（ｘ）", chairController._targetPos.x, -1, 1);
        EditorGUILayout.Slider("目標値（ｙ）", chairController._targetPos.y, -1, 1);
        EditorGUILayout.Slider("目標値（ｚ）", chairController._targetPos.z, -1, 1);
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        #endregion

        #region 椅子に入っている値を表示
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "椅子に入っている値(ReadOnly)");
        if (chairController.controller)
        {
            EditorGUILayout.Slider("前後", chairController.controller.pitch, -1, 1);
            EditorGUILayout.Slider("左右", chairController.controller.roll, -1, 1);
            EditorGUILayout.Slider("上下", chairController.controller.heave, -1, 1);
        }
        else
        {
            EditorGUILayout.Slider("前後", 0, -1, 1);
            EditorGUILayout.Slider("左右", 0, -1, 1);
            EditorGUILayout.Slider("上下", 0, -1, 1);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);
        #endregion

        #region 椅子の動きを調整する用
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "椅子の動きを調整する係数");
        chairController.farwordCoefficient = EditorGUILayout.Slider("前後", chairController.farwordCoefficient, 0, 5);
        chairController.rightCoefficient = EditorGUILayout.Slider("左右", chairController.rightCoefficient, 0, 5);
        chairController.upCoefficient = EditorGUILayout.Slider("上下", chairController.upCoefficient, 0, 5);
        EditorGUILayout.Space(10);

        chairController.accelCoefficient = EditorGUILayout.Slider("アクセル", chairController.accelCoefficient, 0, 5);
        chairController.brakeCoefficient = EditorGUILayout.Slider("ブレーキ", chairController.brakeCoefficient, 0, 5);
        chairController.shiftUpCoefficient = EditorGUILayout.Slider("シフトアップ", chairController.shiftUpCoefficient, 0, 5);
        chairController.shiftDownCoefficient = EditorGUILayout.Slider("シフトダウン", chairController.shiftDownCoefficient, 0, 5);
        EditorGUILayout.Space(10);

        chairController.speedCoefficient.z = EditorGUILayout.Slider("速度（前後）", chairController.speedCoefficient.z, 0, 5);
        chairController.speedCoefficient.x = EditorGUILayout.Slider("速度（左右）", chairController.speedCoefficient.x, 0, 5);
        chairController.speedCoefficient.y = EditorGUILayout.Slider("速度（上下）", chairController.speedCoefficient.y, 0, 5);
        EditorGUILayout.Space(10);
        
        chairController.maxRotate = EditorGUILayout.Slider("回転角度の上限", chairController.maxRotate, 0, 1);
        chairController.impactCoefficient.y = EditorGUILayout.Slider("衝撃の係数（前後）", chairController.impactCoefficient.y, 0, 5);
        chairController.impactCoefficient.x = EditorGUILayout.Slider("衝撃の係数（左右）", chairController.impactCoefficient.x, 0, 5);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        chairController.OverallTravelFactor = EditorGUILayout.Slider("椅子全体の移動量の係数", chairController.OverallTravelFactor, 0, 5);
        chairController.OverallSpeedFactor = EditorGUILayout.Slider("椅子全体の速度の係数", chairController.OverallSpeedFactor, 0, 5);
        EditorGUILayout.LabelField("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        EditorGUILayout.Space(10);

        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        #endregion

        #region ほかのパラメータ
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "ほかのパラメータ");
        chairController.zAxisThreshold = EditorGUILayout.IntSlider("Z軸のフレーム数の閾値", chairController.zAxisThreshold, 0, 10);
        chairController.RollingRandomRange = EditorGUILayout.Slider("振動のランダム範囲", chairController.RollingRandomRange, 0, 1);
        chairController.movePercent.x = EditorGUILayout.Slider("移動のパーセンテージ(x)", chairController.movePercent.x, 0, 100);
        chairController.movePercent.y = EditorGUILayout.Slider("移動のパーセンテージ(y)", chairController.movePercent.y, 0, 100);
        chairController.movePercent.z = EditorGUILayout.Slider("移動のパーセンテージ(z)", chairController.movePercent.z, 0, 100);
        chairController.returnPercent.x = EditorGUILayout.Slider("戻るのパーセンテージ(x)", chairController.returnPercent.x, 0, 100);
        chairController.returnPercent.y = EditorGUILayout.Slider("戻るのパーセンテージ(y)", chairController.returnPercent.y, 0, 100);
        chairController.returnPercent.z = EditorGUILayout.Slider("戻るのパーセンテージ(z)", chairController.returnPercent.z, 0, 100);
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);

        chairController.rollingCurve = EditorGUILayout.CurveField("振動の頻度", chairController.rollingCurve);
        #endregion

        #region ぶつかった後の処理用
        EditorGUILayout.BeginFoldoutHeaderGroup(true, "ぶつかった後の処理用");
        EditorGUILayout.Toggle("ぶつかった", chairController.bGotHit);
        EditorGUILayout.IntSlider("カウンター", chairController.hitCnt, 0, chairController.hitCntThreshold);
        chairController.hitCntThreshold = EditorGUILayout.IntSlider("ぶつかった後の処理の時間", chairController.hitCntThreshold, 1, 60);
        EditorGUILayout.Slider("ヒットした時の速度", chairController.hitSpeed, 0, 65);
        chairController.hitImpactCurve = EditorGUILayout.CurveField("ぶつかった後椅子の速度", chairController.hitImpactCurve);
        chairController.accelerationAfterHit = EditorGUILayout.Slider("ぶつかった後椅子の加速度", chairController.accelerationAfterHit, 0, 1);
        chairController.maxSpeedAfterHit = EditorGUILayout.Slider("ぶつかった後椅子の最大速度", chairController.maxSpeedAfterHit, 0, 1);

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(10);
        #endregion

        EditorUtility.SetDirty(chairController);
    }
}
#endif