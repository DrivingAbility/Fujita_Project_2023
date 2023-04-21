using System;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityStandardAssets.CrossPlatformInput;


[RequireComponent(typeof(CarController))]
public class CarUserControl : MonoBehaviour
{
    private CarController _car; // the car controller we want to use
    private PlayerInput _playerInput;
    private float _vertical;
    private float _horizontal;
    private float _brake;
    private void Awake()
    {
        // get the car controller
        _car = GetComponent<CarController>();
        _playerInput = FindObjectOfType<PlayerInput>();
    }
    private void FixedUpdate()
    {
        _car.Move(_horizontal, _vertical, _brake);
    }
    public void OnVertical(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<float>();
        _vertical = value;
    }
    public void OnHorizontal(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        var normalizedScale = 45;
        _horizontal = Vector2.SignedAngle(value, Vector2.up) / normalizedScale;
    }
    public void OnBrake(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<float>();
        _brake = value;
    }
    private void Update()
    {

    }
    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
    }
}


