using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
//using System.Linq;


public class LvlSelect : MonoBehaviour
{
    private Transform _hline;
    private Transform _vline;
    private List<Transform> _lvlIcons;
    private List<TMP_Text> _lvlTexts;
    private int _currentIndex;
    private int _previousIndex;
    private int _defaultFontSize;
    private int _highlightFontSize;
    private AudioManager _audioManager;

    private void Awake()
    {
        _audioManager = GameObject.FindObjectOfType<AudioManager>();

        _hline = GameObject.Find("hline").transform;
        _vline = GameObject.Find("vline").transform;

        // Find all level icons
        _defaultFontSize = 80;
        _highlightFontSize = 110;
        _lvlIcons = new List<Transform>(transform.childCount - 2);
        _lvlTexts = new List<TMP_Text>(transform.childCount - 2); 
         Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.GetComponent<TMP_Text>() != null)
            {
                _lvlIcons.Add(child.parent.transform);
                //_lvlIcons.Add(child.transform);
                child.GetComponent<TMP_Text>().fontSize = _defaultFontSize;
                _lvlTexts.Add(child.GetComponent<TMP_Text>());
            }
        }    
      
    }
    void Start()
    {
        _currentIndex = 0;
        _previousIndex = 0;
        _hline.localPosition = new Vector3(_hline.localPosition.x, _lvlIcons[_currentIndex].localPosition.y, _hline.localPosition.z);
        _vline.localPosition = new Vector3(_lvlIcons[_currentIndex].localPosition.x, _vline.localPosition.y, _vline.localPosition.z);
        _lvlTexts[_currentIndex].fontSize = _highlightFontSize;
        _audioManager.playSound("moveSelection");

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("StartMenu");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _audioManager.playSound("select");
            //SceneManager.LoadScene($"Lvl{_currentIndex+1}");
            SceneManager.LoadScene(_currentIndex + 2);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _previousIndex = _currentIndex;
            _currentIndex = Mathf.Min(_currentIndex + 1, transform.childCount-2-1);
            highlightNextLevelIcon(_currentIndex, _previousIndex);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _previousIndex = _currentIndex;
            _currentIndex = Mathf.Max(_currentIndex - 1, 0);
            highlightNextLevelIcon(_currentIndex, _previousIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) // hardcoded to rows of length 7
        {
            
            if (_currentIndex+7 <= _lvlIcons.Count-1)
            {
                _previousIndex = _currentIndex;
                _currentIndex = _currentIndex + 7;
                highlightNextLevelIcon(_currentIndex, _previousIndex);
            } 
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) // hardcoded to rows of length 7
        {
            if (_currentIndex - 7 >= 0)
            {
                _previousIndex = _currentIndex;
                _currentIndex = _currentIndex - 7;
                highlightNextLevelIcon(_currentIndex, _previousIndex);
            }
        }
    }

    private void highlightNextLevelIcon(int currentIndex, int previousIndex)
    {
        if (currentIndex != previousIndex)
        {
            _hline.localPosition = new Vector3(_hline.localPosition.x, _lvlIcons[currentIndex].localPosition.y, _hline.localPosition.z);
            _vline.localPosition = new Vector3(_lvlIcons[currentIndex].localPosition.x, _vline.localPosition.y, _vline.localPosition.z);
            _lvlTexts[currentIndex].fontSize = _highlightFontSize;
            _lvlTexts[previousIndex].fontSize = _defaultFontSize;
            _audioManager.playSound("moveSelection");
        }
    }
}
