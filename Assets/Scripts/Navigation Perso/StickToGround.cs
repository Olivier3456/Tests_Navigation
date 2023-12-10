using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class StickToGround : MonoBehaviour
{
    [SerializeField] private MoveOnGround moveOnGround;

    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private float groundDistanceMargin = 0.1f;
    [SerializeField] private float groundingSpeed = 0.5f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float rotationTimeStep = 0.5f;

    private float rotationTimer = 0;
    private Vector3 direction_Vector_To_Ground_Object_Closest_Point_For_Actual_Rotation = Vector3.zero;

    private GameObject actual_Ground_Object = null;
    private Vector3 closest_Point_Of_Actual_Ground_Object = Vector3.zero;

    private float distance_To_The_Closest_Point_Of_Actual_Ground_Object = 0;

    private Vector3 direction_Vector_To_Ground_Object_Closest_Point;


    void Update()
    {
        if (actual_Ground_Object != null)
        {
            Vector3 vector_To_Ground_Object_Closest_Point = closest_Point_Of_Actual_Ground_Object - transform.position;
            direction_Vector_To_Ground_Object_Closest_Point = vector_To_Ground_Object_Closest_Point.normalized;

            StayGrounded();
            StayAlignedWithGround();

            // déplacements dans un second temps :
            moveOnGround.Move(direction_Vector_To_Ground_Object_Closest_Point);
        }
    }



    private void StayGrounded()
    {
        if (distance_To_The_Closest_Point_Of_Actual_Ground_Object > groundDistance + groundDistanceMargin)
        {
            transform.position += direction_Vector_To_Ground_Object_Closest_Point * groundingSpeed * Time.deltaTime;

            //Debug.Log("Character too far from the ground. Grounding character.");
        }
        else if (distance_To_The_Closest_Point_Of_Actual_Ground_Object < groundDistance)
        {
            transform.position -= direction_Vector_To_Ground_Object_Closest_Point * groundingSpeed * Time.deltaTime;

            //Debug.Log("Character too close to the ground. Elevating character.");
        }
    }


    private void StayAlignedWithGround()
    {
        rotationTimer += Time.deltaTime;

        if (rotationTimer >= rotationTimeStep)
        {
            rotationTimer = 0;
            direction_Vector_To_Ground_Object_Closest_Point_For_Actual_Rotation = direction_Vector_To_Ground_Object_Closest_Point;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction_Vector_To_Ground_Object_Closest_Point_For_Actual_Rotation, -direction_Vector_To_Ground_Object_Closest_Point_For_Actual_Rotation);
        float maxDegreesDelta = rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
    }


    public void Check_If_An_Object_Is_The_New_Ground(Vector3 closestPoint, Transform objectTransform)
    {
        if (objectTransform.gameObject == actual_Ground_Object)
        {
            closest_Point_Of_Actual_Ground_Object = closestPoint;
        }

        float distance_To_The_Closest_Point_Of_This_Object = Vector3.Distance(closestPoint, transform.position);
        distance_To_The_Closest_Point_Of_Actual_Ground_Object = Vector3.Distance(closest_Point_Of_Actual_Ground_Object, transform.position);

        if (distance_To_The_Closest_Point_Of_This_Object < distance_To_The_Closest_Point_Of_Actual_Ground_Object)
        {
            closest_Point_Of_Actual_Ground_Object = closestPoint;
            distance_To_The_Closest_Point_Of_Actual_Ground_Object = distance_To_The_Closest_Point_Of_This_Object;
            actual_Ground_Object = objectTransform.gameObject;

            Debug.Log("New closest object: " + actual_Ground_Object.name + ", at a distance of " + distance_To_The_Closest_Point_Of_Actual_Ground_Object + ".");
        }
    }


    public void Object_Exiting_Proximity_Trigger(GameObject exitingObject)
    {
        if (exitingObject == actual_Ground_Object)
        {
            actual_Ground_Object = null;
            distance_To_The_Closest_Point_Of_Actual_Ground_Object = 0;

            //Debug.Log("No object is acutally near the caracter.");
        }
    }


    //WaitForSeconds waitLengthBeforeNextGroundObjectChange = new WaitForSeconds(0.5f);
    //private IEnumerator WaitBeforeAuthoriseNextGroundObjectChange()
    //{

    //}



    #region TRIGGER FUNCTIONS
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ground>(out Ground ground))
        {
            Check_If_An_Object_Is_The_New_Ground(other.ClosestPoint(transform.position), other.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ground>(out Ground ground))
        {
            Check_If_An_Object_Is_The_New_Ground(other.ClosestPoint(transform.position), other.transform);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ground>(out Ground ground))
        {
            Object_Exiting_Proximity_Trigger(other.gameObject);
        }
    }
    #endregion
}
