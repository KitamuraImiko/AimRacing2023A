//===================================================
// ファイル名	：DriftMnager.cs
// 概要			：ドリフト実行スクリプト
// 作成者		：東樹　潤弥
// 作成日		：2020.07.31
// 
//---------------------------------------------------
// 更新履歴：
// 2020/07/31 [東樹 潤弥] スクリプト作成
// 2020/08/05 [東樹 潤弥] ドリフト割合計算を追加
// 2020/08/06 [東樹 潤弥] ドリフト割合を使って、サイドブレーキ処理
// 2020/08/18 [東樹 潤弥] ドリフト角度維持の為に、回転減衰処理追加
// 2020/08/20 [東樹 潤弥] ドリフト割合と回転減衰の式を変更
// 2020/09/03 [東樹 潤弥] クラスを分割
//===================================================

using UnityEngine;

namespace AIM
{
    [System.Serializable]
    public class DriftManager
    {
        private VehicleController vc;

        public bool isEnable;
        public float handle;

        public DriftSideBrake dSideBrake = new DriftSideBrake();
        public DriftBraking dBraking = new DriftBraking();
        public DriftSlide dSlide = new DriftSlide();
        public DriftInertial dInertial = new DriftInertial();

        //=================================================================================
        public void Initialize(VehicleController vc_)
        {
            vc = vc_;
            dSideBrake.Initialize(vc_);
            dBraking.Initialize(vc_);
            dSlide.Initialize(vc_);
            dInertial.Initialize(vc_);
        }
        //=================================================================================
        public void Update()
        {
            if (dSideBrake.isEnable) { dSideBrake.Update(); }
            if (dBraking.isEnable) { dBraking.Update(); }
            if(dSlide.isEnable){ dSlide.Update(); }
			//if (dInertial.isEnable) { dInertial.Update(); }
        }
    }
}