using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
    /// <summary>
    /// 慣性ドリフトのシステム
    /// アクセルを緩めた状態でハンドルを切り、アクセルを踏む事で滑らせる
    /// NFS Heatを参考に、アクセルを離してすぐ押すことで車体を滑らせ、ハンドルでコントロールする
    /// </summary>
    [System.Serializable]
    public class DriftInertial
    {
        [Header("スリップ判定変数")]
        public bool isEnable;
        VehicleController vc;

        // 連続押し判定の間隔
        const float PUSH_INTERVAL = 1.5f;
        const float ACCEL_THRESHOLD = 0.4f;
        const float SLIP_WAIT_LIMIT = 0.5f;
        const float HANDLE_THRESHOLD = 0.25f;

        public enum AccelState
        {
            None,
            ON,
            TurnOff,
            Slip
		}
        public enum HandleState
		{
            None,
            Right,
            Left
		}
        public float startTime = 0.0f;
        public float slipTime = 0.0f;

        public AccelState accelState = AccelState.None;
        public HandleState handleState = HandleState.None;

        [Header("スリップ挙動変数")]
        public float slipMax = 1.3f;
        public float slipMin = 1.0f;

        public float slipValue = 1.0f;

        public float slipAccel = 0.15f;
        public float slipDecel = 0.25f;
        public float slipSlowDecel = 0.1f;

        public float slipStartValue = 1.15f;

        public bool isAccel;
        public bool isHandle;

        public float VeloAngle;

        const float ANGLE_THRESHOLD = 4.0f;

        //=================================================================================
        public void Initialize(VehicleController _vc)
        {
            vc = _vc;
        }
        //=================================================================================
        /*public void Update()
        {
            //UpdateAccelState();

            //SlipUpdate();

            //isAccel = (Mathf.Abs(vc.input.Vertical) > ACCEL_THRESHOLD);
            //isHandle = (Mathf.Abs(vc.input.Horizontal) > HANDLE_THRESHOLD);

            //SlipSide();

        }*/
        //=================================================================================
        void UpdateAccelState()
		{
            // ゲームブックみたいな処理
			switch (accelState)
            {
                // アクセルを踏んだら次へ
                case AccelState.None:
                    if (vc.input.Vertical > ACCEL_THRESHOLD)
                    {
                        accelState = AccelState.ON;
                    }
                    break;
                // アクセルを離したら次へ
                case AccelState.ON:
                    if(vc.input.Vertical <= ACCEL_THRESHOLD)
					{
                        accelState = AccelState.TurnOff;
                        startTime = Time.time;
                    }
					break;
                // アクセルを一定時間以内に押し、ハンドルも一定以上切っていたら次へ
                // 押せなかったら最初へ戻る
				case AccelState.TurnOff:
                    if(Time.time - startTime < PUSH_INTERVAL /*&& Mathf.Abs(vc.input.Horizontal) > HANDLE_THRESHOLD*/)
					{
                        if(vc.input.Vertical > ACCEL_THRESHOLD)
						{
                            accelState = AccelState.Slip;
                            handleState = HandleState.None;
                            SlipStart();
                            slipTime = 0.0f;
						}
					}
                    else
					{
                        accelState = AccelState.None;
					}
					break;
                // 一定時間アクセルを押していなければ最初へ戻る
                case AccelState.Slip:
                    if(vc.input.Vertical < ACCEL_THRESHOLD)
					{
                        slipTime += Time.deltaTime;
					}
                    else
					{
                        slipTime = Mathf.Clamp(slipTime - Time.deltaTime,0.0f, SLIP_WAIT_LIMIT);
					}
                    // スリップ値が最小値になったら最初へ戻る
                    if(slipTime > SLIP_WAIT_LIMIT || slipValue == slipMin)
					{
                        accelState = AccelState.None;
					}
                    break;
				default:
					break;
			}
		}
        //=================================================================================
        void SlipUpdate()
		{
            
            if(accelState == AccelState.Slip)
			{
                // ハンドルの向きを設定
                if(handleState == HandleState.None)
				{
                    if(vc.input.Horizontal > HANDLE_THRESHOLD)
					{
                        handleState = HandleState.Right;
					}
                    else if(vc.input.Horizontal < -HANDLE_THRESHOLD)
					{
                        handleState = HandleState.Left;
					}
				}
            }
            // スリップ状態でないなら、減衰
            else
            {
                slipValue = Mathf.Clamp(slipValue - slipDecel * Time.deltaTime, slipMin, slipMax);
                handleState = HandleState.None;
                return;
            }

            // アクセルとハンドルの状態で、スリップ度合いを変化させる
            if(Mathf.Abs(vc.input.Vertical) > ACCEL_THRESHOLD && Mathf.Abs(vc.input.Horizontal) > HANDLE_THRESHOLD)
			{
                slipValue += slipAccel * Time.deltaTime;
			}
            else if(Mathf.Abs(vc.input.Vertical) > ACCEL_THRESHOLD || Mathf.Abs(vc.input.Horizontal) > HANDLE_THRESHOLD)
			{
                slipValue -= slipSlowDecel * Time.deltaTime;
			}
            else
			{
                slipValue -= slipSlowDecel * Time.deltaTime;
            }

            slipValue = Mathf.Clamp(slipValue, slipMin, slipMax);

            if (handleState == HandleState.None)
			{
                return;
			}
            // スリップ時に、反対にハンドルを切ったらスリップ度合いを初期化する
            else if(handleState == HandleState.Right)
			{
                if(vc.input.Horizontal <= 0.0f)
				{
                    slipValue = slipMin;
				}
			}
            else if(handleState == HandleState.Left)
			{
                if (vc.input.Horizontal >= 0.0f)
                {
                    slipValue = slipMin;
                }
            }
		}
        //=================================================================================
        void SlipStart()
		{
            slipValue = slipStartValue;
        }
        //=================================================================================
        void SlipSide()
		{
   //         if(handleState == HandleState.None)
			//{
   //             return;
			//}
            VeloAngle = Vector2.SignedAngle(new Vector2(vc.transform.forward.x, vc.transform.forward.z), new Vector2(vc.rb.velocity.x, vc.rb.velocity.z));

   //         if(VeloAngle > ANGLE_THRESHOLD)
			//{
   //             vc.rb.AddForce(vc.transform.right * 50000.0f);
   //         }
   //         else if(VeloAngle < -ANGLE_THRESHOLD)
			//{
   //             vc.rb.AddForce(vc.transform.right * -50000.0f);
   //         }
        }
    }
}