using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class PlayerColor : MonoBehaviour
{
    private Controller _controller;

    private Rigidbody _rigidbodyComponent;
    private Renderer _rendererComponent;

    // Player movement input variables
    private float _horizontalInput;
    private float _verticalInput;

    // Current player variables
    private float _playerDimension;
    
    // User input, uses Recorder class
    private Recorder _userInput;
    private Saver _userInputSaver;
    private SaverRewind _objectRewindSaver;

    // Status controling related variables;
    private Vector3 _initialPosition;
    private Vector3 _initialRotation;
    public bool isDead; // this is called by DeadZone.cs

    // Variables for createExplodingPiece() method
    private float _explodingPieceCubeSize;
    private int _explodingPieceCubesInRow;
    private float _explodingPieceCubesPivotDistance;
    private float _explosionRadius;
    private float _explosionForce;
    private float _explosionUpward;

    // Camera shake
    private StressReceiver _cameraShake;

    // For snap-to-grid movement
    private Vector3 _gridTargetPosition;
    private bool _gridIsMoving;
    private float _moveSpeed;
    private float _gridCollisionRayLenght;
    private Coroutine _gridMovementCoroutine; // Tfis must be defined so we can stop it instantly: https://answers.unity.com/questions/300864/how-to-stop-a-co-routine-in-c-instantly.html
    private Vector3 _currentPosition;
    private Vector3 _previousPosition;
    private MeshRenderer _blockMoveLeft;
    private MeshRenderer _blockMoveRight;
    private MeshRenderer _blockMoveForward;
    private MeshRenderer _blockMoveBack;

    // For emission
    private Material _playerColorMaterial;

    // Rewind
    private GameObject _trailColor;
    private int _residueRewind;
    private int _trailFramesN;

    // Path show
    private LineRenderer _line;
    private float _verticalGap;
    private PathColor _linePointSaver;
    private GameObject _crosshairColor;
    private MeshRenderer _crosshairColorRenderer;
    private float _crosshairColorRotationSpeed;

    // Audio maganer
    private AudioManager _audioManager;

    // Keep track of player status
    public enum status
    {
        recording,
        rewinding,
        replaying,
        idle
    }
    public status _currentStatus;

    private void onSceneStateChangeHandler(sceneState sceneState)
    {

        if (sceneState == sceneState.selectPlayer)
        {
            playerStatusIsIdle();
            _line = _linePointSaver.drawLinePath(_line);
        }
        else if (sceneState == sceneState.playerRed || sceneState == sceneState.playerBlue || sceneState == sceneState.playerGreen || sceneState == sceneState.playerYellow)
        {
            if (gameObject.tag == _controller.playerColorCurrentTag)
            {
                playerStatusIsRecording();
            }
            else if (_userInputSaver.playerHasAlreadyRecordedInput())
            {
                playerStatusIsReplaying();
            }
        }
        else if (sceneState == sceneState.turnOver)
        {
            if (_objectRewindSaver.objectHasAlreadyRecordedPos())
            {
                playerStatusIsIdle();
            }
        }
        else if (sceneState == sceneState.rewind)
        {
            if (_objectRewindSaver.objectHasAlreadyRecordedPos())
            {
                playerStatusIsRewinding();
            }
        }
        else if (sceneState == sceneState.levelComplete)
        {
            playerStatusIsIdle();

            //playerColorMaterialEmissionTurnOn("All6");
            // Flicker player emission lights...
            // do something (ascension/turn on the lights/...)
        }
        else if (sceneState == sceneState.setup)
        {
            if (isDead)
            {
                playerRespawn();
            }
            else
            {
                resetPlayerPosition();
            }
        }
        else if (sceneState == sceneState.fail)
        {
            playerStatusIsIdle();
        }
    }

    private void Awake()
    {
        Controller.onSceneStateChange += onSceneStateChangeHandler;
        _audioManager = GameObject.FindObjectOfType<AudioManager>();

        // Initialize the variables. GetComponent searches for the script on the same game object.
        _rigidbodyComponent = GetComponent<Rigidbody>();

        _userInput = GetComponent<Recorder>();
        _userInputSaver = GetComponent<Saver>();
        _objectRewindSaver = GetComponent<SaverRewind>(); // For rewinding

        _initialPosition = _rigidbodyComponent.position;
        _initialRotation = _rigidbodyComponent.rotation.eulerAngles;
        _currentPosition = _initialPosition;
        _previousPosition = _initialPosition;
        _currentStatus = status.idle;

        _playerDimension = gameObject.GetComponent<BoxCollider>().bounds.size.x / 2;

        // Allow fall down deahts
        gameObject.GetComponent<BoxCollider>().size = new Vector3(0.999f, 1, 0.999f);

        // Prevent bouncing
        _rigidbodyComponent.maxDepenetrationVelocity = 1f;

        // Exploding cubes parameters
        _explodingPieceCubeSize = 0.2f;
        _explodingPieceCubesInRow = 5;
        _explodingPieceCubesPivotDistance = _explodingPieceCubeSize * _explodingPieceCubesInRow / 2;
        _explosionRadius = 1.5f*5.0f;
        _explosionForce = 5f/1.5f;
        _explosionUpward = 0.3f;

        // Used for snap-to-grid movement
        _moveSpeed = 4f;
        _gridCollisionRayLenght = 1.495f; // 1.499f;
        GameObject _blockMoveLeftGameObject = Instantiate(Resources.Load<GameObject>($"BlockMoves/blockMoveLeft"));
        _blockMoveLeft = _blockMoveLeftGameObject.GetComponent<MeshRenderer>();
        _blockMoveLeft.enabled = false;
        GameObject _blockMoveRightGameObject = Instantiate(Resources.Load<GameObject>($"BlockMoves/blockMoveRight"));
        _blockMoveRight = _blockMoveRightGameObject.GetComponent<MeshRenderer>();
        _blockMoveRight.enabled = false;
        GameObject _blockMoveUpGameObject = Instantiate(Resources.Load<GameObject>($"BlockMoves/blockMoveForward"));
        _blockMoveForward = _blockMoveUpGameObject.GetComponent<MeshRenderer>();
        _blockMoveForward.enabled = false;
        GameObject _blockMoveDownGameObject = Instantiate(Resources.Load<GameObject>($"BlockMoves/blockMoveBack"));
        _blockMoveBack = _blockMoveDownGameObject.GetComponent<MeshRenderer>();
        _blockMoveBack.enabled = false;

        // Used for emission
        _rendererComponent = gameObject.GetComponent<Renderer>();
        _playerColorMaterial = _rendererComponent.material;

        // Used for rewind
        if (Resources.Load<GameObject>($"PlayerColorTrails/{gameObject.tag.Replace("player", "Hologram")}") != null)
        {
            _trailColor = Resources.Load<GameObject>($"PlayerColorTrails/{gameObject.tag.Replace("player", "Hologram")}");
        }
        _residueRewind = 0;
        _trailFramesN = 20;

        // Path show
        _line = gameObject.GetComponent<LineRenderer>();
        _linePointSaver = gameObject.GetComponent<PathColor>();
        
        if (transform.tag == "playerRed")
        {
            _verticalGap = 0.45f;
        }
        else if (transform.tag == "playerBlue")
        {
            _verticalGap = 0.35f;
        }
        else if (transform.tag == "playerYellow")
        {
            _verticalGap = 0.25f;
        }
        else if (transform.tag == "playerGreen")
        {
            _verticalGap = 0.15f;
        }
        _crosshairColor = Instantiate(Resources.Load<GameObject>($"{gameObject.tag.Replace("player", "crosshair")}"));
        Debug.Log(_crosshairColor);
        _crosshairColorRenderer = _crosshairColor.GetComponent<MeshRenderer>();
        if (_crosshairColorRenderer.enabled)
        {
            _crosshairColorRenderer.enabled = false;
        }
        _crosshairColorRotationSpeed = 90f;

        isDead = true;
    }
    // It is a good practice to always unsubscribe when this class gets destroyed. OnDestroy() is called when the Scene is closed and a new Scene is loaded.
    private void OnDestroy()
    {
        Controller.onSceneStateChange -= onSceneStateChangeHandler;
    }
    // Start is called before the first frame update
    void Start()
    {
        _controller = Controller.instance;
        _cameraShake = CameraController.instance.gameObject.GetComponent<StressReceiver>();
        gameObject.SetActive(false);
    }

    // FixedUpdate is called once every physics update, which is 50 per second. CAN BE CHANGED as Time.fixedDeltaTime = 1f / 60f;
    private void FixedUpdate()
    {
        // Only record and replay if the gameObject has not yet been destroyed or disabled. This part can be now moved here from Update(), because we no longer use jumps.
        if (gameObject.activeSelf && !isDead)
        {
            if (_currentStatus == status.recording)
            {
                _userInput.getInputs();
                userInputStruct userInputStruct = _userInput.getInputStruct();

                _horizontalInput = userInputStruct.horizontalInput;
                _verticalInput = userInputStruct.verticalInput;

                if (!_gridIsMoving && isGrounded() && ((_horizontalInput == 1f && gridCanMove(Vector3.right))
                                                    || (_horizontalInput == -1f && gridCanMove(Vector3.left))
                                                    || (_verticalInput == 1f && gridCanMove(Vector3.forward))
                                                    || (_verticalInput == -1f && gridCanMove(Vector3.back))))
                {
                    _userInputSaver.addToUserInputSaverDict(_controller.frame, userInputStruct); // save input
                }
                else
                {
                    _userInputSaver.addToUserInputSaverDict(_controller.frame, new userInputStruct(0.0f, 0.0f)); // else save empty input
                }
                
                _objectRewindSaver.addToObjectPosSaverDict(_controller.frame, transform.position); // save position

                _linePointSaver.addToLinePointSaverDict(_controller.frame, transform.position - new Vector3(0, _verticalGap, 0)); // save path line position

                _userInput.resetInput();
            }
            else if (_currentStatus == status.rewinding)
            {
                if (_objectRewindSaver.keyExistsPosSaverDict(_controller.frame))
                {
                    transform.position = _objectRewindSaver.getSavedPos(_controller.frame);
                    // instantiate trail object every N (=20) frames
                    if (_controller.frame % _trailFramesN == _residueRewind)
                    {
                        // This will compare positions for previous, current and next frame - if they all differ, a trail will be drawn
                        if (_objectRewindSaver.keyExistsPosSaverDict(_controller.frame - 1) && _objectRewindSaver.keyExistsPosSaverDict(_controller.frame + 1))
                        {
                            // checks if the object is in movement
                            if (_objectRewindSaver.getSavedPos(_controller.frame) != _objectRewindSaver.getSavedPos(_controller.frame - 1) && _objectRewindSaver.getSavedPos(_controller.frame) != _objectRewindSaver.getSavedPos(_controller.frame + 1))
                            {
                                instantiateTrail(_objectRewindSaver.getSavedPos(_controller.frame)); // instantiate a trail
                            }
                            // If not, check the first next frame. The idea is that we do not skip too many frames if we are too "unlucky"
                            else
                            {
                                _residueRewind = _residueRewind - 1;
                                if (_residueRewind < 0)
                                {
                                    _residueRewind = _trailFramesN - 1;
                                }
                            }
                        }
                        // If not, check the first next frame. The idea is that we do not skip too many frames if we are too "unlucky"
                        else
                        {
                            _residueRewind = _residueRewind - 1;
                            if (_residueRewind < 0)
                            {
                                _residueRewind = _trailFramesN - 1;
                            }
                        }
                    }
                }
            }
            else if (_currentStatus == status.replaying)
            {
                if (_userInputSaver.keyExists(_controller.frame))
                {
                    userInputStruct userInputStructSaved = _userInputSaver.getSavedInput(_controller.frame); // replay input

                    _horizontalInput = userInputStructSaved.horizontalInput;
                    _verticalInput = userInputStructSaved.verticalInput;

                    _objectRewindSaver.addToObjectPosSaverDict(_controller.frame, transform.position); // save position
                    // Delete path line point at frame
                    if (_line.positionCount > 0)
                    {
                        Vector3[] currentPositions = new Vector3[_line.positionCount];
                        _line.GetPositions(currentPositions);
                        Vector3 currentDisplacement = (transform.position - new Vector3(0, _verticalGap, 0)) - currentPositions[0];
                        if (currentDisplacement != Vector3.zero) // if displaced
                        {
                            currentPositions = currentPositions.Select(position => position + currentDisplacement).ToArray(); // add displacement to all elements
                        }
                        Vector3[] newPositions = new Vector3[currentPositions.Length - 1];
                        Array.Copy(currentPositions, 1, newPositions, 0, currentPositions.Length - 1);
                        _line.positionCount -= 1; // Reduce the position count by 1
                        _line.SetPositions(newPositions); // Set the new positions
                    }
                }
                else
                {
                    playerStatusIsIdle();
                    _controller.checkIfAllPlayedAndAllReplayedAndAllNowIdleButLevelIsNotCompleted(); // This checks if level has failed
                }
            }

        }

        snapToGridMovement(_horizontalInput, _verticalInput);

        // This is for fixing buggs where the playerColor is not centered to the grid and is slightly displaced mainly due to moving when getting carried
        _currentPosition = transform.position;
        if (!_gridIsMoving && isGrounded() && _currentStatus != status.idle
            && _horizontalInput==0f && _verticalInput==0f 
            && (Mathf.Abs(transform.position.x%1)!=0 || Mathf.Abs(transform.position.z % 1) != 0)
            && _previousPosition == _currentPosition)
        {
            if (Mathf.Abs(_initialPosition.x % 1) == 0)
            {
                transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, transform.position.z);
            }
            if (Mathf.Abs(_initialPosition.z % 1) == 0)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z));
            }
            Debug.Log(gameObject.tag + " snapped to grid");
        }
        _previousPosition = _currentPosition;

        // If player is falling down, increase gravity
        if (_rigidbodyComponent.velocity.y < 0)
        {
            // 2021-10-29: Physics.gravity.y from -20 to -10;
            // Instead of multiplying Physics.gravity.y by 2.25 just use the float -10 * 2.25. Multiplying is not good since this script is used by more than 1 player and thus the multiplication can happen more than once.
            _rigidbodyComponent.velocity += Vector3.up * (-10) * 2.25f * Time.fixedDeltaTime;
        }
    }

    void Update()
    {
        if (_line.positionCount > 0)
        {
            if (!_crosshairColorRenderer.enabled)
            {
                _crosshairColorRenderer.enabled = true;
            }
            Vector3 lastPoint = _line.GetPosition(_line.positionCount - 1);
            _crosshairColor.transform.position = lastPoint;
            // Rotate the GameObject around the y-axis
            _crosshairColor.transform.Rotate(Vector3.up, _crosshairColorRotationSpeed * Time.deltaTime);
        }
        else
        {
            if (_crosshairColorRenderer.enabled)
            {
                _crosshairColorRenderer.enabled = false;
            }
        }
    }

    // PLAYER STATUS METHODS
    private void playerStatusIsRewinding()
    {
        resetMovementInput();
        _residueRewind = _controller.frame % _trailFramesN;
        _currentStatus = status.rewinding;
        Debug.Log(gameObject.tag + ": my status is Rewinding");
    }
    public void playerStatusIsIdle()
    {
        resetMovementInput();
        if (_currentStatus != status.idle)
        {
            _currentStatus = status.idle;
        }
        Debug.Log(gameObject.tag + ": my status is Idle");
    }
    private void playerStatusIsReplaying()
    {
        resetMovementInput();
        _objectRewindSaver.clearObjectPosSaverDictHistory();
        _currentStatus = status.replaying;
        Debug.Log(gameObject.tag + ": my status is Replaying");
    }
    private void playerStatusIsRecording()
    {
        _userInputSaver.clearUserInputSaverDictHistory();
        _objectRewindSaver.clearObjectPosSaverDictHistory();
        _currentStatus = status.recording;
        Debug.Log(gameObject.tag + ": my status is Recording");
    }

    // PLAYER STATUS-RELATED ADDITIONAL METHODS
    public void resetPlayerPosition()
    {
        if (transform.parent != null)
        {
            transform.parent = null;
        }
        if (transform.position != _initialPosition)
        {
            transform.position = _initialPosition;
        }
        if (transform.eulerAngles != _initialRotation)
        {
            transform.eulerAngles = _initialRotation;
        }
    }
    private void resetMovementInput()
    {
        _horizontalInput = 0f;
        _verticalInput = 0f;
    }
    public void playerRespawn()
    {
        playerStatusIsIdle();
        if (transform.parent != null)
        {
            transform.parent = null;
        }
        playerColorMaterialEmissionTurnOff(); // is this needed???

        // If the player dies, it has to be played again
        _userInputSaver.clearUserInputSaverDictHistory();
        _objectRewindSaver.clearObjectPosSaverDictHistory();

        // If the player respawns, the path line has to be recorded again
        _linePointSaver.clearLinePointSaverDictHistory();
        _line.positionCount = 0;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        if (isDead)
        {
            isDead = false;
        }
        if (_gridMovementCoroutine != null)
        {
            StopCoroutine(_gridMovementCoroutine);
        }
        if (_gridIsMoving)
        {
            _gridIsMoving = false;
        }
        if (transform.eulerAngles != _initialRotation)
        {
            transform.eulerAngles = _initialRotation;
        }
        _audioManager.playSound("playerRespawn");
        _rigidbodyComponent.velocity = new Vector3(0, 0, 0); // Make sure this is not actually needed, although it could help avoiding some bugs
        _rigidbodyComponent.position = _initialPosition + new Vector3(0, 0.85f, 0);
        
    }

    // PLAYER EXPLOSION METHODS
    public void playerExplode(Vector3 explosionPosition)
    {
        Debug.Log($"{gameObject.tag}: I have been Destroyed!");
        Vector3 lastPlayerPosition = transform.position;
        transform.parent = null;
        isDead = true;
        if (_crosshairColorRenderer.enabled)
        {
            _crosshairColorRenderer.enabled = false;
        }
        gameObject.SetActive(false);
        
        Debug.Log("Explosion position = " + explosionPosition);

        // Fractured playerColor
        GameObject playerFracture;
        playerFracture = (GameObject)Instantiate(Resources.Load(gameObject.tag.Replace("player", "fracture")));//(GameObject)Instantiate(Resources.Load("player_fractured"));
        playerFracture.transform.position = transform.position + new Vector3(+12, 0.25f, 6.0578f);
        playerFracture.transform.tag = "toDestroy";
        // Explosion particles
        GameObject explosionParticles;
        explosionParticles = (GameObject)Instantiate(Resources.Load(gameObject.tag.Replace("player", "explosion")));
        explosionParticles.transform.position = transform.position;
        explosionParticles.transform.tag = "toDestroy";
        // Blow up the fractured playerColor
        Collider[] collidersInPlayerExplosionRadius = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider collider in collidersInPlayerExplosionRadius)
        {   
            Rigidbody colliderRigidbodyComponent = collider.GetComponent<Rigidbody>();
            if (colliderRigidbodyComponent != null)
            {
                colliderRigidbodyComponent.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, _explosionUpward);
            }
        }
        // Additional explosion effects
        _cameraShake.InduceStress(0.5f);
        _audioManager.playSound("playerExplosion");
        _controller.playersToRespawn.Add(this.gameObject);
        if (_controller.sceneState != sceneState.fail)
        {
            _controller.updateSceneState(sceneState.fail);
        }
    }

    // REWIND TRAIL DRAW METHOD
    private void instantiateTrail(Vector3 position)
    {
        GameObject trailColorInstance;
        trailColorInstance = Instantiate(_trailColor, position, Quaternion.identity);
        Destroy(trailColorInstance, 0.4f);
    }

    // PLAYER EMISSION-RELATED METHODS
    public void playerColorMaterialEmissionTurnOn()
    {
        if (!_playerColorMaterial.IsKeywordEnabled("_EMISSION"))
        {
            _playerColorMaterial.EnableKeyword("_EMISSION");
        }
    }
    public void playerColorMaterialEmissionTurnOff()
    {
        if (_playerColorMaterial.IsKeywordEnabled("_EMISSION"))
        {
            _playerColorMaterial.DisableKeyword("_EMISSION");
        }
    }

    // PLAYER MOVEMENT RELATED METHODS
    private void popUpBlockMove(MeshRenderer blockMove)
    {
        _audioManager.playSound("blockMove");
        blockMove.transform.position = transform.position + new Vector3(0, _playerDimension, 0);
        if (!blockMove.enabled)
        {
            blockMove.enabled = true;
        }
        popUpBlockMoveDisable(blockMove);
    }
    private void popUpBlockMoveDisable(MeshRenderer blockMove)
    {
        StartCoroutine(popUpBlockMoveDisableCouritine(blockMove));
    }
    private IEnumerator popUpBlockMoveDisableCouritine(MeshRenderer blockMove)
    {
        yield return new WaitForSeconds((1 / _moveSpeed - Time.fixedDeltaTime)-0.05f);
        if (blockMove.enabled)
        {
            blockMove.enabled = false;
        }
    }
    private void snapToGridMovement(float horizontalInput, float verticalInput)
    {
        if (!_gridIsMoving && isGrounded())
        {
            if (horizontalInput == 1f)
            {
                if (gridCanMove(Vector3.right))
                {
                    // Draw colored arrow
                    _gridMovementCoroutine = StartCoroutine(gridMovePlayer(Vector3.right));
                }  
                else if (_currentStatus == status.replaying)
                {
                    popUpBlockMove(_blockMoveRight);
                }
            }
            else if (horizontalInput == -1f)
            {
                if (gridCanMove(Vector3.left))
                {
                    _gridMovementCoroutine = StartCoroutine(gridMovePlayer(Vector3.left));
                }
                else if (_currentStatus == status.replaying)
                {
                    popUpBlockMove(_blockMoveLeft);
                }
            }
            else if (verticalInput == 1f)
            {
                if (gridCanMove(Vector3.forward))
                {
                    _gridMovementCoroutine = StartCoroutine(gridMovePlayer(Vector3.forward));
                }
                else if (_currentStatus == status.replaying)
                {
                    popUpBlockMove(_blockMoveForward);
                }
            }
            else if (verticalInput == -1f)
            {
                if (gridCanMove(Vector3.back))
                {
                    _gridMovementCoroutine = StartCoroutine(gridMovePlayer(Vector3.back));
                }
                else if (_currentStatus == status.replaying)
                {
                    popUpBlockMove(_blockMoveBack);
                }
            }
        }
    }
    private IEnumerator gridMovePlayer(Vector3 direction)
    {
        _gridIsMoving = true;

        float elapsedTime = 0;

        _gridTargetPosition = _rigidbodyComponent.position + direction;
        
        while (elapsedTime < 1 / _moveSpeed - Time.fixedDeltaTime)
        {
            _rigidbodyComponent.MovePosition(_rigidbodyComponent.position + direction * Time.fixedDeltaTime * _moveSpeed);
            elapsedTime = elapsedTime + Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
        _rigidbodyComponent.position = _gridTargetPosition;
        _linePointSaver.finalTilePosition(transform.position - new Vector3(0, _verticalGap, 0)); // so that the path line points to the last tile if turn was ended mid gridMovePlayer
        _gridIsMoving = false;
    }
    private bool gridCanMove(Vector3 direction)
    {
        bool frontCheck1 = !Physics.Raycast(transform.position + new Vector3(0, _playerDimension - 0.001f, 0) + Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), direction, _gridCollisionRayLenght);
        bool frontCheck2 = !Physics.Raycast(transform.position - new Vector3(0, _playerDimension - 0.001f, 0) - Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), direction, _gridCollisionRayLenght);
        bool frontCheck3 = !Physics.Raycast(transform.position + new Vector3(0, _playerDimension - 0.001f, 0) - Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), direction, _gridCollisionRayLenght);
        bool frontCheck4 = !Physics.Raycast(transform.position - new Vector3(0, _playerDimension - 0.001f, 0) + Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), direction, _gridCollisionRayLenght);

        bool groundCheck1 = Physics.Raycast(transform.position + direction * (_playerDimension + 0.001f) + Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), Vector3.down, 8);
        bool groundCheck2 = Physics.Raycast(transform.position + direction * (_playerDimension + 0.001f) - Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), Vector3.down, 8);
        bool groundCheck3 = Physics.Raycast(transform.position + direction * (_playerDimension * 3 - 0.001f) + Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), Vector3.down, 8);
        bool groundCheck4 = Physics.Raycast(transform.position + direction * (_playerDimension * 3 - 0.001f) - Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), Vector3.down, 8);

        isGroudedRaycast(frontCheck1, transform.position + new Vector3(0, _playerDimension - 0.001f, 0) + Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), direction * _gridCollisionRayLenght);
        isGroudedRaycast(frontCheck2, transform.position - new Vector3(0, _playerDimension - 0.001f, 0) - Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), direction * _gridCollisionRayLenght);
        isGroudedRaycast(frontCheck3, transform.position + new Vector3(0, _playerDimension - 0.001f, 0) - Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), direction * _gridCollisionRayLenght);
        isGroudedRaycast(frontCheck4, transform.position - new Vector3(0, _playerDimension - 0.001f, 0) + Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), direction * _gridCollisionRayLenght);

        isGroudedRaycast(groundCheck1, transform.position + direction * (_playerDimension + 0.001f) + Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), Vector3.down * 8);
        isGroudedRaycast(groundCheck2, transform.position + direction * (_playerDimension + 0.001f) - Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), Vector3.down * 8);
        isGroudedRaycast(groundCheck3, transform.position + direction * (_playerDimension * 3 - 0.001f) + Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), Vector3.down * 8);
        isGroudedRaycast(groundCheck4, transform.position + direction * (_playerDimension * 3 - 0.001f) - Vector3.Cross(direction, Vector3.up) * (_playerDimension - 0.001f), Vector3.down * 8);

        if (frontCheck1 && frontCheck2 && frontCheck3 && frontCheck4 && groundCheck1 && groundCheck2 && groundCheck3 && groundCheck4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void isGroudedRaycast(bool rayCast, Vector3 startPosition, Vector3 vector)
    {
        Color rayColor;
        if (rayCast)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(startPosition, vector, rayColor);
    }
    private bool isGrounded()
    {
        /*
        bool grounded1 = Physics.Raycast(transform.position + new Vector3(_playerDimension - 0.001f, 0, _playerDimension - 0.001f), Vector3.down, _playerDimension + 0.05f);
        bool grounded2 = Physics.Raycast(transform.position + new Vector3(-(_playerDimension - 0.001f), 0, _playerDimension - 0.001f), Vector3.down, _playerDimension + 0.05f);
        bool grounded3 = Physics.Raycast(transform.position + new Vector3(_playerDimension - 0.001f, 0, -(_playerDimension - 0.001f)), Vector3.down, _playerDimension + 0.05f);
        bool grounded4 = Physics.Raycast(transform.position + new Vector3(-(_playerDimension - 0.001f), 0, -(_playerDimension - 0.001f)), Vector3.down, _playerDimension + 0.05f);

        isGroudedRaycast(grounded1, transform.position + new Vector3(_playerDimension - 0.001f, 0, _playerDimension - 0.001f), Vector3.down * (_playerDimension + 0.05f));
        isGroudedRaycast(grounded2, transform.position + new Vector3(-(_playerDimension - 0.001f), 0, _playerDimension - 0.001f), Vector3.down * (_playerDimension + 0.05f));
        isGroudedRaycast(grounded3, transform.position + new Vector3(_playerDimension - 0.001f, 0, -(_playerDimension - 0.001f)), Vector3.down * (_playerDimension + 0.05f));
        isGroudedRaycast(grounded4, transform.position + new Vector3(-(_playerDimension - 0.001f), 0, -(_playerDimension - 0.001f)), Vector3.down * (_playerDimension + 0.05f));
        */

        bool grounded0 = Physics.Raycast(transform.position, Vector3.down, _playerDimension + 0.01f);
        isGroudedRaycast(grounded0, transform.position, Vector3.down * (_playerDimension + 0.01f));

        if (grounded0)// (grounded1 && grounded2 && grounded3 && grounded4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void erasePathLine()
    {
        _line.positionCount = 0;
    }
  
}