using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaverRewind : MonoBehaviour
{
    // Before we need the rotation data aswell, all the lines related to rotation are commented out

    //private Dictionary<int, objectPosRotStruct> objectPosRotSaverDict;
    private Dictionary<int, Vector3> objectPosSaverDict;

    void Awake()
    {
        //objectPosRotSaverDict = new Dictionary<int, objectPosRotStruct>();
        objectPosSaverDict = new Dictionary<int, Vector3>();
    }

    public void addToObjectPosSaverDict(int frame, Vector3 objectPos)
    {
        objectPosSaverDict.Add(frame, objectPos);
    }

    public void clearObjectPosSaverDictHistory()
    {
        if (objectPosSaverDict.Count != 0)
        {
            objectPosSaverDict = new Dictionary<int, Vector3>();
            //Debug.Log("Object position history cleared.");
        }
    }

    public bool keyExistsPosSaverDict(int key)
    {
        return objectPosSaverDict.ContainsKey(key);
    }

    public Vector3 getSavedPos(int frame)
    {
        return objectPosSaverDict[frame];
    }

    public bool objectHasAlreadyRecordedPos()
    {
        return (objectPosSaverDict.Count != 0);
    }
}
