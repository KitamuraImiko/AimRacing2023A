//────────────────────────────────────────────
// ファイル名	：FrictionCircle.cs
// 概要			：荷重移動によって変化する摩擦円の実装
// 作成者		：東樹 潤弥
// 作成日		：2020.4.27
// 
//────────────────────────────────────────────
// 更新履歴：
// 2020/04/27 [東樹 潤弥] クラス作成
//────────────────────────────────────────────

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIM
{
	public class FrictionCircle : MonoBehaviour
	{
		Load load;

		float staticFriction_FL;
		float staticFriction_FR;
		float staticFriction_RL;
		float staticFriction_RR;

		float frictionCoeff_FL;
		float frictionCoeff_FR;
		float frictionCoeff_RL;
		float frictionCoeff_RR;

		[SerializeField] private float frictionCoeff_normal = 0.8f;
		[SerializeField] private float frictionCoeff_threshold = 100.0f;

		Vector3 fl_pos;
		Vector3 fr_pos;
		Vector3 rl_pos;
		Vector3 rr_pos;

		bool isStartDraw = false;

		static float gravitationalAccel = 9.8f;

		[Header("DebugLine用")]
		[SerializeField] private int circleAccuracy;
		[SerializeField] private float circlePosY;
		[SerializeField] private float gizmosSize = 0.001f;
		[SerializeField] private float frontBack_length = 4.0f;
		[SerializeField] private float rightLeft_length = 4.0f;

		const float FRICTION_COEFF = 0.01f;

		private void Start()
		{
			load = GetComponent<Load>();

			isStartDraw = true;
		}

		private void Update()
		{
			// 最大静止摩擦力 F = 摩擦係数 * 荷重 * 重力加速度
			staticFriction_FL = frictionCoeff_FL * load.load_FL * gravitationalAccel;
			staticFriction_FR = frictionCoeff_FR * load.load_FR * gravitationalAccel;
			staticFriction_RL = frictionCoeff_RL * load.load_RL * gravitationalAccel;
			staticFriction_RR = frictionCoeff_RR * load.load_RR * gravitationalAccel;

			UpdatePos();

			CalcFrictionCoeff();
		}

		void UpdatePos()
		{
			fl_pos = transform.position + (transform.forward * frontBack_length) - (transform.right * rightLeft_length);
			fr_pos = transform.position + (transform.forward * frontBack_length) + (transform.right * rightLeft_length);
			rl_pos = transform.position - (transform.forward * frontBack_length) - (transform.right * rightLeft_length);
			rr_pos = transform.position - (transform.forward * frontBack_length) + (transform.right * rightLeft_length);
		}

		private void OnDrawGizmos()
		{
			if (isStartDraw)
			{
				Gizmos.color = new Color32(0, 0, 255, 255);

				DrawCircle(fl_pos, gizmosSize * staticFriction_FL);
				DrawCircle(fr_pos, gizmosSize * staticFriction_FR);
				DrawCircle(rl_pos, gizmosSize * staticFriction_RL);
				DrawCircle(rr_pos, gizmosSize * staticFriction_RR);
			}
		}

		// 球体ではなく、円で表示
		// 円のほうが見やすいしかっこいい
		void DrawCircle(Vector3 pos, float radius)
		{
			Vector3[] point = new Vector3[circleAccuracy];
			for (int i = 0; i < circleAccuracy; ++i)
			{
				float debugAngle = (360.0f / circleAccuracy * i) * Mathf.Deg2Rad;
				Vector3 targetPos;
				targetPos.x = pos.x + radius * Mathf.Cos(debugAngle);
				targetPos.z = pos.z + radius * Mathf.Sin(debugAngle);
				targetPos.y = pos.y + circlePosY;

				point[i] = targetPos;

				if (i > 0)
				{
					Gizmos.DrawLine(point[i - 1], point[i]);
					// 始点と終点を繋ぐ
					if (i == circleAccuracy - 1) { Gizmos.DrawLine(point[i], point[0]); }
				}
			}
		}

		void CalcFrictionCoeff()
		{
			// タイヤの荷重が、閾値より大きいなら、そのタイヤの摩擦係数を下げる

			// FL
			if (load.load_FL >= frictionCoeff_threshold)
			{
				// 差が100kgで、摩擦係数が0.1下がる
				frictionCoeff_FL = frictionCoeff_normal + (load.load_FL - frictionCoeff_threshold) * FRICTION_COEFF;
			}
			else { frictionCoeff_FL = frictionCoeff_normal; }

			// FR
			if (load.load_FR >= frictionCoeff_threshold)
			{
				frictionCoeff_FR = frictionCoeff_normal + (load.load_FR - frictionCoeff_threshold) * FRICTION_COEFF;
			}
			else { frictionCoeff_FR = frictionCoeff_normal; }

			// RL
			if (load.load_RL >= frictionCoeff_threshold)
			{
				frictionCoeff_RL = frictionCoeff_normal - (load.load_RL - frictionCoeff_threshold) * FRICTION_COEFF;
			}
			else { frictionCoeff_RL = frictionCoeff_normal; }

			// RR
			if (load.load_RR >= frictionCoeff_threshold)
			{
				frictionCoeff_RR = frictionCoeff_normal - (load.load_RR - frictionCoeff_threshold) * FRICTION_COEFF;
			}
			else { frictionCoeff_RR = frictionCoeff_normal; }
		}
	}
}