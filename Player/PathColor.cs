using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathColor : MonoBehaviour
{
    private Dictionary<int, Vector3> _linePointSaverDict;
    // For line to extend on top of the endColor
    private Vector3 _finalTile;
    private bool _onlySaveOnPlaying;

    // Start is called before the first frame update
    void Awake()
    {
        _linePointSaverDict = new Dictionary<int, Vector3>();
        _onlySaveOnPlaying = true;
    }
    public void finalTilePosition(Vector3 tilePosition)
    {
        if (_onlySaveOnPlaying) // check if "uninitialized"
        {
            _finalTile = tilePosition;
        }
    }

    public LineRenderer drawLinePath(LineRenderer line)
    {
        line.positionCount = 0; // remove all points that might have stayed from previous replay
        foreach (KeyValuePair<int, Vector3> entry in _linePointSaverDict)
        {
            line.positionCount += 1;
            line.SetPosition(line.positionCount - 1, entry.Value);
        }
        if (line.positionCount != 0)
        {
            _onlySaveOnPlaying = false; // if the trail was drawn, it means that the player has already played
        }
        if (line.positionCount != 0)
        {
            line.SetPosition(line.positionCount - 1, _finalTile); // overwrite the last point with the final tile position
        }
        return line;
    }
    public void addToLinePointSaverDict(int frame, Vector3 linePoint)
    {
        _linePointSaverDict.Add(frame, linePoint);
    }
    public void clearLinePointSaverDictHistory()
    {
        if (_linePointSaverDict.Count != 0)
        {
            _linePointSaverDict = new Dictionary<int, Vector3>();
        }
        if (!_onlySaveOnPlaying)
        {
            _onlySaveOnPlaying = true;
        }
    }
    public bool keyExistsLinePointSaverDict(int key)
    {
        return _linePointSaverDict.ContainsKey(key);
    }

    public Vector3 getSavedPoint(int frame)
    {
        return _linePointSaverDict[frame];
    }

    public bool lineHasAlreadyRecordedPoints()
    {
        return (_linePointSaverDict.Count != 0);
    }
}
