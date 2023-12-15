using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour, ITravel
{
    public Spider spider;

    public void Travel()
    {
        Vector3 lastPosition = spider.triggerTransform.position;

        Vector3 movementDirection = spider.visualTransform.forward;
        Vector3 movement = movementDirection * spider.travelSpeed * Time.deltaTime;
        spider.triggerTransform.position += movement;

        Vector3 newPosition = spider.triggerTransform.position;

        spider.actualTravelSpeed = Vector3.Distance(lastPosition, newPosition) / Time.deltaTime;
    }

    public void RotateVisual()
    {
        // Ajuster la rotation pour tenir compte de l'inclinaison du sol
        Quaternion rotationToGround = Quaternion.FromToRotation(spider.visualTransform.up, spider.raycastDatas.groundNormal) * spider.visualTransform.rotation;

        // For the rotation to be at a more constant speed.
        //float angle = Quaternion.Angle(spider.visualTransform.rotation, rotationToGround);
        float slerpStatus = Time.deltaTime * spider.rotationSpeed * spider.actualTravelSpeed;
        //slerpStatus *= 1 / (angle / 360);

        spider.visualTransform.rotation = Quaternion.Slerp(spider.visualTransform.rotation, rotationToGround, slerpStatus);
    }
}
