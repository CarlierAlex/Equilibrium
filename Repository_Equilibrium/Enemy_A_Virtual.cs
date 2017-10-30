using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class Enemy_A_Virtual : MonoBehaviour {

    protected Enemy_Base _baseScript;
    protected UnityEngine.AI.NavMeshAgent _navMeshAgent;

    protected float _animationTimer = 0.0f;
    protected bool _doesAttack = false;
    protected AnimationPhase _animPhase = AnimationPhase.NONE;

    protected GameObject _hitBox;
    protected bool _postInitialize = false;
    protected Vector3 _direction = Vector3.zero;

    // Use this for initialization
    protected virtual void Start()
    {
        _baseScript = this.gameObject.GetComponent<Enemy_Base>();
        _navMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
	
	// Update is called once per frame
	void Update () {

    }

    public virtual void Delete()
    {
        
    }

    public virtual AnimationPhase GetAnimState()
    {
        return _animPhase;
    }

    public virtual EnemyState GetEnemyState()
    {
        return _baseScript.GetState();
    }

    public virtual float GetAttackRange()
    {
        return _baseScript.GetRangeAttack();
    }

    public virtual float GetAttackTime()
    {
        return 0;
    }

    public virtual bool GetLunge()
    {
        return false;
    }

    public virtual Vector3 GetTargetDirection()
    {
        return _direction;
    }
}
