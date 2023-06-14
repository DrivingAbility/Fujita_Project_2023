using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestPointer : MonoBehaviour
{
    [SerializeField] Transform _player;
    [SerializeField] Transform _targetTf;
    Vector3 _targetClosestPoint;
    Vector3 _playerClosestPoint;
    Collider _targetCollider;
    Collider _playerCollider;
    // Start is called before the first frame update
    void Start()
    {
        ClosestPointStart();
    }
    private void ClosestPointStart()
    {
        _playerCollider = _player.GetComponent<Collider>();

        Vector3 rayOffsetPosition = new Vector3(0, 0.4f, 0);
        Vector3 direction = _targetTf.position - _player.position;
        RaycastHit hit;
        if (Physics.Raycast(_player.position + rayOffsetPosition, direction, out hit, Mathf.Infinity))
        {
            if (_targetCollider == null)
            {
                _targetCollider = hit.collider;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        ClosestPointing();
    }
    private void ClosestPointing()
    {
        _targetClosestPoint = _targetCollider.ClosestPoint(_player.position);
        _playerClosestPoint = _playerCollider.ClosestPoint(_targetClosestPoint);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(_playerClosestPoint, 0.1f);
        Gizmos.DrawSphere(_targetClosestPoint, 0.1f);
        Gizmos.DrawLine(_playerClosestPoint, _targetClosestPoint);
    }
}
