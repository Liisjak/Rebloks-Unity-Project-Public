using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lvlChecks : MonoBehaviour
{
    private List<GameObject> _lvlChecks;
    private GameStateSaveSystem _gameStateSaveSystem;

    private void Awake()
    {
        _lvlChecks = new List<GameObject>(transform.childCount);

        foreach (Transform child in transform)
        {
            _lvlChecks.Add(child.gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _gameStateSaveSystem = GameStateSaveSystem.instance;

        for (int index = 1; index <= 21; index++) // hardcoded total number of levels 
        {
            Debug.Log("index: " + index + "  stars: " + _gameStateSaveSystem.loadLevelState(index));
            if (_gameStateSaveSystem.loadLevelState(index) != 1)
            {
                _lvlChecks[index-1].SetActive(false);
            }
                
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
