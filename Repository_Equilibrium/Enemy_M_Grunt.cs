using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

public class Enemy_M_Grunt : MonoBehaviour
{
    private Enemy_Base _baseScript;
    private Transform _currTarget;
    private Transform _rune;
    private List<Transform> _targetArr;
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
    private float _heightRay = 1.0f;
    [SerializeField]
    private float _widthRay = 1.0f;

    [SerializeField]
    private float _sightRange = 10.0f;
    [SerializeField]
    private float _sightDotFOV = 0.3f;

    [SerializeField]
    private float _attackDotFOV = 0.95f;
    [SerializeField]
    private float _attackRange = 0.0f;
    [SerializeField]
    private float _attackRangeRune = 2.0f;

    private const float WANDER_RADIUS = 7.0f;
    private const float WANDER_ANGLE = 45.0f;

    private bool _staggered = false;
    private EnemyState _state = EnemyState.IDLE;
    private bool _postInitialize = false;

    [SerializeField]
    private bool _knockBackAttack = true;
    private bool _canMove = true;

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

        _targetArr = new List<Transform>();
        var listT = GameObject.FindGameObjectsWithTag("Player");
        if(listT != null)
        {
            foreach (var i in listT)
                _targetArr.Add(i.GetComponent<Transform>());
        }

        Wander();
    }

    // Update is called once per frame
    void Update()
    {
        //Postinitalize
        //--------------------------------------------------------------------------------------------------------------------------------------
        if (_postInitialize == false)
        {
            _baseScript.SetRangeAttack(_attackRange);
            _baseScript.SetRangeRune(_attackRangeRune);
            if (_attackRange != 0)
                _postInitialize = true;
        }

        //Check states + updating targets
        //-------------------------------------------------------------------------------------------------------------------
        _state = _baseScript.GetState();
        bool attacking = _baseScript.GetAttack();
        UpdateRune();
        UpdateTarget();

        //Movement based on state
        //-------------------------------------------------------------------------------------------------------------------
        if ((_state == EnemyState.IDLE || _state == EnemyState.MOVE) && attacking == false)
        {
            if (_staggered == true)
            {
                _navMeshAgent.velocity = Vector3.zero;
                _navMeshAgent.stoppingDistance = _stopDistance;
                Wander();
                _staggered = false;
            }
            if (_state != EnemyState.ATTACK)
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

    //Causes the enemy to wander towards a random position in front of the enemy
    //*************************************************************************************************************************************************************************
    private void Wander()
    {
        //Vector3 wanderTarget = Vector3.zero;
        //wanderTarget = RandomNavSphere(WANDER_RADIUS, -1);
        //_navMeshAgent.SetDestination(wanderTarget);


    }

    //Calculates the position for the wander
    //*************************************************************************************************************************************************************************
    public Vector3 RandomNavSphere(float dist, int layermask)
    {

        Vector3 randDirection = Vector3.zero;
        randDirection = Quaternion.Euler(0, Random.Range(-WANDER_ANGLE, WANDER_ANGLE), 0) * _transform.forward * Random.Range(0, dist);
        randDirection += _transform.position;

        UnityEngine.AI.NavMeshHit navHit;
        UnityEngine.AI.NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    //Regular movement
    //*************************************************************************************************************************************************************************
    private void SeekAndDestroy()
    {
        if (_currTarget == null)
            return;

        //Sets the target
        if (_baseScript.GetAttack() == false)
        {
            if (InRange() == false)
            {
                _navMeshAgent.stoppingDistance = _stopDistance;
                _navMeshAgent.updateRotation = true;
                _navMeshAgent.SetDestination(_currTarget.position);
            }
            else
            {
                _navMeshAgent.stoppingDistance = 5000;
                _navMeshAgent.velocity = Vector3.zero;
                _navMeshAgent.SetDestination(this.transform.position);
                _baseScript.SetAttack(true);
                _baseScript.SetCurrTarget(_currTarget, AttackingRune());
            }
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

    //Check range
    //*************************************************************************************************************************************************************************
    public bool InRange()
    {
        if (_currTarget == null)
        {
            return false;
        }

        //Check for obstacles
        //-------------------------------------------------------------------------------------------------------------------
        RaycastHit hit;
        LayerMask enemyMask = 1 << 8;

        enemyMask = ~enemyMask;
        Vector3 rayPos = this.transform.position;
        rayPos.y = _heightRay;
        Vector3 rayEnd = _currTarget.position;
        rayEnd.y = _heightRay;

        rayPos.x += _widthRay / 2.0f;
        rayEnd.x += _widthRay / 2.0f;

        if (Physics.Raycast(rayPos, (rayEnd - rayPos).normalized, out hit, 100.0f, enemyMask))
        {
            Debug.DrawLine(rayPos, rayEnd, Color.green);
            if ((hit.collider.gameObject.CompareTag("Player") == false && hit.collider.gameObject.CompareTag("Runestone") == false) && hit.collider.gameObject != this.gameObject)
                return false;
        }

        rayPos.x -= _widthRay;
        rayEnd.x -= _widthRay;

        if (Physics.Raycast(rayPos, (rayEnd - rayPos).normalized, out hit, 100.0f, enemyMask))
        {
            Debug.DrawLine(rayPos, rayEnd, Color.green);
            if ((hit.collider.gameObject.CompareTag("Player") == false && hit.collider.gameObject.CompareTag("Runestone") == false) && hit.collider.gameObject != this.gameObject)
                return false;
        }

        //Check range
        //-------------------------------------------------------------------------------------------------------------------
        if (_currTarget != null)
        {
            Vector3 targetDir = _currTarget.position - _transform.position;
            Vector3 forwardDir = _transform.forward;

            float dot = Vector3.Dot(targetDir, forwardDir);
            float distance = targetDir.magnitude;

            float range = (_currTarget == _rune) ? _attackRangeRune : _attackRange;

            if (dot >= _attackDotFOV && distance <= range)
                return true;
        }

        return false;
    }

    //Check if attacking rune
    //*************************************************************************************************************************************************************************
    public bool AttackingRune()
    {
        if (_rune == _currTarget)
                return true;
        return false;
    }

    //Update target
    //*************************************************************************************************************************************************************************
    private void UpdateTarget()
    {
        float distance = 1000;
        if (_rune != null)
        {
            _currTarget = _rune;
        }
        if (_targetArr != null)
        {
            foreach (var player in _targetArr)
            {
                Vector3 targetDir = player.position - _transform.position;
                Vector3 forwardDir = _transform.forward;

                float dot = Vector3.Dot(targetDir, forwardDir);
                float distanceNew = targetDir.magnitude;

                if (dot > _sightDotFOV && distanceNew < distance && distanceNew < _sightRange)
                {
                    _currTarget = player;
                    distance = distanceNew;
                }
            }
        }
    }

    //Update rune target
    //*************************************************************************************************************************************************************************
    public void UpdateRune()
    {
        if(_rune != this.gameObject.GetComponent<Enemy_Base>().GetRune())
            _rune = this.gameObject.GetComponent<Enemy_Base>().GetRune();
    }
}
