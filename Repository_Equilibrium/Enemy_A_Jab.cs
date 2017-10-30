using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

public class Enemy_A_Jab : MonoBehaviour
{
    [SerializeField]
    private Transform _hitBoxSpawn;
    [SerializeField]
    private GameObject _hitBoxPrefab;
    [SerializeField]
    private int _damage = 1;

    private Enemy_Base _baseScript;
    private Rigidbody _rigidBody;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;

    [SerializeField]
    private float ATTACK_RANGE = 5.0f;
    [SerializeField]
    private float ATTACK_START = 0.5f;
    [SerializeField]
    private float ATTACK_TIME = 0.5f;
    [SerializeField]
    private float ANIMATION_TIME = 1.2f;
    private float _animationTimer = 0.0f;

    private float _resetTimer = 0.0f;
    [SerializeField]
    private float RESET_TIME = 0.4f;

    private float _distanceTotal = 0.0f;

    private bool _doesAttack = false;
    private bool _postInitialize = false;
    private bool _dash = false;
    private AnimationPhase _animPhase = AnimationPhase.NONE;
    private GameObject _hitBox;
    protected Vector3 _direction = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        _baseScript = this.gameObject.GetComponent<Enemy_Base>();
        _navMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        _rigidBody = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    /// <summary>
    /// 
    /// </summary>
    void Update () {
        if (_postInitialize == false)
        {
            ATTACK_RANGE = _baseScript.GetRangeAttack();
            _postInitialize = true;
        }

        if (_doesAttack == false)
        {
            _doesAttack = _baseScript.GetAttack();
            if(_doesAttack == true)
            {
                _animPhase = AnimationPhase.NONE;
                _baseScript.SetCanAttack(true);
                _dash = !_baseScript.IsTargetRune();
            }
        }


        if (_doesAttack == true)
        {
            var state = _baseScript.GetState();

            if (_baseScript.GetState() == EnemyState.STAGGER)
            {
                return;
            }

            _animationTimer += Time.deltaTime;
            if(_animationTimer >= ANIMATION_TIME)
            {
                _animPhase = AnimationPhase.RESET;

                _animationTimer = 0;
                _doesAttack = false;
                _distanceTotal = 0;
                _baseScript.SetAttack(false);

                if (_hitBoxPrefab != null && _hitBox != null)
                {
                    Destroy(_hitBox);
                    _hitBox = null;
                }
            }
            else if((_animationTimer >= ATTACK_START + ATTACK_TIME || _distanceTotal >= ATTACK_RANGE) && _animPhase == AnimationPhase.ATTACKSTART)
            {
                _animPhase = AnimationPhase.ATTACKSTOP;
            }
            else if (_animationTimer >= ATTACK_START && _animPhase == AnimationPhase.NONE)
            {
                _animPhase = AnimationPhase.ATTACKSTART;
                _direction = _baseScript.GetCurrTarget() - this.gameObject.transform.position;
                _direction = _direction.normalized;
            }

            switch (_animPhase)
            {
                case AnimationPhase.ATTACKSTART:
                    //Dash
                    if (_dash == true)
                    {
                        bool wasSmaller = true;
                        if (_distanceTotal >= ATTACK_RANGE)
                            wasSmaller = false;
                        float distance = (ATTACK_RANGE / ATTACK_TIME) * Time.deltaTime;
                        _distanceTotal += distance;

                        _navMeshAgent.autoBraking = true;
                        if (_distanceTotal < ATTACK_RANGE)
                        {
                            float vel = 0;
                            if (Time.deltaTime != 0)
                                vel = distance / Time.deltaTime;
                            if (vel != 0 && state != EnemyState.STAGGER)
                            {
                                _navMeshAgent.velocity = _direction * vel;
                                _navMeshAgent.autoBraking = false;
                                //_rigidBody.velocity = this.gameObject.transform.forward * vel;
                            }
                        }
                        else if (_distanceTotal > ATTACK_RANGE && wasSmaller == true && state != EnemyState.STAGGER)
                        {
                            float remDistance = (ATTACK_RANGE - (_distanceTotal - distance));
                            _navMeshAgent.velocity = _direction * (remDistance / Time.deltaTime);
                            _navMeshAgent.autoBraking = true;
                            //_rigidBody.velocity = this.gameObject.transform.forward * (remDistance / Time.deltaTime);
                        }
                    }

                    //Hitbox spawn
                    if (_hitBoxPrefab != null && _hitBox == null)
                    {
                        _hitBox = Instantiate(_hitBoxPrefab, _hitBoxSpawn.position, _hitBoxSpawn.rotation);
                        _hitBox.GetComponent<Transform>().transform.position = _hitBoxSpawn.position;
                        _hitBox.GetComponent<Transform>().transform.rotation = _hitBoxSpawn.rotation;
                        _hitBox.transform.parent = this.gameObject.transform;

                    }
                    if (_hitBox != null)
                    {
                        GameObject child = _hitBox.GetComponentInChildren<Collider>().gameObject;
                        child.GetComponent<Collider>().enabled = true;
                        child.GetComponent<Hitbox>().SetDamage(_damage);

                        _hitBox.GetComponent<Transform>().transform.position = _hitBoxSpawn.position;
                        _hitBox.GetComponent<Transform>().transform.rotation = _hitBoxSpawn.rotation;
                        _hitBox.GetComponent<Transform>().transform.Rotate(0, 180, 0);
                    }
                    break;

                case AnimationPhase.ATTACKSTOP:
                    //Hitbox delete
                    if (_hitBox != null)
                    {
                        Destroy(_hitBox);
                        _hitBox = null;
                    }
                    break;

                case AnimationPhase.NONE:
                    Vector3 targetDir = _baseScript.GetCurrTarget() - this.gameObject.transform.position;
                    targetDir = targetDir.normalized;

                    float angle = Mathf.DeltaAngle(this.transform.forward.y, targetDir.y);
                    if (angle != 0)
                    {
                        float sign = angle / Mathf.Abs(angle);
                        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                            Quaternion.LookRotation(targetDir.normalized), Time.deltaTime * 5.0f);
                    }
                    break;

                default:
                    break;
            }



        }
    }

    public void Delete ()
    {
        DestroyImmediate(_hitBox);
    }
}


