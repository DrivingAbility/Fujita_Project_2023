using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 startPos;
    float zDistance;
    [SerializeField] Transform targetTf;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        zDistance = startPos.z - targetTf.position.z;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos = startPos;
        pos.z = targetTf.position.z + zDistance;
        transform.position = pos;
    }
}
