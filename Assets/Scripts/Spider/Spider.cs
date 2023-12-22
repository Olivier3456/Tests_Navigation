using System.Collections;
using System.Collections.Generic;
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
    private float spiderSize = 0.05f;

    public enum TravelType { ToDestination, JustWalk }

    public Transform triggerTransform;
    public Transform visualTransform;
    [Tooltip("The scale of the model when it measures one meter.")]
    [SerializeField] private float modelScaleFactor;
    [SerializeField] private Transform modelTransform;
    public SphereCollider trigger;
    [Space(20)]
    public Transform destination;
    [Space(20)]
    [Tooltip("The scale factor of the sphere collider that detects ground colliders around the spider")]
    [Range(2, 10)][SerializeField] private float groundDetectionTriggerScaleFactor;
    public LayerMask groundLayerMask;
    [Space(20)]
    public float rotationSpeed = 2f;
    [Space(20)]
    public bool travel;
    public TravelType travelType;
    public float travelSpeed = 0.5f;


    [HideInInspector] public float arrivalDistanceMargin;
    [HideInInspector] public float groundDistance;
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
    private float groundingSpeed;
    private float groundDistanceMargin = 0.025f;


    [Space(20)]
    [SerializeField] private bool limitWalkGroundAnglesDelta;
    [SerializeField] private float maxGroundAnglesDelta = 60;


    private void Awake()
    {
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


    public void SetSpiderSize(float spiderSize)
    {
        travelSpeed *= spiderSize;
        groundDistance = spiderSize / 2;
        trigger.radius = groundDistance * groundDetectionTriggerScaleFactor;
        groundingSpeed = travelSpeed * 1.5f;
        groundDistanceMargin = spiderSize / 20f;
        arrivalDistanceMargin = spiderSize / 10f;

        float modelScale = modelScaleFactor * spiderSize;
        modelTransform.localScale = new Vector3(modelScale, modelScale, modelScale);
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
            if (GetGroundAnglesDelta() > maxGroundAnglesDelta)
            {
                Debug.Log($"Ground angle delta is wider than maximum allowed ({maxGroundAnglesDelta}°).");

                travel = false;
                if (move is Walk)
                {
                    (move as Walk).StopTurn();
                }
            }
        }



        // ====================== DEBUG ======================
        if (!IsTurning() && travel && raycastDatas != null && closestGroundPoint != Vector3.zero)
        {
            if (move is Walk)
            {
                float angle = Random.Range(-30, 30);
                float length = Random.Range(2, 5);

                Turn(angle, length);
            }
        }
        // ===================================================



        closestGroundPoint = Vector3.zero;
        directionToClosestGroundPoint = Vector3.zero;
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
            if (raycastDatas != null)
            {
                triggerTransform.position -= raycastDatas.groundNormal * groundingSpeed * Time.deltaTime;
            }
            else
            {
                triggerTransform.position += directionToClosestGroundPoint * groundingSpeed * Time.deltaTime;
            }
        }
        else if (distanceToClosestGroundPoint < groundDistance)
        {
            if (raycastDatas != null)
            {
                triggerTransform.position += raycastDatas.groundNormal * groundingSpeed * Time.deltaTime;
            }
            else
            {
                triggerTransform.position -= directionToClosestGroundPoint * groundingSpeed * Time.deltaTime;
            }
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


        float margin = spiderSize / 50;
        if (distance < margin)
        {
            initialPlacement = false;
        }
    }


    private RaycastDatas UpdateRaycastDatas()
    {
         float maxDistance = groundDistance * groundDetectionTriggerScaleFactor;
        if (Physics.Raycast(triggerTransform.position, directionToClosestGroundPoint, out RaycastHit hit, maxDistance, groundLayerMask))
        {
            lastRaycastDatas = raycastDatas;
            return new RaycastDatas() { groundNormal = hit.normal, hitPoint = hit.point };
        }
        else
        {
            lastRaycastDatas = raycastDatas;
            Debug.Log("Raycast haven't hit anything. Returning null.");
            return null;
        }
    }


    public float GetActualTravelSpeed()
    {
        return actualTravelSpeed;
    }


    public void ChangeTravelSpeed(float speed)
    {
        travelSpeed = speed * spiderSize;
        groundingSpeed = travelSpeed * 1.5f;
    }


    public void SetPosition(Vector3 position)
    {
        triggerTransform.position = position;
        visualTransform.position = position;

        //Debug.Log($"Spider's children objects position set to {position}");
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


    private float GetGroundAnglesDelta()
    {
        return Vector3.Angle(raycastDatas.groundNormal, lastRaycastDatas.groundNormal);
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
