using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.IO;

public class EnemyManager : MonoBehaviour
{
    private List<GameObject> _enemyArr;
    [SerializeField]
    private GameObject _enemyJabPrefab;
    [SerializeField]
    private GameObject _enemyRangePrefab;
    [SerializeField]
    private GameObject _enemySiegePrefab;
    [SerializeField]
    private GameObject _enemyHybridPrefab;

    [SerializeField]
    private List<TextAsset> _listWavesArr;
    private int _waveCount = 0;
    private int _enemyTotalCount = 0;
    private int _enemyTotalKill = 0;

    [SerializeField]
    private List<S_EnemySpawn> _spawnPoints;
    private List<float> _spawnCooldowns;

    private bool _wasFilled = false;
    private bool _endGame = false;
    private bool _nextWave = false;

    [SerializeField]
    private List<float> _listWavesTimers;
    private float _nextWaveTimer = 0.0f;
    private float _nextWaveMaxTime = 5.0f;

    private Transform _currRune;

    private bool _afterStartInitialize = false;
    private bool _isWaveStarting = true;
    private bool _runesNotSet = false;

    // Use this for initialization
    void Start()
    {
        if (_enemyArr == null)
            _enemyArr = new List<GameObject>();
        if (_spawnPoints == null)
            _spawnPoints = new List<S_EnemySpawn>();

        if(_listWavesTimers.Count > 0)
            _nextWaveMaxTime = _listWavesTimers[0];
        else
            _nextWaveMaxTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        bool noEnemies = NoEnemies();
        bool finishedSpawning = AreSpawnsFinished();
        _nextWave = false;
        if (noEnemies == true && finishedSpawning == true && _afterStartInitialize == true) {
            _isWaveStarting = false;

            if (_waveCount + 1 >= _listWavesArr.Count || _listWavesArr[_waveCount + 1] == null) {
                _nextWave = false;
                _endGame = true;
            } else if (_listWavesArr[_waveCount + 1].text.Length == 0) {
                _nextWave = false;
                _endGame = true;
            } else {
                _nextWaveTimer += Time.deltaTime;
                if (_nextWaveTimer >= _nextWaveMaxTime) {
                    _nextWave = true;
                    _waveCount++;
                    _isWaveStarting = true;
                    _nextWaveTimer = 0;
                    if (_listWavesTimers.Count > _waveCount)
                        _nextWaveMaxTime = _listWavesTimers[_waveCount];
                    else
                        _nextWaveMaxTime = 0;
                }
            }

        }

        if ((_afterStartInitialize == false || (_nextWave == true && finishedSpawning == true)) && _endGame == false && _spawnPoints != null)
        {
            if(_afterStartInitialize == false)
            {
                var list = GameObject.FindGameObjectsWithTag("EnemySpawn");
                if (list.Length > 0)
                    foreach (var i in list)
                    {
                        var script = i.GetComponent<S_EnemySpawn>();
                        if (script != null && _spawnPoints.Contains(script) == false)
                            _spawnPoints.Add(script);
                    }
                _afterStartInitialize = true;
            }


            foreach (var i in _spawnPoints)
            {
                var script = i.GetComponent<S_EnemySpawn>();
                if (i != null && _waveCount < _listWavesArr.Count && script.GetActive() == false)
                {
                    SetSpawnLists(_listWavesArr[_waveCount]);
                    script.SetActive(true);
                }
            }
        }

        if (_currRune == null)
        {
            UpdateRune();
        }
        else if(_runesNotSet == true)
        {
            foreach (var enemy in _enemyArr)
            {
                if (enemy.GetComponent<Enemy_Base>() != null)
                {
                    enemy.GetComponent<Enemy_Base>().SetRune(_currRune);
                }
            }
        }

        if (_wasFilled == false && _enemyArr.Count > 0)
            _wasFilled = true;

        ///////////////////////////////////////////////////////////////
        // Draw enemy indicators for each enemy outside of screen space
        EmperorScript emperor = GetComponent<EmperorScript>();
        CameraController cam = emperor.GetCameraController();
        for(int i = 0; i < _enemyArr.Count; i++) 
        {
            // Is enemy outside screen space?
            if (!cam.IsInViewSpace(_enemyArr[i].transform.position)) {
                _enemyArr[i].GetComponent<Enemy_Base>().EnableIndicator(true);
                // Calculate indicator transform
                SetIndicatorPos(_enemyArr[i].transform, cam);

            }
            else
                _enemyArr[i].GetComponent<Enemy_Base>().EnableIndicator(false);
        }

    }

