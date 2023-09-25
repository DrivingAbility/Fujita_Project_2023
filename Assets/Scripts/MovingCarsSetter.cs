using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCarsSetter : MonoBehaviour
{
    [SerializeField]float _distInterval=100;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnValidate(){
        var childPositionZ=_distInterval;
        foreach(Transform child in transform){
            var childPos=child.position;
            childPos.z=childPositionZ;
            child.position=childPos;
            childPositionZ+=_distInterval;
        }
    }
}
