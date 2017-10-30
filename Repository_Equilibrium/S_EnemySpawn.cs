using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_EnemySpawn : MonoBehaviour
{
    private const float SPAWN_TIME_JAB = 0.7f;
    private const float SPAWN_TIME_SIEGE = 1.7f;
    private const float SPAWN_TIME_GUNNER = 1.2f;


    private List<EnemySpawnData> _enemyArr;
    private float _spawnEnemyTime = 0.0f;
    private float _spawnTimer = 0;
    private bool _doSpawn = false;
    private bool _isListSet = false;
    private bool _finishedSpawn = false;
    private int _count = 0;

    [SerializeField]
    private int _idx = 0;

    [SerializeField]
    private EnemyManager _enemyManager;

    // Use this for initialization
    void Start() {
        var list = GameObject.FindGameObjectsWithTag("Game");
        if (list.Length > 1)
            Debug.Log("Why are there multiple game tags?");
        if (list.Length > 0)
            _enemyManager = list[0].GetComponent<EnemyManager>();
        _enemyArr = new List<EnemySpawnData>();

        //Dont render arrow
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;

        _isListSet = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_enemyManager != null && _enemyArr != null)
        {
            if (_isListSet == true && _enemyArr.Count > _count && _doSpawn == true)
            {
                //Delay
                //--------------------------------------------------------------------------------------------------------------------------
                _spawnEnemyTime = _enemyArr[_count].delay;

                //Type time
                //--------------------------------------------------------------------------------------------------------------------------
                if (_count > 0)
                {
                    if (_enemyArr[_count].type == EnemyType.GRUNT)
                        _spawnEnemyTime += SPAWN_TIME_JAB;
                    else if (_enemyArr[_count].type == EnemyType.GUNNER)
                        _spawnEnemyTime += SPAWN_TIME_GUNNER;
                    else if (_enemyArr[_count].type == EnemyType.SIEGE)
                        _spawnEnemyTime += SPAWN_TIME_SIEGE;
                }


                //Spawn on time
                //--------------------------------------------------------------------------------------------------------------------------
                _spawnTimer += Time.deltaTime;
                if (_spawnTimer > _spawnEnemyTime)
                {
                    _spawnTimer = 0;
                    if (_enemyArr.Count > _count)
                        _enemyManager.SpawnEnemy(this.transform.position, this.transform.forward, _enemyArr[_count].type, _enemyArr[_count].faction);
                    _count++;
                }
            }
        }

        //Finish check
        //--------------------------------------------------------------------------------------------------------------------------
        if (_enemyArr.Count <= _count)
        {
            _isListSet = false;
            _doSpawn = false;
            _finishedSpawn = true;
        }
    }

    //Check for finished spawning
    //*************************************************************************************************************************************************************************
    public bool IsFinished()
    {
        return _finishedSpawn;
    }

    //Set active
    //*************************************************************************************************************************************************************************
    public void SetActive(bool active = false)
    {
        _doSpawn = active;
    }

    //Check active
    //*************************************************************************************************************************************************************************
    public bool GetActive()
    {
        return _doSpawn;
    }

    //Set spawn list
    //*************************************************************************************************************************************************************************
    public void SetSpawnList(List<EnemySpawnData> enemyArr = null)
    {
        _enemyArr.Clear();

        _enemyArr = enemyArr;
        _isListSet = true;
        _doSpawn = true;
        _finishedSpawn = false;
        _count = 0;
    }

    //Set spawn idx for corresponding part of the wave
    //*************************************************************************************************************************************************************************
    public void SetSpawnIdx(int i = -1)
    {
        _idx = i;
    }

    //Get spawn part idx
    //*************************************************************************************************************************************************************************
    public int GetSpawnIdx()
    {
        return _idx;
    }
}
