using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Enemy_A_Explode : Enemy_A_Virtual
{
    [SerializeField]
    private Transform _hitBoxSpawn;
    [SerializeField]
    private GameObject _hitBoxPrefab;
    [SerializeField]
    private GameObject _explodePrefab;

    [SerializeField]
    private int _damage = 3;
    [SerializeField]
    private int _explodeDamage = 10;

    private float _attackRange = 4.0f;
    [SerializeField]
    private float _attackStart = 2.0f;
    [SerializeField]
    private float _attackStop = 2.0f;


    private float _explodeRange = 4.0f;
    [SerializeField]
    private float _explodeStart = 2.0f;
    [SerializeField]
    private float _explodeStop = 2.0f;
    [SerializeField]
    private float _explodeTime = 0.5f;

    [SerializeField]
    private float _attackAnimationTime = 3.0f;
    [SerializeField]
    private float _explodeAnimationTime = 3.0f;

    private bool _explode = false;
    private bool _exploded = false;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        if (_baseScript == null)
            _baseScript = this.gameObject.GetComponent<Enemy_Base>();
    }

    // Update is called once per frame
    void Update()
    {
        //Post initialize
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_postInitialize == false)
        {
            _attackRange = _baseScript.GetRangeAttack();
            _explodeRange = _baseScript.GetRangeRune();
            if(_explodeRange > 0 && _attackRange > 0)
                _postInitialize = true;
        }

        //Check for attack start
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_doesAttack == false)
        {
            _doesAttack = _baseScript.GetAttack();
            if (_doesAttack == true)
            {
                _animPhase = AnimationPhase.NONE;
                _baseScript.SetCanAttack(true);
                _explode = _baseScript.IsTargetRune();
            }
        }

        //Execute attack
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_doesAttack == true)
        {
            _baseScript.SetAttack(_doesAttack);
            _animationTimer += Time.deltaTime;

            if (_explode)
                Explode();
            else
                Hit();
        }

    }

    //BASIC ATTACK
    //*********************************************************************************************************************************************************
    private void Hit()
    {
        //Animation state based on time
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_animationTimer >= _attackAnimationTime)
        {
            _animPhase = AnimationPhase.RESET;

            if (_hitBoxPrefab != null && _hitBox != null)
            {
                Destroy(_hitBox);
                _hitBox = null;
            }

            //Reset
            if (_explode)
            {
                DestroyImmediate(this.gameObject);
            }
            else
            {
                _animationTimer = 0;
                _doesAttack = false;
                _baseScript.SetAttack(false);
                _baseScript.SetCanAttack(false);
            }

        }
        else if (_animationTimer >= _attackStop && (_animPhase == AnimationPhase.ATTACKSTART || _animPhase == AnimationPhase.NONE))
        {
            _animPhase = AnimationPhase.ATTACKSTOP;
        }
        else if (_animationTimer >= _attackStart && _animPhase == AnimationPhase.NONE)
        {
            _animPhase = AnimationPhase.ATTACKSTART;
            _direction = _baseScript.GetCurrTarget() - this.gameObject.transform.position;
            _direction = _direction.normalized;
        }

        //Execute based on animation phase
        //-------------------------------------------------------------------------------------------------------------------------------------------
        switch (_animPhase)
        {
            case AnimationPhase.ATTACKSTART:
                //Hitbox spawn
                if (_hitBoxPrefab != null && _hitBox == null)
                {
                    _hitBox = Instantiate(_hitBoxPrefab, _hitBoxSpawn.position, _hitBoxSpawn.rotation);
                    _hitBox.transform.parent = this.gameObject.transform;
                    _hitBox.GetComponentInChildren<Hitbox>().SetDamage(_damage);
                }
                if (_hitBox != null)
                {
                    _hitBox.GetComponentInChildren<Collider>().enabled = true;

                    _hitBox.GetComponent<Transform>().transform.position = _hitBoxSpawn.position;
                    _hitBox.GetComponent<Transform>().transform.rotation = _hitBoxSpawn.rotation;
                }
                break;

            case AnimationPhase.ATTACKSTOP:
                //Hitbox delete
                if (_hitBoxPrefab != null)
                {
                    Destroy(_hitBox);
                    _hitBox = null;
                }
                break;

            default:
                break;
        }
    }

    //EXPLOSION
    //*********************************************************************************************************************************************************
    private void Explode()
    {
        //Animation state based on time
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_animationTimer >= _explodeAnimationTime)
        {
            _animPhase = AnimationPhase.RESET;

            if (_hitBoxPrefab != null && _hitBox != null)
            {
                Destroy(_hitBox);
                _hitBox = null;
            }

            //Reset
            if (_explode)
            {
                _baseScript.SetDead();
            }
            else
            {
                _animationTimer = 0;
                _doesAttack = false;
                _baseScript.SetAttack(false);
                _baseScript.SetCanAttack(false);
            }

        }
        else if (_animationTimer >= _explodeStop && (_animPhase == AnimationPhase.ATTACKSTART || _animPhase == AnimationPhase.NONE))
        {
            _animPhase = AnimationPhase.ATTACKSTOP;
        }
        else if (_animationTimer >= _explodeStart && _animPhase == AnimationPhase.NONE)
        {
            _animPhase = AnimationPhase.ATTACKSTART;
        }

        //Execute based on animation phase
        //-------------------------------------------------------------------------------------------------------------------------------------------
        switch (_animPhase)
        {
            case AnimationPhase.ATTACKSTART:

                break;

            case AnimationPhase.ATTACKSTOP:
                //Hitbox spawn
                if (_explodePrefab != null && _hitBox == null && _exploded == false)
                {
                    _hitBox = Instantiate(_explodePrefab, _hitBoxSpawn.position, _hitBoxSpawn.rotation);
                    _hitBox.transform.parent = this.gameObject.transform;
                    _hitBox.GetComponentInChildren<Hitbox>().SetDamage(_explodeDamage);
                    _hitBox.GetComponentInChildren<Hitbox>().SetExpire(_explodeTime);
                    _hitBox.GetComponentInChildren<Hitbox>().SetExplode(true);
                    _exploded = true;
                }
                break;

            default:
                break;
        }
    }

    //DESTROY HITBOX
    //*********************************************************************************************************************************************************
    public override void Delete()
    {
        DestroyImmediate(_hitBox);
    }

    //GET ANIMATION STATE
    //*********************************************************************************************************************************************************
    public override AnimationPhase GetAnimState()
    {
        return _animPhase;
    }

    //GET ENEMY STATE
    //*********************************************************************************************************************************************************
    public override EnemyState GetEnemyState()
    {
        if(_doesAttack == true)
            return (_explode) ? EnemyState.EXPLODE : EnemyState.ATTACK;
        return _baseScript.GetState();
    }

    //GET ATTACK RANGE
    //*********************************************************************************************************************************************************
    public override float GetAttackRange()
    {
        return (_explode) ? _explodeRange : _attackRange;
    }

    //GET ATTACK TIME
    //*********************************************************************************************************************************************************
    public override float GetAttackTime()
    {
        return (_explode) ? Mathf.Abs(_explodeStart - _explodeStop) : Mathf.Abs(_attackStart - _attackStop);
    }

    //CHECK FOR DASH
    //*********************************************************************************************************************************************************
    public override bool GetLunge()
    {
        if(GetEnemyState() == EnemyState.ATTACK)
            return true;
        return false;
    }

    //GET TARGET DIRECTION
    //*********************************************************************************************************************************************************
    public override Vector3 GetTargetDirection()
    {
        return _direction;
    }
}
