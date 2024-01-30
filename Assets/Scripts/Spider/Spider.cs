using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;



public class Spider : MonoBehaviour
{
    [SerializeField] private SphereCollider trigger;
    [SerializeField] private Transform triggerTransform;
    [SerializeField] private Transform visualTransform;
    [Space(20)]
    [SerializeField] private SpiderTriggerZone spiderTriggerZone;
    [Space(20)]
    [Tooltip("The scale of the model when it measures one meter.")]
    [SerializeField] private float modelScaleFactor;
    [SerializeField] private Transform modelTransform;
    [Space(20)]
    [SerializeField] private LayerMask groundLayerMask;
    [Space(20)]
    [SerializeField] private float rotationSpeed = 2f;
    [Space(20)]
    [SerializeField] private float speed = 0.5f;
    [Space(20)]
    [SerializeField] private bool limitWalkGroundAnglesDelta;
    [SerializeField] private bool stopWhenAngleChangeIsDetected;
    //[SerializeField] private float maxGroundAnglesDelta = 60;


    private float spiderSize = 0.05f;
    private float groundDistance;
    private Vector3 closestGroundPoint = Vector3.zero;

    private float distanceToClosestGroundPoint = 0;
    private Vector3 directionToClosestGroundPoint;
    private float actualSpeed = 0;
    private Walk walk;

    private Vector3 lastVisualTransformPosition = Vector3.zero;

    private bool initialVisualTransformPlacement = true;
    private bool initialVisualTransformRotation = true;

    private float initialAngle;
    private float groundDetectionTriggerScaleFactor = 2f;

    private enum Direction { Left, Right, None }


    // Settings of the floor angle change detection:
    private float raycastHitDistanceExpectedOnFlatGround;
    private float forwardFactor = 1f;
    private float sideFactor = 0.5f;


    public Transform TriggerTransform { get { return triggerTransform; } }
    public Transform VisualTransform { get { return visualTransform; } }
    public float Speed { get { return speed; } }
    public float ActualSpeed { get { return actualSpeed; } set { actualSpeed = value; } }
    public Vector3 LastVisualTransformPosition { get { return lastVisualTransformPosition; } }


    public void SetSpiderValues(float size, Vector3 position, float angle, float speed)
    {
        initialAngle = angle;

        SetSize(size);
        SetPosition(position);
        SetSpeed(speed);

        SetRaycastHitDistanceExpectedOnFlatGround();
    }

    private void SetSize(float spiderSize)
    {
        this.spiderSize = spiderSize;
        groundDistance = spiderSize * 0.5f;
        trigger.radius = groundDistance * groundDetectionTriggerScaleFactor;

        float modelScale = modelScaleFactor * spiderSize;
        modelTransform.localScale = new Vector3(modelScale, modelScale, modelScale);
    }

    private void SetPosition(Vector3 position)
    {
        triggerTransform.position = position;
        visualTransform.position = position;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed * spiderSize;

        if (this.speed == 0)
        {
            walk.StopTurn();
        }
    }


    private void Awake()
    {
        walk = transform.AddComponent<Walk>();
        walk.spider = this;
    }


