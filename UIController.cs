using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private Controller _controller;
    public static UIController instance;
    private AudioManager _audioManager;

    // Arrow related things
    private bool isCurrentlySelectPlayerPhase;
    private GameObject selectPlayerArrow;
    private GameObject selectPlayerOutline;
    private Quaternion selectPlayerArrowInitialRotation;
    private List<string> playerColorAll;
    private List<Transform> playerColorAllTransforms;
    private int currentSelectPlayerIndex;
    private float extraPositionY;

    // Game screen
    private GameObject _spaceStart;
    private GameObject _spaceEnd;
    private GameObject _keyRetry;
    private GameObject _keyArrows;
    private GameObject _keyRetry_lower;

    // Escape screen
    private GameObject _escOptions;
    private Transform _hline;
    private Transform _box;
    private List<Transform> _pauseOptions;
    private int _currentIndexEsc;
    private int _previousIndexEsc;
    private int _defaultFontSize;
    private int _highlightFontSize;
    // Helper text related things
    //TextFlashingEffect onScreenCanvasScript;


    // Rewind related
    private int _frame = 0;
    private int _frameDelay = 20; // This is used as a gap between rewind and playerSelect phases. It allows playerColor trails to die out and maybe some additional things
    private enum UIstatus
    {
        selectPlayer,
        playing,
        turnOver,
        rewinding,
        fail,
        other
    }
    private UIstatus currentUIStatus;
    private sceneState previousState;

    private void onSceneStateChangeHandler(sceneState sceneState)
    {
 
        if (sceneState == sceneState.selectPlayer)// || sceneState == sceneState.fail) // why is sceneState.fail here?
        {
            /*
            if (previousState == sceneState.setup)
            {
                createBothListsOfplayerColorAll();
            }*/
            currentUIStatus = UIstatus.selectPlayer;
            activateSelectPlayerPhase();
            if (_controller.playersAlreadyPlayed.Count == 0)
            {
                gameScreenControls(new List<bool> {true, false, false, true, false});
            }
            else
            {
                gameScreenControls(new List<bool> { true, false, true, true, false });
            }
        }
        else if (sceneState == sceneState.playerRed || sceneState == sceneState.playerBlue || sceneState == sceneState.playerGreen || sceneState == sceneState.playerYellow)
        {
            currentUIStatus = UIstatus.playing;
            _frame = 0;
            deactivateSelectPlayerPhase();
            gameScreenControls(new List<bool> { false, true, false, false, true });
        }
        else if (sceneState == sceneState.turnOver)
        {
            currentUIStatus = UIstatus.turnOver;
            gameScreenControls(new List<bool> { false, false, false, false, false });
        }
        else if (sceneState == sceneState.rewind)
        {
            currentUIStatus = UIstatus.rewinding;
        }
        else if (sceneState == sceneState.levelComplete)
        {
            gameScreenControls(new List<bool> { false, false, false, false, false });
        }
        else if (sceneState == sceneState.fail)
        {
            currentUIStatus = UIstatus.fail;
            levelFailedUI();
        }
        else if (sceneState == sceneState.setup)
        {
            //createBothListsOfplayerColorAll(); //we will move this into the .selectPlayerPhase
            setupHandler(); // dirty bugfix
        }
        previousState = sceneState;
    }

    // THIS IS A VERY DIRTY BUGFIX ==================================== (search for "dirty bugfix")
    private void setupHandler()
    {
        StartCoroutine(setupToPlayerSelect());
    }
    private IEnumerator setupToPlayerSelect()
    {
        yield return new WaitForSeconds(0.025f);
        createBothListsOfplayerColorAll();
    }
    //=================================================================

    private void OnDestroy()
    {
        Controller.onSceneStateChange -= onSceneStateChangeHandler;
    }

    private void Awake()
    {
        instance = this;

        Controller.onSceneStateChange += onSceneStateChangeHandler;
        _audioManager = GameObject.FindObjectOfType<AudioManager>();

        // Select-player related things
        if (GameObject.Find("selectPlayerArrow") != null)
        {
            selectPlayerArrow = GameObject.Find("selectPlayerArrow");
            selectPlayerArrowInitialRotation = selectPlayerArrow.transform.rotation;
        }
        if (GameObject.Find("selectPlayerOutline") != null)
        {
            selectPlayerOutline = GameObject.Find("selectPlayerOutline");
        }
        if (GameObject.Find("keySpaceStart") != null)
        {
            _spaceStart = GameObject.Find("keySpaceStart");
        }
        if (GameObject.Find("keySpaceEnd") != null)
        {
            _spaceEnd = GameObject.Find("keySpaceEnd");
        }
        if (GameObject.Find("keyR") != null)
        {
            _keyRetry = GameObject.Find("keyR");
        }
        if (GameObject.Find("keyArrows") != null)
        {
            _keyArrows = GameObject.Find("keyArrows");
        }
        if (GameObject.Find("keyR_lower") != null)
        {
            _keyRetry_lower = GameObject.Find("keyR_lower");
        }

        // Creates lists for playerColorAll and playerColorAllTransforms
        createBothListsOfplayerColorAll();
        currentSelectPlayerIndex = 0;
        extraPositionY = 1.25f;
        moveSelectPlayerArrow();

    }

    void Start()
    {
        _controller = Controller.instance;

        //onScreenCanvasScript = GameObject.Find("onScreenCanvas").GetComponent<TextFlashingEffect>();

        // UI text related things
        //onScreenCanvasScript.activateText();


        if (GameObject.Find("escOptions") != null)
        {
            _escOptions = GameObject.Find("escOptions");
            _hline = GameObject.Find("hline").transform;
            _box = GameObject.Find("box").transform;
            _pauseOptions = new List<Transform>(2);
            _pauseOptions.Add(GameObject.Find("resumeText").transform);
            _pauseOptions.Add(GameObject.Find("exitText").transform);
            _currentIndexEsc = 0;
            _previousIndexEsc = 0;
            _defaultFontSize = 130;
            _highlightFontSize = 150;
            _hline.localPosition = new Vector3(_hline.localPosition.x, _pauseOptions[_currentIndexEsc].localPosition.y, _hline.localPosition.z);
            _box.localPosition = _pauseOptions[_currentIndexEsc].localPosition;
            _pauseOptions[_currentIndexEsc].GetComponent<Text>().fontSize = _highlightFontSize;
            _escOptions.SetActive(false);
        }

        deactivateSelectPlayerPhase();
        gameScreenControls(new List<bool> {false, false, false, false, false });
        levelFailedUI();
        currentUIStatus = UIstatus.other;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_escOptions.activeSelf == false)
            {
                _escOptions.SetActive(true);
                _controller.PauseGame();
                _audioManager.playSound("moveSelection");
            }
            else
            {
                _hline.localPosition = new Vector3(_hline.localPosition.x, _pauseOptions[0].localPosition.y, _hline.localPosition.z);
                _box.localPosition = new Vector3(_box.localPosition.x, _pauseOptions[0].localPosition.y, _box.localPosition.z);
                _pauseOptions[_currentIndexEsc].GetComponent<Text>().fontSize = _defaultFontSize;
                _pauseOptions[0].GetComponent<Text>().fontSize = _highlightFontSize;
                _currentIndexEsc = 0;
                _previousIndexEsc = 0;
                _escOptions.SetActive(false);
                _controller.ResumeGame();
            }
        }
        // Pause menu
        if (_controller.gameIsPaused)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _previousIndexEsc = _currentIndexEsc;
                _currentIndexEsc = Mathf.Min(_currentIndexEsc + 1, 1);
                if (_currentIndexEsc != _previousIndexEsc)
                {
                    _hline.localPosition = new Vector3(_hline.localPosition.x, _pauseOptions[_currentIndexEsc].localPosition.y, _hline.localPosition.z);
                    _box.localPosition = new Vector3(_box.localPosition.x, _pauseOptions[_currentIndexEsc].localPosition.y, _box.localPosition.z);
                    _pauseOptions[_currentIndexEsc].GetComponent<Text>().fontSize = _highlightFontSize;
                    _pauseOptions[_previousIndexEsc].GetComponent<Text>().fontSize = _defaultFontSize;
                    _audioManager.playSound("moveSelection");
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _previousIndexEsc = _currentIndexEsc;
                _currentIndexEsc = Mathf.Max(_currentIndexEsc - 1, 0);
                if (_currentIndexEsc != _previousIndexEsc)
                {
                    _hline.localPosition = new Vector3(_hline.localPosition.x, _pauseOptions[_currentIndexEsc].localPosition.y, _hline.localPosition.z);
                    _box.localPosition = new Vector3(_box.localPosition.x, _pauseOptions[_currentIndexEsc].localPosition.y, _box.localPosition.z);
                    _pauseOptions[_currentIndexEsc].GetComponent<Text>().fontSize = _highlightFontSize;
                    _pauseOptions[_previousIndexEsc].GetComponent<Text>().fontSize = _defaultFontSize;
                    _audioManager.playSound("moveSelection");
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_currentIndexEsc == 0)
                {
                    _hline.localPosition = new Vector3(_hline.localPosition.x, _pauseOptions[0].localPosition.y, _hline.localPosition.z);
                    _box.localPosition = new Vector3(_box.localPosition.x, _pauseOptions[0].localPosition.y, _box.localPosition.z);
                    _pauseOptions[_currentIndexEsc].GetComponent<Text>().fontSize = _defaultFontSize;
                    _pauseOptions[0].GetComponent<Text>().fontSize = _highlightFontSize;
                    _currentIndexEsc = 0;
                    _previousIndexEsc = 0;
                    _escOptions.SetActive(false);
                    _controller.ResumeGame();
                }
                else if (_currentIndexEsc == 1)
                {
                    _controller.ResumeGame();
                    SceneManager.LoadScene("LvlSelect");
                }
            }
        }
        // Everything but the pause menu
        else
        {
            // you cannot retry if level is already finished or if no players have played yet
            if (Input.GetKeyDown(KeyCode.R) && _controller.sceneState != sceneState.levelComplete && (_controller.playersAlreadyPlayed.Count != 0)) 
            {
                if (currentUIStatus == UIstatus.selectPlayer || currentUIStatus == UIstatus.playing)
                {
                    // Reset the players' turns
                    //createBothListsOfplayerColorAll();
                    currentSelectPlayerIndex = 0;
                    if (selectPlayerOutline.activeSelf)
                    {
                        selectPlayerOutline.SetActive(false);
                    }
                    // Retry the scene
                    _controller.RetrytScene();
                    // Reset arrow rotation 
                    selectPlayerArrow.transform.rotation = selectPlayerArrowInitialRotation;
                    if (selectPlayerArrow.activeSelf)
                    {
                        selectPlayerArrow.SetActive(false);
                    }
                    gameScreenControls(new List<bool> { false, false, false, false, false });
                }
            }
            if (currentUIStatus == UIstatus.selectPlayer)
            {
                //moveSelectPlayerArrow();
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    currentSelectPlayerIndexPlusOne();
                    _audioManager.playSound("moveSelection");
                    moveSelectPlayerArrow(); // first move arrow
                                             //selectPlayerArrow.transform.rotation = selectPlayerArrowInitialRotation; // then reset rotation
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    currentSelectPlayerIndexMinusOne();
                    _audioManager.playSound("moveSelection");
                    moveSelectPlayerArrow(); // first move arrow
                                             //selectPlayerArrow.transform.rotation = selectPlayerArrowInitialRotation; // then reset rotation
                }
                if (Input.GetKeyDown("space"))
                {
                    selectPlayer();
                    _audioManager.playSound("timer");
                }

                selectPlayerArrow.transform.Rotate(new Vector3(0f, 100f, 0f) * Time.deltaTime);
            }
            else if (currentUIStatus == UIstatus.playing) // && a certain level is being played... (only allow to manually end the turn in the levels where you need it to solve it)
            {
                if (Input.GetKeyDown("space") && playerColorAll.Count > 0) // Only allow to manually end turn when it is not the last player's turn
                {
                    _controller.updateSceneState(sceneState.turnOver);
                    _audioManager.playSound("endTurn");
                }
            }
        }

    }
    private void FixedUpdate()
    {
        if (currentUIStatus == UIstatus.playing)
        {
            _frame = _frame + 1;
        }
        else if (currentUIStatus == UIstatus.rewinding)
        {
            _frame = _frame - 1;
            if (_frame == -_frameDelay)
            {
                _controller.updateSceneState(sceneState.selectPlayer);
            }
        }
    }


    // SELECT-PLAYER RELATED METHODS
    private void selectPlayer()
    {
        if (playerColorAll[currentSelectPlayerIndex] == "playerRed")
        {
            _controller.updateSceneState(sceneState.playerRed);
        }
        else if (playerColorAll[currentSelectPlayerIndex] == "playerBlue")
        {
            _controller.updateSceneState(sceneState.playerBlue);
        }
        else if (playerColorAll[currentSelectPlayerIndex] == "playerGreen")
        {
            _controller.updateSceneState(sceneState.playerGreen);
        }
        else if (playerColorAll[currentSelectPlayerIndex] == "playerYellow")
        {
            _controller.updateSceneState(sceneState.playerYellow);
        }
        // Remove the selected player from the lists
        playerColorAll.RemoveAt(currentSelectPlayerIndex);
        playerColorAllTransforms.RemoveAt(currentSelectPlayerIndex);
        // Decrease the index by one
        currentSelectPlayerIndexMinusOne();
    }
    private void currentSelectPlayerIndexPlusOne()
    {
        currentSelectPlayerIndex = (currentSelectPlayerIndex + 1) % playerColorAll.Count;
    }
    private void currentSelectPlayerIndexMinusOne()
    {
        currentSelectPlayerIndex = currentSelectPlayerIndex - 1;
        if (currentSelectPlayerIndex < 0)
        {
            currentSelectPlayerIndex = currentSelectPlayerIndex + playerColorAll.Count;
        }
    }
    private void moveSelectPlayerArrow()
    {
        if (selectPlayerArrow.transform.position != playerColorAllTransforms[currentSelectPlayerIndex].position + new Vector3(0, extraPositionY, 0))
        {
            selectPlayerArrow.transform.position = playerColorAllTransforms[currentSelectPlayerIndex].position + new Vector3(0, extraPositionY, 0);
        }
        if (selectPlayerOutline.transform.position != playerColorAllTransforms[currentSelectPlayerIndex].position)
        {
            selectPlayerOutline.transform.position = playerColorAllTransforms[currentSelectPlayerIndex].position;
        }
    }

    // SELECT-PLAYER UI (DE)ACTIVATION METHODS
    private void activateSelectPlayerPhase()
    {
        //onScreenCanvasScript.activateText();

        if (!selectPlayerArrow.activeSelf)
        {
            selectPlayerArrow.SetActive(true);
        }
        if (!selectPlayerOutline.activeSelf)
        {
            selectPlayerOutline.SetActive(true);
        }
        moveSelectPlayerArrow();
        _audioManager.playSound("moveSelection");
    }
    public void deactivateSelectPlayerPhase()
    {
        //onScreenCanvasScript.deactivateText();
        
        if (selectPlayerArrow.activeSelf)
        {
            selectPlayerArrow.SetActive(false);
        }
        if (selectPlayerOutline.activeSelf)
        {
            selectPlayerOutline.SetActive(false);
        }
    }
    public void gameScreenControls(List<bool> boolList)
    {
        if (boolList[0] && !_spaceStart.activeSelf)
        {
            _spaceStart.SetActive(true);
        }
        else if (!boolList[0] && _spaceStart.activeSelf)
        {
            _spaceStart.SetActive(false);
        }
        if (boolList[1] && !_spaceEnd.activeSelf)
        {
            _spaceEnd.SetActive(true);
        }
        else if (!boolList[1] && _spaceEnd.activeSelf)
        {
            _spaceEnd.SetActive(false);
        }
        if (boolList[2] && !_keyRetry.activeSelf)
        {
            _keyRetry.SetActive(true);
        }
        else if (!boolList[2] && _keyRetry.activeSelf)
        {
            _keyRetry.SetActive(false);
        }
        if (boolList[3] && !_keyArrows.activeSelf)
        {
            _keyArrows.SetActive(true);
        }
        else if (!boolList[3] && _keyArrows.activeSelf)
        {
            _keyArrows.SetActive(false);
        }
        if (boolList[4] && !_keyRetry_lower.activeSelf)
        {
            _keyRetry_lower.SetActive(true);
        }
        else if (!boolList[4] && _keyRetry_lower.activeSelf)
        {
            _keyRetry_lower.SetActive(false);
        }
    }

    private void createBothListsOfplayerColorAll()
    {
        // First empty the lists in case there are still some players left to play from the previous try
        playerColorAll = new List<string>();
        playerColorAllTransforms = new List<Transform>();
        currentSelectPlayerIndex = 0;

        // Then fill the lists
        if (GameObject.Find("playerRed") != null)
        {
            playerColorAll.Add("playerRed");
            playerColorAllTransforms.Add(GameObject.Find("playerRed").transform);
        }
        if (GameObject.Find("playerBlue") != null)
        {
            playerColorAll.Add("playerBlue");
            playerColorAllTransforms.Add(GameObject.Find("playerBlue").transform);
        }
        if (GameObject.Find("playerGreen") != null)
        {
            playerColorAll.Add("playerGreen");
            playerColorAllTransforms.Add(GameObject.Find("playerGreen").transform);
        }
        if (GameObject.Find("playerYellow") != null)
        {
            playerColorAll.Add("playerYellow");
            playerColorAllTransforms.Add(GameObject.Find("playerYellow").transform);
        }

    }

    // FAIL AND LEVEL COMPLETE UI METHODS
    private void levelFailedUI()
    {
        gameScreenControls(new List<bool> { false, false, false, false, false });
        // Turn off everything else
        if (selectPlayerArrow.activeSelf)
        {
            selectPlayerArrow.SetActive(false);
        }
        if (selectPlayerOutline.activeSelf)
        {
            selectPlayerOutline.SetActive(false);
        }
        if (isCurrentlySelectPlayerPhase)
        {
            isCurrentlySelectPlayerPhase = false;
            //onScreenCanvasScript.deactivateText();
        }
    }

}
