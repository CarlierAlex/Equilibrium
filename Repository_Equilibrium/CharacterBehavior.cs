using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]

public class CharacterBehavior : MonoBehaviour
{
    #region Data initialization

    // External
    [SerializeField]
    private CameraController _camController;
    [SerializeField]
    private ParticleSystem _dashParticles;
    [SerializeField]
    private ParticleSystem _chargeAttackParticles;
    [SerializeField]
    private ParticleSystem _waveParticles;
    [SerializeField]
    private GameObject _damagedParticles;
    [SerializeField]
    private GameObject _swordSlicePrefab;
    [SerializeField]
    private GameObject _beamPrefab;
    [SerializeField]
    private Material _damagedMaterial;

    // Internal
    private CharacterController _controller;
    private Transform _playerTransform;
    private Material _baseMaterial;
    private GameObject _lightBeamInstance;

    // MOVEMENT
    private Vector3 _moveVelocity = Vector3.zero;
    private Quaternion _rotation;
    private Vector3 _dashDirection = Vector3.zero;
    private Vector3 _staggerDirection = Vector3.zero;

    private const float MOVE_ACCELERATION = 90.1f;
    private const float MAX_SPEED = 10.5f;
    private const float DASH_DISTANCE = 8.0f;
    private const float DASH_TIME = 0.12f;
    private const float DASH_COOLDOWN = 1.2f;
    private const float STAGGER_TIME = 0.05f;
    private const float STAGGER_DISTANCE = 0.8f;
    private const float GRAVITY = 20f;

    private float ROTATION_SPEED_KEY = 20.0f;
    private float ROTATION_SPEED_JOY = 20.0f;
    private float ROTATION_SPEED_BEAM_MAX = 100.0f;
    private float ROTATION_SPEED_BEAM_MIN = 10.0f;

    private float _dashingTimer = 0;
    private float _dashResetTimer = 0;

    private bool _canDash = true;

    // ATTACK
    private Vector3 _prevMouseTarget;
    private Vector3 _sliceScale;
    private Vector3 _sliceOriginalScale;

    private const float ATTACK_TIME = 0.15f;
    private const float ATTACK_MAX_CHARGETIME = 1.0f;
    private const float ATTACK_COOLDOWN = 0.15f;
    private const float SUPERSLICE_SCALE = 1.8f;

    private float _attackTimer = 0;
    private float _attackChargeTime = 0;
    private bool _canAttack = true;
    private bool _isDashAttack = false;

    // BEAM
    private Collider _otherBeamCollider;

    private const float BEAM_COOLDOWN = 0.7f;
    private const float CHARGE_SLOW_MOV = 3.0f;
    private bool _beamStart = false;

    private bool _canBeam = true;
    private bool _didBeam = false;

    private const float MAX_ENERGY = 100;
    private float _energy = MAX_ENERGY;
    private const float ENERGY_DEPLETE_TIME = 1.5f;
    private const float ENERGY_REGENERATE_TIME = 3.0f;
    private const float ENERGY_THRESHOLD = 66;
    private float _beamTimer = 0;
    private float _beamChargeTimer = 0;

    // DAMAGE
    private const int MAX_HEALTH = 15;
    private int _health = MAX_HEALTH;
    private const int DAMAGE_BOOST = 1;
    private const int DAMAGE_SWORD = 3;
    private const int DAMAGE_BEAM = 5;   

    private bool _isHit = false;
    private int _damageTaken = 0;
    private float _invincibleTime = 0;
    private const float INVINCIBLE_DURATION = 0.5f;

    // CONTROlS
    private string _controlHorizontal = "";
    private string _controlVertical = "";
    private string _controlAimHorizontal = "";
    private string _controlAimVertical = "";
    private string _controlDash = "";
    private string _controlBeam = "";
    private string _controlAttack = "";

    // MISC
    public const float KILL_DEPTH = -10.5f;

    [SerializeField]
    private bool _isLightType = true;
    private bool _isInOppositeBeam = false;

    private string _oppositeBeamTag;

    private ControlPlayer _controlsPlayer = ControlPlayer.NONE;
    private ControlSettings _controlsSetting = ControlSettings.NONE;
    private ControlAccess _controlsAccess = ControlAccess.FULL;
    private PlayerState _playerState = PlayerState.IDLE;

