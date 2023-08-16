using UnityEngine;
using System.Collections;

namespace AIM
{
    [System.Serializable]
    public class Fuel
    {
        public bool useFuel = false;

        public float capacity = 50f;
        public float amount = 50f;
        public float efficiencyPer = 0.45f;
        
        private float unitConsumption;
        private float consumptionPerHour;
        private float maxConsumptionPerHour = 20f;
        private float distanceTraveled = 0f;
        private float measuredConsumption = 0f;

        private VehicleController vc;

        public bool HasFuel
        {
            get
            {
                if (!useFuel)
                {
                    return true;
                }
                else
                {
                    if(amount > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public float FuelPercentage
        {
            get
            {
                return Mathf.Clamp01(amount / capacity);
            }
        }

        public float ConsumptionLitersPerSecond
        {
            get
            {
                return consumptionPerHour / 3600f;
            }
        }
        public float ConsumptionMPG
        {
            get
            {
                return UnitConverter.L100kmToMpg(unitConsumption);
            }
        }
        public float ConsumptionLitersPer100Kilometers
        {
            get
            {
                return unitConsumption;
            }
        }
        public float ConsumptionKilometersPerLiter
        {
            get
            {
                return UnitConverter.L100kmToKml(unitConsumption);
            }
        }

        public void Initialize(VehicleController vc)
        {
            this.vc = vc;
        }

        public void Update()
        {
            if(useFuel && vc.engine.IsRunning)
            {
                // MJ/L 1KWh = 3.6MJ
                // 1L = 36MJ = 10kWH
                maxConsumptionPerHour = (vc.engine.maxPower / 10f) * (1f - efficiencyPer);

                consumptionPerHour = (vc.engine.Power / vc.engine.maxPower) * maxConsumptionPerHour;
                consumptionPerHour = Mathf.Clamp(consumptionPerHour, maxConsumptionPerHour * 0.01f, Mathf.Infinity);

                // 空になるまで燃料を使用
                amount -= (consumptionPerHour / 3600) * Time.fixedDeltaTime;
                amount = Mathf.Clamp(amount, 0f, capacity);

                // 燃料が0ならストップ
                if(amount == 0 && vc.engine.IsRunning)
                {
                    vc.engine.Stop();
                }

                distanceTraveled = vc.Speed * Time.fixedDeltaTime;
                measuredConsumption = (consumptionPerHour / 3600f) * Time.fixedDeltaTime;

                float perHour = 3600f / Time.fixedDeltaTime;
                float measuredConsPerHour = measuredConsumption * perHour;
                float measuredDistPerHour = (distanceTraveled * perHour) / 100000f;
                unitConsumption = measuredDistPerHour == 0 ? 0 : Mathf.Clamp(measuredConsPerHour / measuredDistPerHour, 0f, 99.9f);
            }
            else
            {
                consumptionPerHour = 0f;
                unitConsumption = 0;
            }
        }
    }
}