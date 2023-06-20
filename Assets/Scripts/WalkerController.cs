using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkerController : MonoBehaviour
{
    private Vector3 _targetPos;
    private NavMeshAgent _agent;
    private Animator _animator;
    int _animIDSpeed;
    int _animIDMotionSpeed;
    bool _hasAnimator;
    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updatePosition = false;
        _agent.updateRotation = false;
        _targetPos = transform.position + 140 * Vector3.forward;

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
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, 2.0f);
            _animator.SetFloat(_animIDMotionSpeed, 1.0f);
        }

        _agent.destination = _targetPos;
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