    #endregion

    //START
    //*********************************************************************************************************************************************************
    void Start()
    {
        SetPlayerType(_isLightType);
        _playerTransform = GetComponent<Transform>();
        _controller = GetComponent<CharacterController>();

        _sliceOriginalScale = _swordSlicePrefab.transform.localScale;
        _sliceScale = _sliceOriginalScale;
        _baseMaterial = GetComponent<MeshRenderer>().material;
        _chargeAttackParticles.gameObject.SetActive(false);

        DisableCollision(false);
    }

    //UPDATE
    //*********************************************************************************************************************************************************
    void Update()
    {
        //Check if friendly beam collider still exists
        if (!_otherBeamCollider && _isInOppositeBeam)
            _isInOppositeBeam = false;

        //Display Player State in console
        //DisplayPlayerState();
        
        // Settings --------------------------------------------------------------------------------------------------------------------------
        CheckSettings();

        // Movement --------------------------------------------------------------------------------------------------------------------------
        Move();

        // Attack ----------------------------------------------------------------------------------------------------------------------------
        Attack();

        // Rotate ----------------------------------------------------------------------------------------------------------------------------
        Aim();

        //Damage check
        DamageCheck();

        // Final movement
        Final();
    }

    //TRIGGERS
    //*********************************************************************************************************************************************************
    private void OnTriggerEnter(Collider other)
    {
        if(this.GetComponent<CapsuleCollider>().isTrigger == false)
        {
            if (other.gameObject.CompareTag(_oppositeBeamTag))
            {
                _isInOppositeBeam = true;
                _otherBeamCollider = other;
            }

            if (other.gameObject.CompareTag("EnemyAttack") && _isHit == false)
            {
                _staggerDirection = _playerTransform.position - other.transform.position;
                _staggerDirection.y = 0;
                _staggerDirection = _staggerDirection.normalized;
                _playerState = PlayerState.STAGGER;

                _isHit = true;
                Bleed();
                if (other.gameObject.GetComponent<Hitbox>() != null)
                    _damageTaken = other.gameObject.GetComponent<Hitbox>().GetDamage();
                else
                    _damageTaken = other.gameObject.transform.parent.gameObject.GetComponent<Enemy_Base>().GetDamage();
            }
        }

    }

