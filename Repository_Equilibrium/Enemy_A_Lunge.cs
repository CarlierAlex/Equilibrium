using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

public class Enemy_A_Lunge : MonoBehaviour
{
    private float _distanceTotal = 0.0f;
    private bool _dash = false;

    protected Enemy_A_Virtual _baseScript;
    protected UnityEngine.AI.NavMeshAgent _navMeshAgent;

    // Use this for initialization
    void Start () {
        _baseScript = this.gameObject.GetComponent<Enemy_A_Virtual>();
        if(_baseScript == null)
            _baseScript = this.gameObject.GetComponent<Enemy_A_Melee>();
        if (_baseScript == null)
            _baseScript = this.gameObject.GetComponent<Enemy_A_Ranged>();
        if (_baseScript == null)
            _baseScript = this.gameObject.GetComponent<Enemy_A_Explode>();

        _navMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
	
	// Update is called once per frame
	void Update () {
        //Execute if attack has started
        //-------------------------------------------------------------------------------------------------------------------------------------------
        if (_baseScript.GetAnimState() == AnimationPhase.ATTACKSTART)
        {
            float range = _baseScript.GetAttackRange();
            float time = _baseScript.GetAttackTime();
            Vector3 direction = _baseScript.GetTargetDirection();

            //Overshoot check
            //-------------------------------------------------------------------------------------------------------------------------------------------
            bool wasSmaller = true;
            if (_distanceTotal >= range)
                wasSmaller = false;
            float distance = (range / time) * Time.deltaTime;
            _distanceTotal += distance;

            _navMeshAgent.autoBraking = true;

            //Counter overshoot
            //-------------------------------------------------------------------------------------------------------------------------------------------
            if (_distanceTotal < range)
            {
                float vel = 0;
                if (Time.deltaTime != 0)
                    vel = distance / Time.deltaTime;
                if (vel != 0 && _baseScript.GetEnemyState() != EnemyState.STAGGER)
                {
                    _navMeshAgent.velocity = direction * vel;
                    _navMeshAgent.autoBraking = false;
                }
            }
            else if (_distanceTotal > range && wasSmaller == true && _baseScript.GetEnemyState() != EnemyState.STAGGER)
            {
                float remDistance = (range - (_distanceTotal - distance));
                _navMeshAgent.velocity = direction * (remDistance / Time.deltaTime);
                _navMeshAgent.autoBraking = true;
            }
        }
        else
        {
            _distanceTotal = 0;
        }
    }
}
