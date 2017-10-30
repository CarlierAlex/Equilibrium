using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Base : MonoBehaviour {

    [SerializeField]
    private Material _darkMaterial;
    [SerializeField]
    private Material _lightMaterial;
    [SerializeField]
    private Material _neutralMaterial;
    [SerializeField]
    private Material _invulnMat;
    [SerializeField]
    private GameObject _darkArrow;
    [SerializeField]
    private GameObject _lightArrow;
    [SerializeField]
    private GameObject _shield;
    [SerializeField]
    private GameObject _enemyMesh;
    [SerializeField]
    private ParticleSystem _darkFog;
    [SerializeField]
    private ParticleSystem _lightFog;

    private Behaviour _halo;
    private Renderer _meshRenderer;

    private EnemyState _state = EnemyState.IDLE;
    private EnemyType _type = EnemyType.NONE;
    private Faction _faction = Faction.NONE;
    private GameObject _indicator;

    private Vector3 _prevPos;
    private Vector3 _staggerDirection = Vector3.zero;
    private float _staggerTimer = 0.0f;

    private bool _isDead = false;
    [SerializeField]
    private int _maxHealth = 1;
    private int _health = 1;
    private int _damage = 1;
    private bool _attack = false;
    private bool _canAttack = true;

    private bool _hit = false;
    private int _damageTaken = 0;
    private float _damageTimer = 0;
    private const float DAMAGE_TIME_RESET = 0.1f;

    private bool _isInEnemyZone = false;
    private bool _isInFriendlyZone = false;
    private const float IFRAMETIME = 0.45f;
    private float _iFrameTimer = 0.0f;

    private string _enemyBeamTag = "BeamLight";
    private string _enemySwordTag = "SwordLight";

    [SerializeField]
    private float _staggerTime = 0.1f;
    private const float STAGGER_DISTANCE = 2.0f;
    private float _staggerDistance = 2.0f;

    private bool _attackingRune = false;
    private bool _postInitialize = false;
    private Transform _currTarget;
    private Transform _rune;
    private float _rangeAttack = 0;
    private float _rangeRune = 0;

    public const float KILL_DEPTH = -10.5f;

    [SerializeField]
    private float _resetTime = 0.4f;
    private float _resetTimer = 0.0f;

    void Start()
    {
        _halo = (Behaviour)this.GetComponent("Halo");

        if(_enemyMesh != null)
        {
            if (_enemyMesh.GetComponent<SkinnedMeshRenderer>() != null)
                _meshRenderer = _enemyMesh.GetComponent<SkinnedMeshRenderer>();
        }

        if(_meshRenderer == null)
            _meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //Postinitialize
        //---------------------------------------------------------------------------------------------------------------------------------------------
        if (_postInitialize == false)
        {
            _health = _maxHealth;
            SetMaterials(_faction);
            _postInitialize = true;
        }

        //Stagger timer
        //---------------------------------------------------------------------------------------------------------------------------------------------
        Vector3 currPos = transform.position;
        EnemyState prevS = _state;
        if (_state == EnemyState.STAGGER || _state == EnemyState.SWITCH)
        {
            _staggerTimer += Time.deltaTime;
            if (_staggerTimer >= _staggerTime)
            {
                _state = EnemyState.IDLE;
                _staggerTimer = 0;
            }
        }

        //Stagger reset
        //---------------------------------------------------------------------------------------------------------------------------------------------
        if (_state != EnemyState.STAGGER && _state != EnemyState.SWITCH)
        {
            if (GetAttack() == true)
            {
                switch (_type)
                {
                    case EnemyType.HYBRID:
                        _state = (IsTargetRune()) ? EnemyState.EXPLODE : EnemyState.ATTACK;
                        break;
                    default:
                        _state = EnemyState.ATTACK;
                        break;
                }
            }
            else if (_prevPos != currPos)
                _state = EnemyState.MOVE;
            else if (_state != EnemyState.IDLE)
                _state = EnemyState.IDLE;
        }
        _prevPos = currPos;


        //Attack cooldown
        //---------------------------------------------------------------------------------------------------------------------------------------------
        if (_canAttack == false)
        {
            _resetTimer += Time.deltaTime;
            if (_resetTimer >= _resetTime)
            {
                _resetTimer = 0;
                _canAttack = true;
            }
        }

        //Damage
        //---------------------------------------------------------------------------------------------------------------------------------------------
        if (_hit == true)
        {
            if (_damageTimer == 0)
            {
                DamageHealth(_damageTaken);
            }

            _damageTimer += Time.deltaTime;
            if (_damageTimer >= DAMAGE_TIME_RESET)
            {
                _damageTimer = 0;
                _hit = false;
                _halo.enabled = false;
            }
        }

        //Kill
        //---------------------------------------------------------------------------------------------------------------------------------------------
        if (_health <= 0 || KILL_DEPTH > this.gameObject.transform.position.y)
        {
            _state = EnemyState.DEAD;
        }

        // invincibility frames
        //---------------------------------------------------------------------------------------------------------------------------------------------
        if (_iFrameTimer < IFRAMETIME) {
            _iFrameTimer += Time.deltaTime;
            _meshRenderer.material = _invulnMat; // enemy turns pink when invulnerable
        }
        else if (_iFrameTimer != 0)
        {
            SetMaterials(_faction);
            if (_shield != null)
                _shield.GetComponent<Renderer>().enabled = false;

            //_iFrameTimer = 0;
        }


    }

    //Ontrigger, damage + stagger check
    //*************************************************************************************************************************************************************************
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SwordLight") || other.gameObject.CompareTag("SwordDark") && _hit == false)
        {
            SwordSlice slice = other.GetComponent<SwordSlice>();

            RaycastHit hit;
            LayerMask enemyMask = 1 << 8;
            LayerMask decorationMask = 1 << 11;
            enemyMask = enemyMask | decorationMask;
            enemyMask = ~enemyMask;

            Vector3 rayPlayer = slice.GetPlayerPosition();
            rayPlayer.y += 2.0f;
            Vector3 rayEnemy = transform.position;
            rayEnemy.y += 2.0f;

            if (Physics.Raycast(rayPlayer, (rayEnemy - rayPlayer).normalized, out hit, 100.0f, enemyMask))
            {
                if (hit.collider.gameObject.CompareTag("Player") == false && hit.collider.gameObject != this.gameObject 
                    && other.gameObject.CompareTag("SwordLight") == false && other.gameObject.CompareTag("SwordDark") == false)
                    return;
            }

            _staggerDirection = this.gameObject.transform.position - slice.GetPlayerPosition();
            _staggerDirection.y = 0;
            _staggerDirection = _staggerDirection.normalized;
            _state = EnemyState.STAGGER;
            _staggerDistance = STAGGER_DISTANCE * slice.GetForce();


            if (other.gameObject.CompareTag(_enemySwordTag))
            {
                _damageTaken = Mathf.Abs(other.gameObject.GetComponent<SwordSlice>().GetDamage());
                _hit = true;
                _halo.enabled = true;
                //Debug.Log("NO!");
            }
            else if (other.gameObject.CompareTag("SwordLight") || other.gameObject.CompareTag("SwordDark"))
            {
                if (_shield != null)
                {
                    _shield.GetComponent<Renderer>().enabled = true;
                    _shield.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 3, 0);
                }
            }
        }
    }

    //Sets the faction
    //*************************************************************************************************************************************************************************
    public void SetFaction(Faction fact = Faction.NONE)
    {
        _faction = fact;
        switch (fact)
        {
            case Faction.NONE:
                break;
            case Faction.DARK:
                _enemyBeamTag = "BeamLight";
                _enemySwordTag = "SwordLight";
                SetMaterials(Faction.DARK);
                if (_darkArrow != null)
                {
                    if(_indicator != null)
                        DestroyImmediate(_indicator);
                    _indicator = Instantiate(_darkArrow);
                }


                if(_darkFog != null)
                    _darkFog.enableEmission = true;
                if (_lightFog != null)
                    _lightFog.enableEmission = false;
                break;
            case Faction.LIGHT:
                _enemyBeamTag = "BeamDark";
                _enemySwordTag = "SwordDark";
                SetMaterials(Faction.LIGHT);
                if (_lightArrow != null)
                {
                    if (_indicator != null)
                        DestroyImmediate(_indicator);
                    _indicator = Instantiate(_lightArrow);
                }

                if (_darkFog != null)
                    _darkFog.enableEmission = false;
                if (_lightFog != null)
                    _lightFog.enableEmission = true;
                break;
            default:
                break;
        }
    }

    //Get faction
    //*************************************************************************************************************************************************************************
    public Faction GetFaction()
    {
        return _faction;
    }

    //Set enemy type
    //*************************************************************************************************************************************************************************
    public void SetEnemyType(EnemyType type = EnemyType.NONE)
    {
        _type = type;
    }

    //Get enemy type
    //*************************************************************************************************************************************************************************
    public EnemyType GetEnemyType()
    {
        return _type;
    }

    //Returns true when dead
    //*************************************************************************************************************************************************************************
    public bool IsDead()
    {
        if (EnemyState.DEAD == _state || EnemyState.DESTROY == _state) {
            Destroy(_indicator);
            return true;
        }
        return false;
    }

    //Set rune target
    //*************************************************************************************************************************************************************************
    public void SetRune(Transform rune = null)
    {
        _rune = rune;
    }

    //Get rune target
    //*************************************************************************************************************************************************************************
    public Transform GetRune()
    {
        return _rune;
    }

    //Set attack avaibility
    //*************************************************************************************************************************************************************************
    public void SetCanAttack(bool attack)
    {
        _canAttack = attack;
    }

    //Set attack state
    //*************************************************************************************************************************************************************************
    public void SetAttack(bool attack)
    {
        if(_canAttack == true)
            _attack = attack;
        else
            _attack = false;
    }

    //Return attack state
    //*************************************************************************************************************************************************************************
    public bool GetAttack()
    {
        if (_canAttack == true)
            return _attack;
        else
            return false;
    }

    //Forced dead state
    //*************************************************************************************************************************************************************************
    public void SetDead()
    {
        _state = EnemyState.DEAD;
    }

    //Applies damage to health
    //*************************************************************************************************************************************************************************
    public void DamageHealth(int damage = 1)
    {
        _health -= damage;
        _iFrameTimer = 0;


        float minScale = 0.5f;
        float maxScale = 1.0f;

        float scaleAmount = ((float)_health / (float)_maxHealth);
        scaleAmount *= (maxScale - minScale);
        scaleAmount += minScale;

        this.gameObject.transform.localScale = new Vector3(
            scaleAmount,
            scaleAmount,
            scaleAmount
        );

        //Hybrid type switch
        //------------------------------------------------------------------------------------------
        if (_type == EnemyType.HYBRID)
        {
            _state = EnemyState.SWITCH;
            if (_faction == Faction.LIGHT)
            {
                SetFaction(Faction.DARK);
            }
            else if (_faction == Faction.DARK)
            {
                SetFaction(Faction.LIGHT);
            }

        }
    }

    //Check whether enemy is in darkzone and if damage cooldown is done
    public bool IsVulnerable()
    {
        //TODO: Set to true if an enemy is not in a zone of its faction --> it can be damaged by light of the opposite faction
        if (_iFrameTimer >= IFRAMETIME
            && !_isInFriendlyZone)
        {
            return true;
        }
        else
            return false;
    }

    //Current target position
    //*************************************************************************************************************************************************************************
    public Vector3 GetCurrTarget()
    {
        return _currTarget.position;
    }

    //Check if target is rune
    //*************************************************************************************************************************************************************************
    public bool IsTargetRune()
    {
        return _attackingRune;
    }

    //Set current target
    //*************************************************************************************************************************************************************************
    public void SetCurrTarget(Transform target, bool isRune = true)
    {
        _currTarget = target;
        _attackingRune = isRune;
    }

    //Get attack range
    //*************************************************************************************************************************************************************************
    public float GetRangeAttack()
    {
        return _rangeAttack;
    }

    //Set range attack
    public void SetRangeAttack(float range)
    {
        _rangeAttack = range;
    }

    //Get range attack for rune
    //*************************************************************************************************************************************************************************
    public float GetRangeRune()
    {
        return _rangeRune;
    }

    //Range for attack rune
    //*************************************************************************************************************************************************************************
    public void SetRangeRune(float range)
    {
        _rangeRune = range;
    }

    //Returns damage
    //*************************************************************************************************************************************************************************
    public int GetDamage()
    {
        return _damage;
    }

    //Returns state
    //*************************************************************************************************************************************************************************
    public EnemyState GetState()
    {
        return _state;
    }

    //Returns stagger time
    //*************************************************************************************************************************************************************************
    public float GetStaggerTime()
    {
        return _staggerTime;
    }

    //Returns stagger distance
    //*************************************************************************************************************************************************************************
    public float GetStaggerDistance()
    {
        return _staggerDistance;
    }

    //Returns stagger direction
    //*************************************************************************************************************************************************************************
    public Vector3 GetStaggerDirection()
    {
        return _staggerDirection;
    }

    //Returns stagger direction
    //*************************************************************************************************************************************************************************
    public void SetMaterials(Faction faction)
    {
        if (_meshRenderer != null)
        {
            int length = _meshRenderer.materials.Length;

            if (length >= 2)
            {
                var matList = new Material[length];
                if (faction == Faction.DARK)
                {
                    matList[0] = _darkMaterial;
                    matList[1] = _lightMaterial;
                }
                else if (faction == Faction.LIGHT)
                {
                    matList[0] = _lightMaterial;
                    matList[1] = _darkMaterial;
                }

                if (length > 2)
                    matList[2] = _neutralMaterial;

                _meshRenderer.materials = matList;
            }
        }
    }

    //Enemy indicator
    //************************************************************************************************************************************************************************* 
    public void EnableIndicator(bool enabled) {
        _indicator.SetActive(enabled);
    }

    //Set indicator parent
    //*************************************************************************************************************************************************************************
    public void SetIndicatorParent(Transform trans) {
        _indicator.transform.SetParent(trans);
    }

    //Get indicator transform
    //*************************************************************************************************************************************************************************
    public Transform GetIndicatorTransform() {
        return _indicator.transform;
    }
}