    private void Update()
    {
        closestGroundPoint = spiderTriggerZone.GetClosestGroundPoint(out distanceToClosestGroundPoint);

        if (closestGroundPoint != Vector3.zero)
        {
            directionToClosestGroundPoint = (closestGroundPoint - triggerTransform.position).normalized;

            TriggerTransformStayGrounded();

            if (speed != 0)
            {
                walk.Travel();
                RotateVisualTransform();
            }
            else
            {
                actualSpeed = 0;

                if (initialVisualTransformRotation)
                {
                    RotateVisualTransform();
                }
            }

            if (speed != 0 || initialVisualTransformPlacement)
            {
                lastVisualTransformPosition = visualTransform.position;
                PlaceVisualTransform();
            }






            if (limitWalkGroundAnglesDelta)
            {
                Direction change = DetectGroundAngleChange();

                if (change == Direction.Left)
                {
                    //Debug.Log("Ground Angle change detected on left.");
                    if (stopWhenAngleChangeIsDetected)
                    {
                        speed = 0;
                        walk.StopTurn();
                    }
                    else
                    {
                        float angle = 90;
                        float length = 0f;
                        Turn(angle, length);
                    }
                }
                else if (change == Direction.Right)
                {
                    //Debug.Log("Ground Angle change detected on right.");

                    if (stopWhenAngleChangeIsDetected)
                    {
                        speed = 0;
                        walk.StopTurn();
                    }
                    else
                    {
                        float angle = -90;
                        float length = 0f;
                        Turn(angle, length);
                    }
                }
            }
        }



        // ====================== DEBUG ======================
        //if (!IsTurning() && speed != 0 && closestGroundPoint != Vector3.zero)
        //{
        //    if (move is Walk)
        //    {
        //        float angle = Random.Range(-30, 30);
        //        float length = Random.Range(2, 5);

        //        Turn(angle, length);
        //    }

        //    SetSpeed(Random.Range(0.75f, 1.25f));
        //}
        // ===================================================



        closestGroundPoint = Vector3.zero;
        //lastDirectionToClosestGroundPoint = directionToClosestGroundPoint;
        directionToClosestGroundPoint = Vector3.zero;
    }


    private void TriggerTransformStayGrounded()
    {
        triggerTransform.position += directionToClosestGroundPoint * (distanceToClosestGroundPoint - groundDistance);
    }


    private void PlaceVisualTransform()
    {
        // For the Lerp speed to be more linear when the spider rotates on wall <--> ground acute (= inner) angles.
        Vector3 actualPosition = visualTransform.position;
        Vector3 targetPosition = closestGroundPoint;
        float distance = Vector3.Distance(actualPosition, targetPosition);
        float lerp = speed / distance;

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


    public void Turn(float angle, float length)
    {
        walk.Turn(angle, length);
    }


    public bool IsTurning()
    {
        return walk.IsTurning();
    }


    private Direction DetectGroundAngleChange()
    {
        Vector3 leftRaycastDirection = ((-visualTransform.right * sideFactor) + (visualTransform.forward * forwardFactor) + directionToClosestGroundPoint).normalized;
        Vector3 rightRaycastDirection = ((visualTransform.right * sideFactor) + (visualTransform.forward * forwardFactor) + directionToClosestGroundPoint).normalized;

        float margin = raycastHitDistanceExpectedOnFlatGround * 0.1f;

        //Debug.Log($"raycastHitDistanceOnFlatGround = {raycastHitDistanceExpectedOnFlatGround}");

        if (Physics.Raycast(TriggerTransform.position, leftRaycastDirection, out RaycastHit leftHit, groundLayerMask))
        {
            if (leftHit.distance > raycastHitDistanceExpectedOnFlatGround + margin || leftHit.distance < raycastHitDistanceExpectedOnFlatGround - margin)
            {
                //Debug.Log($"leftHit.distance = {leftHit.distance}");
                return Direction.Left;
            }
        }
        else
        {
            return Direction.Left;
        }

        if (Physics.Raycast(TriggerTransform.position, rightRaycastDirection, out RaycastHit rightHit, groundLayerMask))
        {
            if (rightHit.distance > raycastHitDistanceExpectedOnFlatGround + margin || rightHit.distance < raycastHitDistanceExpectedOnFlatGround - margin)
            {
                return Direction.Right;
            }
        }
        else
        {
            return Direction.Right;
        }

        return Direction.None;
    }


    private void SetRaycastHitDistanceExpectedOnFlatGround()
    {
        if (!stopWhenAngleChangeIsDetected)
        {
            forwardFactor += speed / spiderSize;
        }

        raycastHitDistanceExpectedOnFlatGround = ((Vector3.right * sideFactor * groundDistance) + (Vector3.forward * forwardFactor * groundDistance) + (Vector3.down * groundDistance)).magnitude;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(triggerTransform.position, trigger.radius);

        if (!EditorApplication.isPlaying)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            visualTransform.position = triggerTransform.position;
            triggerTransform.rotation = Quaternion.identity;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(triggerTransform.position, groundDistance);
    }
#endif
}
