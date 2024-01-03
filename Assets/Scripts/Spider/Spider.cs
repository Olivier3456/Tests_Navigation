using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public interface ITravel
{
    public void Travel();
}


public class Spider : MonoBehaviour
{
    private float spiderSize = 0.05f;

    public enum TravelType { ToDestination, JustWalk }

    public Transform triggerTransform;
    public Transform visualTransform;
    [Space(20)]
    [SerializeField] private SpiderTriggerZone spiderTriggerZone;
    [Space(20)]
    [Tooltip("The scale of the model when it measures one meter.")]
    [SerializeField] private float modelScaleFactor;
    [SerializeField] private Transform modelTransform;
    public SphereCollider trigger;
    [Space(20)]
    public Transform destination;
    [Space(20)]
    public LayerMask groundLayerMask;
    [Space(20)]
    public float rotationSpeed = 2f;
    [Space(20)]
    public TravelType travelType;
    public float travelSpeed = 0.5f;


    [HideInInspector] public float arrivalDistanceMargin;
    [HideInInspector] public float groundDistance;
    [HideInInspector] public Vector3 closestGroundPoint = Vector3.zero;

    [HideInInspector] public float distanceToClosestGroundPoint = 0;
    [HideInInspector] public Vector3 directionToClosestGroundPoint;
    [HideInInspector] public Vector3 lastDirectionToClosestGroundPoint;
    [HideInInspector] public float actualTravelSpeed = 0;
    [HideInInspector] public ITravel move;
    private Walk walk;
    private WalkToDestination walkToDestination;

    private bool initialVisualTransformPlacement = true;
    private bool initialVisualTransformRotation = true;
    private bool initialTriggerTransformPlacement = true;

    private float groundingSpeed;
    private float groundDistanceMargin = 0.025f;
    private float initialAngle;
    private float groundDetectionTriggerScaleFactor = 2f;


    [Space(20)]
    [SerializeField] private bool limitWalkGroundAnglesDelta;
    [SerializeField] private float maxGroundAnglesDelta = 60;


    public void SetSpiderValues(float size, Vector3 position, float angle, float speed)
    {
        initialAngle = angle;

        SetSpiderSize(size);
        SetPosition(position);
        SetTravelSpeed(speed);
    }

    private void SetSpiderSize(float spiderSize)
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

    private void SetPosition(Vector3 position)
    {
        triggerTransform.position = position;
        visualTransform.position = position;
    }

    public void SetTravelSpeed(float speed)
    {
        travelSpeed = speed * spiderSize;
        groundingSpeed = travelSpeed * 1.5f;

        if (travelSpeed == 0)
        {
            if (move is Walk)
            {
                (move as Walk).StopTurn();
            }
        }
    }


    private void Awake()
    {
        walkToDestination = transform.AddComponent<WalkToDestination>();
        walkToDestination.spider = this;

        walk = transform.AddComponent<Walk>();
        walk.spider = this;
    }


    private void Update()
    {
        SwitchMovementTypeIfNeeded();

        closestGroundPoint = spiderTriggerZone.GetClosestGroundPoint(out distanceToClosestGroundPoint);

        if (closestGroundPoint != Vector3.zero)
        {
            directionToClosestGroundPoint = (closestGroundPoint - triggerTransform.position).normalized;

            TriggerTransformStayGrounded();

            if (travelSpeed != 0)
            {
                move.Travel();
                RotateVisualTransform();
            }
            else
            {
                actualTravelSpeed = 0;

                if (initialVisualTransformRotation)
                {
                    RotateVisualTransform();
                }
            }

            if (travelSpeed != 0 || initialVisualTransformPlacement)
            {
                PlaceVisualTransformOnGround();
            }

            if (limitWalkGroundAnglesDelta)
            {
                if (GetGroundAnglesDelta() > maxGroundAnglesDelta)
                {
                    Debug.Log($"Ground angle delta is wider than maximum allowed ({maxGroundAnglesDelta}°).");

                    travelSpeed = 0;

                    if (move is Walk)
                    {
                        (move as Walk).StopTurn();
                    }
                }
            }
        }



        // ====================== DEBUG ======================
        //if (!IsTurning() && travelSpeed != 0 && closestGroundPoint != Vector3.zero)
        //{
        //    if (move is Walk)
        //    {
        //        float angle = Random.Range(-30, 30);
        //        float length = Random.Range(2, 5);

        //        Turn(angle, length);
        //    }

        //    SetTravelSpeed(Random.Range(0.75f, 1.25f));
        //}
        // ===================================================



        closestGroundPoint = Vector3.zero;
        lastDirectionToClosestGroundPoint = directionToClosestGroundPoint;
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


    private void TriggerTransformStayGrounded()
    {
        if (initialTriggerTransformPlacement)
        {
            triggerTransform.position += directionToClosestGroundPoint * (distanceToClosestGroundPoint - groundDistance);
            initialTriggerTransformPlacement = false;

            Debug.Log("Initial trigger transform placement done.");

            return;
        }

        if (distanceToClosestGroundPoint > groundDistance + groundDistanceMargin)
        {
            triggerTransform.position += directionToClosestGroundPoint * groundingSpeed * Time.deltaTime;
        }
        else if (distanceToClosestGroundPoint < groundDistance)
        {
            triggerTransform.position -= directionToClosestGroundPoint * groundingSpeed * Time.deltaTime;
        }
    }


    private void PlaceVisualTransformOnGround()
    {
        // For the Lerp speed to be more linear when the spider rotates on wall <--> ground acute (= inner) angles.
        Vector3 actualPosition = visualTransform.position;
        Vector3 targetPosition = closestGroundPoint;
        float distance = Vector3.Distance(actualPosition, targetPosition);
        float lerp = travelSpeed / distance;

        if (initialVisualTransformPlacement)
        {
            visualTransform.position = targetPosition;
            initialVisualTransformPlacement = false;
        }
        else
        {
            visualTransform.position = Vector3.Lerp(actualPosition, targetPosition, Time.deltaTime * lerp);
        }
    }


    public void RotateVisualTransform()
    {
        // Ajuster la rotation pour tenir compte de l'inclinaison du sol.
        Quaternion rotationToGround = Quaternion.FromToRotation(visualTransform.up, -directionToClosestGroundPoint) * visualTransform.rotation;

        // For the rotation to be at a more constant speed.
        //float angle = Quaternion.Angle(spider.visualTransform.rotation, rotationToGround);
        float slerp = Time.deltaTime * rotationSpeed;
        //slerp *= 1 / (angle / 360);


        if (initialVisualTransformRotation)
        {
            visualTransform.rotation = rotationToGround;
            visualTransform.rotation *= Quaternion.AngleAxis(initialAngle, Vector3.up);

            initialVisualTransformRotation = false;
        }
        else
        {
            visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, rotationToGround, slerp);
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


    private float GetGroundAnglesDelta()
    {
        return Vector3.Angle(directionToClosestGroundPoint, lastDirectionToClosestGroundPoint);
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
