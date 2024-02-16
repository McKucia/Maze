using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] float _viewAngle = 90f;
    [SerializeField] float _attackAngle = 20f;
    [SerializeField] float _sightRange = 3f;
    [SerializeField] float _hearRange = 1f;
    [SerializeField] float _attackRange = 1f;
    [SerializeField] float _searchingTime = 10f;
    [SerializeField] float _restingTime = 3f;
    [SerializeField] float _attackTime = 1f;
    [SerializeField] float _runSpeed = 5f;
    [SerializeField] float _patrolSpeed = 1f;
    [SerializeField] NavMeshAgent _agent;
    [SerializeField] GameObject _missilePrefab;
    // [SerializeField] CircleFillHandler _followingCircleBar;
    float _runAngularSpeed = 400f;
    float _patrolAngularSpeed = 300f;

    Transform _player;

    // Patroling
    Vector3 _walkPoint;
    bool _walkPointSet;
    float _restingTimeElapsed;
    Room _room;

    // Following
    float _searchingTimeElapsed;

    // Attacking
    float _attackTimeElapsed;

    // States
    bool _isFollowing = false;
    bool _isResting = false;
    bool _isAttacking = false;

    bool _init = false;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();    
    }

    void Update()
    {
        if (!GameManager.Instance.Initialized) return;
        if (!_init)
        {
            _player = GameObject.FindWithTag("Player").transform;
            _init = true;
        }

        if (!_isFollowing)
        {
            if (CanSeeTarget() || CanHearTarget())
            {
                _isFollowing = true;
                // _followingCircleBar.fillValue = 100;
                _agent.speed = _runSpeed;
                _agent.angularSpeed = _runAngularSpeed;
                _walkPointSet = false;
            }
            else
                Patroling();
        }
        else
        {
            ChasePlayer();

            if(!CanSeeTarget() && !CanHearTarget())
            {
                _searchingTimeElapsed += Time.deltaTime;
                // _followingCircleBar.fillValue = (_searchingTime - _searchingTimeElapsed) * 10;
                if (_searchingTimeElapsed >= _searchingTime)
                {
                    _searchingTimeElapsed = 0;
                    _isFollowing = false;
                    _agent.speed = _patrolSpeed;
                    _agent.angularSpeed = _patrolAngularSpeed;
                    // _followingCircleBar.fillValue = 0;
                }
            }
            else
            {
                //_followingCircleBar.fillValue = 100;
                _searchingTimeElapsed = 0;
            }
        }
    }

    void Patroling()
    {
        if (!_walkPointSet && !_isResting)
            SearchWalkPoint();

        if (_isResting)
        {
            _restingTimeElapsed += Time.deltaTime;
            if (_restingTimeElapsed >= _restingTime)
            { 
                _restingTimeElapsed = 0;
                _isResting = false;
            }

            return;
        }

        Vector3 distanceToWalkPoint = transform.position - _walkPoint;

        if (distanceToWalkPoint.magnitude < 0.02f)
        {
            _isResting = true;
            _walkPointSet = false;
        }
    }

    void SearchWalkPoint()
    {
        float randomX = Random.Range(_room.Position.x, _room.Position.x + _room.Size.x);
        float randomZ = Random.Range(_room.Position.y, _room.Position.y + _room.Size.y);

        _walkPoint = new Vector3(randomX, transform.position.y, randomZ);
        _agent.SetDestination(_walkPoint);
        _walkPointSet = true;
    }

    void ChasePlayer()
    {
        _agent.SetDestination(_player.position);

        if (InRange(_attackRange))
        {
            _agent.isStopped = true;
            if (!InAngle(_attackAngle))
            {
                var lookPos = _player.position - transform.position;
                lookPos.y = 0;
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 6f);

                return;
            }
            else
            {
                AttackPlayer();
            }
        }
        else
        {
            _agent.isStopped = false;
            _attackTimeElapsed = 0;
        }
    }

    void AttackPlayer()
    {
        _attackTimeElapsed += Time.deltaTime;

        if (_attackTimeElapsed >= _attackTime)
        {
            var missile = Instantiate(_missilePrefab, transform.position, Quaternion.identity);
            missile.GetComponent<Missile>().Target = _player;
            _attackTimeElapsed = 0;
        }
    }

    bool InRange(float range)
    {
        Vector3 toTarget = _player.position - transform.position;

        return Physics.Raycast(transform.position, toTarget, out RaycastHit hit, range) &&
            hit.transform.CompareTag("Player");
    }

    bool InAngle(float angle)
    {
        Vector3 toTarget = _player.position - transform.position;

        return Vector3.Angle(transform.forward, toTarget) <= angle;
    }

    bool CanSeeTarget() { return InAngle(_viewAngle) && InRange(_sightRange); }

    bool CanHearTarget() { return InRange(_hearRange); }

    public void SetRoom(Room room) => _room = room;
}