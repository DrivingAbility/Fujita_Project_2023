using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CpuCarController : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private float _velocityPerHour = 60.0f;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.mass = 1000;
        _rb.velocity = transform.forward * _velocityPerHour * 1000.0f / 3600.0f;
    }
}
