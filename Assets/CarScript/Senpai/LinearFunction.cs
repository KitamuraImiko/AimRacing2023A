//────────────────────────────────────────────
// ファイル名	：LinearFunction.cs
// 概要			：一次関数関連の計算を行う機能
// 作成者		：東樹 潤弥
// 作成日		：2020.3.18
// 
//────────────────────────────────────────────
// 更新履歴：
// 2020/03/18 [東樹 潤弥]	二点から一次関数の係数を計算する機能の追加
//							二直線の交点を計算する機能の追加
//────────────────────────────────────────────

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearFunction
{
	public enum AxisParallel
	{
		none,
		isX,
		isY
	}

	// 二点から一次関数の係数を計算するメソッド
	public Vector2 CalcLinearFunctionOfTwoPoints(ref AxisParallel axis_, Vector3 start_, Vector3 target_)
	{
		Vector2 coeff = Vector2.zero;

		// X軸が同じ つまりY軸と平行(横線)
		if (start_.x == target_.x)
		{
			axis_ = AxisParallel.isY;
			coeff.x = start_.x;
		}
		// Y軸が同じ つまりX軸と平行(縦線)
		else if (start_.z == target_.z)
		{
			axis_ = AxisParallel.isX;
			coeff.y = start_.y;
		}
		// X軸Y軸とも平行ではない場合（一次関数）
		else
		{
			// 一次関数の係数（傾き：ｘ 切片：ｙ）を求める
			coeff.x = (start_.z - target_.z) / (start_.x - target_.x);
			coeff.y = (coeff.x * -target_.x) + target_.z;

			axis_ = AxisParallel.none;
		}
		return coeff;
	}

	// 二直線の交点を求めるメソッド
	public Vector2 CalcIntersectionOfTwoStraightLines(Vector2 line1_, Vector2 line2_)
	{
		Vector2 dataA = line1_;
		Vector2 dataB = line2_;

		// 連立方程式を解く
		// 使用する一次関数は、yに係数がない為、まずyを消す
		//     y = a1x + b1
		// (-) y = a2x + b2
		//----------------------------
		//     0 = (a1-a2)x + (b1-b2)

		// 傾きと切片を求める
		// α = (a1-a2)
		// β = (b1-b2)
		float resultAlpha = dataA.x - dataB.x;
		float resultBeta = dataA.y - dataB.y;

		Vector2 resultData;
		// 0 = αx + βを、x = の形に直す
		// x = -β/α
		resultData.x = -resultBeta / resultAlpha;

		// xを代入して、yを求める
		// y = ax + b
		resultData.y = (dataA.x * resultData.x) + dataA.y;

		return resultData;
	}
	Vector2 CalcIntersectionOfTwoStraightLines(Vector3 line1_, Vector3 line2_)
	{
		Vector2 dataA = new Vector2(line1_.x, line1_.z);
		Vector2 dataB = new Vector2(line2_.x, line2_.z);

		// 連立方程式を解く
		// 使用する一次関数は、yに係数がない為、まずyを消す
		//     y = a1x + b1
		// (-) y = a2x + b2
		//----------------------------
		//     0 = (a1-a2)x + (b1-b2)

		// 傾きと切片を求める
		// α = (a1-a2)
		// β = (b1-b2)
		float resultAlpha = dataA.x - dataB.x;
		float resultBeta = dataA.y - dataB.y;

		Vector2 resultData;
		// 0 = αx + βを、x = の形に直す
		// x = -β/α
		resultData.x = -resultBeta / resultAlpha;

		// xを代入して、yを求める
		// y = ax + b
		resultData.y = (dataA.x * resultData.x) + dataA.y;

		return resultData;
	}
}
