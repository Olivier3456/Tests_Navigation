using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Displacements : MonoBehaviour
{
    [SerializeField] private Transform spiderTransform;
    [SerializeField] private Transform targetRotationTransform;
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

            RotateTowardsDestination(actualProjectedDestination, groundNormalVector);
        }
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
