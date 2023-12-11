using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Travel : MonoBehaviour
{
    [SerializeField] private Transform spiderTransform;
    [SerializeField] private Transform destinationRotationTransform;
    [Space(20)]
    [SerializeField] private Transform destination;
    [Space(20)]
    [SerializeField] private float travelSpeed = 1f;
    [SerializeField] private float arrivalDistanceMargin = 0.1f;
    [SerializeField] private bool moveToDestination = false;
    [Space(20)]
    [SerializeField] private GameObject DEBUG_projectedDestinationVisualMarker;


    private Vector3 actualProjectedDestination;



    public void Proceed(Vector3 groundNormalVector)
    {
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

            RotateTowardsDestination(actualProjectedDestination);
        }
    }


    private void RotateTowardsDestination(Vector3 actualProjectedDestination)
    {
        Vector3 relativePosition = actualProjectedDestination - spiderTransform.position;

        
        Quaternion rotation = Quaternion.LookRotation(relativePosition, destinationRotationTransform.up);
        destinationRotationTransform.rotation = rotation;

        //destinationRotationTransform.localRotation = new Quaternion(4, 32, 187, 95);

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
