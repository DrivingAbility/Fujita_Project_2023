using System;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityStandardAssets.CrossPlatformInput;


[RequireComponent(typeof(CarController))]
public class CarUserControl : MonoBehaviour
{
    private CarController m_Car; // the car controller we want to use
    private PlayerInput playerInput;
    [NonSerialized] public float v2;
    [NonSerialized] public float steering;
    private bool isFirstShiftDrive = false;
    private bool isShiftDrive = false;
    private bool isShiftReverse = false;
    float sdkMax = 32767f;
    LogitechGSDK.DIJOYSTATE2ENGINES rec;
    static public bool isHit = false;
    private void Awake()
    {
        // get the car controller
        m_Car = GetComponent<CarController>();
        playerInput = FindObjectOfType<PlayerInput>();
        LogitechGSDK.LogiSteeringInitialize(false);
    }
    private void FixedUpdate()
    {
        float v = 0f;
        float b = 0f;
        steering = 0.0f;
        var steeringVector2 = playerInput.currentActionMap["Horizontal"].ReadValue<Vector2>();
        var h1 = Vector2.SignedAngle(steeringVector2, Vector2.up);

        if ((LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)))
        {
            rec = LogitechGSDK.LogiGetStateUnity(0);
            //LogitechGSDK.LogiPlaySpringForce(0, 0, 30, 100);
            steering = rec.lX / sdkMax;
            isShiftDrive = (rec.rgbButtons[12] == 128) ? true : false;
            isShiftReverse = (rec.rgbButtons[17] == 128) ? true : false;
            float accel = rec.lY / sdkMax;
            float brake = rec.lRz / sdkMax;
            accel = -1 * (accel + 1) / 2 + 1;
            brake = (brake + 1) / 2 - 1;
            v = accel;
            b = brake;
            FirstShiftDrive();
            if (!isFirstShiftDrive) v = b = 0f;
            if (!isShiftDrive && !isShiftReverse) v = 0;
            if (isShiftReverse) v *= -0.5f;
        }
        //v += v1 + v2;
        //b = v1 + b;
        //steering = h1 + steering;
        if (Mathf.Abs(steering) < 0.001f) steering = 0;
        else steering = (steering - 0.01f) / 0.99f;
        //v = 1;
        //steering = 0;
        //m_Car.Move(steering, v1, b);
    }
    public void OnVertical(InputValue value)
    {
        var v = value.Get<float>();
        print(v);
    }
    private void Update()
    {

    }
    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        LogitechGSDK.LogiSteeringShutdown();
    }
    private void FirstShiftDrive()
    {
        if (isFirstShiftDrive) return;
        isFirstShiftDrive = (rec.rgbButtons[12] == 128) ? true : false;
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Obstacle"))
        {
            isHit = true;
        }
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Obstacle"))
        {
            isHit = false;
        }
    }
}


