using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
    /// <summary>
    /// 入力したデータを保存するクラス
    /// </summary>
    [HideInInspector]
    [System.Serializable]
	public class InputStates
	{
        [HideInInspector]
        public bool settable = true;

        [SerializeField]private float horizontal;
        [SerializeField] private float vertical;

        [SerializeField] private float accel;
        [SerializeField] private float brake;

        [SerializeField] private float clutch;
      
        private float handbrake;

        private bool shiftUp;
        private bool shiftDown;

        public bool leftBlinker;
        public bool rightBlinker;
        public bool lowBeamLights;
        public bool fullBeamLights;
        public bool hazardLights;

        public bool flipOver;

        public bool horn;

        private VehicleController vc;

        public void Initialize(VehicleController vc)
        {
            this.vc = vc;
        }

        // シフトアップ
		public bool ShiftUp
        {
            get
            {
                if (!vc.Active) return false;
                return shiftUp;
            }
            set
            {
                shiftUp = value;
            }
		}

        // シフトダウン
		public bool ShiftDown
		{
			get
			{
                if (!vc.Active) return false;
                return shiftDown;
			}
            set
            {
                shiftDown = value;
            }
		}

        // ハンドル
        public float Horizontal
        {
            get
            {
                if (!vc) return 0.0f;
                if (!vc.Active) return 0.0f;

                return horizontal;
            }

            set
            {
                if(settable) horizontal = Mathf.Clamp(value, -1.0f, 1.0f);
            }
        }

        // アクセル・ブレーキ
        public float Vertical
        {
            get
            {
                if (!vc) return 0.0f;
                if (!vc.Active) return 0.0f;

                return vertical;
            }

            set
            {
                if (settable) vertical = Mathf.Clamp(value, -1.0f, 1.0f);
            }
        }

        public float Accel
		{
			get { return accel; }

            set
            {
                if (settable) accel = Mathf.Clamp01(value);
            }
        }

        public float Brake
		{
            get { return brake; }

            set
            {
                if (settable) brake = Mathf.Clamp01(value);
            }
        }

        // クラッチ
        public float Clutch
        {
            get
            {
                return clutch;
            }
            set
            {
                clutch = Mathf.Clamp01(value);
            }
        }

        // サイド・ハンドブレーキ
        public float Handbrake
        {
            get
            {
                if (!vc.Active) return 0.0f;
                return handbrake;
            }

            set
            {
                handbrake = Mathf.Clamp01(value);
            }
        }
    }
}