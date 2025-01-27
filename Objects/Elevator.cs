using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Controller _controller;

    private SaverRewind _objectRewindSaver;
    private List<string> _possibleTriggerObjects;
    public List<Rigidbody> _rigidbodiesList;
    Transform _transform;
    private Vector3 _lastPosition;

    public Vector3[] pointsToMoveBetween;
    public int pointNumber;
    private Vector3 _currentTarget;

    [SerializeField] private float _tolerance;
    [SerializeField] private float _speed;
    [SerializeField] private float _delayOnTrigger;

    private BoxCollider _boxColliderTrigger;
    private float _boxColliderTriggerWidth;
    private float _boxColliderTriggerLength;
    private IEnumerator onTriggerStartDelayIEnumerator;
    private IEnumerator onTriggerEndDelayIEnumerator;
    [SerializeField] private bool _triggered;
    [SerializeField] private int _boxCollidersInTrigger;

    //private AudioManager _audioManager;

    private enum status
    {
        playing,
        rewinding,
        idle
    }

    private status _currentStatus;

    private void onSceneStateChangeHandler(sceneState sceneState)
    {
        removeAll();

        // Reset the elevator to initial position
        if ((sceneState == sceneState.playerRed || sceneState == sceneState.playerBlue || sceneState == sceneState.playerGreen || sceneState == sceneState.playerYellow))
        {
            elevatorStatusIsPlaying();

            if (pointsToMoveBetween.Length > 0)
            {
                transform.position = pointsToMoveBetween[0];
            }
        }
        else if (sceneState == sceneState.rewind)
        {
            elevatorStatusIsRewinding();
        }
        else if (sceneState == sceneState.setup)
        {
            _currentStatus = status.idle;
            // Reset everything in case player died when on elevator
            if (_rigidbodiesList.Count != 0)
            {
                _rigidbodiesList = new List<Rigidbody>();
            }
            if (_boxCollidersInTrigger != 0)
            {
                _boxCollidersInTrigger = 0;
            }
            if (_triggered)
            {
                _triggered = false;
            }
            if (pointNumber != 0)
            {
                pointNumber = 0;
            }
            if (_transform.position != pointsToMoveBetween[0])
            {
                _transform.position = pointsToMoveBetween[0];
            }
            _objectRewindSaver.clearObjectPosSaverDictHistory();
        }

        else
        {
            _currentStatus = status.idle;
        }
    }

    private void Awake()
    {
        Controller.onSceneStateChange += onSceneStateChangeHandler;
        //_audioManager = GameObject.FindObjectOfType<AudioManager>();

        _objectRewindSaver = GetComponent<SaverRewind>();
        _possibleTriggerObjects = new List<string> { "playerBlue", "playerRed", "playerYellow", "playerGreen" };
        _rigidbodiesList = new List<Rigidbody>();
        _transform = transform;
        _lastPosition = _transform.position;
        _boxColliderTriggerWidth = 0.15f;
        _boxColliderTriggerLength = 1.08f;
        _boxColliderTrigger = gameObject.AddComponent<BoxCollider>();
        _boxColliderTrigger.size = new Vector3(_boxColliderTriggerWidth, _boxColliderTriggerLength, _boxColliderTriggerWidth);
        _boxColliderTrigger.isTrigger = true;


        if (pointsToMoveBetween.Length > 0)
        {
            _currentTarget = pointsToMoveBetween[0];
        }
        pointNumber = 0;
        _speed = 2.5f;
        _tolerance = _speed * Time.fixedDeltaTime;
        _delayOnTrigger = 0.125f;
        _boxCollidersInTrigger = 0;

        _currentStatus = status.idle;
    }

    void Start()
    {
        _controller = Controller.instance;
    }

    private void OnDestroy()
    {
        Controller.onSceneStateChange -= onSceneStateChangeHandler;
    }

    void FixedUpdate()
    {
        if (_currentStatus == status.playing)
        {
            if (_triggered && _transform.position != pointsToMoveBetween[pointsToMoveBetween.Length - 1])
            {
                moveForward();
            }
            else if (!_triggered && _transform.position != pointsToMoveBetween[0])
            {
                moveBackward();
            }

            if (_rigidbodiesList.Count > 0)
            {
                for (int i = 0; i < _rigidbodiesList.Count; i++)
                {
                    Rigidbody rb = _rigidbodiesList[i];
                    Vector3 carrierVelocity = (_transform.position - _lastPosition);
                    rb.transform.Translate(carrierVelocity, _transform);
                }
            }
            _lastPosition = _transform.position;

            _objectRewindSaver.addToObjectPosSaverDict(_controller.frame, _transform.position);
        }
        else if (_currentStatus == status.rewinding)
        {
            if (_objectRewindSaver.keyExistsPosSaverDict(_controller.frame))
            {
                transform.position = _objectRewindSaver.getSavedPos(_controller.frame);
            }
        }
    }

    // STATUS RELATED METHODS
    private void elevatorStatusIsRewinding()
    {
        _currentStatus = status.rewinding;
        Debug.Log(gameObject.tag + ": my status is Rewinding");
    }
    private void elevatorStatusIsPlaying()
    {
        _triggered = false;
        if (_boxCollidersInTrigger != 0)
        {
            _boxCollidersInTrigger = 0;
        }
        _objectRewindSaver.clearObjectPosSaverDictHistory();
        _currentStatus = status.playing;
        Debug.Log(gameObject.name + ": my status is Recording");
    }

    // PLATFORM/ELEVATOR MOVING RELATED METHODS
    private void moveForward()
    {
        if (_transform.position != _currentTarget)
        {
            movePlatform();
        }
        else
        {
            nextPlatform();
        }
    }
    private void moveBackward()
    {
        if (_transform.position != _currentTarget)
        {
            movePlatform();
        }
        else
        {
            previousPlatform();
        }
    }
    private void movePlatform()
    {
        Vector3 headingDirection = _currentTarget - _transform.position;
        _transform.position = _transform.position + headingDirection / headingDirection.magnitude * _speed * Time.fixedDeltaTime;
        if (headingDirection.magnitude < _tolerance)
        {
            _transform.position = _currentTarget;
        }
    }
    private void nextPlatform()
    {
        if (pointNumber < pointsToMoveBetween.Length)
        {
            pointNumber = pointNumber + 1;
        }
        _currentTarget = pointsToMoveBetween[pointNumber];
    }
    private void previousPlatform()
    {

        if (pointNumber > 0)
        {
            pointNumber = pointNumber - 1;
        }
        _currentTarget = pointsToMoveBetween[pointNumber];
    }

    // ON TRIGGER METHODS AND RELATED COROUTINES
    private void OnTriggerEnter(Collider other)
    {
        if (_possibleTriggerObjects.Contains(other.gameObject.tag))
        {
            _boxCollidersInTrigger = _boxCollidersInTrigger + 1;
            Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();

            if (otherRigidbody != null)
            {
                add(otherRigidbody);
                // Turn on emission on the player
                /*
                if (other.GetComponent<PlayerColor>() != null)
                {
                    other.GetComponent<PlayerColor>().playerColorMaterialEmissionTurnOn("side4");
                }*/
                if ((_currentStatus == status.playing))
                {
                    //_audioManager.playSound("elevator");
                }
                    
                if (_rigidbodiesList.Count == 1 && _boxCollidersInTrigger == 1)
                {
                    if (onTriggerEndDelayIEnumerator != null)
                    {
                        StopCoroutine(onTriggerEndDelayIEnumerator);
                    }

                    onTriggerStartDelayIEnumerator = onTriggerStartDelay();
                    StartCoroutine(onTriggerStartDelayIEnumerator);
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (_possibleTriggerObjects.Contains(other.gameObject.tag))
        {
            _boxCollidersInTrigger = _boxCollidersInTrigger - 1;
            Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();

            if (otherRigidbody != null)
            {
                remove(otherRigidbody);
                // Turn off emission on the player
                /*
                if (other.GetComponent<PlayerColor>() != null)
                {
                    other.GetComponent<PlayerColor>().playerColorMaterialEmissionTurnOff();
                }
                */
                if (_rigidbodiesList.Count == 0 && _boxCollidersInTrigger == 0)
                {
                    onTriggerEndDelayIEnumerator = onTriggerEndDelay();
                    StartCoroutine(onTriggerEndDelayIEnumerator);
                }
            }
        }
    }
    private IEnumerator onTriggerStartDelay()
    {
        yield return new WaitForSeconds(_delayOnTrigger);

        if (!_triggered)
        {
            _triggered = true;
        }
    }
    private IEnumerator onTriggerEndDelay()
    {
        yield return new WaitForSeconds(_delayOnTrigger);

        if (_triggered)
        {
            _triggered = false;
        }

    }

    // METHODS FOR ADDING AND REMOVING RIGIDBODY COMPONENTS FROM THE LIST
    private void add(Rigidbody otherRigidbody)
    {
        if (!_rigidbodiesList.Contains(otherRigidbody))
        {
            _rigidbodiesList.Add(otherRigidbody);
        }
    }
    private void remove(Rigidbody otherRigidbody)
    {
        if (_rigidbodiesList.Contains(otherRigidbody))
        {
            _rigidbodiesList.Remove(otherRigidbody);
        }
    }
    private void removeAll()
    {
        if (_rigidbodiesList.Count > 0)
        {
            for (int i = 0; i < _rigidbodiesList.Count; i++)
            {
                _rigidbodiesList.Remove(_rigidbodiesList[i]);
            }
        }
    }

}
