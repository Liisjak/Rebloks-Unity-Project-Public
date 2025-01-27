using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndColor : MonoBehaviour
{
    private Controller _controller;

    private float _boxColliderTriggerWidth;
    private float _boxColliderTriggerLength;
    private BoxCollider _boxColliderTrigger;
    private string _triggeredByPlayerColor;

    private int _boxCollidersByPlayerColorInTrigger;

    private Material _endColorEmmissionMaterial;

    private PlayerColor _triggeredByPlayerColorScript;
    private void onSceneStateChangeHandler(sceneState sceneState)
    {
        if (sceneState == sceneState.fail || sceneState == sceneState.setup)
        {
            if (_boxCollidersByPlayerColorInTrigger != 0)
            {
                _boxCollidersByPlayerColorInTrigger = 0;
            }
            // this turns off the emission if the player dies while standing on end (or if for any other reason it is still enabled)
            if (_endColorEmmissionMaterial.IsKeywordEnabled("_EMISSION"))
            {
                _endColorEmmissionMaterial.DisableKeyword("_EMISSION");
            }
        }
    }
    private void Awake()
    {
        Controller.onSceneStateChange += onSceneStateChangeHandler;

        _triggeredByPlayerColor = gameObject.tag.Replace("end", "player");
        _triggeredByPlayerColorScript = GameObject.FindWithTag(_triggeredByPlayerColor).GetComponent<PlayerColor>();

        _boxColliderTriggerWidth = 0.25f;
        _boxColliderTriggerLength = 1.06f;
        _boxColliderTrigger = gameObject.AddComponent<BoxCollider>();
        _boxColliderTrigger.size = new Vector3(_boxColliderTriggerWidth, _boxColliderTriggerLength, _boxColliderTriggerWidth);
        _boxColliderTrigger.isTrigger = true;
        
        _endColorEmmissionMaterial = gameObject.GetComponent<Renderer>().material;

        _boxCollidersByPlayerColorInTrigger = 0;
        _endColorEmmissionMaterial.EnableKeyword("_EMISSION");
        _endColorEmmissionMaterial.SetTexture("_EmissionMap", Resources.Load<Texture2D>($"tileblocks-endblock-color-emission-map"));
        _endColorEmmissionMaterial.DisableKeyword("_EMISSION");
    }
    private void Start()
    {
        _controller = Controller.instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == _triggeredByPlayerColor)
        {
            _boxCollidersByPlayerColorInTrigger = _boxCollidersByPlayerColorInTrigger + 1;
            if (_boxCollidersByPlayerColorInTrigger == 1 && other != null)
            {
                Debug.Log(gameObject.tag + " : playerColor entered");
                if (!_endColorEmmissionMaterial.IsKeywordEnabled("_EMISSION"))
                {
                    _endColorEmmissionMaterial.EnableKeyword("_EMISSION");
                }

                _triggeredByPlayerColorScript.playerColorMaterialEmissionTurnOn();
                _triggeredByPlayerColorScript.erasePathLine(); // this is only relevant when players can be displaced (when turn is ended manually)

                if (_controller.playersAlreadyPlayed.Count == _controller.endColorsAllNumber) 
                {
                    _triggeredByPlayerColorScript.playerStatusIsIdle();
                }

                _controller.endColorHasBeenSteppedOn(gameObject.tag);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == _triggeredByPlayerColor)
        {
            if (_boxCollidersByPlayerColorInTrigger > 0)
            {
                _boxCollidersByPlayerColorInTrigger = _boxCollidersByPlayerColorInTrigger - 1;
            }
            if (_boxCollidersByPlayerColorInTrigger == 0 && other != null)
            {
                Debug.Log(gameObject.tag + " : playerColor exited");
                
                if (_endColorEmmissionMaterial.IsKeywordEnabled("_EMISSION"))
                {
                    _endColorEmmissionMaterial.DisableKeyword("_EMISSION");
                }

                // Add sounds etc.
                _controller.endColorHasBeenSteppedOff(gameObject.tag);

                _triggeredByPlayerColorScript.playerColorMaterialEmissionTurnOff();
            } 
        }
    }
}
