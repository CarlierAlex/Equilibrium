using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]

public class CharacterAnimations : MonoBehaviour {

    private CharacterBehavior _behavior;
    private Animator _animator;
    private AnimationStep _animState = AnimationStep.IDLE;
    private PlayerState _playerState = PlayerState.IDLE;


    // Use this for initialization
    void Start () {
        _animator = this.GetComponent<Animator>();
        _behavior = this.GetComponent<CharacterBehavior>();
    }
	
	// Update is called once per frame
	void Update () {

        _playerState = _behavior.GetPlayerState();

        // Animation -------------------------------------------------------------------------------------------------------------------------
        if (_playerState == PlayerState.WALK)
            _animState = AnimationStep.MOVE;
        else if(_playerState == PlayerState.ATTACK)
            _animState = AnimationStep.ATTACK;
        else if (_playerState == PlayerState.ATTACKCHARGE)
            _animState = AnimationStep.ATTACKCHARGE;
        else if (_playerState == PlayerState.BEAM)
            _animState = AnimationStep.BEAM;
        else if (_playerState == PlayerState.DASH)
            _animState = AnimationStep.DASH;
        else if (_playerState == PlayerState.DEAD)
            _animState = AnimationStep.DEATH;
        else
            _animState = AnimationStep.IDLE;

        if (_animState == AnimationStep.MOVE)
        {
            if (_animator.GetBool("Moving") == false)
            {
                _animator.SetBool("Moving", true);
                _animator.SetBool("Beam", false);
                _animator.SetBool("Attack", false);
                _animator.SetBool("Death", false);
            }
        }
        else if (_animState == AnimationStep.ATTACK)
        {
            if (_animator.GetBool("Attack") == false)
            {
                _animator.SetBool("Moving", false);
                _animator.SetBool("Beam", false);
                _animator.SetBool("Attack", true);
                _animator.SetBool("Death", false);
            }
        }
        else if (_animState == AnimationStep.BEAM)
        {
            if (_animator.GetBool("Beam") == false)
            {
                _animator.SetBool("Moving", false);
                _animator.SetBool("Beam", true);
                _animator.SetBool("Attack", false);
                _animator.SetBool("Death", false);
            }
        }
        else if(_animState == AnimationStep.DEATH)
        {
            if (_animator.GetBool("Death") == false)
            {
                _animator.SetBool("Moving", false);
                _animator.SetBool("Beam", false);
                _animator.SetBool("Attack", true);
                _animator.SetBool("Death", true);
            }
        }
        else
        {
            if (_animator.GetBool("Attack") == true || _animator.GetBool("Moving") == true || _animator.GetBool("Beam") == true)
            {
                _animator.SetBool("Moving", false);
                _animator.SetBool("Beam", false);
                _animator.SetBool("Attack", false);
                _animator.SetBool("Death", false);
            }
        }
    }

    private void PlayAnimation(string stateName)
    {
        _animator.Play(stateName);
    }
}
