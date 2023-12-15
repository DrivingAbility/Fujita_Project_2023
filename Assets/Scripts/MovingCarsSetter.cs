using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MovingCarsSetter : MonoBehaviour
{
    [SerializeField] float _distInterval = 100;
    [SerializeField] bool _isActiveCollider;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = _isActiveCollider;
            }

            var childPositionZ = _distInterval;
            foreach (Transform child in transform)
            {
                var childPos = child.position;
                childPos.z = childPositionZ;
                child.position = childPos;
                childPositionZ += _distInterval;
            }
        }
    }
    void OnValidate()
    {

    }
}
