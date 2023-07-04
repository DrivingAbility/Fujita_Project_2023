using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class LookAtWeight
{
    [SerializeField] private float _maxWeight = 0.95f;
    [SerializeField] private float _minWeight = 0.05f;
    [SerializeField] private float _bodyWeight = 0.05f;
    [SerializeField] private float _headWeight = 0.5f;
    [SerializeField] private float _eyeWeight = 0.8f;
    [SerializeField] private float _clampWeight = 0.05f;
    [SerializeField] private float _weightAccelRatio = 5.0f;

    public float MaxWeight { get => _maxWeight; }
    public float MinWeight { get => _minWeight; }
    public float BodyWeight { get => _bodyWeight; }
    public float HeadWeight { get => _headWeight; }
    public float EyeWeight { get => _eyeWeight; }
    public float ClampWeight { get => _clampWeight; }
    public float WeightAccelRatio { get => _weightAccelRatio; }
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class WalkerController : MonoBehaviour
{
    [SerializeField] private LookAtWeight _weightParams;
    private CPUDirector _cpuDirector;
    [SerializeField] private float _lookStartDistance = 15.0f;
    [SerializeField] private float _avoidEndDistance = 5.0f;
    [SerializeField] private float _lookAtTargetHeight = 0.5f;
    [SerializeField] private float _avoidanceAngle = 45.0f;
    [SerializeField] private float _agentSpeed = 1.2f;
    [SerializeField] private float _changeTargetRemaining = 3.0f;
    [SerializeField] private float _navigationBorder = -4.0f;
    private Vector3 _lookAtTargetPosition;
    private bool _isWeightOverMax = false;
    private bool _looked = false;
    private float _lookAtWeight = 0.0f;
    private CarController _myCar;
    private Vector2 _velocity = Vector2.zero;
    private Vector3 _targetPos = Vector3.zero;
    private NavMeshAgent _agent;
    private Animator _animator;
    private CharacterController _charController;
    int _animIDVelocityX;
    int _animIDVelocityY;
    bool _hasAnimator;
    int _forwardSign;
    private Vector3 _startPosition;
    private int _startPriority;


    // Start is called before the first frame update
    void Start()
    {
        _charController = GetComponent<CharacterController>();
        _agent = GetComponent<NavMeshAgent>();
        _cpuDirector = FindObjectOfType<CPUDirector>();
        _myCar = FindObjectOfType<CarController>();

        _charController.stepOffset = 0;

        _forwardSign = (int)ForwardSign();

        _startPosition = transform.position;
        _targetPos = GetDestinationPosition();
        _agent.destination = _targetPos;

        _agent.updatePosition = false;

        _startPriority = GetPriority();
        _agent.avoidancePriority = _startPriority;

        _agent.speed = _agentSpeed;
        _hasAnimator = TryGetComponent<Animator>(out _animator);
        AssignAnimationIDs();
    }
    private void AssignAnimationIDs()
    {
        _animIDVelocityX = Animator.StringToHash("velx");
        _animIDVelocityY = Animator.StringToHash("vely");
    }
    private int GetPriority()
    {
        int priority = 0;
        int halfMin = _cpuDirector.Priority.MinAgentPriority / 2;
        int halfMax = _cpuDirector.Priority.MaxAgentPriority / 2;
        priority = 2 * Random.Range(halfMin, halfMax) + (_forwardSign + 1) / 2;
        return priority;
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
        _agent.avoidancePriority = _startPriority;

        if (_looked && _myCar)
        {
            var direction = transform.position - _myCar.transform.position;
            var angleRatio = Mathf.Clamp((transform.position.x - _cpuDirector.NavMeshAreaBorder) / 1, 0, 1);
            if (direction.z > -_avoidEndDistance && angleRatio > 0)
            {
                _agent.avoidancePriority = _cpuDirector.Priority.LookedAgentPriority + _forwardSign;

                direction = new Vector3(Mathf.Sign(transform.position.x) * Mathf.Sin(_avoidanceAngle * Mathf.Deg2Rad * angleRatio), 0, _forwardSign * Mathf.Cos(_avoidanceAngle * Mathf.Deg2Rad * angleRatio));
                _agent.nextPosition = transform.position + _agent.speed * direction.normalized * Time.deltaTime;
            }
        }
        Vector3 worldDeltaPosition = _agent.nextPosition - transform.position;

        // worldDeltaPosition をローカル空間にマップします
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);
        // velocity (速度) を更新します
        _velocity = deltaPosition / Time.deltaTime;

        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDVelocityX, _velocity.x);
            _animator.SetFloat(_animIDVelocityY, _velocity.y);
        }

        transform.position = new Vector3(_agent.nextPosition.x, 0, _agent.nextPosition.z);
    }

    /*キャラクターコントローラーで制御
    private void Move2()
    {
        _desVelocity = new Vector3(_agent.desiredVelocity.x, 0, _agent.desiredVelocity.z);
        if (_looked && _myCar)
        {
            var distanceZ = (transform.position - _myCar.transform.position).z;
            if (distanceZ > -_avoidEndDistance)
            {
                var direction = new Vector3(Mathf.Sign(transform.position.x) * Mathf.Sin(_avoidanceAngle * Mathf.Deg2Rad), 0, Mathf.Cos(_avoidanceAngle * Mathf.Deg2Rad));
                _desVelocity = direction.normalized * _desVelocity.magnitude;
            }
        }
        float dx = Vector3.Dot(transform.right, _agent.velocity);
        float dy = Vector3.Dot(transform.forward, _agent.velocity);
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDVelocityX, dx);
            _animator.SetFloat(_animIDVelocityY, dy);
        }
        print(transform.position.y);
        _charController.Move(_desVelocity * Time.deltaTime);
        _agent.velocity = _charController.velocity;
    }*/
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
    private void OnAnimatorIK()
    {
        LookAtIK();
    }
    private void LookAtIK()
    {
        if (!_myCar) return;
        _lookAtTargetPosition = _myCar.transform.position + _lookAtTargetHeight * Vector3.up;
        var isNear = Vector3.Distance(_lookAtTargetPosition, transform.position) < _lookStartDistance;
        var lerpTarget = (!_isWeightOverMax && isNear) ? 1 : 0;
        _lookAtWeight = Mathf.Lerp(_lookAtWeight, lerpTarget, Time.deltaTime * _weightParams.WeightAccelRatio);
        _animator.SetLookAtWeight(_lookAtWeight, _weightParams.BodyWeight, _weightParams.HeadWeight,
            _weightParams.EyeWeight, _weightParams.ClampWeight);
        _animator.SetLookAtPosition(_lookAtTargetPosition);
        if (!_looked && _lookAtWeight > _weightParams.MaxWeight)
        {
            _isWeightOverMax = true;
        }
        if (_isWeightOverMax && _lookAtWeight < _weightParams.MinWeight)
        {
            _looked = true;
        }
    }
}