    //MOVEMENT
    //*********************************************************************************************************************************************************
    //Includes dashing and stagger
    private void Move()
    {
        float deadzoneStick = 0.7f;
        float testValueController = 0.05f;
        float testValueAxis = 0.2f;

        float yVelocity = _moveVelocity.y;
        // Normal movement -------------------------------------------------------------------------------------------------------------------------
        #region Normal Movement
        if (_playerState != PlayerState.DASH && _playerState != PlayerState.ATTACK)
        {
            Vector3 currMoveDirection = _controller.velocity.normalized;
            // TODO: Change dash particles to work properly
            _dashParticles.Play();

            //Check settings are on keyboard or controller
            if (_controlsSetting == ControlSettings.KEYBOARD)
            {
                float xVelocity = Input.GetAxis(_controlHorizontal);
                if (xVelocity >= testValueController || xVelocity <= -testValueController)
                {
                    if (xVelocity > 0)
                        xVelocity = 1;
                    else if (xVelocity < 0)
                        xVelocity = -1;
                    else
                        xVelocity = 0;
                }

                float zVelocity = Input.GetAxis(_controlVertical);
                if (zVelocity >= testValueController || zVelocity <= -testValueController)
                {
                    if (zVelocity > 0)
                        zVelocity = 1;
                    else if (zVelocity < 0)
                        zVelocity = -1;
                    else
                        zVelocity = 0;
                }

                if (zVelocity == 0 && xVelocity == 0)
                    Drag();
                else
                {
                    Vector3 dir = new Vector3(xVelocity, 0, zVelocity);
                    _moveVelocity = dir.normalized * MAX_SPEED;
                }
            }
            else
            {

                Vector3 stickInput = new Vector3(-Input.GetAxis(_controlHorizontal), 0, Input.GetAxis(_controlVertical));
                if (stickInput.magnitude < deadzoneStick)
                    stickInput = Vector3.zero;
                else
                    stickInput = stickInput.normalized * ((stickInput.magnitude - deadzoneStick) / (1.0f - deadzoneStick));

                if (stickInput == Vector3.zero)
                    Drag();
                else
                {
                    stickInput = stickInput.normalized;
                    _moveVelocity = stickInput * MAX_SPEED;
                }
            }

            if (_playerState == PlayerState.BEAM || _playerState == PlayerState.ATTACKCHARGE)
                _moveVelocity = _moveVelocity / CHARGE_SLOW_MOV;
        }

        _moveVelocity.y = yVelocity;
        #endregion

        // Dash ------------------------------------------------------------------------------------------------------------------------------------
        #region Dash
        // Dash -------------------------------------------------------------------------------------------------------------------------
        bool dashInput = false;
        if(_controlsAccess >= ControlAccess.DASH)
        {
            //Check settings are on keyboard or controller
            //dashInput = Input.GetButtonDown(_controlDash);
            if (_controlsSetting == ControlSettings.KEYBOARD)
            {
                dashInput = Input.GetButtonDown(_controlDash);
            }
            else
            {
                float weight = Input.GetAxisRaw(_controlDash);
                if (weight > testValueAxis)// || Input.GetAxis(_controlDash) < -testValueController)

                dashInput = true;
            }
        }

        //Check for dash: start, busy or nothing
        if (dashInput == true && _canDash == true && _playerState != PlayerState.ATTACK)
        {
            if (_playerState == PlayerState.ATTACKCHARGE)
                _isDashAttack = true;
            _canDash = false;
            _playerState = PlayerState.DASH;
        }
        if (_playerState == PlayerState.DASH)
        {
            Dash(DASH_DISTANCE, DASH_TIME);
        }
        else
        {
            DisableCollision(false);
        }

        //Dash reset timer
        if (_canDash == false)
        {
            _dashResetTimer += Time.deltaTime;
            if (_dashResetTimer > DASH_COOLDOWN)
            {
                _canDash = true;
                _dashResetTimer = 0;
            }
        }
        #endregion

        // Stagger ---------------------------------------------------------------------------------------------------------------------------------
        if (_playerState == PlayerState.STAGGER)
        {
            Stagger(_staggerDirection);
            _dashingTimer += Time.deltaTime;
            if (_dashingTimer >= STAGGER_TIME)
            {
                _playerState = PlayerState.IDLE;
                _dashingTimer = 0;
            }
        }
    }

    //DRAG | FRICTION
    //*********************************************************************************************************************************************************
    private void Drag()
    {
        //Replicates drag when there is no input
        if (_moveVelocity.x > 0)
        {
            _moveVelocity.x -= MOVE_ACCELERATION * Time.deltaTime;
            if (_moveVelocity.x < 0)
                _moveVelocity.x = 0;
        }
        else if (_moveVelocity.x < 0)
        {
            _moveVelocity.x += MOVE_ACCELERATION * Time.deltaTime;
            if (_moveVelocity.x > 0)
                _moveVelocity.x = 0;
        }

        if (_moveVelocity.z > 0)
        {
            _moveVelocity.z -= MOVE_ACCELERATION * Time.deltaTime;
            if (_moveVelocity.z < 0)
                _moveVelocity.z = 0;
        }
        else if (_moveVelocity.z < 0)
        {
            _moveVelocity.z += MOVE_ACCELERATION * Time.deltaTime;
            if (_moveVelocity.z > 0)
                _moveVelocity.z = 0;
        }
    }

    //DAMAGE CHECK
    //*********************************************************************************************************************************************************
    private void DamageCheck()
    {
        if (_isHit == true)
        {
            Behaviour halo = (Behaviour)GetComponent("Halo");
            halo.enabled = false;
            GetComponent<MeshRenderer>().material = _damagedMaterial;

            if (_invincibleTime == 0)
            {
                _camController.ScreenShake();
                _health -= _damageTaken;
                Debug.Log("Auw! U little ****!");
                if (_health <= 0)
                    SetDead();
            }

            _invincibleTime += Time.deltaTime;

            if (_invincibleTime >= INVINCIBLE_DURATION)
            {
                _invincibleTime = 0;
                _isHit = false;

                GetComponent<MeshRenderer>().material = _baseMaterial;
                halo.enabled = true;
            }
        }
    }

