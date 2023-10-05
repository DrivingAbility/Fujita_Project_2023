using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelLineController : MonoBehaviour
{
    [SerializeField] float _startWidth;
    [SerializeField] float _endRatio = 1.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnValidate()
    {
        LineRenderer[] lines = GetComponentsInChildren<LineRenderer>();
        for (int i = 0; i < lines.Length; i++)
        {
            Vector3 direction = lines[i].GetPosition(lines[i].positionCount - 1) - lines[i].GetPosition(0);
            switch (direction.normalized)
            {
                case new Vector3(0, 1, 0):
                    break;
                case Vector3.right:
                    break;
                case Vector3.forward:
                    break;
            }
            Debug.Log(direction.normalized == Vector3.up);
        }
    }
}
