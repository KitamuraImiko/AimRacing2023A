using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Text;

public class CarInput : MonoBehaviour
{
    Func<int, float> RoundValue = (value) => { return (value == 0) ? 0.0f : 1.0f - ((float)(value - Int16.MinValue) / (float)UInt16.MaxValue); };

    /*定数----------------------------------*/
    const byte PUSH = 128;
    const int LEFT = 5;
    const int RIGHT = 4;
    const int LOW = 12;
    const int REVERSE = 18;
    const int UP = 12;
    const int DOWN = 13;
    const int PLAY = 20;//遊びの範囲（度数）要調整
    const float RANGE = ((float)PLAY * 0.5f) / 180.0f * (float)Int16.MaxValue;//遊びの範囲
    const float SPEED = 0.1f;
    const int TOP = 6;
    const float BRAKE = 0.6f;
    /*-----------------------------------------*/

    /*クラス内変数---------------------------*/
    bool isShiftLever = false;//シフトレバーが入っているか
    bool isShiftPaddle = false;//パドル押されているか
    bool preIsShiftLever = false;//前フレーム情報
    bool preIsShiftPaddle = false;//前フレーム情報

    int shiftLever = 1;
    int shiftNumber = 1;
    bool logicoolG29 = false;//G29かどうか
    LogitechGSDK.LogiControllerPropertiesData properties;
    LogitechGSDK.DIJOYSTATE2ENGINES rec;
    float keyBrake = 0.0f;
    float keyAccel = 0.0f;
    float keySteer = 0.0f;
    float handleSteer = 0.0f;
    /*--------------------------------------*/


