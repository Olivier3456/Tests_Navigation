using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class StickToGround : MonoBehaviour
{
    [SerializeField] private Travel travel;
    [SerializeField] private Transform groundRotationTransform;
    [Space(10)]
    [SerializeField] private LayerMask groundLayerMask;
    [Space(10)]
    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private float groundDistanceMargin = 0.1f;
    [SerializeField] private float groundingSpeed = 0.5f;

    private GameObject actualGroundObject = null;
    private Vector3 closest_Point_Of_Actual_Ground_Object = Vector3.zero;

    private float distance_To_The_Closest_Point_Of_Actual_Ground_Object = 0;

    Vector3 groundNormalVector = Vector3.zero;


    public void Proceed()
    {
        if (actualGroundObject != null)
        {
            groundNormalVector = UpdateGroundNormalVector();
            PlaceOnGround();
            RotatesTowardsTheGround(groundNormalVector);
        }
    }




    public void PlaceOnGround()
    {
        if (distance_To_The_Closest_Point_Of_Actual_Ground_Object > groundDistance + groundDistanceMargin)
        {
            transform.position -= groundNormalVector * groundingSpeed * travel.GetTravelSpeed() * Time.deltaTime;
        }
        else if (distance_To_The_Closest_Point_Of_Actual_Ground_Object < groundDistance)
        {
            transform.position += groundNormalVector * groundingSpeed * travel.GetTravelSpeed() * Time.deltaTime;
        }
    }

    private void RotatesTowardsTheGround(Vector3 groundNormal)
    {
        Quaternion targetRotation = Quaternion.FromToRotation(groundRotationTransform.up, groundNormal) * groundRotationTransform.rotation;
        float rotationSpeed = 200f;
        float maxDegreesDelta = rotationSpeed * travel.GetTravelSpeed() * Time.deltaTime;
        groundRotationTransform.rotation = Quaternion.RotateTowards(groundRotationTransform.rotation, targetRotation, maxDegreesDelta);
    }



    private Vector3 UpdateGroundNormalVector()
    {
        RaycastHit hit;
        Vector3 raycastDirection = (closest_Point_Of_Actual_Ground_Object - transform.position).normalized;
        float maxDistance = 100f;

        if (Physics.Raycast(transform.position, raycastDirection, out hit, maxDistance, groundLayerMask))
        {
            return hit.normal;
        }
        else
        {
            Debug.Log("No Object detected by the raycast. Can't align character with ground! Returning last ground normal vector.");
            return groundNormalVector;
        }
    }

    public Vector3 GetGroundNormalVector()
    {
        return groundNormalVector;
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
