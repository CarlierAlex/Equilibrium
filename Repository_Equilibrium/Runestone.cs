using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runestone : MonoBehaviour {

    [SerializeField]
    private int _idx = 0;
    [SerializeField]
    private int _waveIdx = 0;

    private const int MAX_HEALTH = 100;
    private int _health = MAX_HEALTH;
    private bool _isActive = false;
    private bool _isHit = false;
    private int _damageTaken = 0;
    private float _invincibleTime = 0;
    private const float INVINCIBLE_DURATION = 0.05f;
    private bool _isTakingDamage = false;

    [SerializeField]
    private GameObject _indicator;

    // Use this for initialization
    void Start () {
        _indicator = Instantiate(_indicator);
	}
	
	// Update is called once per frame
	void Update () {
        if (_isActive == true)
        {
            if (_isHit == true)
            {
                if (_invincibleTime == 0)
                {
                    _health -= _damageTaken;
                }

                _invincibleTime += Time.deltaTime;
                if (_invincibleTime >= INVINCIBLE_DURATION)
                {
                    _invincibleTime = 0;
                    _isHit = false;
                }
            }

            if (_health < 0)
                _health = 0;
        }
    }

    //Trigger: damage check
    //*************************************************************************************************************************************************************************
    private void OnTriggerEnter(Collider other)
    {
        if (_isActive == true)
        {
            if (other.gameObject.CompareTag("EnemyAttack") && _isHit == false)
            {
                _isTakingDamage = true;
                if (other.gameObject.GetComponent<Hitbox>() != null)
                    _damageTaken = other.gameObject.GetComponent<Hitbox>().GetDamage();
                else
                    _damageTaken = other.gameObject.transform.parent.gameObject.GetComponent<Enemy_Base>().GetDamage();
                _isHit = true;
            }
        }
    }

    //Reset damage check
    //*************************************************************************************************************************************************************************
    public bool IsTakingDamage() {
        if(_isTakingDamage) {
            _isTakingDamage = !_isTakingDamage;
            return true;
        }
        return false;
    }

    //Check if rune is active
    //*************************************************************************************************************************************************************************
    public void SetActive(bool isActive)
    {
        _isActive = isActive;
    }

    //Get health rune
    //*************************************************************************************************************************************************************************
    public int GetHealth()
    {
        return _health;
    }

    //Get max health rune
    //*************************************************************************************************************************************************************************
    public int GetMaxHealth()
    {
        return MAX_HEALTH;
    }

    //Get rune idx
    //*************************************************************************************************************************************************************************
    public int GetIndex()
    {
        return _idx;
    }

    //Get enemy wave activation idx
    //*************************************************************************************************************************************************************************
    public int GetWaveIndex()
    {
        return _waveIdx;
    }

    //Damage
    //*************************************************************************************************************************************************************************
    public void DamageHealth(int damage) {
        _health -= damage;
        if (_health < 0)
            _health = 0;
    }

    /* --- Indicator methods --- */ 
    public void EnableIndicator(bool enabled) {
        _indicator.SetActive(enabled);
    }

    public void SetIndicatorParent(Transform trans) {
        _indicator.transform.SetParent(trans);
    }

    public Transform GetIndicatorTransform() {
        return _indicator.transform;
    }
}
