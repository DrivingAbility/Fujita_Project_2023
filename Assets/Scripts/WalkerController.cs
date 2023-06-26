using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class LookAtWeight
{
    [SerializeField] private float _bodyWeight;
    [SerializeField] private float _headWeight;
    [SerializeField] private float _eyeWeight;
    [SerializeField] private float _clampWeight;

    public float BodyWeight { get => _bodyWeight; }
    public float HeadWeight { get => _headWeight; }
    public float EyeWeight { get => _eyeWeight; }
    public float ClampWeight { get => _clampWeight; }
}
public class WalkerController : MonoBehaviour
{
    [SerializeField] private Transform _head;
    [SerializeField] private LookAtWeight _weightParams;
    [SerializeField] private float _lookAtBoarder = 5;
    [SerializeField] private float _weightRatio = 5.0f;
    [SerializeField] private float _avoidanceAngle = 45;
    private Vector3 _lookAtTargetPosition;
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

        if (_head)
        {
            _lookAtTargetPosition = _head.position + transform.position;
        }
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
        transform.position = _agent.nextPosition;
    }
    void OnAnimatorMove()
    {
        // position (位置) を agent (エージェント) の位置に更新します

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
        if (!_myCar) return;
        _lookAtTargetPosition = _myCar.transform.position;
        //_lookAtTargetPosition.y = _head.position.y;
        var isNear = Vector3.Distance(_lookAtTargetPosition, transform.position) < _lookAtBoarder;
        var lerpTarget = (!_looked && isNear) ? 1 : 0;
        _lookAtWeight = Mathf.Lerp(_lookAtWeight, lerpTarget, Time.deltaTime * _weightRatio);
        _animator.SetLookAtWeight(_lookAtWeight, _weightParams.BodyWeight, _weightParams.HeadWeight,
            _weightParams.EyeWeight, _weightParams.ClampWeight);
        _animator.SetLookAtPosition(_lookAtTargetPosition);
        if (!_looked && _lookAtWeight > 0.95f)
        {
            _looked = true;
        }
    }
}