    // Returns a bool value depending on if there are any enemies left
    public bool DestroyedAllEnemies()
    {
        if (_endGame == true)// && AreSpawnsFinished() == true)
            return true;
        return false;
    }

    public void UpdateRune()
    {
        _currRune = this.gameObject.GetComponent<RuneManager>().GetRuneTransform();
    }

    public bool AreSpawnsFinished()
    {
        if (_spawnPoints == null)
            return false;
        if (_spawnPoints.Count <= 0)
            return false;

        foreach (var i in _spawnPoints)
        {
            if(i.IsFinished() == false)
                return false;
        }
        return true;
    }

    public void SpawnEnemy(Vector3 pos, Vector3 direction, EnemyType type, Faction fact)
    {
        if(_waveCount < _listWavesArr.Count)
        {
            GameObject obj = null;
            if (type == EnemyType.GRUNT)
            {
                //GameObject prefabEnemy = (fact == Faction.LIGHT) ? (_enemyLightPrefab) : (_enemyDarkPrefab);
                obj = Instantiate(_enemyJabPrefab, pos, Quaternion.LookRotation(direction)) as GameObject;
            }
            else if (type == EnemyType.GUNNER)
            {
                obj = Instantiate(_enemyRangePrefab, pos, Quaternion.LookRotation(direction)) as GameObject;        
            }
            else if (type == EnemyType.SIEGE)
            {
                obj = Instantiate(_enemySiegePrefab, pos, Quaternion.LookRotation(direction)) as GameObject;
            }
            else if (type == EnemyType.HYBRID)
            {
                obj = Instantiate(_enemyHybridPrefab, pos, Quaternion.LookRotation(direction)) as GameObject;
            }

            // parent enemy indicater to HUD
            if (obj != null)
            {
                _enemyTotalCount++;
                obj.GetComponent<Enemy_Base>().SetFaction(fact);
                obj.GetComponent<Enemy_Base>().SetEnemyType(type);
                if (_currRune != null)
                    obj.GetComponent<Enemy_Base>().SetRune(_currRune);
                else
                    _runesNotSet = true;
                obj.GetComponent<Enemy_Base>().SetIndicatorParent(this.gameObject.transform);
                _enemyArr.Add(obj);
            }
        }
    }

    //Checks for enemies tagged dead and updates accordingly
    //Also adds any enemies that were not yet included to the list
    private bool NoEnemies()
    {
        foreach (var enemy in _enemyArr)
        {
            if (enemy.GetComponent<Enemy_Base>() != null)
            {
                if (enemy.GetComponent<Enemy_Base>().IsDead() == true)
                {
                    DestroyImmediate(enemy);
                    _enemyTotalKill++;
                }
            }


        }
        _enemyArr.RemoveAll(list_item => list_item == null);

        if(_enemyArr.Count <= 0 && _wasFilled == true)
            return true;
        return false;
    }

    public void CheckEnemiesNotInList()
    {
        var list = GameObject.FindGameObjectsWithTag("Enemy");
        if (_enemyArr == null)
        {
            _enemyArr = new List<GameObject>();
            foreach (var obj in list)
                _enemyArr.Add(obj);
        }
        else
        {
            foreach (var obj in list)
            {
                bool found = false;
                if (_enemyArr.Contains(obj.gameObject))
                        found = true;

                if (found == false)
                {
                    _enemyArr.Add(obj);
                    _enemyTotalCount++;
                }
            }
        }
    }

