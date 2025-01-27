using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{

    private Controller _controller;

    private void Start()
    {
        _controller = Controller.instance;
    }
    private void OnTriggerEnter(Collider other)
    {
        string otherTag = other.gameObject.tag;
        if (otherTag == "playerBlue" || otherTag == "playerRed" || otherTag == "playerYellow" || otherTag == "playerGreen")
        {
            
            PlayerColor otherPlayerColorScript = other.gameObject.GetComponent<PlayerColor>();
            otherPlayerColorScript.isDead = true;

            if (_controller.sceneState != sceneState.fail)
            {
                _controller.updateSceneState(sceneState.fail);
            }
        }
    }
}
