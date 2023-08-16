using UnityEngine;

namespace AIM
{
    /// <summary>
    /// 走行アシストの機能を管理する
    /// </summary>
    [System.Serializable]
    public class DrivingAssists
    {
        [System.Serializable]
        public class DrivingAid
        {
            // 機能が有効か
            public bool enabled = false;

            // 機能が働いているか
            [HideInInspector]
            public bool active;

            [Range(0.0f, 1.0f)]
            public float intensity = 0.3f;
        }
        [SerializeField]
        public CruiseControl cruiseControl = new CruiseControl();

        [SerializeField]
        public ABS abs = new ABS();

        [SerializeField]
        public TractionControl tcs = new TractionControl();

        [Tooltip("Stability assist system.")]
        [SerializeField]
        public Stability stability = new Stability();

        [SerializeField]
        public DriftAssist driftAssist = new DriftAssist();

        private VehicleController vc;

        public void Initialize(VehicleController vc)
        {
            this.vc = vc;
        }

        // 各機能の更新
        public void Update()
        {
            if (cruiseControl.enabled) cruiseControl.Update(vc);
            if (abs.enabled) abs.Update(vc);
            if (tcs.enabled) tcs.Update(vc);
            if (stability.enabled) stability.Update(vc);
            if (driftAssist.enabled) driftAssist.Update(vc);
        }
    }
}