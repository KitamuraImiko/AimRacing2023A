/*------------------------------------------------------------------
* ファイル名：AxelPedal.cs
* 概要：ペダルの関係するのクラス
* 担当者：キタムラ
* 作成日：2023/06/02
-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelPedal : MonoBehaviour
{
    private AIM.WheelController wc = new AIM.WheelController(); //
    private AIM.WheelInput input;                               // 入力取得用
    [SerializeField] private float AccelDeskTopSee;
    // Start is called before the first frame update
    void Start()
    {
        // 使う場所を選択
        input = transform.root.GetComponent<AIM.VehicleController>().wheelInput;
    }

    // Update is called once per frame
    void Update()
    {
        // アクセルの踏み具合が8割を超えるとき
        AccelPower();
    }

    private void AccelPower()
    {
        AccelDeskTopSee = input.inputAxel;
        // アクセルの踏み具合が 0.8f を超えるとき
        if (input.inputAxel >= 0.8f)
        {

        }
    }
}