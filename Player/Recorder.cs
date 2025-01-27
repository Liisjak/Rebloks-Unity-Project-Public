using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Recorder : MonoBehaviour
{
    // All inputs that user makes
    private float horizontalValue;
    private float verticalValue;


    public void getInputs()
    {
        horizontalValue = Input.GetAxisRaw("Horizontal");
        verticalValue = Input.GetAxisRaw("Vertical");
    }

    public userInputStruct getInputStruct()
    {
        userInputStruct playerInputs = new userInputStruct(horizontalValue, verticalValue);
        return playerInputs;
    }

    public void resetInput()
    {
        horizontalValue = 0;
        verticalValue = 0;
    }


}

/* Structure is a custom data type, e.g., (float, float, bool). It is also a value type, as opposed to class, which is a reference type.
   Within structure, we can use variables, methods, and constructors.
   We cannot initialize variables, like we can in the class: public int x = 4
 */
public struct userInputStruct // this data type is named userInputStruct
{
    public float verticalInput;
    public float horizontalInput;


    public userInputStruct(float horizontalValue, float verticalValue) // this is constructor
    {
        verticalInput = verticalValue;
        horizontalInput = horizontalValue;
    }
}
