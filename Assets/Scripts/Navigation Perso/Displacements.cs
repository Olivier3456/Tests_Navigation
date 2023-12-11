using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Displacements : MonoBehaviour
{
    [SerializeField] private Transform spiderTransform;
    [SerializeField] private Transform targetRotationTransform;
    [Space(20)]
    [SerializeField] private Transform destination;
    [Space(20)]
    [SerializeField] private Transform raycastOriginForGroundAnglesDetection;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float maxRaycastHitDistanceToAllowPassage = 0.75f;
    [SerializeField] private float minRaycastDistanceAllowedToAllowPassage = 0.35f;
    [Space(20)]
    [SerializeField] private float travelSpeed = 1f;
    [SerializeField] private float arrivalDistanceMargin = 0.1f;
    [SerializeField] private float maxAngleBetweenConsecutiveGroundNormals = 45f;
    [SerializeField] private bool moveToDestination = false;
    [Space(20)]
    [SerializeField] private GameObject DEBUG_projectedDestinationVisualMarker;




    private Vector3 actualProjectedDestination;

    private Vector3 lastGroundNormal = Vector3.zero;


    public void Proceed(Vector3 groundNormalVector)
    {
        if (DetectTooWideGroundAngles())
        {
            return;
        }


        actualProjectedDestination = destination.position - Vector3.Dot(groundNormalVector, destination.position - spiderTransform.position) * groundNormalVector;

        if (DEBUG_projectedDestinationVisualMarker != null)
        {
            DEBUG_projectedDestinationVisualMarker.transform.position = actualProjectedDestination;
        }


        if (moveToDestination)
        {
            float distanceToArrival = Vector3.Distance(actualProjectedDestination, spiderTransform.position);

            if (distanceToArrival > arrivalDistanceMargin)
            {
                Vector3 movementDirection = (actualProjectedDestination - spiderTransform.position).normalized;
                Vector3 movement = movementDirection * travelSpeed * Time.deltaTime;
                spiderTransform.position += movement;
            }

            RotateTowardsDestination(actualProjectedDestination, groundNormalVector);
        }

        lastGroundNormal = groundNormalVector;
    }


    private bool DetectTooWideGroundAngles()
    {
        // Detecting too wide intern angles:
        //if (lastGroundNormal != Vector3.zero)
        //{
        //    float angleBetweenGroundNormalAndLastGroundNormal = Vector3.Angle(groundNormalVector, lastGroundNormal);

        //    if (angleBetweenGroundNormalAndLastGroundNormal > maxAngleBetweenConsecutiveGroundNormals)
        //    {
        //        Debug.Log($"Too much angle between two consecutive ground normal: {angleBetweenGroundNormalAndLastGroundNormal}°. Spider can't continueits path.");
        //        return true;
        //    }
        //}


        RaycastHit hit;
        float maxDistance = maxRaycastHitDistanceToAllowPassage;
        Vector3 raycastDirection;

        // Detecting too wide extern angles:
        raycastDirection = -raycastOriginForGroundAnglesDetection.up;
        if (!Physics.Raycast(raycastOriginForGroundAnglesDetection.position, raycastDirection, out hit, maxDistance, groundLayerMask))
        {
            Debug.Log($"Raycast for extern angles detection has a too high hit distance (more than {maxDistance}m. Spider can't continue its path.");
            return true;
        }

        // Detecting too wide intern angles:
        raycastDirection = raycastOriginForGroundAnglesDetection.forward;
        if (Physics.Raycast(raycastOriginForGroundAnglesDetection.position, raycastDirection, out hit, maxDistance, groundLayerMask))
        {
            if (hit.distance < minRaycastDistanceAllowedToAllowPassage)
            {
                Debug.Log($"Raycast for intern angles detection has a too small hit distance (less than {minRaycastDistanceAllowedToAllowPassage}m. Spider can't continue its path.");
                return true;
            }
        }

        return false;
    }



    private void RotateTowardsDestination(Vector3 actualProjectedDestination, Vector3 groundNormalVector)
    {
        // Chat GPT 3.5:

        Vector3 relativePosition = actualProjectedDestination - spiderTransform.position;
        Quaternion rotationToDestination = Quaternion.LookRotation(relativePosition, targetRotationTransform.up);

        // Appliquer la rotation vers la destination
        targetRotationTransform.rotation = rotationToDestination;

        // Ajuster la rotation pour tenir compte de l'inclinaison du sol
        Quaternion rotationToGround = Quaternion.FromToRotation(targetRotationTransform.up, groundNormalVector);
        targetRotationTransform.rotation *= rotationToGround;
    }


    public float GetTravelSpeed()
    {
        return travelSpeed;
    }

    public bool IsMovingToDestination()
    {
        return moveToDestination;
    }
}
