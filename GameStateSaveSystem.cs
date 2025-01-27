using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class GameStateSaveSystem : MonoBehaviour
{

    public Dictionary<int, int> gameStateDataDict;
    public static GameStateSaveSystem instance;

    private void Awake()
    {
        instance = this;

        gameStateDataDict = loadGameState();
    }
    public void saveLevelState(int levelValue, int starsEarnedValue)
    {
        // If the level has already been completed, but the current completion rating is better that the one saved, overwrite the saved rating
        if (gameStateDataDict.ContainsKey(levelValue))
        {
            if (gameStateDataDict[levelValue] < starsEarnedValue)
            {
                gameStateDataDict[levelValue] = starsEarnedValue;
                saveGameState(gameStateDataDict);
            }
        }
        // If the level has not yet been completed, save the current completion rating
        else
        {
            gameStateDataDict.Add(levelValue, starsEarnedValue);
            saveGameState(gameStateDataDict);
        }
        // Either way, unlock the next level if not yet unlocked
        if (!gameStateDataDict.ContainsKey(levelValue + 1))
        {
            gameStateDataDict[levelValue + 1] = 0;
            saveGameState(gameStateDataDict);
        }

    }
    public int loadLevelState(int levelValue)
    {
        if (gameStateDataDict.ContainsKey(levelValue))
        {
            return gameStateDataDict[levelValue];
        }
        else
        {
            return -1;
        }
    }

    private void saveGameState(Dictionary<int, int> gameStateDataDict)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/GameStateData.save";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, gameStateDataDict);
        stream.Close();
    }


    private Dictionary<int, int> loadGameState()
    {
        string path = Application.persistentDataPath + "/GameStateData.save";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            gameStateDataDict = formatter.Deserialize(stream) as Dictionary<int, int>;
            stream.Close();

        }
        else
        {
            gameStateDataDict = new Dictionary<int, int>();
            gameStateDataDict.Add(1, 0);
            gameStateDataDict.Add(2, 0);
            gameStateDataDict.Add(3, 0);
            gameStateDataDict.Add(4, 0);
            gameStateDataDict.Add(5, 0);
            gameStateDataDict.Add(6, 0);
            gameStateDataDict.Add(7, 0);
            gameStateDataDict.Add(8, 0);
            gameStateDataDict.Add(9, 0);
            gameStateDataDict.Add(10, 0);
            gameStateDataDict.Add(11, 0);
            gameStateDataDict.Add(12, 0);
            gameStateDataDict.Add(13, 0);
            gameStateDataDict.Add(14, 0);
            gameStateDataDict.Add(15, 0);
            gameStateDataDict.Add(16, 0);
            gameStateDataDict.Add(17, 0);
            gameStateDataDict.Add(18, 0);
            gameStateDataDict.Add(19, 0);
            gameStateDataDict.Add(20, 0);
            gameStateDataDict.Add(21, 0);
        }
        return gameStateDataDict;
    }

}
