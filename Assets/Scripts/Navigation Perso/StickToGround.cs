using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class StickToGround : MonoBehaviour
{
    [SerializeField] private Travel travel;
    [Space(10)]
    [SerializeField] private LayerMask groundLayerMask;
    [Space(10)]

    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private float groundDistanceMargin = 0.1f;
    [SerializeField] private float groundingSpeed = 0.5f;

    private GameObject actualGroundObject = null;
    private Vector3 closest_Point_Of_Actual_Ground_Object = Vector3.zero;

    private float distance_To_The_Closest_Point_Of_Actual_Ground_Object = 0;

    private Vector3 groundNormalVector;

    private bool initialRotation = true;


    void FixedUpdate()
    {
        if (actualGroundObject != null)
        {
            Vector3 vector_To_Ground_Object_Closest_Point = closest_Point_Of_Actual_Ground_Object - transform.position;
            groundNormalVector = vector_To_Ground_Object_Closest_Point.normalized;

            StayGrounded();

            RotateToFaceTheGround();

            travel.Move(groundNormalVector);
        }
    }


    private void StayGrounded()
    {
        if (distance_To_The_Closest_Point_Of_Actual_Ground_Object > groundDistance + groundDistanceMargin)
        {
            transform.position += groundNormalVector * groundingSpeed * Time.deltaTime;
        }
        else if (distance_To_The_Closest_Point_Of_Actual_Ground_Object < groundDistance)
        {
            transform.position -= groundNormalVector * groundingSpeed * Time.deltaTime;
        }
    }


    private void RotateToFaceTheGround()
    {
        RaycastHit hit;
        Vector3 raycastDirection = (actualGroundObject.transform.position - transform.position).normalized;
        float maxDistance = 100f;

        if (Physics.Raycast(transform.position, raycastDirection, out hit, maxDistance, groundLayerMask))
        {
            //Debug.Log("LE RAYCAST A TOUCHE UN OBJET. Normale du point touché: " + hit.normal);
            groundNormalVector = hit.normal;

            //Quaternion targetRotation = Quaternion.LookRotation(groundNormalVector, -groundNormalVector);
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormalVector) * transform.rotation;

            if (initialRotation)
            {
                transform.rotation = targetRotation;
                initialRotation = false;
            }
            else
            {
                float rotationSpeed = 200f;
                float maxDegreesDelta = rotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
            }
        }
        else
        {
            Debug.Log("No Object detected by the raycast. Can't align character with ground.");
        }
    }


    public void CheckObjectProximity(Vector3 closestPoint, Transform objectTransform)
    {
        if (objectTransform.gameObject == actualGroundObject)
        {
            closest_Point_Of_Actual_Ground_Object = closestPoint;
        }

        float distance_To_The_Closest_Point_Of_This_Object = Vector3.Distance(closestPoint, transform.position);
        distance_To_The_Closest_Point_Of_Actual_Ground_Object = Vector3.Distance(closest_Point_Of_Actual_Ground_Object, transform.position);

        if (distance_To_The_Closest_Point_Of_This_Object < distance_To_The_Closest_Point_Of_Actual_Ground_Object)
        {
            closest_Point_Of_Actual_Ground_Object = closestPoint;
            distance_To_The_Closest_Point_Of_Actual_Ground_Object = distance_To_The_Closest_Point_Of_This_Object;
            actualGroundObject = objectTransform.gameObject;

            Debug.Log("New closest object: " + actualGroundObject.name + ", at a distance of " + distance_To_The_Closest_Point_Of_Actual_Ground_Object + ".");
        }
    }


    public void Object_Exiting_Proximity_Trigger(GameObject exitingObject)
    {
        if (exitingObject == actualGroundObject)
        {
            actualGroundObject = null;
            distance_To_The_Closest_Point_Of_Actual_Ground_Object = 0;

            Debug.Log("No more ground object in the character's trigger zone.");
        }
    }


    #region TRIGGER FUNCTIONS
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ground>(out Ground ground))
        {
            CheckObjectProximity(other.ClosestPoint(transform.position), other.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ground>(out Ground ground))
        {
            CheckObjectProximity(other.ClosestPoint(transform.position), other.transform);
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