    /*プロパティ-----------------------------*/
    public float Accel { private set; get; }
    public float Brake { private set; get; }
    public float Steer { private set; get; }
    public float Clutch { private set; get; }
    public int ShiftNumber { private set { shiftNumber = value; } get { return shiftNumber; } }
    /*--------------------------------------*/
    void Start()
    {
        properties = new LogitechGSDK.LogiControllerPropertiesData();
        properties.forceEnable = false;
        properties.overallGain = 100;
        properties.springGain = 100;
        properties.damperGain = 100;
        properties.defaultSpringEnabled = true;
        properties.defaultSpringGain = 100;
        properties.combinePedals = false;
        properties.wheelRange = 360;//ハンドル取得範囲

        properties.gameSettingsEnabled = false;
        properties.allowGameSettings = false;

        LogitechGSDK.LogiSetPreferredControllerProperties(properties);//上書き
        LogitechGSDK.LogiSteeringInitialize(false);

    }
    void Update()
    {
        LogitechGSDK.LogiControllerPropertiesData actualProperties = new LogitechGSDK.LogiControllerPropertiesData();
        LogitechGSDK.LogiGetCurrentControllerProperties(0, ref actualProperties);
       //Debug.Log("actualProperties.wheelRange" + actualProperties.wheelRange);
        if (Input.GetKey(KeyCode.UpArrow))
        {
            LogitechGSDK.LogiPlaySpringForce(0, 10, 50, 50);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            LogitechGSDK.LogiPlaySpringForce(0, 0, 10, 10);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            LogitechGSDK.LogiStopSpringForce(0);
        }
        Text text = GetComponent<Text>();
        Inputs();


        shiftNumber += KeyShift();


        if (KeyAccel() >= RoundValue(rec.lY))
        {
            Accel = keyAccel;
        }

        if (KeyBrake() >= RoundValue(rec.lRz))
        {
            Brake = keyBrake;
        }

        if ((KeySteer() >= 0) == (handleSteer >= 0))
        {
            if (Mathf.Abs(keySteer) >= Mathf.Abs(handleSteer))
            {
                Steer = keySteer;
            }
        }
        else
        {
            Steer = keySteer + handleSteer;
        }

        text.text = "Steer:" + Steer.ToString() + "\n" + "Accel:" + Accel.ToString() + "\n" + "Brake:" + Brake.ToString() + "\n" + "Clutch:" + Clutch.ToString() + "\n" + "ShiftNumber:" + ShiftNumber.ToString() + "\n" + "logicoolG29:" + logicoolG29.ToString();
        text.text += "\n" + "LogiUpdate:" + LogitechGSDK.LogiUpdate().ToString() + "\n" + "LogiIsConnected:" + LogitechGSDK.LogiIsConnected(0).ToString();
        /*デバッグログ--------------------------------------*/

    }
    int KeyShift()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && (shiftNumber < TOP))
            {
                return 1;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && (0 < shiftNumber))
            {
                return -1;
            }
        }
        return 0;
    }
    float KeyAccel()
    {
        if (Input.GetKey(KeyCode.W))
        {
            keyAccel += SPEED;
        }
        else if (0 < keyAccel)
        {
            keyAccel -= SPEED;
        }
        if (-SPEED < keyAccel && keyAccel < SPEED)
        {
            keyAccel = 0;
        }
        if (keyAccel < 0)
        {
            keyAccel = 0;
        }
        else if (1 < keyAccel)
        {
            keyAccel = 1;
        }

        return keyAccel;
    }
    float KeyBrake()
    {
        if (Input.GetKey(KeyCode.S))
        {
            keyBrake += SPEED;
        }
        else if (0 < keyBrake)
        {
            keyBrake -= SPEED;
        }
        if (-SPEED < keyBrake && keyBrake < SPEED)
        {
            keyBrake = 0;
        }
        if (keyBrake < 0)
        {
            keyBrake = 0;
        }
        else if (1 < keyBrake)
        {
            keyBrake = 1;
        }

        return keyBrake;
    }
    float KeySteer()    
    {
        if (Input.GetKey(KeyCode.D))
        {
            keySteer += SPEED;
        }
        if (Input.GetKey(KeyCode.A))
        {
            keySteer -= SPEED;
        }
        if ((!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) || (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)))
        {
            if (0 < keySteer)
            {
                keySteer -= SPEED;
            }
            else if (keySteer < 0)
            {
                keySteer += SPEED;
            }
        }
        if (-SPEED < keySteer && keySteer < SPEED)
            keySteer = 0;

        if (keySteer < -1)
        {
            keySteer = -1;
        }
        else if (1 < keySteer)
        {
            keySteer = 1;
        }

        return keySteer;
    }
    /*入力を変換し設定する関数----------------*/
    public void Inputs(/*Action shiftChange*/)
    {
        //if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)/*ハンコン入っているかどうか*/)
        //{
        StringBuilder deviceName = new StringBuilder(256);//ハンコン名取得
        LogitechGSDK.LogiGetFriendlyProductName(0, deviceName, 256);

        if (deviceName.ToString() == "Logicool G29 Driving Force Racing Wheel USB")//ハンコン判定
        {
            logicoolG29 = true;
        }
        else
        {
            logicoolG29 = false;
        }

        rec = LogitechGSDK.LogiGetStateUnity(0);//ハンコン値
        Func<int, int, bool> PushJudge = (buttonX, buttonY) => { return ((rec.rgbButtons[buttonX] == PUSH) || (rec.rgbButtons[buttonY] == PUSH)) ? true : false; };//入力判定の関数

        Accel = RoundValue(rec.lY);//アクセル値
        if (logicoolG29)
        {
            if (BRAKE < RoundValue(rec.lRz))//ブレーキ値
            {
                Brake = 1.0f;
            }
            else
            {
                Brake = RoundValue(rec.lRz) / BRAKE;
            }
        }
        else
        {
            Brake = RoundValue(rec.lRz);
        }

        Clutch = RoundValue(rec.rglSlider[0]);//クラッチ値
        if (RANGE > Mathf.Abs(rec.lX))//ハンドル値
        {
            Steer = handleSteer = 0.0f;
        }
        else if (rec.lX > 0)
        {
            Steer = handleSteer = ((float)rec.lX - RANGE) / ((float)Int16.MaxValue - RANGE);
        }
        else
        {
            Steer = handleSteer = -((float)rec.lX + RANGE) / ((float)Int16.MinValue + RANGE);
        }

        if (logicoolG29)//シフトレバー入力
        {
            for (int i = 1, j = LOW; j <= REVERSE; ++i, ++j)//G29のシフトレバー
            {
                if (rec.rgbButtons[j] == PUSH)
                {
                    if (i == 7) //Rがボタン18のため、7にあると6の次になっていまい、ゲームの仕様では1の前になる為、都合よくするため
                    {
                        i = 0;
                    }
                    shiftLever = i;
                    isShiftLever = true;
                    break;
                }
                else
                {
                    isShiftLever = false;
                }
            }
        }
        else
        {
            isShiftLever = PushJudge(UP, DOWN);//入力判定
        }
        /*
         1速->1
         2速->2
         3速->3
         4速->4
         5速->5
         6速->6
         R  ->7->0
         */

        isShiftPaddle = PushJudge(RIGHT, LEFT);//入力判定

        /*
         R  ->0
         1速->1
         2速->2
         3速->3
         4速->4
         5速->5
         6速->6
         */

        if (!preIsShiftLever && isShiftLever)//レバー
        {
            if (logicoolG29)
            {
                shiftNumber = shiftLever;
            }
            else
            {
                if ((rec.rgbButtons[UP] == PUSH) && (shiftNumber < TOP))
                {
                    shiftNumber += 1;
                }
                else if ((rec.rgbButtons[DOWN] == PUSH) && (0 < shiftNumber))
                {
                    shiftNumber -= 1;
                }
            }
        }
        else if (!preIsShiftPaddle && isShiftPaddle) //パドルシフト
        {
            if ((rec.rgbButtons[RIGHT] == PUSH) && (shiftNumber < TOP))
            {
                shiftNumber += 1;
            }
            else if ((rec.rgbButtons[LEFT] == PUSH) && (0 < shiftNumber))
            {
                shiftNumber -= 1;
            }
        }

        preIsShiftLever = isShiftLever;//前回更新
        preIsShiftPaddle = isShiftPaddle;
    }
}