    //FINAL
    //*********************************************************************************************************************************************************
    private void Final()
    {
        // Gravity -----------------------------------------------------------------------------------------------------------------------------------------
        if (_controller.isGrounded == false && _playerState != PlayerState.DASH)
            _moveVelocity.y -= GRAVITY * Time.deltaTime;
        else
            _moveVelocity.y = 0;

        // Execute movement -------------------------------------------------------------------------------------------------------------------------------- 
        if (_playerState != PlayerState.ATTACK)
        {
            // Rotation ------------------------------------------------------------------------------------------------------------------------------------
            // Normal  -------------------------------------------------------------------------------------------------------------------
            if (_playerState != PlayerState.BEAM)
            {
                if (_controlsSetting == ControlSettings.KEYBOARD)
                    _playerTransform.transform.rotation = Quaternion.Slerp(_playerTransform.transform.rotation, _rotation, Time.deltaTime * ROTATION_SPEED_KEY);
                else
                    _playerTransform.transform.rotation = Quaternion.Slerp(_playerTransform.transform.rotation, _rotation, Time.deltaTime * ROTATION_SPEED_JOY);
            }
            // Beam charging -------------------------------------------------------------------------------------------------------------
            else if (_beamStart == false)
            {
                Vector3 targetForward = _rotation.eulerAngles;
                Vector3 originForward = _playerTransform.transform.rotation.eulerAngles;

                float angle = Mathf.DeltaAngle(originForward.y, targetForward.y);
                if (angle != 0)
                {
                    float sign = angle / Mathf.Abs(angle);
                    float rotSpeed = CalculateSlowOnBeam();
                    _playerTransform.transform.Rotate(new Vector3(0, sign * Time.deltaTime * rotSpeed, 0));
                }
            }
            // Beam fire -----------------------------------------------------------------------------------------------------------------
            else
            {
                _beamStart = false;
                _playerTransform.transform.rotation = _rotation;
            }

            // Velocity if withing screenspace --------------------------------------------------------------------------------------------------------------
            if (_camController.IsInViewSpace((_playerTransform.position + _moveVelocity * Time.deltaTime)))
                _controller.Move(_moveVelocity * Time.deltaTime);
        }

        // Reset player state -------------------------------------------------------------------------------------------------------------------------------------------------
        if (_playerState != PlayerState.ATTACK && _playerState != PlayerState.BEAM && _playerState != PlayerState.ATTACKCHARGE && _playerState != PlayerState.DASH
            && _playerState != PlayerState.BEAM && _playerState != PlayerState.STAGGER && _playerState != PlayerState.DEAD)
        {
            if (_moveVelocity.x != 0 || _moveVelocity.z != 0)
            {
                _playerState = PlayerState.WALK;
            }
            else
            {
                _playerState = PlayerState.IDLE;
            }

        }

        // Depth check  -------------------------------------------------------------------------------------------------------------------------------------------------------
        if (KILL_DEPTH > _playerTransform.transform.position.y)
            SetDead();
    }

    //AIM | ROTATION
    //*********************************************************************************************************************************************************
    private void Aim()
    {
        if (_playerState != PlayerState.DASH && _playerState != PlayerState.ATTACK)
        {
            float deadzoneMouse = 0.25f;
            float deadzoneStick = 0.25f;
            Vector3 look;

            //CHECK FOR CONTROL SETTINGS
            //************************//
              
            //KEYBOARD//
            //************************//
            if (_controlsSetting == ControlSettings.KEYBOARD)
            {
                Vector3 mousePos = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                float distance;
                Vector3 mouseTarget;

                //Raycast to get mouse position in plane
                if (plane.Raycast(ray, out distance))
                {
                    mouseTarget = ray.GetPoint(distance);

                    //Check if the distance to prev mouse position is big enough
                    //Else use the rotation of the movement if ther is any
                    if ( (_prevMouseTarget - mousePos).magnitude >= deadzoneMouse || _playerState == PlayerState.ATTACKCHARGE
                        || _playerState == PlayerState.BEAM)
                    {
                        look = ray.GetPoint(distance) - transform.position;
                        _rotation = Quaternion.Euler(0, Mathf.Atan2(look.x, look.z) * Mathf.Rad2Deg, 0);
                    }
                    else
                    {
                        look = _moveVelocity;
                        look.y = 0;
                        if (look != Vector3.zero) {
                            _rotation = Quaternion.LookRotation(look.normalized);
                        }
                        else {
                            _rotation = transform.rotation;
                        }
                    }
                }

                //Set mouse target for review in next loop
                _prevMouseTarget = mousePos;
            }
            // CONTROLLER   
            //************************//
            else
            {
                Vector3 aimInput = new Vector3(-Input.GetAxis(_controlAimHorizontal), 0, Input.GetAxis(_controlAimVertical));

                //Check if stick is pushed far enough
                //Prevents twitching from giving input
                //Else use the rotation of the movement if ther is any
                if (aimInput.magnitude < deadzoneStick) {
                    look = _moveVelocity;
                    look.y = 0;
                    if (look != Vector3.zero)
                    {
                        _rotation = Quaternion.LookRotation(look.normalized);
                    }
                    else
                    {
                        _rotation = transform.rotation;
                    }
                }
                else {
                    _rotation = Quaternion.LookRotation(aimInput.normalized * ((aimInput.magnitude - deadzoneStick) / (1.0f - deadzoneStick)));
                }
            }
        }
    }

