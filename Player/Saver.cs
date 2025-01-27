using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saver : MonoBehaviour
{

    private Dictionary<int, userInputStruct> userInputSaverDict;


    // Start is called before the first frame update
    void Awake()
    {
        userInputSaverDict = new Dictionary<int, userInputStruct>(); // why do we need to initialize it in Start()?
    }

    public void addToUserInputSaverDict(int frame, userInputStruct userInput)
    {
        userInputSaverDict.Add(frame, userInput);
    }

    public void clearUserInputSaverDictHistory()
    {
        if (userInputSaverDict.Count != 0)
        {
            userInputSaverDict = new Dictionary<int, userInputStruct>();
            Debug.Log("Player history cleared.");
        }  
    }

    // Check if we have userInput at a certain time step
    public bool keyExists(int key)
    {
        return userInputSaverDict.ContainsKey(key);
    }

    // Get userInputStruct at a certain time step
    public userInputStruct getSavedInput(int frame)
    {
        return userInputSaverDict[frame];
    }

    public bool playerHasAlreadyRecordedInput()
    {
        return (userInputSaverDict.Count != 0);
    }

}
