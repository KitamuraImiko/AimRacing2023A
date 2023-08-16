using UnityEngine;

namespace AIM
{
    /// <summary>
    /// ��]�̎���O�֎��̒��S�A�܂�ԑ̑O���ɐݒ肷�邽�߂̃N���X
    /// �e�q�֌W�ŏo���Ȃ������ׁA�v�Z�ŉ�]�����Ă���
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

            // �Ԃ̈ړ��E��]
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
            // �n���h�����͂��J�[�u�ŕ␳����
            float anglePower = angleValue * angleRotCurve.Evaluate(Mathf.Abs(vc.input.Horizontal)) * rotSpeed;
            // �A�N�Z���̍Œ�ۏ؂Ƒ��x�̃J�[�u�␳
            float accelPower = (ACCEL_BASE + Mathf.Clamp(accel, ACCEL_MIN, ACCEL_MAX)) * speedRotCurve.Evaluate(vc.SpeedKPH);

            // �A�N�Z���̓��͗ʂƎԑ̑��x�ŁA��]���x��ݒ�
            vc.rb.AddTorque(Vector3.up * anglePower * accelPower);
		}
        //=================================================================================
        Vector3 CalcDriftPos(Transform _trans)
        {
            // ��]���ݒ�i�O���j
            forwardPos = _trans.position + _trans.forward * forwardDistance;

            // ��]�p�x�ݒ�
            float angle = (-vc.input.Horizontal * rotAngle_max) * piCash;

            // �s��ݒ�
            TwoMultiTwoArray[0, 0] = Mathf.Cos(angle);
            TwoMultiTwoArray[0, 1] = -Mathf.Sin(angle);
            TwoMultiTwoArray[1, 0] = Mathf.Sin(angle);
            TwoMultiTwoArray[1, 1] = Mathf.Cos(angle);

            TwoMultiOneArray[0] = _trans.position.x - forwardPos.x;
            TwoMultiOneArray[1] = _trans.position.z - forwardPos.z;

            // �s�񎮌v�Z
            Vector2 move = DeterminantMulti_TowDxOneD(TwoMultiTwoArray, TwoMultiOneArray);
            move.x += forwardPos.x;
            move.y += forwardPos.z;

            return new Vector3(move.x, _trans.position.y, move.y);
        }
        //=================================================================================
        // 2*2��2*1�̍s�񎮂̌v�Z
        Vector2 DeterminantMulti_TowDxOneD(in float[,] A, in float[] B)
        {
            float X = (A[0, 0] * B[0]) + (A[0, 1] * B[1]);
            float Y = (A[1, 0] * B[0]) + (A[1, 1] * B[1]);

            return new Vector2(X, Y);
        }
    }
}