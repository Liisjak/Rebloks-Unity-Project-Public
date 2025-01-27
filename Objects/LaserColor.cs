using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserColor : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private Transform _startPoint;
    private string _playerColorTag;
    private List<string> _playerColorTagAll;
    private Color _laserColor;
    private bool _laserActivated;

    private GameObject _laserStartFlash;
    private GameObject _laserEndFlash;
    private GameObject _laserEndParticles;

    private Vector3 _pointDirection;
    public enum directionOptions
    {
        forward,
        right,
        backward,
        left,
        down
    }

    public directionOptions pointDirection;

    
    void Awake()
    {      
        _playerColorTag = gameObject.tag.Replace("laser", "player");
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lineRenderer.startWidth = 0.25f;
        _lineRenderer.endWidth = 0.25f;
        _lineRenderer.textureMode = LineTextureMode.Tile;
        determineLaserColorForEachPlayer();
        //_lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Determine laser material and its color
        if (Resources.Load<Material>($"Laser/laser{gameObject.tag.Replace("laser", "")}") != null)
        {
            Material laserMaterial = new Material(Resources.Load<Material>($"Laser/laser{gameObject.tag.Replace("laser", "")}"));
            _lineRenderer.material = laserMaterial;
        }
        
        _startPoint = gameObject.transform;
        determinePointDirection();

        _playerColorTagAll = new List<string> { "playerBlue", "playerRed", "playerGreen", "playerYellow" };

        gameObject.GetComponent<LineRenderer>().enabled = false;
        _laserActivated = false;

        // Particles
        if (Resources.Load<GameObject>($"Laser/LaserStartFlash") != null)
        {
            _laserStartFlash = Resources.Load<GameObject>($"Laser/LaserStartFlash");
        }
        if (Resources.Load<GameObject>($"Laser/LaserEndFlash") != null)
        {
            _laserEndFlash = Resources.Load<GameObject>($"Laser/LaserEndFlash");
        }
        if (Resources.Load<GameObject>($"Laser/LaserEndParticles") != null)
        {
            _laserEndParticles = Resources.Load<GameObject>($"Laser/LaserEndParticles");
        }

    }

    void Update()
    {
        _lineRenderer.SetPosition(0, _startPoint.position);
        
        RaycastHit RaycastHit;
        if (Physics.Raycast(transform.position, _pointDirection, out RaycastHit))
        {
            if (RaycastHit.collider)
            {
                _lineRenderer.SetPosition(1, RaycastHit.point);
                if (_laserActivated)
                {
                    _laserEndFlash.transform.position = RaycastHit.point - _pointDirection * 0.06f;
                    _laserEndParticles.transform.position = RaycastHit.point - _pointDirection * 0.04f;
                }
            }
            if (_playerColorTagAll.Contains(RaycastHit.transform.tag))
            {
                if (RaycastHit.transform.tag != _playerColorTag)
                {
                    GameObject playerToDestroy = RaycastHit.transform.gameObject;
                    PlayerColor playerToDestroyPlayerColorScript = playerToDestroy.GetComponent<PlayerColor>();
                    
                    // Make explosion
                    playerToDestroyPlayerColorScript.playerExplode(RaycastHit.point);                 
                }
            }
        }
        else
        {
            _lineRenderer.SetPosition(1, _pointDirection * 50);
            if (_laserActivated)
            {
                _laserEndFlash.transform.position = _pointDirection * 50;
                _laserEndParticles.transform.position = _pointDirection * 50;
            }
        }
    }

    // TURN ON LINERENDERER METHOD USED BY TERRAIN BUILDER
    public void turnOnLaser()
    {
        if (gameObject.GetComponent<LineRenderer>() != null)
        {
            gameObject.GetComponent<LineRenderer>().enabled = true;
        }
        if (_laserStartFlash != null)
        {
            _laserStartFlash = Instantiate(_laserStartFlash, transform.position + _pointDirection * 0.3f, Quaternion.identity);
            _laserStartFlash.transform.parent = transform;
            //_laserStartFlash.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", _laserColor * 100f);
            var main = _laserStartFlash.GetComponent<ParticleSystem>().main;
            main.startColor = _laserColor;
        }
        if (_laserEndFlash != null)
        {
            RaycastHit RaycastHit;
            if (Physics.Raycast(transform.position, _pointDirection, out RaycastHit))
            {
                _laserEndFlash = Instantiate(_laserEndFlash, RaycastHit.point - _pointDirection * 0.06f, Quaternion.identity);
            }
            else
            {
                _laserEndFlash = Instantiate(_laserEndFlash, _pointDirection * 50, Quaternion.identity);
            }
            //_laserEndFlash.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", _laserColor * 100f);
            var main = _laserEndFlash.GetComponent<ParticleSystem>().main;
            main.startColor = _laserColor;
        }
        if (_laserEndParticles != null)
        {
            RaycastHit RaycastHit;
            if (Physics.Raycast(transform.position, _pointDirection, out RaycastHit))
            {
                _laserEndParticles = Instantiate(_laserEndParticles, RaycastHit.point - _pointDirection * 0.04f, Quaternion.LookRotation(-_pointDirection, Vector3.forward));
            }
            else
            {
                _laserEndParticles = Instantiate(_laserEndParticles, _pointDirection * 50, Quaternion.LookRotation(-_pointDirection, Vector3.forward));
            }
            //_laserEndParticles.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", _laserColor * 100f);
            var main = _laserEndParticles.GetComponent<ParticleSystem>().main;
            main.startColor = _laserColor;
        }

        _laserActivated = true;

    }

    // DETERMINE LASER POINT DIRECTION BASED ON PUBLIC ENUM
    private void determinePointDirection()
    {
        switch (pointDirection)
        {
            case directionOptions.forward:
                _pointDirection = Vector3.forward;
                break;
            case directionOptions.right:
                _pointDirection = Vector3.right;
                break;
            case directionOptions.backward:
                _pointDirection = -Vector3.forward;
                break;
            case directionOptions.left:
                _pointDirection = -Vector3.right;
                break;
            case directionOptions.down:
                _pointDirection = Vector3.down;
                break;
        }
    }

    // later this will select the laser shader color
    private void determineLaserColorForEachPlayer()
    {
        switch (gameObject.tag)
        {
            case "laserRed":
                _laserColor = new Color(1, 0, 0, 1);
                break;
            case "laserBlue":
                _laserColor = new Color(0, 0, 1, 1);
                break;
            case "laserGreen":
                _laserColor = new Color(0, 1, 0, 1);
                break;
            case "laserYellow":
                _laserColor = new Color(1, 1, 0, 1);
                break;
        }
    }

}
