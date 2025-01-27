using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carrier : MonoBehaviour
{
    private List<string> possibleCarryObjects;
    public List<Rigidbody> rigidbodiesList;
    BoxCollider boxColliderUpDown;
    float boxColliderTriggerWidth;
    float boxColliderTriggerLength;

    Transform _transform;
    Vector3 lastPosition;

    private void Awake()
    {
        Controller.onSceneStateChange += onSceneStateChangeHandler;
    }
    // It is a good practice to always unsubscribe when this class gets destroyed. OnDestroy() is called when the Scene is closed and a new Scene is loaded.
    private void OnDestroy()
    {
        Controller.onSceneStateChange -= onSceneStateChangeHandler;
    }
    private void onSceneStateChangeHandler(sceneState sceneState)
    {
        removeAll();
    }


    void Start()
    {
        possibleCarryObjects = new List<string> { "playerBlue", "playerRed", "playerYellow", "playerGreen"};
        rigidbodiesList = new List<Rigidbody>();

        _transform = transform;
        lastPosition = _transform.position;

        boxColliderTriggerWidth = 0.10f; // this is now twice as big as if the playerColor would carry the script
        boxColliderTriggerLength = 0.04f; // this is now twice as big as if the playerColor would carry the script
        boxColliderUpDown = gameObject.AddComponent<BoxCollider>();
        boxColliderUpDown.size = new Vector3(boxColliderTriggerWidth, boxColliderTriggerLength, boxColliderTriggerWidth);
        boxColliderUpDown.center = new Vector3(0f, 0.5f, 0f);
        boxColliderUpDown.isTrigger = true;
        
    }


    private void FixedUpdate()
    {
        if (rigidbodiesList.Count > 0)
        {
            for (int i = 0; i < rigidbodiesList.Count; i++)
            {
                Rigidbody rb = rigidbodiesList[i];
                Vector3 carrierVelocity = (_transform.position - lastPosition);
                //Debug.Log(carrierRigidbodyComponent.velocity);
                //Debug.Log(gameObject.tag + ": current = " + _transform.position + "last = " + lastPosition);
                rb.transform.Translate(carrierVelocity, Space.World);//, _transform);
                //Debug.Log("rb = " + rb.velocity);
            }
        }

        lastPosition = _transform.position;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (possibleCarryObjects.Contains(other.gameObject.tag))
        {
            float thisGameobjectY = transform.position.y;
            float otherGameobjectY = other.transform.position.y;

            Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();

            // Only carry the object if it is above the carrier - the treshold must probably be a bit lower than 1.0, if both objects are the size of 1.0
            if (otherRigidbody != null && (otherGameobjectY - thisGameobjectY) >= 0.8f)
            {
                add(otherRigidbody);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (possibleCarryObjects.Contains(other.gameObject.tag))
        {
            Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
            if (otherRigidbody != null)
            {
                remove(otherRigidbody);
            }
        }
    }

    void add(Rigidbody otherRigidbody)
    {
        if (!rigidbodiesList.Contains(otherRigidbody))
        {
            rigidbodiesList.Add(otherRigidbody);
        }
    }

    void remove(Rigidbody otherRigidbody)
    {
        if (rigidbodiesList.Contains(otherRigidbody))
        {
            rigidbodiesList.Remove(otherRigidbody);
        }
    }

    private void removeAll()
    {
        if (rigidbodiesList.Count > 0)
        {
            for (int i = 0; i < rigidbodiesList.Count; i++)
            {
                rigidbodiesList.Remove(rigidbodiesList[i]);
            }
        }
    }

}
