using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnGround : MonoBehaviour
{
    [SerializeField] private Transform destination;
    [SerializeField] private float travelSpeed = 0.1f;
    [SerializeField] private bool moveToDestination = false;


    private Vector3 actualProjectedDestination;


    [SerializeField] private GameObject DEBUG_projectedDestinationVisualMarker;

    public void Move(Vector3 vector_To_Ground_Object_Closest_Point, Vector3 direction_Vector_To_Ground_Object_Closest_Point)
    {
        actualProjectedDestination = Vector3.ProjectOnPlane(destination.position, -direction_Vector_To_Ground_Object_Closest_Point); // - vector_To_Ground_Object_Closest_Point;

        DEBUG_projectedDestinationVisualMarker.transform.position = actualProjectedDestination;


        if (moveToDestination)
        {
            transform.position = Vector3.Lerp(transform.position, actualProjectedDestination, travelSpeed * Time.deltaTime);
        }
    }
}
