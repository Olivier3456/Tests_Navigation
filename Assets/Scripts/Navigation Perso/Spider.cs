using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public interface ITravel
{
    public void Travel();
    public void RotateVisual();
}

public class RaycastDatas
{
    public Vector3 groundNormal;
    public Vector3 hitPoint;
}


public class Spider : MonoBehaviour
{
    public enum TravelType { ToDestination, JustWalk }

    [SerializeField] public Transform triggerTransform;
    [SerializeField] public Transform visualTransform;
    [SerializeField] public SphereCollider trigger;
    [Space(20)]
    [SerializeField] public Transform destination;
    [Space(20)]
    [SerializeField] public LayerMask groundLayerMask;
    [Space(20)]
    [SerializeField] public float groundDistance = 0.25f;
    [SerializeField] public float groundDistanceMargin = 0.025f;
    [SerializeField] public float rotationSpeed = 2f;
    [Space(20)]
    [SerializeField] public bool travel;
    [SerializeField] public TravelType travelType;
    [SerializeField] public float travelSpeed = 0.5f;
    [SerializeField] public float arrivalDistanceMargin = 0.1f;

    [HideInInspector] public float groundingSpeed = 0.5f;
    [HideInInspector] public Vector3 closestGroundPoint = Vector3.zero;
    [HideInInspector] public float distanceToClosestGroundPoint = 0;
    [HideInInspector] public Vector3 directionToClosestGroundPoint;
    [HideInInspector] public RaycastDatas raycastDatas;
    [HideInInspector] public float actualTravelSpeed = 0;
    [HideInInspector] public RaycastDatas lastRaycastDatas;
    [HideInInspector] public ITravel move;
    private Walk walk;
    private WalkToDestination walkToDestination;
    private bool initialPlacement = true;


    // ajout
    [Space(20)]
    [SerializeField] private bool limitWalkGroundAnglesDelta;
    [SerializeField] private float maxGroundAnglesDelta = 60;


    private void Awake()
    {
        float minFactor = 1.5f;
        if (groundingSpeed <= travelSpeed * minFactor)
        {
            groundingSpeed = travelSpeed * minFactor;
            Debug.Log($"Grounding speed must be higher than travel speed to prevent possible bugs when spider walks on certain ground angles. Grounding speed has been set to {groundingSpeed}.");
        }

        raycastDatas = new RaycastDatas() { groundNormal = Vector3.zero, hitPoint = Vector3.zero };
        lastRaycastDatas = new RaycastDatas() { groundNormal = Vector3.zero, hitPoint = Vector3.zero };


        walkToDestination = transform.AddComponent<WalkToDestination>();
        walkToDestination.spider = this;

        walk = transform.AddComponent<Walk>();
        walk.spider = this;

        if (travel)
        {
            initialPlacement = false;
        }
        else
        {
            initialPlacement = true;
        }
    }


    void Update()
    {
        SwitchMovementTypeIfNeeded();

        if (closestGroundPoint != Vector3.zero)
        {
            directionToClosestGroundPoint = (closestGroundPoint - triggerTransform.position).normalized;
            raycastDatas = UpdateRaycastDatas();

            StayGrounded();

            if (raycastDatas != null)
            {
                if (travel)
                {
                    move.Travel();
                }
                else
                {
                    actualTravelSpeed = 0;
                }

                move.RotateVisual();
            }
            else
            {
                actualTravelSpeed = 0;
            }


            if (travel || initialPlacement)
            {
                PlaceVisualOnGround();
            }
        }

        if (limitWalkGroundAnglesDelta)
        {
            if (IsGroundAngleDeltaTooHigh())
            {
                travel = false;
                if (move is Walk)
                {
                    (move as Walk).StopTurn();
                }
            }
        }



        // ====================== DEBUG ======================
        //if (!IsTurning() && travel)
        //{
        //    if (move is Walk)
        //    {
        //        float angle = Random.Range(-45, 45);
        //        float length = Random.Range(2, 5);

        //        Turn(angle, length);
        //    }
        //}
        // ===================================================



        closestGroundPoint = Vector3.zero;
    }


    private void SwitchMovementTypeIfNeeded()
    {
        switch (travelType)
        {
            case TravelType.JustWalk:
                move = walk;
                break;
            case TravelType.ToDestination:
                move = walkToDestination;
                break;
        }
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


    private void PlaceVisualOnGround()
    {
        // For the Lerp speed to be more linear when the spider rotates on wall <--> ground acute (= inner) angles.
        Vector3 actualPosition = visualTransform.position;
        Vector3 targetPosition = closestGroundPoint;
        float distance = Vector3.Distance(actualPosition, targetPosition);
        float lerpSpeed = travelSpeed / distance;

        visualTransform.position = Vector3.Lerp(actualPosition, targetPosition, Time.deltaTime * lerpSpeed);

        if (distance < 0.001f)
        {
            initialPlacement = false;
        }
    }


    private RaycastDatas UpdateRaycastDatas()
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


    public void Turn(float angle, float length)
    {
        if (move is Walk)
        {
            (move as Walk).Turn(angle, length);
        }
        else
        {
            Debug.Log("Spider is currently in Move To Destination Mode. You can't send it a walk command.");
        }
    }


    public bool IsTurning()
    {
        if (move is Walk)
        {
            return (move as Walk).IsTurning();
        }
        else
        {
            return false;
        }
    }


    private bool IsGroundAngleDeltaTooHigh()
    {
        float groundAngleDelta = Vector3.Angle(raycastDatas.groundNormal, lastRaycastDatas.groundNormal);

        if (groundAngleDelta > maxGroundAnglesDelta)
        {
            Debug.Log($"Ground angle delta ({groundAngleDelta}°) is wider than maximum allowed ({maxGroundAnglesDelta}°).");
        }

        return groundAngleDelta > maxGroundAnglesDelta;
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(triggerTransform.position, trigger.radius);

        if (EditorApplication.isPlaying)
        {
            if (move != null && move is WalkToDestination)
            {
                Gizmos.DrawSphere((move as WalkToDestination).projectedDestination, 0.1f);
            }
        }
        else
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            visualTransform.position = triggerTransform.position;
            triggerTransform.rotation = Quaternion.identity;
            destination.gameObject.SetActive(travelType == TravelType.ToDestination);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(triggerTransform.position, groundDistance);
    }
#endif
}
