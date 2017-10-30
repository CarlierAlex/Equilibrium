using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEditor;

public class EmperorScript : MonoBehaviour
{

    [SerializeField]
    private Transform _pickupSpawn;
    [SerializeField]
    private Transform _spawnDark;
    [SerializeField]
    private Transform _spawnLight;
    [SerializeField]
    private CameraController _camController;
    [SerializeField]
    private GameObject _pauseMenu;
    [SerializeField]
    private GameObject _deathScreen;
    [SerializeField]
    private GameObject _victoryScreen;

    private EnemyManager _enemyManager;
    private HUD_Manager _hudManager;
    private PlayerManager _playerManager;
    private RuneManager _runeManager;

    //UI STUFF
    private GameObject _pauseMenuInstance;

    protected static bool _restart = false;

    private const int DARK_HEALTH_WIN = 0;
    private const int LIGHT_HEALTH_WIN = 10;
    private const float GAME_TIME_END = 3.0f * 60.0f;
    private const float RESET_TIME_END = 5.0f;
    private const float PICKUP_TIME = 1.0f;

    private float _gameTime = 0;
    private float _totalGameTime = 0;
    private float _pickupTime = 0;
    private int _health;
    private bool _end = false;

    // Use this for initialization
    void Start()
    {
        _health = (DARK_HEALTH_WIN + LIGHT_HEALTH_WIN) / 2;

        //Get enemy manager and check for enemies already in the level
        _enemyManager = this.gameObject.GetComponent<EnemyManager>();
        _hudManager = this.gameObject.GetComponent<HUD_Manager>();
        _playerManager = this.gameObject.GetComponent<PlayerManager>();
        _runeManager = this.gameObject.GetComponent<RuneManager>();

        _enemyManager.CheckEnemiesNotInList();

        // Make sure the game unpauses at the start of a level
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //*****************//
        //-TOGGLE CONTROLS-//
        //if (Input.GetKeyDown(KeyCode.M))
        //    _playerManager.ToggleKeyboardControls();

        //****************//
        //-INPUT && MENUS-//
        if (Input.GetButtonDown("Reset") == true)
            Reset();

        if (Input.GetButtonDown("Pause") == true) {
            if (_pauseMenuInstance) {
                if(!_pauseMenuInstance.GetComponent<UI_PauseMenu>().AreControlsDisplayed()) {
                    Destroy(_pauseMenuInstance);
                     Pause(false);
                }
            }
            else {
                _pauseMenuInstance = Instantiate(_pauseMenu);
                Pause(true);
            }
        }

        //************//
        //-GAME TIMER-//
        _gameTime += Time.deltaTime;
        if (_gameTime > RESET_TIME_END && _end == true)
        {
            _gameTime = 0;
            Reset();
        }
        else if (_gameTime > GAME_TIME_END)
        {
            _end = true;
            _gameTime = 0;
        }
        _totalGameTime += Time.deltaTime;

        //****//
        //-UI-//
        _hudManager.SetPlayerDarkUI(_playerManager.GetHealthDark(), _playerManager.GetEnergyDark(),
            _playerManager.GetCooldownSlashDark(), _playerManager.GetCooldownBeamDark());
        _hudManager.SetPlayerLightUI(_playerManager.GetHealthLight(), _playerManager.GetEnergyLight(), 
            _playerManager.GetCooldownSlashLight(), _playerManager.GetCooldownBeamLight());
        _hudManager.UpdateEnemyStats(_enemyManager.GetEnemyCount());
        _hudManager.UpdateRunestoneUI(_runeManager.GetHealthCurrent(), _runeManager.IsTakingDamage());

        //**********//
        //--//
        int waveNr = _enemyManager.GetWaveNumber();
        int changeWaveIdx = _runeManager.GetChangeWaveIdx();
        if (waveNr > changeWaveIdx && changeWaveIdx > 0)
        {
            if(waveNr > changeWaveIdx)
                _runeManager.ProceedNext();
            _enemyManager.UpdateRune();
        }

        //**********//
        //-GAME END-//
        if (_enemyManager.DestroyedAllEnemies() == true && _end == false)
        {
            Pause(true);
            Instantiate(_victoryScreen).transform.SetParent(this.transform); // parent it so it finds emperor manager scripts
            _gameTime = (_end == false) ? 0 : _gameTime;
            _end = true;
        }
        else if (_runeManager.GetHealthCurrent() <= 0 && _end == false)
        {
            Pause(true);
            Instantiate(_deathScreen).transform.SetParent(this.transform); // parent it so it finds emperor manager scripts
            _gameTime = (_end == false) ? 0 : _gameTime;
            _end = true;
        }
    }

    //RESET
    //*********************************************************************************************************************************************************
    private void Reset()
    {
        Pause(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //PAUSE
    //*********************************************************************************************************************************************************
    private void Pause(bool isPaused)
    {
        Time.timeScale = (isPaused == true) ? 0 : 1;
    }

    //ACCESS MANAGERS FUNCTONS
    //GET PLAYER MANAGER
    //*********************************************************************************************************************************************************
    public PlayerManager GetPlayerManager() {
        return _playerManager;
    }

    //GET GAME TIME
    //*********************************************************************************************************************************************************
    public float GetGameTime() {
        return _totalGameTime;
    }

    //GET CAM CONTROLLER
    //*********************************************************************************************************************************************************
    public CameraController GetCameraController() {
        return _camController;
    }

    //GET HUD MANAGER
    //*********************************************************************************************************************************************************
    public HUD_Manager GetHUDManager() {
        return _hudManager;
    }
}
