using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Text;

public class CarInput : MonoBehaviour
{
    Func<int, float> RoundValue = (value) => { return (value == 0) ? 0.0f : 1.0f - ((float)(value - Int16.MinValue) / (float)UInt16.MaxValue); };

    /*�萔----------------------------------*/
    const byte PUSH = 128;
    const int LEFT = 5;
    const int RIGHT = 4;
    const int LOW = 12;
    const int REVERSE = 18;
    const int UP = 12;
    const int DOWN = 13;
    const int PLAY = 20;//�V�т͈̔́i�x���j�v����
    const float RANGE = ((float)PLAY * 0.5f) / 180.0f * (float)Int16.MaxValue;//�V�т͈̔�
    const float SPEED = 0.1f;
    const int TOP = 6;
    const float BRAKE = 0.6f;
    /*-----------------------------------------*/

    /*�N���X���ϐ�---------------------------*/
    bool isShiftLever = false;//�V�t�g���o�[�������Ă��邩
    bool isShiftPaddle = false;//�p�h��������Ă��邩
    bool preIsShiftLever = false;//�O�t���[�����
    bool preIsShiftPaddle = false;//�O�t���[�����

    int shiftLever = 1;
    int shiftNumber = 1;
    bool logicoolG29 = false;//G29���ǂ���
    LogitechGSDK.LogiControllerPropertiesData properties;
    LogitechGSDK.DIJOYSTATE2ENGINES rec;
    float keyBrake = 0.0f;
    float keyAccel = 0.0f;
    float keySteer = 0.0f;
    float handleSteer = 0.0f;
    /*--------------------------------------*/


    /*�v���p�e�B-----------------------------*/
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
        properties.wheelRange = 360;//�n���h���擾�͈�

        properties.gameSettingsEnabled = false;
        properties.allowGameSettings = false;

        LogitechGSDK.LogiSetPreferredControllerProperties(properties);//�㏑��
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
        /*�f�o�b�O���O--------------------------------------*/

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
    /*���͂�ϊ����ݒ肷��֐�----------------*/
    public void Inputs(/*Action shiftChange*/)
    {
        //if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)/*�n���R�������Ă��邩�ǂ���*/)
        //{
        StringBuilder deviceName = new StringBuilder(256);//�n���R�����擾
        LogitechGSDK.LogiGetFriendlyProductName(0, deviceName, 256);

        if (deviceName.ToString() == "Logicool G29 Driving Force Racing Wheel USB")//�n���R������
        {
            logicoolG29 = true;
        }
        else
        {
            logicoolG29 = false;
        }

        rec = LogitechGSDK.LogiGetStateUnity(0);//�n���R���l
        Func<int, int, bool> PushJudge = (buttonX, buttonY) => { return ((rec.rgbButtons[buttonX] == PUSH) || (rec.rgbButtons[buttonY] == PUSH)) ? true : false; };//���͔���̊֐�

        Accel = RoundValue(rec.lY);//�A�N�Z���l
        if (logicoolG29)
        {
            if (BRAKE < RoundValue(rec.lRz))//�u���[�L�l
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

        Clutch = RoundValue(rec.rglSlider[0]);//�N���b�`�l
        if (RANGE > Mathf.Abs(rec.lX))//�n���h���l
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

        if (logicoolG29)//�V�t�g���o�[����
        {
            for (int i = 1, j = LOW; j <= REVERSE; ++i, ++j)//G29�̃V�t�g���o�[
            {
                if (rec.rgbButtons[j] == PUSH)
                {
                    if (i == 7) //R���{�^��18�̂��߁A7�ɂ����6�̎��ɂȂ��Ă��܂��A�Q�[���̎d�l�ł�1�̑O�ɂȂ�ׁA�s���悭���邽��
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
            isShiftLever = PushJudge(UP, DOWN);//���͔���
        }
        /*
         1��->1
         2��->2
         3��->3
         4��->4
         5��->5
         6��->6
         R  ->7->0
         */

        isShiftPaddle = PushJudge(RIGHT, LEFT);//���͔���

        /*
         R  ->0
         1��->1
         2��->2
         3��->3
         4��->4
         5��->5
         6��->6
         */

        if (!preIsShiftLever && isShiftLever)//���o�[
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
        else if (!preIsShiftPaddle && isShiftPaddle) //�p�h���V�t�g
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

        preIsShiftLever = isShiftLever;//�O��X�V
        preIsShiftPaddle = isShiftPaddle;
    }
}