    //ATTACK
    //*********************************************************************************************************************************************************
    private void Attack()
    {
        //BEAM INPUT
        //--------------------------------------------------------------------------------------------------------------------------
        bool inputBeam = false;
        float testValueAxis = 0.05f;

        //Check for beam input
        //Can be disabled for tutorial
        if (_canBeam == true && _controlsAccess >= ControlAccess.BEAM)
        {
            if (_controlsSetting == ControlSettings.KEYBOARD)
            {
                if (Input.GetButton(_controlBeam) == true)
                {
                    inputBeam = true;
                }
            }
            else
            {
                if (Input.GetAxisRaw(_controlBeam) < -testValueAxis)
                {
                    inputBeam = true;
                }
            }
        }


        //BASIC_ATTACK | SLASH_ATTACK
        //--------------------------------------------------------------------------------------------------------------------------
        if (Input.GetButton(_controlAttack) == false && _playerState == PlayerState.ATTACKCHARGE && _controlsAccess >= ControlAccess.ATTACK) 
        {
            SwordAttack();
        }


        //BASIC_ATTACK INPUT
        //--------------------------------------------------------------------------------------------------------------------------
        if ((Input.GetButton(_controlAttack) == true) && _canAttack == true
            && _playerState != PlayerState.DASH && _controlsAccess >= ControlAccess.ATTACK)
        {
            //Check for slash attack input
            //Can be disabled for tutorial

            //SET PLAYER STATE TO ATTACK_CHARGE
            //*******************************//
            _playerState = PlayerState.ATTACKCHARGE;

            //SET PARTICLE EFFECTS
            //******************//
            if (_chargeAttackParticles)
                _chargeAttackParticles.gameObject.SetActive(true);
            if (_waveParticles)
            {
                _waveParticles.playbackSpeed = 2;
                _waveParticles.emissionRate = 2;
                _waveParticles.maxParticles = 10;
            }

            //INCREASE ATTACK TIMER
            //*******************//
            if (_attackChargeTime < ATTACK_MAX_CHARGETIME) {
                _attackChargeTime += Time.deltaTime;
            }
            else {
                _attackChargeTime = ATTACK_MAX_CHARGETIME;
            }

        }


        //BEAM
        //--------------------------------------------------------------------------------------------------------------------------
        else if (inputBeam == true && _canBeam == true && _energy > 0)
        {
            //SET PLAYER STATE TO BEAM
            //**********************//
            _playerState = PlayerState.BEAM;

            //CREATE BEAM
            //*********//
            if (_lightBeamInstance == null) {
                _lightBeamInstance = Instantiate(_beamPrefab, _playerTransform, false);
                _beamStart = true;
            }

            //CONSUME ENERGY
            //************//
            _energy -= (100.0f / ENERGY_DEPLETE_TIME) * Time.deltaTime;
            if (_energy < 0)
                _energy = 0;
            if (_energy < ENERGY_THRESHOLD)
                _lightBeamInstance.GetComponent<LightCone>().EnableScalingOverTime(true);
            else
                _lightBeamInstance.GetComponent<LightCone>().EnableScalingOverTime(false);

            //INCREASE BEAM TIME
            //****************//
            _beamTimer += Time.deltaTime;
        }

        //REGENERATE ENERGY
        //--------------------------------------------------------------------------------------------------------------------------
        if (_playerState != PlayerState.BEAM) {
            _energy += (100.0f / ENERGY_REGENERATE_TIME) * Time.deltaTime / 2.0f;
            if (_energy > 100.0f)
                _energy = 100.0f;
        }

        //RESET AFTER BEAM
        //--------------------------------------------------------------------------------------------------------------------------
        if ((inputBeam == false && _playerState == PlayerState.BEAM) || _energy <= 0)
        {
            //DESTROY BEAM
            //**********//
            if (_lightBeamInstance)
                Destroy(_lightBeamInstance);

            //RESET BEAM
            //********//
            _playerState = PlayerState.IDLE;
            _canBeam = false;
            _beamTimer = 0;
        }

        //BEAM COOLDOWN
        //--------------------------------------------------------------------------------------------------------------------------
        if (_canBeam == false)
        {
            //UPDATE COOLDOWN
            //*************//
            _beamTimer += Time.deltaTime;

            //RESET COOLDOWN
            //************//
            if (_beamTimer > BEAM_COOLDOWN)
            {
                _beamTimer = 0;
                _canBeam = true;
            }
        }


        //ATTACK COOLDOWN
        //--------------------------------------------------------------------------------------------------------------------------
        if (_canAttack == false)
        {
            //UPDATE COOLDOWN
            //*************//
            _attackTimer += Time.deltaTime;
            if (_playerState == PlayerState.ATTACK)
            {
                _moveVelocity = Vector3.zero;
                if (_attackTimer > ATTACK_TIME)
                {
                    _playerState = PlayerState.IDLE;
                }
            }

            //RESET COOLDOWN
            //************//
            if (_attackTimer > ATTACK_COOLDOWN + ATTACK_TIME)
            {
                _canAttack = true;
                _attackTimer = 0;
            }
        }
    }

