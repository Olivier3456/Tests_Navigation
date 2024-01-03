using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMesh_Spider : MonoBehaviour
{
    private float spiderSize = 0.05f;

    [Tooltip("The scale of the model when it measures one meter.")]
    [SerializeField] private float modelScaleFactor;
    [SerializeField] private Transform modelTransform;
    [SerializeField] private SphereCollider sphereCollider;

    private float travelSpeed = 0f;

    private NavMeshAgent agent;

    public void SetSpiderValues(Vector3 position, float size, float speed, float initialAngle = 0)
    {
        SetSpiderSize(size);
        SetPosition(position);
        SetTravelSpeed(speed);
        SetNavMeshAgentValues(size, initialAngle);
    }

    private void SetSpiderSize(float spiderSize)
    {
        float modelScale = modelScaleFactor * spiderSize;
        modelTransform.localScale = new Vector3(modelScale, modelScale, modelScale);
    }

    private void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetTravelSpeed(float speed)
    {
        travelSpeed = speed * spiderSize;
    }

    private void SetNavMeshAgentValues(float radius, float initialAngle)
    {
        agent = gameObject.AddComponent<NavMeshAgent>();
        agent.agentTypeID = 0;
        agent.radius = radius;
        agent.height = radius;
        agent.speed = travelSpeed;

        StartCoroutine(WaitForNextFrameAndRotate(initialAngle));
    }

    private IEnumerator WaitForNextFrameAndRotate(float initialAngle)
    {
        yield return null;

        if (initialAngle == 0)
        {
            initialAngle = UnityEngine.Random.Range(0, 360);
        }

        transform.Rotate(0, initialAngle, 0, Space.Self);
    }

    public void SetDestination(Vector3 destination)
    {
        if (agent == null)
        {
            Debug.Log("This spider does not have NavMesh Agent.");
            return;
        }

        agent.SetDestination(destination);
    }

    public float GetActualSpeed()
    {
        return agent.velocity.magnitude;
    }
}
