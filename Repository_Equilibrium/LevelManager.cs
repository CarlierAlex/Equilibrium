using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    [System.Serializable]
    public struct WaveTutorial
    {
        public int waveNr;
        public List<GameObject> tutorialWindows;
    }

    [System.Serializable]
    public struct WaveAbility
    {
        public int waveNr;
        public string access;
    }

    [SerializeField]
    private List<WaveTutorial> _tutorialsList;
    [SerializeField]
    private List<WaveAbility> _abilityList;
    private List<GameObject> _tutorials;

    private EnemyManager _enemyManager;
    private RuneManager _runeManager;
    private PlayerManager _playerManager;

    private int _currentWave = 0;

	void Start () {
        _enemyManager = GetComponent<EnemyManager>();
        _runeManager = GetComponent<RuneManager>();
        _playerManager = GetComponent<PlayerManager>();

        if (!_enemyManager || !_runeManager || !_playerManager)
            Debug.LogError("Failed to load manager scripts from the LevelManager");

        _tutorials = new List<GameObject>();
	}
	
	void Update () {
        if (_enemyManager.IsWaveStarting()) {
            _currentWave = _enemyManager.GetWaveNumber();
            ActivateCurrentAbilities();
            LoadCurrentTutorials();
        }

        DisplayTutorials();
    }

    // Check for abilities to activate during current wave
    //*************************************************************************************************************************************************************************
    private void ActivateCurrentAbilities() {
        for (int i = 0; i < _abilityList.Count; ++i) {
            if (_currentWave == _abilityList[i].waveNr) {
                _playerManager.SetAbilityAccess(GetControlAccessFromString(_abilityList[i].access));
            }
        }
    }

    // Check for tutorials to instantiate during current wave
    //*************************************************************************************************************************************************************************
    private void LoadCurrentTutorials() {

        for (int i = 0; i < _tutorialsList.Count; ++i) {
            if (_currentWave == _tutorialsList[i].waveNr) {
                // fill tutorials list with instances of the tutorials to display
                for (int inst = 0; inst < _tutorialsList[i].tutorialWindows.Count; ++inst) {
                    _tutorials.Add(Instantiate(_tutorialsList[i].tutorialWindows[inst], this.gameObject.transform, false) as GameObject);
                    _tutorials[inst].SetActive(false);
                }
            }
        }
    }

    //Display tutorials
    //*************************************************************************************************************************************************************************
    private void DisplayTutorials() {
        if(_tutorials != null) {
            for (int i = 0; i < _tutorials.Count; ++i) 
            {
                if (_tutorials[i] != null) {
                    _tutorials[i].SetActive(true);
                    break;
                }
            }
            //_tutorials.Clear();
        }
    }

    //Set access
    //*************************************************************************************************************************************************************************
    private ControlAccess GetControlAccessFromString(string access) {
        ControlAccess controlAccess;
        controlAccess = ControlAccess.NONE;
        switch (access) {
            case "ATTACK":
                controlAccess = ControlAccess.ATTACK;
                break;
            case "BEAM":
                controlAccess = ControlAccess.BEAM;
                break;
            case "DASH":
                controlAccess = ControlAccess.DASH;
                break;
            case "FULL":
                controlAccess = ControlAccess.FULL;
                break;
            default:
                Debug.LogError("LevelManager: Incorrect Control Access name chosen");
                break;
        }

        return controlAccess;
    }
}
