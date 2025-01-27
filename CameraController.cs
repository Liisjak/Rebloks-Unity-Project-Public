using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    private Vector3 _cameraRotationStart;
    private Vector3 _cameraPositionStart;
    private Camera _camera;
    private float _ortigraphicSize;



    void Awake()
    {
        instance = this;

        _cameraPositionStart = transform.position;
        _cameraRotationStart = transform.eulerAngles;
        _camera = GetComponent<Camera>();
        //_camera.backgroundColor = new Color32(107, 122, 160, 0); // blue
        //_camera.backgroundColor = new Color32(194, 148, 176, 255); // pink

        //transform.position = _cameraPositionStart - transform.forward*10f;
        _camera.nearClipPlane = -10;
        _camera.farClipPlane = 1000;

        QualitySettings.shadows = ShadowQuality.Disable;
        if (GameObject.Find("ortographicSize")!= null)
        {
            _ortigraphicSize = GameObject.Find("ortographicSize").transform.localScale.x;
        }
        else
        {
            _ortigraphicSize = 7;
        }
        _camera.orthographicSize = _ortigraphicSize;
        Camera cameraSecond = _camera.transform.GetChild(0).GetComponent<Camera>();
        cameraSecond.orthographicSize = _ortigraphicSize;
        cameraSecond.nearClipPlane = -10;
        cameraSecond.farClipPlane = 1000;

        // camera porition settings are saved in empty gameobject "cameraPosRot"
        GameObject cameraPosRot = GameObject.Find("cameraPosRot");
        transform.position = cameraPosRot.transform.position;
        transform.rotation = cameraPosRot.transform.rotation;

        //Cameras forward direction projected onto the ground
        Vector3 horizontDir = transform.forward;
        Debug.Log(horizontDir);
        horizontDir.y = 40;
        
    }

}
