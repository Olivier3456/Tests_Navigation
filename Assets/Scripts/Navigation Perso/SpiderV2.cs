using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SpiderV2 : MonoBehaviour
{
    [SerializeField] private Transform triggerTransform;
    [SerializeField] private Transform visualTransform;
    [Space(20)]
    [SerializeField] private Transform destination;
    [SerializeField] private Transform DEBUG_projectedDestinationVisualMarker;
    [Space(20)]
    [SerializeField] private LayerMask groundLayerMask;
    [Space(20)]
    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private float groundDistanceMargin = 0.1f;
    [SerializeField] private float groundingSpeed = 0.5f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float rotationTimeStep = 0.5f;
    [Space(20)]
    [SerializeField] private float arrivalDistanceMargin = 0.1f;
    [Space(20)]
    [SerializeField] private bool travelToDestination;
    [SerializeField] private float travelSpeed = 1;

    private Vector3 closestGroundPoint = Vector3.zero;
    private float distanceToClosestGroundPoint = 0;
    private Vector3 directionToGroundClosestPoint;
    private Vector3 groundNormal;
    private Vector3 projectedDestination;

    private SphereCollider sphereCollider;


    private void Awake()
    {
        // Reset spider main gameObject position and rotation.
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        sphereCollider = triggerTransform.GetComponent<SphereCollider>();
    }


    void Update()
    {
        if (closestGroundPoint != Vector3.zero)
        {
            directionToGroundClosestPoint = (closestGroundPoint - triggerTransform.position).normalized;
            groundNormal = GetGroundNormalVector();
            StayGrounded();
            RotateVisualTowardsDestination();
            PlaceVisualOnGround();

            if (travelToDestination)
            {
                Travel();
            }
        }

        groundNormal = Vector3.zero;
        closestGroundPoint = Vector3.zero;
    }


    private void StayGrounded()
    {
        if (distanceToClosestGroundPoint > groundDistance + groundDistanceMargin)
        {
            triggerTransform.position += directionToGroundClosestPoint * groundingSpeed * Time.deltaTime;
        }
        else if (distanceToClosestGroundPoint < groundDistance)
        {
            triggerTransform.position -= directionToGroundClosestPoint * groundingSpeed * Time.deltaTime;
        }
    }


    public void UpdateClosestGroundPoint(Vector3 closestPointOfCollider)
    {
        distanceToClosestGroundPoint = Vector3.Distance(closestGroundPoint, triggerTransform.position);

        float distanceToColliderClosestPoint = Vector3.Distance(closestPointOfCollider, triggerTransform.position);

        if (distanceToColliderClosestPoint < distanceToClosestGroundPoint)
        {
            closestGroundPoint = closestPointOfCollider;
            distanceToClosestGroundPoint = distanceToColliderClosestPoint;

            //Debug.Log("New valid closest ground point detected.");
        }
    }



    private void Travel()
    {
        if (groundNormal == Vector3.zero)
        {
            return;
        }


        projectedDestination = destination.position - Vector3.Dot(groundNormal, destination.position - triggerTransform.position) * groundNormal;

        if (DEBUG_projectedDestinationVisualMarker != null)
        {
            DEBUG_projectedDestinationVisualMarker.transform.position = projectedDestination;
        }

        float distanceToArrival = Vector3.Distance(projectedDestination, triggerTransform.position);

        if (distanceToArrival > arrivalDistanceMargin)
        {
            Vector3 movementDirection = (projectedDestination - triggerTransform.position).normalized;
            Vector3 movement = movementDirection * travelSpeed * Time.deltaTime;
            triggerTransform.position += movement;
        }
    }


    private void RotateVisualTowardsDestination()
    {
        // Chat GPT 3.5:

        float rotationSpeed = 10;

        Vector3 relativePosition = projectedDestination - triggerTransform.position;
        Quaternion rotationToDestination = Quaternion.LookRotation(relativePosition, visualTransform.up);

        // Aligner l'objet vers la destination
        visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, rotationToDestination, Time.deltaTime * rotationSpeed);

        // Ajuster la rotation pour tenir compte de l'inclinaison du sol
        Quaternion rotationToGround = Quaternion.FromToRotation(visualTransform.up, groundNormal) * visualTransform.rotation;
        visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, rotationToGround, Time.deltaTime * rotationSpeed);
    }

    private void PlaceVisualOnGround()
    {
        visualTransform.position = closestGroundPoint;
    }



    private Vector3 GetGroundNormalVector()
    {
        if (Physics.Raycast(triggerTransform.position, directionToGroundClosestPoint, out RaycastHit hit, 10f, groundLayerMask))
        {
            return hit.normal;
        }
        else
        {
            Debug.Log("Raycast haven't hit anything: groundNormalVector set to Vector3.zero");
            return Vector3.zero;
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(triggerTransform.position, sphereCollider.radius);
        }
    }
#endif
}
