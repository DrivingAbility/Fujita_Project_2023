using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SampleInputDirector : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _text;
    CarController _car;
    // Start is called before the first frame update
    void Start()
    {
        _car = FindAnyObjectByType<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        var str = "Steering : " + _car.SteeringInput.ToString("f2");
        _text.text = str;
    }
}
