using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField]
    private Enemy_Base _behavior;
    private Animator _animator;
    private AnimationStep _animState = AnimationStep.IDLE;
    private EnemyState _statePrev = EnemyState.NONE;
    private EnemyState _enemyState = EnemyState.IDLE;
    private float _time;
    // Use this for initialization
    void Start()
    {
        _animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        _enemyState = _behavior.GetState();

        // Animation -------------------------------------------------------------------------------------------------------------------------
        if (_enemyState == EnemyState.STAGGER)
        {
            _animator.speed = 0;
            return;
        }
        else
        {
            _animator.speed = 1;
        }

        // State -------------------------------------------------------------------------------------------------------------------------
        if (_enemyState == EnemyState.ATTACK)
            _animState = AnimationStep.ATTACK;
        else if (_enemyState == EnemyState.EXPLODE)
            _animState = AnimationStep.EXPLODE;
        else if (_enemyState == EnemyState.DEAD)
            _animState = AnimationStep.DEATH;
        else if (_enemyState == EnemyState.MOVE)
            _animState = AnimationStep.MOVE;
        else
            _animState = AnimationStep.IDLE;

        // Play animation -------------------------------------------------------------------------------------------------------------------------
        if (_enemyState != _statePrev)
        {
            if (_animState == AnimationStep.MOVE)
            {
                PlayAnimation("Move");
            }
            else if (_animState == AnimationStep.ATTACK)
            {
                PlayAnimation("Attack");
            }
            else if (_animState == AnimationStep.EXPLODE)
            {
                PlayAnimation("Explode");
            }
            else if (_animState == AnimationStep.SWITCH)
            {
                PlayAnimation("Switch");
            }
            else
            {
                IdleAnimation();
            }
            _statePrev = _enemyState;
        }
    }

    //Play corresponding animation and reset rest
    //*************************************************************************************************************************************************************************
    private void PlayAnimation(string stateName)
    {
        if(_animator.GetBool(stateName) == false)
        {
            IdleAnimation();
            _animator.SetBool(stateName, true);
        }
    }

    //Idle reset
    //*************************************************************************************************************************************************************************
    private void IdleAnimation()
    {
        _animator.SetBool("Move", false);
        _animator.SetBool("Attack", false);
        _animator.SetBool("Death", false);
        _animator.SetBool("Stagger", false);
        _animator.SetBool("Explode", false);
        _animator.SetBool("Switch", false);
    }
}