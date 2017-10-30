using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatAnimation : MonoBehaviour {
    [SerializeField]
    private float _time = 1.0f;
    [SerializeField]
    private float _range = 1.0f;
    private float _floatTime = 0.0f;
    private Transform _originalTransform;
    private int _multiplier = 1;

	void Start () {
        _originalTransform = gameObject.transform;
    }
	
	void Update () {
        _floatTime += Time.deltaTime * _multiplier;
        if (_floatTime > 1 || _floatTime < 0)
            _multiplier = -_multiplier;

        float y = Mathf.Lerp(_originalTransform.position.y - _range / 2.0f, _originalTransform.position.y + _range / 2.0f, _floatTime);
        gameObject.transform.position = new Vector3(_originalTransform.position.x, y, _originalTransform.position.z);
	}
}
