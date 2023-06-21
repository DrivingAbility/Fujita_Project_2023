using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkerController : MonoBehaviour
{
    [SerializeField] private Transform trs;
    private CarController _player;
    private Vector2 _smoothDeltaPosition = Vector2.zero;
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
        //_player = FindAnyObjectByType<CarController>();
        _agent = GetComponent<NavMeshAgent>();

        _targetPos = transform.position + 130 * Vector3.forward;
        _targetPos = trs.position;
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
    private void OnFootstep()
    {
        return;//Error解消
    }
}
