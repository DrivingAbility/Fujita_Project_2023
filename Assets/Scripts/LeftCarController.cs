using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftCarController : MonoBehaviour
{
    [SerializeField] int _standardDist = 10;
    [SerializeField] float _moveRatio = 0.01f;
    CarController _playerCar;
    // Start is called before the first frame update
    void Start()
    {
        _playerCar = FindAnyObjectByType<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerCar)
        {
            var distance = _playerCar.transform.position.z - transform.position.z;
            var ratio = (distance - _standardDist) * _moveRatio;
            transform.position = new Vector3(_playerCar.transform.position.x, 0, transform.position.z + distance * ratio);
        }
    }
}
