using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
    [System.Serializable]
    public class DriftBraking
    {
        public bool isEnable;
        
        [Range(0.0f, 30.0f)]
        [SerializeField] private float reverseSteerLerpCoeff;

        [Tooltip("Time：ハンドル割合の絶対値(0～1)    Value：後輪のSteerCoef")]
        public AnimationCurve reverseSteerCurve;

        [SerializeField]public float driftHandleRatio;
        
        [Tooltip("Time：荷重移動の割合（後輪）    Value：後輪のSteerCoefの反映度")]
        [SerializeField] private AnimationCurve gripSteerCurve;

        VehicleController vc;
        
        public float moveMax;
        private float reverseSpeed = 70.0f;

        //=================================================================================
        // 初期化
        public void Initialize(VehicleController vc_)
        {
            vc = vc_;
        }
        //=================================================================================
        // 更新
        public void Update()
        {
            AddSlipMove();
        }
        //=================================================================================
        // 後輪を反対に向けることで強制的に滑らせる
        public float ReverseSteer(float currentSteer)
        {
            // 滑りすぎ防止のため、一時的に荷重移動の影響をカット
            return Mathf.Lerp(currentSteer, 
                              reverseSteerCurve.Evaluate(Mathf.Abs(vc.input.Horizontal)) 
                              * Mathf.Clamp01(vc.SpeedKPH / reverseSpeed)/* - gripSteerCurve.Evaluate(vc.loadCom.forceCoef_R)*/,
                              reverseSteerLerpCoeff * Time.deltaTime);
        }
        //=================================================================================
        private void AddSlipMove()
        {
            // 車体の位置調整
            vc.rb.AddForce(vc.transform.right * moveMax * Mathf.Abs(vc.input.Vertical) * Mathf.Clamp01(vc.SpeedKPH / reverseSpeed)
                             * Mathf.Clamp01(vc.axles[1].geometry.steerCoefficient) * Mathf.Sign(vc.input.Horizontal));
        }
    }
}