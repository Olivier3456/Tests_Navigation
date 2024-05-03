using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class RandomCurveMovement : MonoBehaviour
{
    public Transform rectangleTransform;
    public Vector2 rectangleSize;

    public SpeedManager speedManager;
    private float Speed { get { return speedManager.currentSpeed; } }

    public float minDist = 1f;

    public bool debugSpheres;
    public Transform debugCurrentTargetPos;
    public Transform debugLastTargetPos;
    public Transform debugRealTargetPos;

    private Vector3 currentTarget;
    private Vector3 lastTarget;
    private Vector3 realTarget;
    private float realTargetLerp = 0f;
    private float targetsDistance = 1f;

    private float MaxX { get { return rectangleTransform.position.x + (rectangleSize.x * 0.5f); } }
    private float MinX { get { return rectangleTransform.position.x - (rectangleSize.x * 0.5f); } }
    private float MaxZ { get { return rectangleTransform.position.z + (rectangleSize.y * 0.5f); } }
    private float MinZ { get { return rectangleTransform.position.z - (rectangleSize.y * 0.5f); } }

    [HideInInspector] public float velocity;

    private Vector3 RandomPointOnRectangleSurface()
    {
        float randomX = Random.Range(MinX, MaxX);
        float randomZ = Random.Range(MinZ, MaxZ);

        return new Vector3(randomX, rectangleTransform.position.y, randomZ);
    }


    private void Start()
    {
        currentTarget = RandomPointOnRectangleSurface();
        lastTarget = currentTarget;

        if (debugSpheres) debugCurrentTargetPos.position = currentTarget;
    }

    private void Update()
    {
        realTargetLerp += (Time.deltaTime / (Speed * 0.75f)) / targetsDistance;
        realTarget = Vector3.Lerp(lastTarget, currentTarget, realTargetLerp);
        if (debugSpheres) debugRealTargetPos.position = realTarget;


        Vector3 movementDirection = (realTarget - transform.position).normalized;
        //transform.Translate(movementDirection * Time.deltaTime * currentSpeed);

        velocity = Time.deltaTime * Speed;
        transform.position += movementDirection * velocity;
        transform.LookAt(realTarget);

        float dist = Vector3.Distance(transform.position, currentTarget);
        if (dist < minDist)
        {
            // Time to change target.
            lastTarget = currentTarget;
            currentTarget = GetValidTargetPos();
            targetsDistance = Vector3.Distance(lastTarget, currentTarget);
            realTargetLerp = 0f;
            if (debugSpheres) debugCurrentTargetPos.position = currentTarget;
            if (debugSpheres) debugLastTargetPos.position = lastTarget;
        }
    }

    private Vector3 GetValidTargetPos()
    {
        bool shouldContinue = true;
        Vector3 newPos;
        int iteration = 0;
        int maxIterations = 100;

        do
        {
            iteration++;
            newPos = RandomPointOnRectangleSurface();
            if (Vector3.Distance(newPos, transform.position) > minDist * 2f)
            {
                // New position's distance to object is OK. Now we want to limit the angle from last trajectory at 90°:
                Vector3 directionToCurrentTarget = (currentTarget - transform.position).normalized;
                Vector3 directionToNextPossibleTarget = (newPos - transform.position).normalized;

                if (Vector3.Dot(directionToCurrentTarget, directionToNextPossibleTarget) > 0)
                {
                    shouldContinue = false;
                }
            }

        } while (shouldContinue && iteration < maxIterations);

        Debug.Log($"Valid target found at iteration {iteration}.");

        return newPos;
    }
}