    //SWORD_ATTACK
    //*********************************************************************************************************************************************************
    private void SwordAttack() {
        _canAttack = false;

        //SET PARTICLE EFFECTS
        //******************//
        if (_chargeAttackParticles)
            _chargeAttackParticles.gameObject.SetActive(false);
        if (_waveParticles)
        {
            _waveParticles.playbackSpeed = 1;
            _waveParticles.emissionRate = 1;
            _waveParticles.maxParticles = 5;
        }

        //CREATE SLICE GAMEOBJECT
        //*********************//
        float scale = Mathf.Clamp(_attackChargeTime * 2, 0.7f, 1.5f);
        GameObject obj = Instantiate(_swordSlicePrefab, _playerTransform, false) as GameObject;
        obj.transform.Translate(new Vector3(0, 0.8f, 0));

        SwordSlice slice = obj.GetComponent<SwordSlice>();
        slice.SetForwardAmount(2);
        slice.SetDamage(DAMAGE_SWORD);
        slice.SetScale(scale);

        //SET PLAYER STATE + RESET
        //**********************//
        _playerState = PlayerState.ATTACK;
        _attackChargeTime = 0;
    }

    //DASH
    //*********************************************************************************************************************************************************
    private void Dash(float distance, float duration)
    {
        //Dash launches the player in the current direction
        //This is done with a set distance and time


        DisableCollision(true);
        Vector3 dashDirection = _moveVelocity;
        dashDirection.y = 0;
        dashDirection = dashDirection.normalized * (distance / duration);

        if(_isDashAttack)
            dashDirection = _playerTransform.forward * (distance / duration);

        _dashingTimer += Time.deltaTime;
        if (_dashingTimer > duration)
        {
            _dashingTimer = 0;
            dashDirection = Vector3.zero;
            if (_isDashAttack) {
                _isDashAttack = false;
                SwordAttack();
            }
            else
                _playerState = PlayerState.IDLE;
        }
        _moveVelocity = dashDirection;
    }

    //STAGGER MOVEMENT
    //*********************************************************************************************************************************************************
    private void Stagger(Vector3 direction)
    {
        //Stagger causes the player to be knocked back
        Vector3 staggerVelocity = direction * (STAGGER_DISTANCE / STAGGER_TIME);
        _moveVelocity = staggerVelocity;
    }

