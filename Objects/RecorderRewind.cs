using UnityEngine;

public struct objectPosRotStruct // this data type is named userInputStruct
{
    public Vector3 position;
    public Quaternion rotation;


    public objectPosRotStruct(Vector3 positionValue, Quaternion rotationValue)
    {
        position = positionValue;
        rotation = rotationValue;
    }
}
