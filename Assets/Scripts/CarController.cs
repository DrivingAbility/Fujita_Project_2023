using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 649

internal enum CarDriveType
{
    FrontWheelDrive,
}
[ExecuteAlways]
public class CarController : MonoBehaviour
{
    [SerializeField] private CarDriveType m_CarDriveType = CarDriveType.FrontWheelDrive;
    [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
    [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
    [SerializeField] private Vector3 m_CentreOfMassOffset;
    [SerializeField] private float m_MaximumSteerAngle;
    [SerializeField] private float m_FullTorqueOverAllWheels;
    [SerializeField] private float m_ReverseTorque;
    [SerializeField] private float m_Downforce = 100f;
    public float m_Topspeed = 200;
    private float _creepSpeed = 10f;
    [SerializeField] private float m_BrakeTorque;
    [SerializeField] GameObject handleObj;

    private Quaternion[] m_WheelMeshLocalRotations;
    private float m_SteerAngle;
    private float _currentTorque;
    [NonSerialized] public Rigidbody m_Rigidbody;
    public float BrakeInput { get; private set; }
    public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 3.6f; } }
    public float MaxSpeed { get { return m_Topspeed; } }
    public float AccelInput { get; private set; }
    public float SteeringInput { get; private set; }
    public bool CarIsHitting { get; private set; }
    public static Collider MeshCollider;

    private Transform _meterTf;

    private void Awake()
    {

    }
    // Use this for initialization
    private void Start()
    {
        m_WheelMeshLocalRotations = new Quaternion[4];
        MeshCollider = GetComponentInChildren<MeshCollider>();
        for (int i = 0; i < 4; i++)
        {
            if (m_WheelMeshes[i] != null)
            {
                m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }
        }

        m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

        m_Rigidbody = GetComponent<Rigidbody>();
        _currentTorque = m_FullTorqueOverAllWheels;

        FindMeter();
    }
    private void FindMeter()
    {
        string s = "Meter_Needle";
        List<Transform> list = GetAllChildren.GetAll(transform);
        foreach (Transform tf in list)
        {
            if (tf.gameObject.name == s)
            {
                _meterTf = tf;
            }
        }
        if (_meterTf == null) Debug.Log("Can't Find 'Meter_Needle'");
    }

    public void Move(float steering, float accel, float footbrake)
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            m_WheelColliders[i].GetWorldPose(out position, out quat);
            if (m_WheelMeshes[i] != null)
            {
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }
        }

        //clamp input values
        SteeringInput = Mathf.Clamp(steering, -1, 1);
        AccelInput = accel = Mathf.Clamp(accel, -1, 1);
        BrakeInput = footbrake = Mathf.Clamp(footbrake, 0, 1);

        RotateSteering(SteeringInput);

        //Set the steer on the front wheels.
        //Assuming that wheels 0 and 1 are the front wheels.
        m_SteerAngle = SteeringInput * m_MaximumSteerAngle;
        m_WheelColliders[0].steerAngle = m_SteerAngle;
        m_WheelColliders[1].steerAngle = m_SteerAngle;

        ApplyDrive(accel, footbrake);
        CapSpeed();

        AddDownForce();

        ControllMeterNeedle();
    }

    //速度上限制御
    private void CapSpeed()
    {
        float speed = m_Rigidbody.velocity.magnitude;
        speed *= 3.6f;
        if (speed > m_Topspeed) m_Rigidbody.velocity = (m_Topspeed / 3.6f) * m_Rigidbody.velocity.normalized;
    }

    private void ApplyDrive(float accel, float footbrake)
    {

        float thrustTorque;
        switch (m_CarDriveType)
        {
            case CarDriveType.FrontWheelDrive:
                thrustTorque = Torque(accel);
                m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 0)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
            }
            else if (footbrake > 0)
            {
                m_WheelColliders[i].brakeTorque = 0f;
            }
        }
    }
    private float Torque(float accel)
    {
        float torque = _currentTorque;
        if (accel == 0)
        {
            if (CurrentSpeed > _creepSpeed)
            {
                torque = 0;
            }
            else
            {
                torque = _currentTorque / 4f;
            };
        }
        else
        {
            torque = accel * _currentTorque / 2f;
        }
        return torque;
    }
    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce *
                                                     m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
    }
    private void ControllMeterNeedle()
    {
        if (_meterTf == null) return;

        Vector3 localAngle = _meterTf.eulerAngles;
        float angleScale = -169f / 100f;

        localAngle.z = CurrentSpeed * angleScale;
        _meterTf.eulerAngles = localAngle;
    }
    private void RotateSteering(float h)
    {
        if (handleObj == null) return;
        handleObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -1 * h * 30));
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        CarIsHitting = true;
        StartCoroutine(HittingSwitch());
    }
    private IEnumerator HittingSwitch()
    {
        yield return null;
        CarIsHitting = false;
    }
}