    //BLEED EFFECT
    //*********************************************************************************************************************************************************
    private void Bleed()
    {
        //Bleed activates a special effect, simulating damage
        GameObject blood = Instantiate(_damagedParticles, _playerTransform, false);
        blood.transform.Translate(new Vector3(0, 0, -0.45f));
    }

    //FORCE KILL PLAYER
    //*********************************************************************************************************************************************************
    private void SetDead()
    {
        //Forcefully kill the player
        Destroy(_lightBeamInstance);
        gameObject.SetActive(false);
        _playerState = PlayerState.DEAD;
        _health = 0;
        _energy = 0;
    }

    //RESURRECT PLAYER
    //*********************************************************************************************************************************************************
    public void Resurrect(Vector3 position)
    {
        //Used to resurect the player
        gameObject.SetActive(true);
        _playerState = PlayerState.IDLE;
        _health = MAX_HEALTH;
        _energy = MAX_ENERGY;
        _invincibleTime = -1.5f;
        _chargeAttackParticles.gameObject.SetActive(false);

        _playerTransform.position = position;
    }

    //DISABLE COLLISION
    //*********************************************************************************************************************************************************
    private void DisableCollision(bool disable)
    {
        //Disable collision
        this.GetComponent<CharacterController>().detectCollisions = !disable;
        this.GetComponent<CapsuleCollider>().isTrigger = disable;
        GetComponentInChildren<CapsuleCollider>().isTrigger = disable;
    }

    //ADD HEALTH
    //*********************************************************************************************************************************************************
    public void IncreaseHealth(int health)
    {
        //Increases health when not dead
        if (_playerState != PlayerState.DEAD) {
            _health += health;
            if (_health > MAX_HEALTH)
                _health = MAX_HEALTH;
        }
    }

    //SET PLAYER TYPE
    //*********************************************************************************************************************************************************
    private void SetPlayerType(bool isLight)
    {
        //Sets the type of this player: LIGHT/DARK
        if (isLight)
        {
            SetControls(ControlPlayer.PLAYER1);
            _oppositeBeamTag = "DarkZone";
        }
        else
        {
            SetControls(ControlPlayer.PLAYER2);
            _oppositeBeamTag = "LightZone";
        }
    }

    //SET CONTROL SETTINGS
    //*********************************************************************************************************************************************************
    public void SetControls(ControlPlayer controlP = ControlPlayer.NONE)
    {
        //Sets the controls of this player: KEYBOARD/CONTROLLERS
        _controlsPlayer = controlP;
        CheckSettings();
    }

    //SET ACCESS LEVEL
    //*********************************************************************************************************************************************************
    public void SetAbilityAccess(ControlAccess access = ControlAccess.NONE)
    {
        //Sets the locks on the abilities depending on the phase + level
        _controlsAccess = access;
    }

    //CHECK CONTROL SETTINGS
    //*********************************************************************************************************************************************************
    private void CheckSettings()
    {
        //Check settings for player input -> when multiple are possible for 1 player
        if (_controlsPlayer == ControlPlayer.PLAYER1)
        {
            bool foundKey = false;
            if (Input.GetButton("Dash") == true || Input.GetButton("Beam") == true || Input.GetButton("Attack") == true)
                foundKey = true;
            else if (Input.GetButton("Horizontal") == true || Input.GetButton("Vertical") == true)
                foundKey = true;


            if (_controlsSetting != ControlSettings.KEYBOARD && foundKey == true)
            {
                _controlsSetting = ControlSettings.KEYBOARD;
                _controlHorizontal = "Horizontal";
                _controlVertical = "Vertical";
                _controlAimHorizontal = "Aim_Horizontal_Joy1";
                _controlAimVertical = "Aim_Vertical_Joy1";
                _controlDash = "Dash";
                _controlBeam = "Beam";
                _controlAttack = "Attack";
            }
            else if (_controlsSetting != ControlSettings.CONTROLLER2 && foundKey == false)
            {
                _controlsSetting = ControlSettings.CONTROLLER2;
                _controlHorizontal = "Joy2_Horizontal";
                _controlVertical = "Joy2_Vertical";
                _controlAimHorizontal = "Aim_Horizontal_Joy2";
                _controlAimVertical = "Aim_Vertical_Joy2";
                _controlDash = "Joy2_Dash";
                _controlBeam = "Joy2_Beam";
                _controlAttack = "Joy2_Attack";
            }
        }
        else if (_controlsPlayer == ControlPlayer.PLAYER2 && _controlsSetting != ControlSettings.CONTROLLER1)
        {
            _controlsSetting = ControlSettings.CONTROLLER1;
            _controlHorizontal = "Joy1_Horizontal";
            _controlVertical = "Joy1_Vertical";
            _controlAimHorizontal = "Aim_Horizontal_Joy1";
            _controlAimVertical = "Aim_Vertical_Joy1";
            _controlDash = "Joy1_Dash";
            _controlBeam = "Joy1_Beam";
            _controlAttack = "Joy1_Attack";
        }
    }

