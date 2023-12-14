using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SpiderV2 : MonoBehaviour
{
    private class RaycastDatas
    {
        public Vector3 groundNormal;
        public Vector3 hitPoint;
    }

    [SerializeField] private Transform triggerTransform;
    [SerializeField] private Transform visualTransform;
    [Space(20)]
    [SerializeField] private Transform destination;
    [Space(20)]
    [SerializeField] private LayerMask groundLayerMask;
    [Space(20)]
    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private float groundDistanceMargin = 0.1f;
    [Tooltip("Grounding speed must be higher than travel speed to prevent possible bugs when spider walks on certain ground angles.")]
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
    private RaycastDatas raycastDatas;
    private Vector3 projectedDestination;

    private SphereCollider sphereCollider;

    private float actualTravelSpeed = 0;

    //private Vector3 lastProjectedDestination;
    //private float remainingAngleToFinishRotationToGround = 0;
    private RaycastDatas lastRaycastDatas;

    private void Awake()
    {
        // Reset spider main gameObject position and rotation.
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        sphereCollider = triggerTransform.GetComponent<SphereCollider>();

        float minFactor = 1.5f;
        if (groundingSpeed <= travelSpeed * minFactor)
        {
            groundingSpeed = travelSpeed * minFactor;
            Debug.Log($"Grounding speed must be higher than travel speed to prevent possible bugs when spider walks on certain ground angles. Grounding speed has been set to {groundingSpeed}.");
        }

        raycastDatas = new RaycastDatas() { groundNormal = Vector3.zero, hitPoint = Vector3.zero };
        lastRaycastDatas = new RaycastDatas() { groundNormal = Vector3.zero, hitPoint = Vector3.zero };
    }


    int frames = 0;
    void Update()
    {
        if (closestGroundPoint != Vector3.zero)
        {
            directionToClosestGroundPoint = (closestGroundPoint - triggerTransform.position).normalized;
            raycastDatas = GetRayastDatas();

            StayGrounded();

            if (raycastDatas != null)
            {
                if (travelToDestination)
                {
                    Travel();
                    //Debug.Log($"Travel. Frame {frames}.");
                }

                RotateVisualTowardsDestination();
            }
            else
            {
                travelSpeed = 0;
            }

            PlaceVisualOnGround();
        }

        frames++;

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


    private bool canChangeProjectedDestination = true;
    private IEnumerator WaitAndAuthorizeNextProjectedDestinationChange()
    {
        canChangeProjectedDestination = false;

        float actualTravelSpeedClamped = Mathf.Clamp(actualTravelSpeed, 0.1f, Mathf.Infinity);
        float angleBetweenGroundNormalAndLastGroundNormal = Vector3.Angle(raycastDatas.groundNormal, lastRaycastDatas.groundNormal);
        float waitLength = (angleBetweenGroundNormalAndLastGroundNormal / actualTravelSpeedClamped) * 0.01f;

        Debug.Log($"Waiting to authorize projected destination to change again. angleBetweenGroundNormalAndLastGroundNormal = {angleBetweenGroundNormalAndLastGroundNormal}. Wait length = {waitLength}.");

        yield return new WaitForSeconds(waitLength);

        Debug.Log($"DONE waiting to authorize projected destination to change again.");

        canChangeProjectedDestination = true;
    }


    private void Travel()
    {
        if (canChangeProjectedDestination)
        {
            Vector3 fromGroundToRightPositionAboveTheGround = raycastDatas.hitPoint + (raycastDatas.groundNormal * groundDistance);
            projectedDestination = destination.position - Vector3.Dot(raycastDatas.groundNormal, destination.position - fromGroundToRightPositionAboveTheGround) * raycastDatas.groundNormal;

            if (raycastDatas.groundNormal != lastRaycastDatas.groundNormal)
            {
                StartCoroutine(WaitAndAuthorizeNextProjectedDestinationChange());
            }
        }


        float distanceToArrival = Vector3.Distance(projectedDestination, triggerTransform.position);

        Vector3 lastPosition = triggerTransform.position;

        if (distanceToArrival > arrivalDistanceMargin)
        {
            Vector3 movementDirection = (projectedDestination - triggerTransform.position).normalized;
            Vector3 movement = movementDirection * travelSpeed * Time.deltaTime;
            triggerTransform.position += movement;
        }

        Vector3 newPosition = triggerTransform.position;

        actualTravelSpeed = Vector3.Distance(lastPosition, newPosition) / Time.deltaTime;
    }


    private void RotateVisualTowardsDestination()
    {
        // Chat GPT 3.5:

        Vector3 relativePosition = projectedDestination - triggerTransform.position;
        Quaternion rotationToDestination = Quaternion.LookRotation(relativePosition, visualTransform.up);

        // Aligner l'objet vers la destination
        visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, rotationToDestination, Time.deltaTime * rotationSpeed * actualTravelSpeed);

        // Ajuster la rotation pour tenir compte de l'inclinaison du sol
        Quaternion rotationToGround = Quaternion.FromToRotation(visualTransform.up, raycastDatas.groundNormal) * visualTransform.rotation;
        visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, rotationToGround, Time.deltaTime * rotationSpeed * actualTravelSpeed);

        //remainingAngleToFinishRotationToGround = Quaternion.Angle(visualTransform.rotation, rotationToGround);
        //Debug.Log($"remainingAngleToFinishRotationToGround = {remainingAngleToFinishRotationToGround}.");
    }

    private void PlaceVisualOnGround()
    {
        // For the Lerp speed to be more linear when the spider rotates on wall <--> ground acute (= inner) angles.
        Vector3 actualPosition = visualTransform.position;
        Vector3 targetPosition = closestGroundPoint;
        float distance = Vector3.Distance(actualPosition, targetPosition);
        float lerpSpeed = travelSpeed / distance;

        visualTransform.position = Vector3.Lerp(visualTransform.position, closestGroundPoint, Time.deltaTime * lerpSpeed);
    }



    private RaycastDatas GetRayastDatas()
    {
        float maxDistance = 10f;
        if (Physics.Raycast(triggerTransform.position, directionToClosestGroundPoint, out RaycastHit hit, maxDistance, groundLayerMask))
        {
            lastRaycastDatas = raycastDatas;
            return new RaycastDatas() { groundNormal = hit.normal, hitPoint = hit.point };
        }
        else
        {
            lastRaycastDatas = raycastDatas;
            Debug.Log("Raycast haven't hit anything. Return null.");
            return null;
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
