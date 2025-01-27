using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerrainBuilder : MonoBehaviour
{
    private bool _onetime;

    private void Awake()
    {
        _onetime = false;
    }
    private void Update() // FIND A CLEANER WAY TO DO THIS INSTEAD OF USING THE UPDATE METHOD()
    {
        if (!_onetime)
        {
            SetupLevel();
            _onetime = true;
        }
    }

    // TERRAIN-BUILDING METHODS
    private void SetupLevel()
    {
        StartCoroutine(waitBeforeSetup());
    }
    private IEnumerator waitBeforeSetup()
    {
        yield return new WaitForSeconds(1.5f);
        setupLvl();
    }
    private void setupLvl()
    {
        Controller.instance.updateSceneState(sceneState.setup);

        // Turn on all the lasers in the scene
        LaserColor[] lasers = FindObjectsOfType(typeof(LaserColor)) as LaserColor[];
        if (lasers.Length != 0)
        {
            foreach (LaserColor laser in lasers)
            {
                laser.turnOnLaser();
            }
        }

        // Set up all follow objects
        Follower[] followers = FindObjectsOfType(typeof(Follower)) as Follower[];
        if (followers.Length != 0)
        {
            foreach (Follower follower in followers)
            {
                follower.setFollowObjects();
            }
        }

        // DESTROY THIS SCRIPT SINCE IT WON'T BE NEEDED ANYMORE
        Destroy(this);
    }
}
