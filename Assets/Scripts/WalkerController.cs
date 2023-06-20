using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkerController : MonoBehaviour
{
    private CarController _player;
    private Vector2 _smoothDeltaPosition = Vector2.zero;
    private Vector2 _velocity = Vector2.zero;
    private Vector3 _targetPos;
    private NavMeshAgent _agent;
    private Animator _animator;
    private float _animationBlend;
    [SerializeField] private float _speedChangeRate;
    int _animIDSpeed;
    int _animIDMotionSpeed;
    bool _hasAnimator;
    // Start is called before the first frame update
    void Start()
    {
        _player = FindAnyObjectByType<CarController>();
        _agent = GetComponent<NavMeshAgent>();

        _targetPos = transform.position + 140 * Vector3.forward;
        _agent.updatePosition = false;

        _hasAnimator = TryGetComponent<Animator>(out _animator);
        AssignAnimationIDs();
    }
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    void Move()
    {
        _hasAnimator = TryGetComponent<Animator>(out _animator);
        Vector3 worldDeltaPosition = _agent.nextPosition - transform.position;

        // worldDeltaPosition をローカル空間にマップします
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // deltaMove にローパスフィルターを適用します
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        _smoothDeltaPosition = Vector2.Lerp(_smoothDeltaPosition, deltaPosition, smooth);
        // 時間が進んだら、velocity (速度) を更新します
        if (Time.deltaTime > 1e-5f) _velocity = _smoothDeltaPosition / Time.deltaTime;
        bool shouldMove = _velocity.magnitude > 0.5f && _agent.remainingDistance > _agent.radius;

        _animationBlend = Mathf.Lerp(_animationBlend, _agent.speed, Time.deltaTime * _speedChangeRate);
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, deltaPosition.sqrMagnitude);
            _animator.SetFloat(_animIDMotionSpeed, 1.0f);
        }
    }
    void OnAnimatorMove()
    {
        // position (位置) を agent (エージェント) の位置に更新します
        transform.position = _agent.nextPosition;
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
    private void OnFootstep()
    {
        return;//Error解消
    }
}
