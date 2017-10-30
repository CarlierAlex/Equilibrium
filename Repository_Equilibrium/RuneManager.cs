using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneManager : MonoBehaviour {

    private List<Runestone> _runeArr;
    private Runestone _runeCurr;
    private int _runeIdx = 0;
    private bool _postInitialize = false;
    private const int MAX_HEALTH = 50;

    private bool _isTakingDamage = false;

    // Use this for initialization
    void Start () {
        _runeArr = new List<Runestone>();
    }
	
	// Update is called once per frame
	void Update () {
        if(_postInitialize == false)
        {
            if (_runeArr == null || _runeArr.Count <= 0)
            {
                var list = GameObject.FindGameObjectsWithTag("Runestone");
                
                for(int i = 0; i < list.Length; i++)
                {
                    foreach (var obj in list)
                    {
                        var script = obj.GetComponent<Runestone>();
                        if (_runeArr.Contains(script) == false && script.GetIndex() == i)
                            _runeArr.Add(script);
                    }
                }

            }
            ChangeActive();

            if (_runeArr.Count > 0)
                _postInitialize = true;

            // parent indicators to HUD
            for (int i = 0; i < _runeArr.Count; ++i) {
                _runeArr[i].SetIndicatorParent(gameObject.transform);
            }


            ChangeActive();
        }

        ///////////////////////////////////////////////////////////////
        // Draw runestone indicators for the active runestone outside of screenspace
        EmperorScript emperor = GetComponent<EmperorScript>();
        CameraController cam = emperor.GetCameraController();
        for (int i = 0; i < _runeArr.Count; i++) {
            // Is enemy outside screen space?
            if (!cam.IsInViewSpace(_runeArr[i].transform.position)) {
                _runeArr[i].GetComponent<Runestone>().EnableIndicator(true);
                // Calculate indicator transform
                SetIndicatorPos(_runeArr[i].transform, cam);
                _runeArr[i].GetIndicatorTransform().SetAsLastSibling(); // make sure rune indicator is drawn on top of enemy indicators

            } else
                _runeArr[i].GetComponent<Runestone>().EnableIndicator(false);
        }
    }

    public int GetChangeWaveIdx()
    {
        if (_runeIdx + 1 < _runeArr.Count && _runeCurr != null)
            return _runeArr[_runeIdx + 1].GetComponent<Runestone>().GetWaveIndex();
        return 0;
    }

    public int GetHealthCurrent()
    {
        if (_runeIdx < _runeArr.Count && _runeCurr != null)
            return _runeCurr.GetHealth();
        return GetMaxHealth();
    }

    public void DamageCurrentStone(int damage) {
        _runeCurr.DamageHealth(damage);
        _isTakingDamage = true;
    }

    public bool IsTakingDamage() {
        if(_isTakingDamage || _runeCurr != null)
        {
            if (_runeCurr.IsTakingDamage())
            {
                _isTakingDamage = false;
                return true;
            }
        }
        return false;
    }

    public int GetMaxHealth()
    {
        if(_runeArr.Count > 0)
            return _runeArr[0].GetMaxHealth();
        return MAX_HEALTH;
    }

    public Transform GetRuneTransform()
    {
        if (_runeArr.Count > 0)
            return _runeCurr.gameObject.transform;
        return null;
    }

    public void ProceedNext()
    {
        _runeIdx++;
        ChangeActive();
    }

    public void ChangeActive()
    {
        if(_runeArr != null && _runeArr.Count > 0 && _runeIdx < _runeArr.Count)
        {
            foreach (var rune in _runeArr)
            {
                if(rune.GetIndex() == _runeIdx)
                {
                    rune.SetActive(true);
                    _runeCurr = rune;
                }
                else
                    rune.SetActive(false);
            }
        }
    }

    private void SetIndicatorPos(Transform runestoneTrans, CameraController cam) {

        Transform indicatorTransform = runestoneTrans.GetComponent<Runestone>().GetIndicatorTransform();
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        Vector3 screenPos = cam.GetComponent<Camera>().WorldToScreenPoint(runestoneTrans.position);
        screenPos -= screenCenter;

        float angle = Mathf.Atan2(screenPos.y, screenPos.x);
        angle -= 90 * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        // ratio for x coordinates && offset
        float cot = cos / sin;
        Vector3 screenBounds = 0.95f * screenCenter;

        // up or down
        if (cos > 0) {
            screenPos = new Vector3(-screenBounds.y / cot, screenBounds.y, 0);
            indicatorTransform.rotation = Quaternion.Euler(0, 0, -180);
        } else {
            screenPos = new Vector3(screenBounds.y / cot, -screenBounds.y, 0);
            indicatorTransform.rotation = Quaternion.Euler(0, 0, 0);
        }
        // if outside of x range
        if (screenPos.x > screenBounds.x) {
            screenPos = new Vector3(screenBounds.x, -screenBounds.x * cot, 0);
            indicatorTransform.rotation = Quaternion.Euler(0, 0, -90);

        } else if (screenPos.x < -screenBounds.x) {
            screenPos = new Vector3(-screenBounds.x, screenBounds.x * cot, 0);
            indicatorTransform.rotation = Quaternion.Euler(0, 0, 90);
        }

        screenPos += screenCenter;
        indicatorTransform.position = screenPos;
    }
}
