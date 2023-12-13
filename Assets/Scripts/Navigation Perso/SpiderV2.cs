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
    private Vector3 directionToClosestGroundPoint;
    private Vector3 groundNormal;
    private Vector3 projectedDestination;

    private SphereCollider sphereCollider;

    private float actualTravelSpeed = 0;


    private void Awake()
    {
        // Reset spider main gameObject position and rotation.
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        sphereCollider = triggerTransform.GetComponent<SphereCollider>();
    }


    int frames = 0;
    void Update()
    {
        
        if (closestGroundPoint != Vector3.zero)
        {
            directionToClosestGroundPoint = (closestGroundPoint - triggerTransform.position).normalized;
            groundNormal = GetGroundNormalVector();

            StayGrounded();
            
            if (travelToDestination)
            {
                Travel();
                //Debug.Log($"Travel. Frame {frames}.");
            }

            RotateVisualTowardsDestination();

            PlaceVisualOnGround();
        }

        frames++;
       
        groundNormal = Vector3.zero;
        closestGroundPoint = Vector3.zero;
    }


    private void StayGrounded()
    {
        if (distanceToClosestGroundPoint > groundDistance + groundDistanceMargin)
        {
            triggerTransform.position += directionToClosestGroundPoint * groundingSpeed * Time.deltaTime;
        }
        else if (distanceToClosestGroundPoint < groundDistance)
        {
            triggerTransform.position -= directionToClosestGroundPoint * groundingSpeed * Time.deltaTime;
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
        }
    }



    private void Travel()
    {
        if (groundNormal == Vector3.zero)
        {
            actualTravelSpeed = 0;
            return;
        }

        projectedDestination = destination.position - Vector3.Dot(groundNormal, destination.position - triggerTransform.position) * groundNormal;

        float distanceToArrival = Vector3.Distance(projectedDestination, triggerTransform.position);

        Vector3 lastPosition = triggerTransform.position;

        if (distanceToArrival > arrivalDistanceMargin)
        {
            Vector3 movementDirection = (projectedDestination - triggerTransform.position).normalized;
            Vector3 movement = movementDirection * travelSpeed * Time.deltaTime;
            triggerTransform.position += movement;
        }

        Vector3 newPosition = triggerTransform.position;

        actualTravelSpeed = Vector3.Distance(lastPosition, newPosition) * Time.deltaTime;
    }


    private void RotateVisualTowardsDestination()
    {
        // Chat GPT 3.5:

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
        // For the Lerp speed to be more linear when the spider rotates on wall <--> ground angles.
        Vector3 actualPosition = visualTransform.position;
        Vector3 targetPosition = closestGroundPoint;
        float distance = Vector3.Distance(actualPosition, targetPosition);
        float lerpSpeed = travelSpeed / distance;

        visualTransform.position = Vector3.Lerp(visualTransform.position, closestGroundPoint, Time.deltaTime * lerpSpeed);
    }



    private Vector3 GetGroundNormalVector()
    {
        if (Physics.Raycast(triggerTransform.position, directionToClosestGroundPoint, out RaycastHit hit, 10f, groundLayerMask))
        {
            return hit.normal;
        }
        else
        {
            Debug.Log("Raycast haven't hit anything: groundNormalVector set to Vector3.zero");
            return Vector3.zero;
        }
    }

    public float GetActualTravelSpeed()
    {
        return actualTravelSpeed;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(triggerTransform.position, sphereCollider.radius);
            Gizmos.DrawSphere(projectedDestination, 0.1f);
        }
        else
        {
            visualTransform.position = triggerTransform.position;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(triggerTransform.position, groundDistance);
    }
#endif
}
