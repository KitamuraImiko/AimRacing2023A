using UnityEngine;

namespace AIM
{
    /// <summary>
    /// 回転の軸を前輪軸の中心、つまり車体前方に設定するためのクラス
    /// 親子関係で出来なかった為、計算で回転させている
    /// </summary>
    [System.Serializable]
    public class DriftSlide
    {
        public bool isEnable;

        public float forwardDistance;
        public Vector3 forwardPos;
        public Vector3 movePos;

        public float rotAngle_max;
        public float moveSpeed;
        public float rotSpeed;
        float angleValue;

        private float[,] TwoMultiTwoArray = new float[2, 2];
        private float[] TwoMultiOneArray  = new float[2];

        VehicleController vc;
        const float piCash = Mathf.PI / 180.0f;

		public float accel;
		public float accelLerp;

        public AnimationCurve speedPowerCurve;
		public AnimationCurve speedRotCurve;
        public AnimationCurve angleRotCurve;

        const float HANDLE_THERSHOLD = 0.3f;
        const float ACCEL_MAX = 0.5f;
        const float ACCEL_MIN = 0.0f;
        const float ACCEL_BASE = 0.5f;

        float sideTime = 0.0f;
        float sideTime_max = 0.6f;
        float sideCoef;
        float sidePower;

        //=================================================================================
        public void Initialize(VehicleController _vc)
        {
            vc = _vc;
            sideCoef = 1.0f / sideTime_max;
        }
        //=================================================================================
        public void Update()
        {
            movePos = CalcDriftPos(vc.transform);
            sideTime = Mathf.Clamp(sideTime, 0.0f, sideTime_max);
            sidePower = sideCoef * sideTime * moveSpeed * speedPowerCurve.Evaluate(vc.SpeedKPH);

            // 車の移動・回転
            //vc.rb.AddForce((movePos - vc.transform.position).normalized * sidePower);

			accel = Mathf.Lerp(accel, Mathf.Clamp01(vc.input.Vertical), accelLerp * Time.deltaTime);

			if (vc.input.Horizontal > HANDLE_THERSHOLD)
			{
                if (sideTime < sideTime_max)
                {
                    sideTime += Time.deltaTime;
                }
                angleValue = 1.0f;
			}
			else if (vc.input.Horizontal < -HANDLE_THERSHOLD)
            {
                if (sideTime < sideTime_max)
                {
                    sideTime += Time.deltaTime;
                }
                angleValue = -1.0f;
			}
			else
			{
                if (sideTime > 0.0f)
                {
                    sideTime -= Time.deltaTime;
                }
				angleValue = 0.0f;
				return;
			}
            // ハンドル入力をカーブで補正する
            float anglePower = angleValue * angleRotCurve.Evaluate(Mathf.Abs(vc.input.Horizontal)) * rotSpeed;
            // アクセルの最低保証と速度のカーブ補正
            float accelPower = (ACCEL_BASE + Mathf.Clamp(accel, ACCEL_MIN, ACCEL_MAX)) * speedRotCurve.Evaluate(vc.SpeedKPH);

            // アクセルの入力量と車体速度で、回転速度を設定
            vc.rb.AddTorque(Vector3.up * anglePower * accelPower);
		}
        //=================================================================================
        Vector3 CalcDriftPos(Transform _trans)
        {
            // 回転軸設定（前方）
            forwardPos = _trans.position + _trans.forward * forwardDistance;

            // 回転角度設定
            float angle = (-vc.input.Horizontal * rotAngle_max) * piCash;

            // 行列設定
            TwoMultiTwoArray[0, 0] = Mathf.Cos(angle);
            TwoMultiTwoArray[0, 1] = -Mathf.Sin(angle);
            TwoMultiTwoArray[1, 0] = Mathf.Sin(angle);
            TwoMultiTwoArray[1, 1] = Mathf.Cos(angle);

            TwoMultiOneArray[0] = _trans.position.x - forwardPos.x;
            TwoMultiOneArray[1] = _trans.position.z - forwardPos.z;

            // 行列式計算
            Vector2 move = DeterminantMulti_TowDxOneD(TwoMultiTwoArray, TwoMultiOneArray);
            move.x += forwardPos.x;
            move.y += forwardPos.z;

            return new Vector3(move.x, _trans.position.y, move.y);
        }
        //=================================================================================
        // 2*2と2*1の行列式の計算
        Vector2 DeterminantMulti_TowDxOneD(in float[,] A, in float[] B)
        {
            float X = (A[0, 0] * B[0]) + (A[0, 1] * B[1]);
            float Y = (A[1, 0] * B[0]) + (A[1, 1] * B[1]);

            return new Vector2(X, Y);
        }
    }
}