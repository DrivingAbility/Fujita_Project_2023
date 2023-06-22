using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkerController : MonoBehaviour
{
    [SerializeField] private Transform _head;
    [SerializeField] private float _lookAtBoarder = 50.0f;
    [SerializeField] private Transform _lookAtTargetTransform;
    [SerializeField] Vector3 _lookAtTargetPosition;
    private float _weightRatio = 2.5f;
    float _lookAtCoolTime = 0.2f;
    public float _lookAtHeatTime = 0.2f;
    public bool _looking = true;
    private bool _looked = false;
    private float _lookAtWeight = 0.0f;

    private Vector3 _lookAtPosition;
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
            _lookAtPosition = _lookAtTargetPosition;
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
                prepos = position;
            }
        }
    }
    private void OnAnimatorIK()
    {
        _lookAtTargetPosition = _lookAtTargetTransform.position;
        _lookAtTargetPosition.y = _head.position.y;
        int lerpTarget;
        if (!_looked)
        {
            lerpTarget = 1;
        }
        else
        {
            lerpTarget = 0;
        }
        Debug.Log(_lookAtWeight);
        _lookAtWeight = Mathf.Lerp(_lookAtWeight, lerpTarget, Time.deltaTime * _weightRatio);
        _animator.SetLookAtWeight(_lookAtWeight, 0.2f, 0.5f, 0.7f, 0.2f);
        _animator.SetLookAtPosition(_lookAtTargetPosition);
        if (_lookAtWeight / lerpTarget > 0.9999f)
        {
            _looked = true;
        }
    }
}
