using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Travel : MonoBehaviour
{
    [SerializeField] private Transform spider;
    [SerializeField] private Transform destination;
    [SerializeField] private RotateTowardsDestination rotateTowardsDestination;
    [SerializeField] private float travelSpeed = 1f;
    [SerializeField] private float arrivalDistanceMargin = 0.1f;
    [SerializeField] private bool moveToDestination = false;
    [SerializeField] private GameObject DEBUG_projectedDestinationVisualMarker;


    private Vector3 actualProjectedDestination;




    public void Move(Vector3 groundNormalVector)
    {
        actualProjectedDestination = destination.position - Vector3.Dot(groundNormalVector, destination.position - spider.position) * groundNormalVector;

        if (DEBUG_projectedDestinationVisualMarker != null)
        {
            DEBUG_projectedDestinationVisualMarker.transform.position = actualProjectedDestination;
        }


        if (moveToDestination)
        {
            float distanceToArrival = Vector3.Distance(actualProjectedDestination, spider.position);

            if (distanceToArrival > arrivalDistanceMargin)
            {
                Vector3 movementDirection = (actualProjectedDestination - spider.position).normalized;
                Vector3 movement = movementDirection * travelSpeed * Time.deltaTime;
                spider.position += movement;
            }

            rotateTowardsDestination.Rotate(actualProjectedDestination);
        }
    }
}
