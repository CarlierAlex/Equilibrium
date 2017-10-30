using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Enemy_A_Ranged : Enemy_A_Virtual
{
    [SerializeField]
    private Transform _leftHand;
    [SerializeField]
    private Transform _rightHand;
    [SerializeField]
    private GameObject _projectilePrefab;
    [SerializeField]
    private float _speed = 3.0f;
    [SerializeField]
    private int _damage = 3;
    [SerializeField]
    private bool _isLobbed = false;

    private float _attackRange = 10.0f;
    [SerializeField]
    private float _spawnProjectile = 2.0f;
    [SerializeField]
    private float _fireProjectile = 0.0f;
    [SerializeField]
    private float _animationTime = 3.0f;
    [SerializeField]
    private float _expireTime = 5.0f;

    private Transform _target;

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
        if (_postInitialize == false)
        {
            _attackRange = _baseScript.GetRangeAttack();
            if (_attackRange > 0)
                _postInitialize = true;
        }

        if (_doesAttack == false)
        {
            _doesAttack = _baseScript.GetAttack();
            _animPhase = AnimationPhase.RESET;
            _animationTimer = 0;
            if (_doesAttack == true)
            {
                _animPhase = AnimationPhase.NONE;
                _baseScript.SetCanAttack(true);
            }
        }

        if (_doesAttack == true)
        {
            _baseScript.SetAttack(true);

            //Stop on stagger
            //-------------------------------------------------------------------------------------------------------------------------------------------
            if (_baseScript.GetState() == EnemyState.STAGGER)
            {
                return;
            }

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
            }
            else if (_animationTimer >= _spawnProjectile && _animPhase == AnimationPhase.NONE)
            {
                _animPhase = AnimationPhase.ATTACKSTART;
            }
            else if (_animationTimer >= _fireProjectile && (_animPhase == AnimationPhase.ATTACKSTART || _animPhase == AnimationPhase.NONE))
            {
                _animPhase = AnimationPhase.ATTACKSTOP;
                _direction = _baseScript.GetCurrTarget() - ((_rightHand.position + _leftHand.position) / 2.0f);
                _direction = _direction.normalized;
            }

            //Execute based on animation phase
            //-------------------------------------------------------------------------------------------------------------------------------------------
            Vector3 targetDir;
            float angle;
            switch (_animPhase)
            {
                case AnimationPhase.ATTACKSTART:
                    targetDir = _baseScript.GetCurrTarget() - this.gameObject.transform.position;
                    targetDir = targetDir.normalized;

                    angle = Mathf.DeltaAngle(this.transform.forward.y, targetDir.y);
                    if (angle != 0)
                    {
                        float sign = angle / Mathf.Abs(angle);
                        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                            Quaternion.LookRotation(targetDir.normalized), Time.deltaTime * 10.0f);
                    }


                    //Projectile spawn
                    if (_projectilePrefab != null && _hitBox == null)
                    {
                        _hitBox = Instantiate(_projectilePrefab, ((_rightHand.position + _leftHand.position) / 2.0f), Quaternion.identity);
                        _hitBox.GetComponent<Transform>().position = ((_rightHand.position + _leftHand.position) / 2.0f);
                        _hitBox.GetComponent<Transform>().rotation = Quaternion.identity;
                        _hitBox.transform.parent = this.transform;
                    }

                    if (_hitBox != null)
                    {
                        _hitBox.GetComponent<Transform>().position = ((_rightHand.position + _leftHand.position) / 2.0f);
                        _hitBox.GetComponent<Transform>().rotation = Quaternion.identity;
                        _hitBox.GetComponent<Rigidbody>().useGravity = false;
                        _hitBox.GetComponent<Rigidbody>().velocity = Vector3.zero;

                        GameObject child = _hitBox.GetComponentInChildren<Collider>().gameObject;
                        child.GetComponent<Collider>().enabled = false;
                        child.GetComponent<Hitbox>().SetDamage(_damage);
                    }
                    break;

                case AnimationPhase.ATTACKSTOP:
                    if (_hitBox != null)
                    {
                        GameObject child = _hitBox.GetComponentInChildren<Collider>().gameObject;
                        child.GetComponent<Collider>().enabled = true;
                        child.GetComponent<Hitbox>().SetExpire(_expireTime);

                        _hitBox.GetComponent<Rigidbody>().velocity = _direction * _speed;
                        _hitBox.transform.parent = null;
                        _hitBox = null;

                        Vector3 target = _baseScript.GetCurrTarget();
                        target.y += 2.0f;
                        targetDir = target - this.gameObject.transform.position;
                        targetDir = targetDir.normalized;

                        angle = Mathf.DeltaAngle(this.transform.forward.y, targetDir.y);
                        if (angle != 0)
                        {
                            float sign = angle / Mathf.Abs(angle);
                            this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                Quaternion.LookRotation(targetDir.normalized), Time.deltaTime * 20.0f);
                        }
                    }
                    break;

                default:
                    break;
            }


        }
    }
}
