using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
    [System.Serializable]
    public class DriftSideBrake
    {
        public bool isEnable;

        // ドリフトに入るハンドルの値
        private float driftOn_SteeringValue;

        // ドリフト時の曲がる方向
        private bool driftIsRight;

        [Header("ドリフト割合")]
        public float driftRatio;

        [Range(0.0f, 1.0f)]
        [SerializeField] private float ratioMax;
        [SerializeField] private float ratioAccel_hBrake;
        [SerializeField] private float ratioAccel_handle;
        [SerializeField] private float ratioDecel_pedal;
        [SerializeField] private float ratioDecel;

        [Range(0.0f, 1.0f)]
        [SerializeField] private float driftHandleRatio;

        [Range(0.0f, 1.0f)]
        [SerializeField] private float driftPedalRatio;

        [SerializeField] private float driftSpeed;

        [Tooltip("Time：ハンドル割合　　　Value：増加度")]
        [SerializeField] private AnimationCurve driftRatioCurve;

        [Header("ドリフト時の回転減衰")]
        //[SerializeField] private float driftAngDrag;
        [SerializeField] private float angDrag_max;
        //[SerializeField] private float angDrag_startAngle;
        //[SerializeField] private float angDrag_endAngle;

        float defAngDrag;

        [Tooltip("Time：回転角度　　　Value：回転減衰度")]
        [SerializeField] private AnimationCurve angDragCurve_angle;

        [Tooltip("Time：ドリフト割合　　　Value：回転減衰度")]
        [SerializeField] private AnimationCurve angDragCurve_drift;

        [Header("ドリフトカウンター")]
        [SerializeField] private bool isStartDrift;
        [SerializeField] private bool isCounter;
        [SerializeField] private float counterAngDrag;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float dirDecision_handle;

        [Tooltip("1なら、右　-1なら、左　0なら未設定")]
        private int driftDirection = 0;

        VehicleController vc;

        //=================================================================================
        // 初期化
        public void Initialize(VehicleController vc_)
        {
            vc = vc_;

            defAngDrag = vc.rb.angularDrag;
        }
        //=================================================================================
        // 更新
        public void Update()
        {
            UpdateRatio();
            UpdateDriftAngularDrag();

            if (!isStartDrift && driftRatio > 0.0f)
            {
                DriftStart();
            }
            else if (isStartDrift)
            {
                DriftCounterCheck();
                DriftEnd();
            }

            if (isCounter)
            {
                DriftCounter();
            }
        }
        //=================================================================================
        // ドリフト割合を更新するメソッド
        private void UpdateRatio()
        {
            // サイドブレーキが入っているなら増加
            if (CheckSideBrakeDrift())
            {
                driftRatio += ratioAccel_hBrake * Time.deltaTime;
            }
            else
            {
                if (driftRatio == 0.0f) { return; }

                // 一定速度以下なら、ドリフト割合減少
                if (vc.SpeedKPH < driftSpeed)
                {
                    driftRatio -= ratioDecel * Time.deltaTime;
                }

                else if (Mathf.Abs(vc.input.Horizontal) > driftHandleRatio)
                {

                    driftRatio += ratioAccel_handle * driftRatioCurve.Evaluate(Mathf.Abs(vc.input.Horizontal)) * Time.deltaTime;
                }
                else if (AIM.InputHandleController.GetHandleAccel() > driftPedalRatio)
                {
                    driftRatio -= ratioDecel_pedal * Time.deltaTime;
                }
                else
                {
                    driftRatio -= ratioDecel * Time.deltaTime;
                }
            }
            // 最大値、最小値更新
            if (driftRatio <= 0.0f) { driftRatio = 0.0f; }
            else if (driftRatio > ratioMax) { driftRatio = ratioMax; }
        }

        //=================================================================================
        // ドリフト割合を反転して返すメソッド
        public float ReverseDriftRatio()
        {
            // 1の時、0を返し、0の時に1を返す
            return Mathf.Abs(driftRatio - 1.0f);
        }

        //=================================================================================
        // ドリフトの条件管理メソッド
        private bool CheckSideBrakeDrift()
        {
            if (vc.input.Handbrake == 1.0f)
            {
                return true;
            }
            return false;
        }

        //=================================================================================
        // ハンドルの値が一定以上か確認するメソッドs
        private bool CheckHandleAngle()
        {
            if (Mathf.Abs(vc.input.Horizontal) > driftHandleRatio)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //=================================================================================
        // ドリフト時の曲がる方向を設定
        private bool SetDriftAngle()
        {
            // ハンドルの角度判定
            if (vc.input.Horizontal > driftOn_SteeringValue)
            {
                driftIsRight = true;
                return true;
            }
            else if (vc.input.Horizontal < -driftOn_SteeringValue)
            {
                driftIsRight = false;
                return true;
            }
            return false;
        }

        //=================================================================================
        // ドリフト回転減衰更新
        private void UpdateDriftAngularDrag()
        {
            // 回転減衰をカーブで決定
            //float ad1 = angDragCurve_angle.Evaluate(vc.carsRespawn.carAngle);
            float ad2 = angDragCurve_drift.Evaluate(Mathf.Clamp01(driftRatio));

            vc.rb.angularDrag = Mathf.Clamp(/*ad1 + */ad2, defAngDrag, angDrag_max);
        }

        //=================================================================================
        // ドリフト時のカウンター判定
        private void DriftCounterCheck()
        {
            // 右にドリフト中なら
            if (driftDirection == 1)
            {
                // 左（反対）にハンドルを切ったら、カウンター始動
                if (vc.input.Horizontal < 0.0f)
                {
                    isCounter = true;
                }
            }
            else if (driftDirection == -1)
            {
                if (vc.input.Horizontal > 0.0f)
                {
                    isCounter = true;
                }
            }
        }
        //=================================================================================
        // ドリフト時のカウンター処理
        private void DriftCounter()
        {
            vc.rb.angularDrag = counterAngDrag;

            if (driftDirection == 1)
            {
                if (vc.input.Horizontal > 0.0f)
                {
                    isCounter = false;
                }
            }
            else if (driftDirection == -1)
            {
                if (vc.input.Horizontal < 0.0f)
                {
                    isCounter = false;
                }
            }
        }
        //=================================================================================
        // ドリフトスタート処理
        private void DriftStart()
        {
            // ハンドル入力でドリフト方向を決定
            // 何回かデバッグして決めること！
            if (Mathf.Abs(vc.input.Horizontal) > dirDecision_handle)
            {
                if (vc.input.Horizontal > 0.0f)
                {
                    driftDirection = 1;
                }
                else
                {
                    driftDirection = -1;
                }
                isStartDrift = true;
            }
        }
        //=================================================================================
        // ドリフト終了処理
        private void DriftEnd()
        {
            if (driftRatio == 0.0f)
            {
                isStartDrift = false;
                isCounter = false;
            }
        }
    }
}