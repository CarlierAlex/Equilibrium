using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

public class Enemy_M_Siege : MonoBehaviour
{
    private Enemy_Base _baseScript;
    private Transform _currTarget;
    private Transform _rune;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Transform _transform;

    private const float ACCELERATION = 8.0f;
    private const float MAX_ANGULAR_SPEED = 120f;
    [SerializeField]
    private float _moveSpeed = 3.5f;
    [SerializeField]
    private float _stopDistance = 1.0f;

    [SerializeField]
    private float NAVMESH_RADIUS = 0.5f;
    [SerializeField]
    private float NAVMESH_HEIGHT = 2f;
    [SerializeField]
    private float NAVMESH_BASE_OFFSET = 1f;

    [SerializeField]
    private float _attackDotFOV = 0.95f;
    [SerializeField]
    private float _attackRange = 3.0f;
    [SerializeField]
    private float _attackRangeRune = 3.0f;

    private const float WANDER_RADIUS = 7.0f;
    private const float WANDER_ANGLE = 45.0f;

    private bool _doesAttack = false;
    private bool _staggered = false;
    private EnemyState _state = EnemyState.IDLE;
    private bool _postInitialize = false;

    [SerializeField]
    private bool _knockBackAttack = true;

    // Use this for initialization
    void Start()
    {
        _baseScript = this.gameObject.GetComponent<Enemy_Base>();
        _navMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        _transform = this.GetComponent<Transform>();

        _navMeshAgent.radius = NAVMESH_RADIUS;
        _navMeshAgent.height = NAVMESH_HEIGHT;
        _navMeshAgent.baseOffset = NAVMESH_BASE_OFFSET;

        _navMeshAgent.angularSpeed = MAX_ANGULAR_SPEED;
        _navMeshAgent.acceleration = ACCELERATION;
        _navMeshAgent.speed = _moveSpeed;
        _navMeshAgent.stoppingDistance = _stopDistance;
    }

    // Update is called once per frame
    void Update()
    {

        //Postinitialize
        //-------------------------------------------------------------------------------------------------------------------
        if (_postInitialize == false)
        {
            _baseScript.SetRangeAttack(_attackRange);
            _baseScript.SetRangeRune(_attackRange);

            if (_attackRange != 0)
                _postInitialize = true;
        }

        //Check for obstacles
        //-------------------------------------------------------------------------------------------------------------------
        _state = _baseScript.GetState();
        _doesAttack = _baseScript.GetAttack();
        UpdateRune();

        //Movement based on state
        //-------------------------------------------------------------------------------------------------------------------
        if (_state == EnemyState.IDLE || _state == EnemyState.MOVE)
        {
            if (_staggered == true)
            {
                _navMeshAgent.velocity = Vector3.zero;
                _navMeshAgent.stoppingDistance = _stopDistance;
                _staggered = false;
            }
            if (_doesAttack == false)
            {
                SeekAndDestroy();
            }

        }
        else if ((_state == EnemyState.STAGGER || _state == EnemyState.SWITCH) && _knockBackAttack == true)
        {
            _staggered = true;
            Stagger();
        }

    }

    /*-----------------------*/
    /* PATHING AND BEHAVIOUR */
    /*-----------------------*/

    //Sets the target the enemy will follow
    //*************************************************************************************************************************************************************************
    public void SetTarget(Transform target)
    {
        _currTarget = target;
    }

    //Regular movement
    //*************************************************************************************************************************************************************************
    private void SeekAndDestroy()
    {
        _currTarget = _rune;
        if (_currTarget == null)
            return;
        _navMeshAgent.SetDestination(_currTarget.position);

        if (InRange() == false)
        {
            _navMeshAgent.stoppingDistance = _stopDistance;
            _navMeshAgent.updateRotation = true;
        }
        else
        {
            _navMeshAgent.stoppingDistance = 500;
            _baseScript.SetAttack(true);
            _baseScript.SetCurrTarget(_currTarget);
        }

    }

    //Pushback enemy
    //*************************************************************************************************************************************************************************
    private void Stagger()
    {
        _navMeshAgent.stoppingDistance = 500;
        _navMeshAgent.updateRotation = false;
        Vector3 staggerDirection = _baseScript.GetStaggerDirection();
        float staggerDistance = _baseScript.GetStaggerDistance();
        float staggerTime = _baseScript.GetStaggerTime();

        Vector3 staggerVelocity = staggerDirection * (staggerDistance / staggerTime);
        _navMeshAgent.velocity = staggerVelocity;
    }

    //Check withing range
    //*************************************************************************************************************************************************************************
    public bool InRange()
    {
        if(_currTarget == null)
            return false;

        Vector3 targetDir = _currTarget.position - _transform.position;
        Vector3 forwardDir = _transform.forward;

        float dot = Vector3.Dot(targetDir, forwardDir);
        float distance = targetDir.magnitude;

        if (dot > _attackDotFOV && distance < _attackRange)
        {
            return true;
        }

        return false;
    }

    // Update current rune
    //*************************************************************************************************************************************************************************
    public void UpdateRune()
    {
        if (_rune != this.gameObject.GetComponent<Enemy_Base>().GetRune())
            _rune = this.gameObject.GetComponent<Enemy_Base>().GetRune();
    }
}
