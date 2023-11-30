using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SteeringDebugger : MonoBehaviour
{
    [SerializeField] RectTransform _steeringImageTf;
    [SerializeField] TextMeshProUGUI _steeringText;
    CarController _car;
    // Start is called before the first frame update
    void Start()
    {
        _car = FindObjectOfType<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        SteeringDebugControll();
    }
    void SteeringDebugControll()
    {
        var steering = _car.SteeringInput * 450;
        _steeringImageTf.rotation = Quaternion.Euler(0, 0, -steering);
        _steeringText.text = steering.ToString("f1");
    }
}