    private void SetSpawnLists(TextAsset fileName)
    {
        string allData = fileName.text;
        string data = "";

        if (allData == null)
            Debug.LogError("Failed to load enemy list:" + fileName);

        if (allData.Length > 0)
        {
            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                var spawn = _spawnPoints[i];

                if (spawn != null)
                {
                    int spawnIdx = spawn.GetSpawnIdx();
                    int lineIdx = allData.IndexOf("<");
                    int lineNewIdx = allData.IndexOf(">");

                    for (int f = 0; f < spawnIdx; f++)
                    {
                        lineIdx = allData.IndexOf("<", lineIdx + 1);
                        lineNewIdx = allData.IndexOf(">", lineNewIdx + 1);
                    }

                    if (lineNewIdx <= 0 || lineIdx < 0 || lineNewIdx < lineIdx)
                        data = null;
                    else
                    {
                        data = allData.Substring(lineIdx, lineNewIdx - lineIdx + 1);
                        var list = CreateEnemyList(data);
                        if(list != null)
                            spawn.SetSpawnList(list);
                    }
                }
            }
        }
    }

    private List<EnemySpawnData> CreateEnemyList(string data)
    {
        List<EnemySpawnData> enemyArr = new List<EnemySpawnData>();
        string line = "";
        int lineIdx = 0;
        int lineNewIdx = 0;
        EnemySpawnData enemy;
        enemy.type = EnemyType.NONE;
        enemy.faction = Faction.NONE;
        enemy.delay = 0;

        if (data == null)
            Debug.LogError("Failed to load enemy list:" + data);
        do
        {
            line = null;
            lineNewIdx = data.IndexOf(";", lineIdx + 1);
            if (lineNewIdx <= 0)
            {
                lineNewIdx = data.IndexOf(">", lineIdx + 1);
                if (lineNewIdx <= 0)
                    line = null;
            }
            else
                line = data.Substring(lineIdx, lineNewIdx - lineIdx + 1);

            if (line != null)
            {
                string result = "";
                int nextIdx = 0, idx = 0;
                idx = line.IndexOf("delay:");

                if (idx > 0)
                {
                    idx += 6;
                    idx = line.IndexOf("|", idx) + 1;
                    nextIdx = line.IndexOf("|", idx);

                    if (nextIdx > idx && idx > 0)
                    {
                        result = line.Substring(idx, nextIdx - idx);
                    }
                }
                if(result.Length > 0)
                {
                    float amount = int.Parse(result);
                    if (amount > 0)
                        enemy.delay = amount;
                }


                idx = line.IndexOf("type:");
                idx += 5;
                nextIdx = line.IndexOf("faction:", idx);
                result = line.Substring(idx, nextIdx - idx);
                if (result.Contains("grunt"))
                    enemy.type = EnemyType.GRUNT;
                else if (result.Contains("ranged"))
                    enemy.type = EnemyType.GUNNER;
                else if (result.Contains("siege"))
                    enemy.type = EnemyType.SIEGE;
                else if (result.Contains("hybrid"))
                    enemy.type = EnemyType.HYBRID;
                else
                    enemy.type = EnemyType.NONE;


                idx = nextIdx;
                idx += 8;
                nextIdx = line.IndexOf(";", idx);
                result = line.Substring(idx, nextIdx - idx);
                if (result.Contains("light"))
                    enemy.faction = Faction.LIGHT;
                else if (result.Contains("dark"))
                    enemy.faction = Faction.DARK;
                else
                    enemy.faction = Faction.NONE;
            }
            if (enemy.type != EnemyType.NONE && enemy.faction != Faction.NONE)
                enemyArr.Add(enemy);

            lineIdx = lineNewIdx;
            enemy.type = EnemyType.NONE;
            enemy.faction = Faction.NONE;
            enemy.delay = 0;
        }
        while (line != null);

        if (enemyArr.Count > 0)
            return enemyArr;
        return null;
    }

    public int GetEnemyCount()
    {
        return _enemyArr.Count;
    }

    public int GetTotalEnemyCount()
    {
        return _enemyTotalCount;
    }

    public int GetTotalKilledCount()
    {
        return _enemyTotalKill;
    }

    public int GetWaveNumber()
    {
        if(_listWavesArr.Count <= _waveCount)
            return _listWavesArr.Count + 1;
        return _waveCount + 1;
    }

    public int GetTotalWaveNumber()
    {
        return _listWavesArr.Count + 1;
    }

    public bool IsWaveStarting() {
        if (_isWaveStarting) {
            _isWaveStarting = false;
            return true;
        }
        return false;
    }

    private void SetIndicatorPos(Transform enemyTrans, CameraController cam) {

        Transform indicatorTransform = enemyTrans.GetComponent<Enemy_Base>().GetIndicatorTransform();
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        Vector3 screenPos = cam.GetComponent<Camera>().WorldToScreenPoint(enemyTrans.position);
        screenPos -= screenCenter;

        float angle = Mathf.Atan2(screenPos.y, screenPos.x);
        angle -= 90 * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        //Debug.Log("X: " + screenPos.x + ",    Y: " + screenPos.y);
        //Debug.Log("Angle: " + angle);
        //Debug.Log("Cos: " + cos + ",   Sin: " + sin);

        // ratio for x coordinates && offset
        float cot = cos / sin;
        Vector3 screenBounds = 0.95f * screenCenter;

        // up or down
        if(cos > 0) {
            screenPos = new Vector3(-screenBounds.y / cot, screenBounds.y, 0);
            indicatorTransform.rotation = Quaternion.Euler(0, 0, 0);
        } else {
            screenPos = new Vector3(screenBounds.y / cot, -screenBounds.y, 0);
            indicatorTransform.rotation = Quaternion.Euler(0, 0, -180); 
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
        indicatorTransform.position= screenPos;
    }
}
