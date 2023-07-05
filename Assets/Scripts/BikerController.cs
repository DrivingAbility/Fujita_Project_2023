using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SBPScripts;

public class BikerController : MonoBehaviour
{
    [SerializeField] float _agentSpeed = 8.333333333f;
    private NavMeshAgent _agent;
    private BicycleController _bicycle;
    private CPUDirector _cpuDirector;
    private int _forwardSign;
    private Vector3 _targetPos;
    private Vector3 _startPosition;
    private Vector3 _velocity;
    private float _changeTargetRemaining = 3;
    public float InputVertical { get; private set; }
    public float InputHorizontal { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _bicycle = GetComponent<BicycleController>();
        _cpuDirector = FindObjectOfType<CPUDirector>();

        _startPosition = transform.position;
        _targetPos = GetDestinationPosition();
        _agent.destination = _targetPos;
        _agent.updatePosition = false;
        _agent.speed = _agentSpeed;
        _agent.avoidancePriority = _cpuDirector.Priority.BikeAgentPriority;

        _forwardSign = (int)ForwardSign();
    }
    private Vector3 GetDestinationPosition()
    {
        int i = 0;
        Vector3 targetPos = Vector3.zero;
        while (targetPos.z <= transform.position.z)
        {
            targetPos = new Vector3(_startPosition.x, 0, i * _cpuDirector.CrossInterval);
            i++;
        }
        if (_forwardSign == -1)
        {
            targetPos.z -= _cpuDirector.CrossInterval;
        }
        return targetPos;
    }
    private float ForwardSign()
    {
        return Mathf.Sign(Vector3.Dot(Vector3.forward, transform.forward));
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        SetDestinationUpdate();
    }
    void Move()
    {
        Vector3 worldDeltaPosition = _agent.nextPosition - transform.position;

        // worldDeltaPosition をローカル空間にマップします
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dz = Vector3.Dot(transform.forward, worldDeltaPosition);

        Vector3 deltaPosition = new Vector3(dx, 0, dz);
        // velocity (速度) を更新します
        _velocity = deltaPosition / Time.deltaTime;
        InputVertical = _velocity.z;
        _agent.nextPosition = transform.position;
    }
    private void SetDestinationUpdate()
    {
        if (Mathf.Abs(transform.position.z - _targetPos.z) > _changeTargetRemaining) return;
        _targetPos.z += _forwardSign * _cpuDirector.CrossInterval;
        _agent.destination = _targetPos;
    }
    void OnDrawGizmos()
    {
        if (_agent && _agent.enabled)
        {
            Gizmos.color = Color.red;
            Vector3 prepos = transform.position;

            foreach (Vector3 position in _agent.path.corners)
            {
                Gizmos.DrawLine(prepos, position);
                Gizmos.DrawSphere(position, 0.1f);
                prepos = position;
            }
        }
    }
}
