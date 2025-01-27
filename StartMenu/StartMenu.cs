using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{

    private AudioManager _audioManager;
    private bool _buttonPressed;

    private void Awake()
    {
        _audioManager = GameObject.FindObjectOfType<AudioManager>();
        _buttonPressed = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("application quit was ran");
        }
        if (Input.GetKeyDown(KeyCode.Space) && !_buttonPressed)
        {
            _audioManager.playSound("select");
            Invoke("loadLevelSelect", 0.65f);
            _buttonPressed = true;
        }
    }


    void loadLevelSelect()
    {
        //SceneManager.LoadScene("LvlSelect");
        SceneManager.LoadScene("LvlSelect");
    }
}