    //DEBUG PLAYER STATE
    //*********************************************************************************************************************************************************
    public void DisplayPlayerState()
    {
        //Displays the current state of the player: IDLE/BEAM/ATTACK/STAGGER/DASH/SUPERDASH
        switch (_playerState)
        {
            case PlayerState.IDLE:
                Debug.Log("STATE: IDLE");
                break;
            case PlayerState.WALK:
                Debug.Log("STATE: WALK");
                break;
            case PlayerState.DASH:
                Debug.Log("STATE: DASH");
                break;
            case PlayerState.ATTACK:
                Debug.Log("STATE: ATTACK");
                break;
            case PlayerState.BEAM:
                Debug.Log("STATE: BEAM");
                break;
            case PlayerState.DEAD:
                Debug.Log("STATE: DEAD");
                break;
            default:
                break;
        }
    }

    //CHECK DEAD PLAYER
    //*********************************************************************************************************************************************************
    public bool IsDead()
    {
        //Returns true if the player is dead
        if (_playerState == PlayerState.DEAD)
            return true;
        return false;
    }

    //GET CURRENT HEALTH
    //*********************************************************************************************************************************************************
    public int GetHealth()
    {
        //Returns Health
        return _health;
    }

    //GET HEALTH MAX
    //*********************************************************************************************************************************************************
    public int GetHealthMax()
    {
        //Returns Maximum Health
        return MAX_HEALTH;
    }

    //GET CURRENT ENERGY
    //*********************************************************************************************************************************************************
    public int GetEnergy()
    {
        //Returns Energy
        return (int)_energy;
    }

    //GET ENERGY MAX
    //*********************************************************************************************************************************************************
    public int GetEnergyMax()
    {
        //Returns Maximum Energy
        return (int)MAX_ENERGY;
    }

    //GET CONTROLS
    //*********************************************************************************************************************************************************
    public ControlPlayer GetControls()
    {
        //Returns the current control type
        return _controlsPlayer;
    }

    //GET BEAM DAMAGE
    //*********************************************************************************************************************************************************
    public int GetDamageBeam()
    {
        //Returns the current damage for beam attacks
        //Changes when in powered up state
        return DAMAGE_BEAM;
    }

    //GET PLAYER STATE
    //*********************************************************************************************************************************************************
    public PlayerState GetPlayerState()
    {
        //Return player's state
        return _playerState;
    }

    //ENERGY COOLOWN
    //*********************************************************************************************************************************************************
    public float GetCoolddownSlash()
    {
        //Returns Slash Cooldown
        if (_attackTimer > 0 && _attackTimer < ATTACK_COOLDOWN)
            return (ATTACK_COOLDOWN - _attackTimer);
        return 0;
    }

    //ENERGY COOLOWN
    //*********************************************************************************************************************************************************
    public float GetCoolddownBeam()
    {
        //Returns Energy Cooldown
        if (_beamTimer > 0 && _beamTimer < BEAM_COOLDOWN && _canBeam == false)
            return (BEAM_COOLDOWN - _beamTimer);
        return 0;
    }

    //SLOW CALCULATION
    //*********************************************************************************************************************************************************
    private float CalculateSlowOnBeam()
    {
        //float b = 1;
        //float a = (CHARGE_SLOW_MOV - 1) / 0.35f;
        //float slow = (a * _beamTimer) + b;

        float slow = (ROTATION_SPEED_BEAM_MAX - ROTATION_SPEED_BEAM_MIN) * ((ENERGY_DEPLETE_TIME - _beamTimer) / ENERGY_DEPLETE_TIME);
        slow += ROTATION_SPEED_BEAM_MIN;
        return slow;
    }
}