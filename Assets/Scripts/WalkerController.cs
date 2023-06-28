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

    public float MaxWeight { get => _maxWeight; }
    public float MinWeight { get => _minWeight; }
    public float BodyWeight { get => _bodyWeight; }
    public float HeadWeight { get => _headWeight; }
    public float EyeWeight { get => _eyeWeight; }
    public float ClampWeight { get => _clampWeight; }
}
public class WalkerController : MonoBehaviour
{
    [SerializeField] private LookAtWeight _weightParams;
    [SerializeField] private float _lookAtBoarder = 10;
    [SerializeField] private float _lookAtTargetHeight = 1.0f;
    [SerializeField] private float _weightRatio = 5.0f;
    [SerializeField] private float _avoidanceAngle = 45;
    private Vector3 _lookAtTargetPosition;
    private bool _isWeightOverMax = false;
    private bool _looked = false;
    private float _lookAtWeight = 0.0f;
    private CarController _myCar;
    private Vector2 _velocity = Vector2.zero;
    private Vector3 _targetPos;
    private NavMeshAgent _agent;
    private Animator _animator;
    int _animIDVelocityX;
    int _animIDVelocityY;
    bool _hasAnimator;


    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _myCar = FindObjectOfType<CarController>();
        _targetPos = transform.position + 130 * Vector3.forward;

        _agent.destination = _targetPos;
        _agent.updatePosition = false;
        _hasAnimator = TryGetComponent<Animator>(out _animator);
        AssignAnimationIDs();
    }
    private void AssignAnimationIDs()
    {
        _animIDVelocityX = Animator.StringToHash("velx");
        _animIDVelocityY = Animator.StringToHash("vely");
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    void Move()
    {
        if (_looked && _myCar)
        {
            var direction = transform.position - _myCar.transform.position;
            if (direction.z > 0)
            {
                direction = new Vector3(Mathf.Sign(transform.position.x) * Mathf.Sin(_avoidanceAngle * Mathf.Deg2Rad), 0, Mathf.Cos(_avoidanceAngle * Mathf.Deg2Rad));
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
        var isNear = Vector3.Distance(_lookAtTargetPosition, transform.position) < _lookAtBoarder;
        var lerpTarget = (!_isWeightOverMax && isNear) ? 1 : 0;
        _lookAtWeight = Mathf.Lerp(_lookAtWeight, lerpTarget, Time.deltaTime * _weightRatio);
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
