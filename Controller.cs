using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class Controller : MonoBehaviour
{
    private TMP_Text _timerText;
    private float _time;
    public int frame;
    private bool _levelHasBeenCompleted; // IS THIS NEEDED???
    public bool gameIsPaused; 
    private bool _sytemInRewind;
    private float _rewindTimeScale;

    // End colors for level complete variables
    private UnityEngine.Object[] _endColorsAll;
    private List<string> _endColorsAllActivated;
    public int endColorsAllNumber;

    // Calculate level completeness score and save it
    public int levelValue;
    public int starsEarnedValue;
    private GameStateSaveSystem _gameStateSaveSystem;

    // Scene state managing
    public static Controller instance;
    public sceneState sceneState;
    public static event Action<sceneState> onSceneStateChange;
    public string playerColorCurrentTag;

    // Player respawn
    public List<GameObject> playersToRespawn;

    // Level retry
    private List<PlayerColor> _playerColors;
    public List<string> playersAlreadyPlayed;

    // Camera on rewind
    private ColorCurves _globalVolumeProfileColorCurves;
    private ChromaticAberration _globalVolumeProfileChromaticAberration;
    private GameObject _secondaryCamera;

    // Audio maganer
    private AudioManager _audioManager;
    public void updateSceneState(sceneState newSceneState)
    {
        sceneState = newSceneState;

        switch (newSceneState)
        {
            case sceneState.setup:
                Debug.Log("Controller: Scene state is Setup");
                resetTimer();
                setupHandler();
                break;
            case sceneState.selectPlayer:
                Debug.Log("Controller: Scene state is SelectPlayer");
                if (_sytemInRewind)
                {
                    rewindTimerStop();
                    stopRewindMode();
                }
                break;
            case sceneState.playerRed:
                playerColorCurrentTag = "playerRed";
                Debug.Log("Controller: Scene state is PlayerRed");
                resetTimer();
                countTimerStart();
                addPlayerColor(playerColorCurrentTag);
                Debug.Log($"Number of players already played: {playersAlreadyPlayed.Count}");
                break;
            case sceneState.playerBlue:
                playerColorCurrentTag = "playerBlue";
                Debug.Log("Controller: Scene state is PlayerBlue");
                resetTimer();
                countTimerStart();
                addPlayerColor(playerColorCurrentTag);
                Debug.Log($"Number of players already played: {playersAlreadyPlayed.Count}");
                break;
            case sceneState.playerGreen:
                playerColorCurrentTag = "playerGreen";
                Debug.Log("Controller: Scene state is PlayerGreen");
                resetTimer();
                countTimerStart();
                addPlayerColor(playerColorCurrentTag);
                Debug.Log($"Number of players already played: {playersAlreadyPlayed.Count}");
                break;
            case sceneState.playerYellow:
                playerColorCurrentTag = "playerYellow";
                Debug.Log("Controller: Scene state is PlayerYellow");
                resetTimer();
                countTimerStart();
                addPlayerColor(playerColorCurrentTag);
                Debug.Log($"Number of players already played: {playersAlreadyPlayed.Count}");
                break;
            case sceneState.turnOver:
                Debug.Log("Controller: Scene state is TurnOver");
                countTimerStop();
                turnOverHandler();
                break;
            case sceneState.rewind:
                Debug.Log("Controller: Scene state is Rewind");
                rewindTimerStart();
                startRewindMode();
                break;
            case sceneState.fail:
                Debug.Log("Controller: Scene state is Fail");
                failHandler();
                break;
            case sceneState.levelComplete:
                Debug.Log("Controller: Scene state is LevelComplete");
                levelCompleteHandler();
                break;
        }

        onSceneStateChange?.Invoke(newSceneState);
    }

    private void Awake()
    {
        instance = this;

        _audioManager = GameObject.FindObjectOfType<AudioManager>();

        _timerText = GameObject.Find("timer").GetComponent<TMP_Text>();
        _timerText.gameObject.SetActive(false);

        levelValue = SceneManager.GetActiveScene().buildIndex - 1; // StartMenu = 0; LevelSelect = 1; Lvl1 = 2; ...
        starsEarnedValue = 1;
        TMP_Text _levelValueText = GameObject.Find("levelValueText").GetComponent<TMP_Text>();
        _levelValueText.text = string.Format("{0}", levelValue);

        _endColorsAll = UnityEngine.Object.FindObjectsOfType<EndColor>();
        endColorsAllNumber = _endColorsAll.Length;
        _endColorsAllActivated = new List<string>(endColorsAllNumber);

        playersToRespawn = new List<GameObject>(UnityEngine.Object.FindObjectsOfType<PlayerColor>().Length);

        _playerColors = new List<PlayerColor>(FindObjectsOfType<PlayerColor>());
        playersAlreadyPlayed = new List<string>(endColorsAllNumber);


        _levelHasBeenCompleted = false;
        gameIsPaused = false;

        _sytemInRewind = false;
        _rewindTimeScale = 5f;

        // Second camera and rewind greyscale effect for camera 
        _secondaryCamera = GameObject.Find("/Main Camera/Main Camera GreyScale");
        _secondaryCamera.SetActive(false);
        VolumeProfile _globalVolumeProfile;
        _globalVolumeProfile = FindObjectOfType<Volume>().GetComponent<Volume>().profile;
        if (!_globalVolumeProfile.TryGet<ColorCurves>(out var ColorCurves))
        {
            _globalVolumeProfileColorCurves = ColorCurves;
            _globalVolumeProfileColorCurves = _globalVolumeProfile.Add<ColorCurves>(false);
        }
        _globalVolumeProfileColorCurves.hueVsSat.overrideState = true;
        _globalVolumeProfileColorCurves.hueVsSat.value = new TextureCurve(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0)), 0f, false, new Vector2(0f, 1f));
        _globalVolumeProfileColorCurves.active = false;

        /*
        if (!_globalVolumeProfile.TryGet<ChromaticAberration>(out var ChromaticAbberation))
        {
            _globalVolumeProfileChromaticAberration = ChromaticAbberation;
            _globalVolumeProfileChromaticAberration = _globalVolumeProfile.Add<ChromaticAberration>(false);
        }
        _globalVolumeProfileChromaticAberration.intensity.overrideState = true;
        _globalVolumeProfileChromaticAberration.intensity.value = 0.7f;
        _globalVolumeProfileChromaticAberration.active = false;
        */
    }
    private void Start()
    {
        _gameStateSaveSystem = GameStateSaveSystem.instance;
    }

    void FixedUpdate()
    {
        if (sceneState == sceneState.playerRed || sceneState == sceneState.playerBlue || sceneState == sceneState.playerGreen || sceneState == sceneState.playerYellow)
        {
            frame = frame + 1;
        }
        else if (sceneState == sceneState.rewind && frame >= 1)
        {
            frame = frame - 1;
        }
    }

    // START AND STOP REWIND-TIMESCALE CHANGE
    private void startRewindMode()
    {
        _sytemInRewind = true;
        if (!_secondaryCamera.activeSelf)
        {
            _secondaryCamera.SetActive(true);
        }
        if (_globalVolumeProfileColorCurves.active == false)
        {
            _globalVolumeProfileColorCurves.active = true;
        }
        /*
        if (_globalVolumeProfileChromaticAberration.active == false)
        {
            _globalVolumeProfileChromaticAberration.active = true;
        }*/
        if (Time.timeScale == 1)
        { 
            Time.timeScale = _rewindTimeScale;
        } 
    }
    private void stopRewindMode()
    {
        _sytemInRewind = false;
        if (_secondaryCamera.activeSelf)
        {
            _secondaryCamera.SetActive(false);
        }
        if (_globalVolumeProfileColorCurves.active == true)
        {
            _globalVolumeProfileColorCurves.active = false;
        }
        /*
        if (_globalVolumeProfileChromaticAberration.active == true)
        {
            _globalVolumeProfileChromaticAberration.active = false;
        }*/
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }
    }

    // PAUSE AND RESUME GAME
    public void PauseGame()
    {
        if (!gameIsPaused)
        {
            Time.timeScale = 0f;
            gameIsPaused = true;
        } 
    }
    public void ResumeGame()
    {
        if (gameIsPaused)
        {
            Time.timeScale = 1f;
            gameIsPaused = false;
        } 
    }

    // PLAYER TURN IS OVER METHODS
    private void turnOverHandler()
    {
        StartCoroutine(turnOver());
    }

    private IEnumerator turnOver()
    {
        yield return new WaitForSeconds(1.25f);
        updateSceneState(sceneState.rewind);
    }

    // "SETUP" STATUS METHODS
    private void setupHandler()
    {
        frame = 0;
        _endColorsAllActivated = new List<string>(endColorsAllNumber);
        StartCoroutine(setupToPlayerSelect());
    }
    private IEnumerator setupToPlayerSelect()
    {
        yield return new WaitForSeconds(1f);
        updateSceneState(sceneState.selectPlayer);
    }

    // "FAIL" STATUS METHODS
    private void failHandler()
    {
        if (!_levelHasBeenCompleted) // is this needed?
        {
            Debug.Log("LEVEL FAILED COROUTINE STARTED");
            countTimerStop();
            StartCoroutine(failToSetup());
        }  
    }
    private IEnumerator failToSetup()
    {
        yield return new WaitForSeconds(2.5f);

        // We need to get rid of all exploding cubes
        GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag("toDestroy");
        foreach (GameObject objectToDestroy in objectsToDestroy)
        {
            if (objectsToDestroy != null)
            {
                GameObject.Destroy(objectToDestroy);
            }
        }
            
        // Set all exploded players to active
        foreach (GameObject playerColor in playersToRespawn)
        {
            if (!playerColor.activeSelf)
            {
                playerColor.SetActive(true);
            }
        }

        //updateSceneState(sceneState.setup);
        RetrytScene();
    }

    // "LEVEL-COMPLETE" STATUS METHODS
    private void levelCompleteHandler()
    {
        _levelHasBeenCompleted = true;
        _gameStateSaveSystem.saveLevelState(levelValue, starsEarnedValue);
        StartCoroutine(levelCompleteGoNext());
    }

    private IEnumerator levelCompleteGoNext()
    {
        yield return new WaitForSeconds(2.5f);

        NextLevel();
    }

    public void endColorHasBeenSteppedOn(string endColorTag)
    {
        addEndColor(endColorTag);
        if (_endColorsAllActivated.Count < endColorsAllNumber)
        {
            _audioManager.playSound("endTurn");
            if ((endColorTag.Replace("end", "player") == playerColorCurrentTag) && (playersAlreadyPlayed.Count < endColorsAllNumber))
            {
                updateSceneState(sceneState.turnOver);
            }
        }
        else if (_endColorsAllActivated.Count == endColorsAllNumber)
        {
            _audioManager.playSound("levelComplete");
            updateSceneState(sceneState.levelComplete);
        }
        checkIfAllPlayedAndAllReplayedAndAllNowIdleButLevelIsNotCompleted();
    }
    public void endColorHasBeenSteppedOff(string endColorTag)
    {
        removeEndColor(endColorTag);
    }
    private void addEndColor(string endColorTag)
    {
        if (!_endColorsAllActivated.Contains(endColorTag))
        {
            _endColorsAllActivated.Add(endColorTag);
        }
    }
    private void removeEndColor(string endColorTag)
    {
        if (_endColorsAllActivated.Contains(endColorTag))
        {
            _endColorsAllActivated.Remove(endColorTag);
        }
    }

    private void addPlayerColor(string playerColorTag)
    {
        if (!playersAlreadyPlayed.Contains(playerColorTag))
        {
            playersAlreadyPlayed.Add(playerColorTag);
        }
    }

    public void checkIfAllPlayedAndAllReplayedAndAllNowIdleButLevelIsNotCompleted()
    {
        if (playersAlreadyPlayed.Count == endColorsAllNumber && _endColorsAllActivated.Count != endColorsAllNumber)
        {
            int countIdlePlayers = 0;
            foreach (PlayerColor playerColor in _playerColors)
            {
                if (playerColor._currentStatus == PlayerColor.status.idle)
                {
                    countIdlePlayers = countIdlePlayers + 1;
                }
            }
            if (countIdlePlayers == endColorsAllNumber)
            {
                Debug.Log("FAIL: All played and replayed but did not complete the level");
                failHandler();
            }
        }
    }


    // GENERAL SCENE MANAGING METHODS
    public void NextLevel()
    {
        if (SceneUtility.GetBuildIndexByScenePath($"Scenes/Lvl{levelValue + 1}") >= 0)
        {
            SceneManager.LoadScene(levelValue+2);
        }
        else
        {
            SceneManager.LoadScene("LvlSelect");
        }
    }
    public void ReloadScene() // make sure that this is not needed, since it will run the PreGame terrain builder again
    {
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }
    public void RetrytScene()
    {
        // Reset the all players' turns
        playersAlreadyPlayed = new List<string>(endColorsAllNumber);
        Debug.Log($"#players already played: {playersAlreadyPlayed.Count}");

        // Kill all the players, so they will all respawn on setup
        foreach (PlayerColor playerColor in _playerColors)//FindObjectsOfType<PlayerColor>())
        {
            if (!playerColor.isDead) 
            {
                playerColor.isDead = true;
            }
        }

        updateSceneState(sceneState.setup);
    }

    public void LevelSelectScreen()
    {
        SceneManager.LoadScene("LvlSelect");
        ResumeGame();
    }

    public void StartMenuScreen()
    {
        SceneManager.LoadScene("StartMenuTemp");
        ResumeGame();
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("application quit was ran");
    }

    // TIMER RELATED METHODS
    private void resetTimer()
    {
        if (!_timerText.IsActive())
        {
            _timerText.gameObject.SetActive(true);
        }

        int decisec = 0;
        int seconds = 0;
        _time = 0;
        _timerText.text = string.Format("{0:00}:{1:0}", seconds, decisec);
    }

    private void countTimerStart()
    {
        StartCoroutine("countTimer");
    }

    private void countTimerStop()
    {
        StopCoroutine("countTimer");
    }

    private IEnumerator countTimer()
    {
        while (sceneState == sceneState.playerRed || sceneState == sceneState.playerBlue || sceneState == sceneState.playerGreen || sceneState == sceneState.playerYellow)
        {
            _time += Time.deltaTime;
            int decisec = (int)((_time - (int)_time) * 10);
            int seconds = (int)_time;
            _timerText.text = string.Format("{0:00}:{1:0}", seconds, decisec);
            yield return null;
        }
    }

    private void rewindTimerStart()
    {
        StartCoroutine("rewindTimer");
    }

    private void rewindTimerStop()
    {
        StopCoroutine("rewindTimer");
    }

    private IEnumerator rewindTimer()
    {
        while (sceneState == sceneState.rewind && _time >= 0f)
        {
            _time -= Time.deltaTime;
            int decisec = (int)((_time - (int)_time) * 10);
            int seconds = (int)_time;
            _timerText.text = string.Format("{0:00}:{1:0}", seconds, decisec);
            yield return null;
        }
    }

}

public enum sceneState
{
    setup,
    selectPlayer,
    playerRed,
    playerBlue,
    playerGreen,
    playerYellow,
    turnOver,
    rewind,
    fail,
    levelComplete
}
