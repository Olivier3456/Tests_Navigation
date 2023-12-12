using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Displacements : MonoBehaviour
{
    [SerializeField] private Transform spiderTransform;
    [SerializeField] private Transform rotationTransform;
    [SerializeField] private Transform raycastOrigin;
    [Space(20)]
    [SerializeField] private Transform destination;
    [Space(20)]
    [SerializeField] private LayerMask groundLayerMask;
    [Space(20)]
    [SerializeField] private float travelSpeed = 1f;
    [SerializeField] private float arrivalDistanceMargin = 0.1f;
    [SerializeField] private float maxAnglesDelta = 45f;
    [SerializeField] private bool moveToDestination = false;
    [Space(20)]
    [SerializeField] private GameObject DEBUG_projectedDestinationVisualMarker;

    private Vector3 actualProjectedDestination;


    public void Proceed(GroundDatas groundDatas)
    {
        if (groundDatas.actualGroundObject == null)
        {
            Debug.Log("No ground object detected. Spider can't walk.");
            return;
        }

        if (DetectTooWideGroundAnglesDelta(groundDatas))
        {
            return;
        }


        actualProjectedDestination = destination.position - Vector3.Dot(groundDatas.actualGroundNormal, destination.position - spiderTransform.position) * groundDatas.actualGroundNormal;

        DEBUG_projectedDestinationVisualMarker.transform.position = actualProjectedDestination;

        RotateTowardsDestination(actualProjectedDestination, groundDatas.actualGroundNormal);


        if (moveToDestination)
        {
            float distanceToArrival = Vector3.Distance(actualProjectedDestination, spiderTransform.position);

            if (distanceToArrival > arrivalDistanceMargin)
            {
                Vector3 movementDirection = (actualProjectedDestination - spiderTransform.position).normalized;
                Vector3 movement = movementDirection * travelSpeed * Time.deltaTime;
                spiderTransform.position += movement;
            }
        }
    }


    private Vector3 hitPoint;
    private bool DetectTooWideGroundAnglesDelta(GroundDatas groundDatas)
    {
        bool res = false;

        RaycastHit hit;
        float maxDistance = 2f;

        if (Physics.Raycast(raycastOrigin.position, raycastOrigin.transform.forward, out hit, maxDistance, groundLayerMask))
        {
            hitPoint = hit.point;

            float angleBetweenTheTwoGrounds = Vector3.Angle(groundDatas.actualGroundNormal, hit.normal);

            if (angleBetweenTheTwoGrounds > maxAnglesDelta)
            {
                Debug.Log($"Too wide angle delta ({angleBetweenTheTwoGrounds}°) detected in front of the spider, can't continue walking.");
                res = true;
            }
        }
        else
        {
            hitPoint = Vector3.zero;
            Debug.Log("No ground detected in front of the spider, can't continue walking.");
            res = true;
        }

        return res;
    }



    private void RotateTowardsDestination(Vector3 actualProjectedDestination, Vector3 groundNormalVector)
    {
        // Chat GPT 3.5:

        float rotationSpeed = 10;

        Vector3 relativePosition = actualProjectedDestination - spiderTransform.position;
        Quaternion rotationToDestination = Quaternion.LookRotation(relativePosition, rotationTransform.up);

        // Aligner l'objet vers la destination
        rotationTransform.rotation = Quaternion.Slerp(rotationTransform.rotation, rotationToDestination, Time.deltaTime * rotationSpeed);

        // Ajuster la rotation pour tenir compte de l'inclinaison du sol
        Quaternion rotationToGround = Quaternion.FromToRotation(rotationTransform.up, groundNormalVector) * rotationTransform.rotation;
        rotationTransform.rotation = Quaternion.Slerp(rotationTransform.rotation, rotationToGround, Time.deltaTime * rotationSpeed);
    }


    public float GetTravelSpeed()
    {
        return travelSpeed;
    }

    public bool IsMovingToDestination()
    {
        return moveToDestination;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (hitPoint != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(raycastOrigin.position, hitPoint);
        }
    }
#endif
}
