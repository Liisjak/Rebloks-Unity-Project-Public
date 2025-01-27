using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroyBackgroundAudio : MonoBehaviour
{

    private static DoNotDestroyBackgroundAudio instance;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        // this makes sure only 1 copy of this object is in the scene
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
}
