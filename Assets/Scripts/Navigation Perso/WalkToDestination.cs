using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkToDestination : MonoBehaviour, ITravel
{
    private bool canChangeProjectedDestination = true;
    public Vector3 projectedDestination;

    public Spider spider;

    public void Travel()
    {
        if (canChangeProjectedDestination)
        {
            Vector3 fromGroundToRightPositionAboveTheGround = spider.raycastDatas.hitPoint + (spider.raycastDatas.groundNormal * spider.groundDistance);
            projectedDestination = spider.destination.position - Vector3.Dot(spider.raycastDatas.groundNormal, spider.destination.position - fromGroundToRightPositionAboveTheGround) * spider.raycastDatas.groundNormal;

            float angleBetweenGroundNormalAndLastGroundNormal = Vector3.Angle(spider.raycastDatas.groundNormal, spider.lastRaycastDatas.groundNormal);
            if (angleBetweenGroundNormalAndLastGroundNormal > 89f)
            {
                StartCoroutine(WaitAndAuthorizeNextProjectedDestinationChange(angleBetweenGroundNormalAndLastGroundNormal));
            }
        }


        float distanceToArrival = Vector3.Distance(projectedDestination, spider.triggerTransform.position);

        Vector3 lastPosition = spider.triggerTransform.position;

        if (distanceToArrival > spider.arrivalDistanceMargin)
        {
            Vector3 movementDirection = (projectedDestination - spider.triggerTransform.position).normalized;
            Vector3 movement = movementDirection * spider.travelSpeed * Time.deltaTime;
            spider.triggerTransform.position += movement;
        }

        Vector3 newPosition = spider.triggerTransform.position;

        spider.actualTravelSpeed = Vector3.Distance(lastPosition, newPosition) / Time.deltaTime;
    }


    public void RotateVisual()
    {
        // Chat GPT 3.5:

        Vector3 relativePosition = projectedDestination - spider.triggerTransform.position;
        Quaternion rotationToDestination = Quaternion.LookRotation(relativePosition, spider.visualTransform.up);

        // Aligner l'objet vers la destination
        spider.visualTransform.rotation = Quaternion.Slerp(spider.visualTransform.rotation, rotationToDestination, Time.deltaTime * spider.rotationSpeed * spider.actualTravelSpeed);

        // Ajuster la rotation pour tenir compte de l'inclinaison du sol
        Quaternion rotationToGround = Quaternion.FromToRotation(spider.visualTransform.up, spider.raycastDatas.groundNormal) * spider.visualTransform.rotation;

        // For the rotation to be at a more constant speed.
        //float angle = Quaternion.Angle(spider.visualTransform.rotation, rotationToGround);
        float slerpStatus = Time.deltaTime * spider.rotationSpeed * spider.actualTravelSpeed;
        //slerpStatus *= 1 / (angle / 360);

        spider.visualTransform.rotation = Quaternion.Slerp(spider.visualTransform.rotation, rotationToGround, slerpStatus);
    }


    private IEnumerator WaitAndAuthorizeNextProjectedDestinationChange(float angleBetweenGroundNormalAndLastGroundNormal)
    {
        canChangeProjectedDestination = false;

        float actualTravelSpeedClamped = Mathf.Clamp(spider.actualTravelSpeed, 0.1f, Mathf.Infinity);

        float waitLength = (angleBetweenGroundNormalAndLastGroundNormal / actualTravelSpeedClamped) * 0.01f;

        Debug.Log($"Waiting to authorize projected destination to change again. angleBetweenGroundNormalAndLastGroundNormal = {angleBetweenGroundNormalAndLastGroundNormal}. Wait length = {waitLength}.");

        yield return new WaitForSeconds(waitLength);

        Debug.Log($"DONE waiting to authorize projected destination to change again.");

        canChangeProjectedDestination = true;
    }
}
