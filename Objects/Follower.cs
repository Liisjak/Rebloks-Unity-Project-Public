using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public GameObject objectFollowX;
    public GameObject objectFollowZ;
    public GameObject objectFollowY;
    private GameObject _objectFollowXBackup;
    private GameObject _objectFollowZBackup;
    private GameObject _objectFollowYBackup;

    public enum flipDirection
    {
        no,
        yes
    }
    public flipDirection flipX;
    private int flipXint;
    public flipDirection flipZ;
    private int flipZint;
    public flipDirection flipY;
    private int flipYint;
    public float[] boundsX = new float[2];
    public float[] boundsZ = new float[2];
    public float[] boundsY = new float[2];
    public float gapX;
    public float gapZ;
    public float gapY;
    

    private Vector3 initialPosition;

    // The following 3 on scene state change related methods should not be needed since we have rewind???
    private void Awake()
    {
        Controller.onSceneStateChange += onSceneStateChangeHandler;

        initialPosition = transform.position;

        // Save object followers in a seperate variable and un-select them. Later, terrainBuilder.cs will set them up again.
        if (objectFollowX != null)
        {
            _objectFollowXBackup = objectFollowX;
            objectFollowX = null;
        }
        if (objectFollowZ != null)
        {
            _objectFollowZBackup = objectFollowZ;
            objectFollowZ = null;
        }
        if (objectFollowY != null)
        {
            _objectFollowYBackup = objectFollowY;
            objectFollowY = null;
        }

        switch (flipX)
        {
            case flipDirection.no:
                flipXint = 1;
                break;
            case flipDirection.yes:
                flipXint = -1;
                break;
        }
        switch (flipZ)
        {
            case flipDirection.no:
                flipZint = 1;
                break;
            case flipDirection.yes:
                flipZint = -1;
                break;
        }
        switch (flipY)
        {
            case flipDirection.no:
                flipYint = 1;
                break;
            case flipDirection.yes:
                flipYint = -1;
                break;
        }
    }
    private void OnDestroy()
    {
        Controller.onSceneStateChange -= onSceneStateChangeHandler;
    }
    private void onSceneStateChangeHandler(sceneState sceneState)
    {
        if (sceneState == sceneState.playerRed || sceneState == sceneState.playerBlue || sceneState == sceneState.playerGreen || sceneState == sceneState.playerYellow)
        {
            transform.position = initialPosition;
        }
    }

    private void FixedUpdate()
    {
        float transformedPositionX;
        float transformedPositionZ;
        float transformedPositionY;

        if (objectFollowX != null)
        {
            transformedPositionX = transformWithinBounds(flipXint * objectFollowX.transform.position.x + gapX, boundsX);
        } else
        {
            transformedPositionX = transform.position.x;
        }
               
        if (objectFollowY != null)
        {
            transformedPositionY = transformWithinBounds(flipYint * objectFollowY.transform.position.y + gapY, boundsY);
        }
        else
        {
            transformedPositionY = transform.position.y;
        }

        if (objectFollowZ != null)
        {
            transformedPositionZ = transformWithinBounds(flipZint * objectFollowZ.transform.position.z + gapZ, boundsZ);
        }
        else
        {
            transformedPositionZ = transform.position.z;
        }

        //Vector3 translateDirection = new Vector3(transformedPositionX, transformedPositionY, transformedPositionZ) - transform.position;
        //transform.Translate(translateDirection, Space.Self);


        transform.position = new Vector3(transformedPositionX, transformedPositionY, transformedPositionZ);

    }

    // TRANSFORM FOLLOWER POSITION WITHIN BOUNDS
    private float transformWithinBounds(float axisPosition, float[] axisBounds)
    {
        if (axisPosition < axisBounds[0])
        {
            return axisBounds[0];
        } else if (axisPosition > axisBounds[1])
        {
            return axisBounds[1];
        } else
        {
            return axisPosition;
        }
    }

    // SET THE FOLLOW OBJECTS BACK TO THE INITIAL ONES SET PUBLICLY
    public void setFollowObjects()
    {
        if (_objectFollowXBackup != null)
        {
            objectFollowX = _objectFollowXBackup;
        }
        if (_objectFollowZBackup != null)
        {
            objectFollowZ= _objectFollowZBackup;
        }
        if (_objectFollowYBackup != null)
        {
            objectFollowY = _objectFollowYBackup;
        }
    }

}
