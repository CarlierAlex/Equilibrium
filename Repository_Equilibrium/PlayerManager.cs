using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

    [SerializeField]
    private GameObject _playerDark;
    private CharacterBehavior _behaviourDark;
    [SerializeField]
    private GameObject _respawnEffectDark;
    [SerializeField]
    private Transform _spawnLocationDark;

    [SerializeField]
    private GameObject _playerLight;
    private CharacterBehavior _behaviourLight;
    [SerializeField]
    private GameObject _respawnEffectLight;
    [SerializeField]
    private Transform _spawnLocationLight;

    private Pickup[] _healthPickupsArr;

    private bool _postInitialize = false;
    private const float RES_COOLDOWN = 3.0f;
    private float _resurrectTimeLight = 0.0f;
    private float _resurrectTimeDark = 0.0f;

    void Start () {
        _behaviourDark = _playerDark.GetComponent<CharacterBehavior>();
        _behaviourLight = _playerLight.GetComponent<CharacterBehavior>();

        _healthPickupsArr = Object.FindObjectsOfType<Pickup>();
    }
	
	void Update () {
        if(_postInitialize == false)
        {
            _behaviourLight.SetControls(ControlPlayer.PLAYER1);
            _behaviourDark.SetControls(ControlPlayer.PLAYER2);
        }

        /* ---RESURRECT CODE--- */
        if (_behaviourDark.IsDead()) {
            _resurrectTimeDark += Time.deltaTime;

            if (_resurrectTimeDark > RES_COOLDOWN) {
                _behaviourDark.Resurrect(_spawnLocationDark.position);
                Instantiate(_respawnEffectDark, _behaviourDark.transform, false); // spawn res effect on player
                GameObject inst = Instantiate(_respawnEffectDark, GetComponent<RuneManager>().GetRuneTransform(), false); // spawn res effect on runestone
                // correct transform to fit runestone
                inst.transform.Rotate( new Vector3(90, 0, 0));
                inst.transform.Translate(new Vector3(0, 0, 5));
                _resurrectTimeDark = 0;

                GetComponent<RuneManager>().DamageCurrentStone(20);
            }
        }

        if(_behaviourLight.IsDead()) {
            _resurrectTimeLight += Time.deltaTime;

            if(_resurrectTimeLight > RES_COOLDOWN) {
                _behaviourLight.Resurrect(_spawnLocationLight.position);
                Instantiate(_respawnEffectLight, _behaviourLight.transform, false);
                GameObject inst = Instantiate(_respawnEffectLight, GetComponent<RuneManager>().GetRuneTransform(), false); // spawn res effect on runestone
                // correct transform to fit runestone
                inst.transform.Rotate(new Vector3(90, 0, 0));
                inst.transform.Translate(new Vector3(0, 0, 5));
                _resurrectTimeLight = 0;

                GetComponent<RuneManager>().DamageCurrentStone(20);
            }
        }

        /* ---PICKUPS CODE--- */
        for (int i = 0; i < _healthPickupsArr.Length; i++) {
            if (_healthPickupsArr[i]) {
                if(_healthPickupsArr[i].IsPickedUP()) {
                    HealPlayers(_healthPickupsArr[i].GetAmountOfHealing());
                    Destroy(_healthPickupsArr[i].gameObject);
                }
            }
        }
    }

    //Ability access
    //*************************************************************************************************************************************************************************
    public void SetAbilityAccess(ControlAccess access) {
        _behaviourDark.SetAbilityAccess(access);
        _behaviourLight.SetAbilityAccess(access);
    }

    //Heal
    //*************************************************************************************************************************************************************************
    private void HealPlayers(int health) {
        _behaviourDark.IncreaseHealth(health);
        _behaviourLight.IncreaseHealth(health);
        Instantiate(_respawnEffectDark, _behaviourDark.transform, false);
        Instantiate(_respawnEffectLight, _behaviourLight.transform, false);
    }


    //Get energy level dark
    //*************************************************************************************************************************************************************************
    public int GetEnergyDark()
    {
        return _behaviourDark.GetEnergy();
    }

    //Get energy max level dark 
    //*************************************************************************************************************************************************************************
    public int GetEnergyMaxDark() {
        return _behaviourDark.GetEnergyMax();
    }

    //Get energy level light
    //*************************************************************************************************************************************************************************
    public int GetEnergyLight()
    {
        return _behaviourLight.GetEnergy();
    }

    //Get energy max level light
    //*************************************************************************************************************************************************************************
    public int GetEnergyMaxLight() {
        return _behaviourLight.GetEnergyMax();
    }

    //Get health dark
    //*************************************************************************************************************************************************************************
    public int GetHealthDark()
    {
        return _behaviourDark.GetHealth();
    }

    //Get max health dark
    //*************************************************************************************************************************************************************************
    public int GetHealthMaxDark() {
        return _behaviourDark.GetHealthMax();
    }

    //Get health light
    //*************************************************************************************************************************************************************************
    public int GetHealthLight()
    {
        return _behaviourLight.GetHealth();
    }

    //Get max health light
    //*************************************************************************************************************************************************************************
    public int GetHealthMaxLight() {
        return _behaviourLight.GetHealthMax();
    }

    //Get cooldown slash dark
    //*************************************************************************************************************************************************************************
    public float GetCooldownSlashDark()
    {
        return _behaviourDark.GetCoolddownSlash();
    }

    //Get cooldown slash light
    //*************************************************************************************************************************************************************************
    public float GetCooldownSlashLight()
    {
        return _behaviourLight.GetCoolddownSlash();
    }

    //Get cooldown beam dark
    //*************************************************************************************************************************************************************************
    public float GetCooldownBeamDark()
    {
        return _behaviourDark.GetCoolddownBeam();
    }

    //Get cooldown beam dark
    //*************************************************************************************************************************************************************************
    public float GetCooldownBeamLight()
    {
        return _behaviourLight.GetCoolddownBeam();
    }

    //Check if someone is dead
    //*************************************************************************************************************************************************************************
    public bool IsAPlayerDead()
    {
        bool isDead = false;

        if (_behaviourLight.IsDead())
            isDead = true;
        if ( _behaviourDark.IsDead())
            isDead = true;

        return isDead;
    }

    //Check dark dead
    //*************************************************************************************************************************************************************************
    public bool IsDarkdead() {
        return _behaviourDark.IsDead();
    }

    //Check light dead
    //*************************************************************************************************************************************************************************
    public bool IsLightdead() {
        return _behaviourLight.IsDead();
    }

    //Resurrect dark
    //*************************************************************************************************************************************************************************
    public float GetResurrectCooldownDark() {
        return 1 - _resurrectTimeDark / RES_COOLDOWN;
    }

    //Resurrect light
    //*************************************************************************************************************************************************************************
    public float GetResurrectCooldownLight() {
        return 1 - _resurrectTimeLight / RES_COOLDOWN;
    }
}
