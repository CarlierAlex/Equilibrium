using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Enemy_A_Melee : Enemy_A_Virtual
{
    [SerializeField]
    private Transform _hitBoxSpawn;
    [SerializeField]
    private GameObject _hitBoxPrefab;
    [SerializeField]
    private int _damage = 3;

    private float _attackRange = 4.0f;
    [SerializeField]
    private float _attackStart = 2.0f;
    [SerializeField]
    private float _attackStop = 2.0f;
    [SerializeField]
    private float _animationTime = 3.0f;

    [SerializeField]
    private bool _dashOnAttack = false;
    private bool _dash = false;

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
        //Postinitialize
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_postInitialize == false)
        {
            _attackRange = _baseScript.GetRangeAttack();

            if (_attackRange > 0)
                _postInitialize = true;
        }

        //Check for attack state
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_doesAttack == false)
        {
            _doesAttack = _baseScript.GetAttack();
            if (_doesAttack == true)
            {
                _animPhase = AnimationPhase.NONE;
                _baseScript.SetCanAttack(true);
            }
        }

        //Execute attack
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_doesAttack == true)
        {
            _baseScript.SetAttack(_doesAttack);

            //Lunge check
            //-------------------------------------------------------------------------------------------------------------------------------------------
            _dash = DoLunge();

            //Animation state based on time
            //-------------------------------------------------------------------------------------------------------------------------------------------
            _animationTimer += Time.deltaTime;
            if (_animationTimer >= _animationTime)
            {
                _animPhase = AnimationPhase.RESET;

                //Reset
                _animationTimer = 0;
                _doesAttack = false;
                _baseScript.SetAttack(false);
                _baseScript.SetCanAttack(false);

                if (_hitBoxPrefab != null && _hitBox != null)
                {
                    Destroy(_hitBox);
                    _hitBox = null;
                }
            }
            else if (_animationTimer >= _attackStop && (_animPhase == AnimationPhase.ATTACKSTART || _animPhase == AnimationPhase.NONE))
            {
                _animPhase = AnimationPhase.ATTACKSTOP;
            }
            else if(_animationTimer >= _attackStart && _animPhase == AnimationPhase.NONE)
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
    }

    //DESTROY HITBOX
    //*********************************************************************************************************************************************************
    public override void Delete()
    {
        DestroyImmediate(_hitBox);
    }

    //SET LUNGE OK
    //*********************************************************************************************************************************************************
    public virtual bool DoLunge()
    {
        return !_baseScript.IsTargetRune();
    }

    //GET ATTACK RANGE
    //*********************************************************************************************************************************************************
    public override float GetAttackRange()
    {
        return _attackRange;
    }

    //GET ATTACK TIME
    //*********************************************************************************************************************************************************
    public override float GetAttackTime()
    {
        return Mathf.Abs(_attackStart - _attackStop);
    }

    //CHECK IF LUNGE IS EXECUTED FOR CHARACTER
    //*********************************************************************************************************************************************************
    public override bool GetLunge()
    {
        if (_dash == true && _dashOnAttack == true)
            return true;
        return false;
    }

    //CHECK TARGET DIRECTION
    //*********************************************************************************************************************************************************
    public override Vector3 GetTargetDirection()
    {
        return _direction;
    }
}
