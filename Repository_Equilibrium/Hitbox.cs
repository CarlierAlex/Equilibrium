using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {

    private float _expireTime = 0.5f;
    private int _damage = 3;
    private bool _explode = false;
    private bool _postInitialize = false;
    private Vector3 _scaleMax;

    // Use this for initialization
    void Start () {
        _scaleMax = GetComponent<Transform>().localScale;
        this.gameObject.GetComponent<Transform>().localScale = Vector3.zero;
    }
	
	// Update is called once per frame
	void Update () {
		if(_explode)
        {
            float time = this.gameObject.GetComponent<Expire>().GetCurrentTime();
            float scale = (time / _expireTime);

            this.gameObject.GetComponent<Transform>().localScale = _scaleMax * scale;
        }
        else
        {
            if(_postInitialize == false)
            {
                this.gameObject.GetComponent<Transform>().localScale = _scaleMax;
                _postInitialize = true;
            }

        }
	}

    //Returns the damage
    //*************************************************************************************************************************************************************************
    public int GetDamage()
    {
        return _damage;
    }

    //Sets the value for the damage
    //*************************************************************************************************************************************************************************
    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    //Set expire time
    //*************************************************************************************************************************************************************************
    public void SetExpire(float time)
    {
        _expireTime = time;
        this.gameObject.AddComponent<Expire>();
        this.gameObject.GetComponent<Expire>().SetExpireTime(time);
    }

    //Set explosion
    //*************************************************************************************************************************************************************************
    public void SetExplode(bool explode)
    {
        _explode = explode;
    }
}
