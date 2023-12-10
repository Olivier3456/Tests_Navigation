using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnGround : MonoBehaviour
{
    [SerializeField] private Transform character;
    [SerializeField] private Transform destination;
    [SerializeField] private float travelSpeed = 0.1f;
    [SerializeField] private bool moveToDestination = false;
    [SerializeField] private GameObject DEBUG_projectedDestinationVisualMarker;


    private Vector3 actualProjectedDestination;

    public void Move(Vector3 groundNormalVector)
    {
        actualProjectedDestination = destination.position - Vector3.Dot(groundNormalVector, destination.position - character.position) * groundNormalVector;

        if (DEBUG_projectedDestinationVisualMarker != null)
        {
            DEBUG_projectedDestinationVisualMarker.transform.position = actualProjectedDestination;
        }

        if (moveToDestination)
        {
            character.position = Vector3.Lerp(transform.position, actualProjectedDestination, travelSpeed * Time.deltaTime);
        }
    }
}
