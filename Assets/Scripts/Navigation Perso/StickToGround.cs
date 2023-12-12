using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.TextCore.Text;


public struct GroundDatas
{
    public GameObject actualGroundObject;
    public Vector3 actualGroundNormal;
    public float actualGroundDistance;
    public Vector3 hitPoint;
}


public class StickToGround : MonoBehaviour
{
    [SerializeField] private Displacements displacements;
    [SerializeField] private Transform rotationTransform;
    [Space(10)]
    [SerializeField] private LayerMask groundLayerMask;
    [Space(10)]
    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private float groundDistanceMargin = 0.1f;
    [SerializeField] private float groundingSpeed = 0.5f;

    public GroundDatas groundDatas;


    public void Proceed()
    {
        UpdateGroundDatas();

        if (groundDatas.actualGroundObject != null)
        {
            PlaceOnGround();
        }
    }


    private void UpdateGroundDatas()
    {
        RaycastHit hit;
        Vector3 raycastDirection = -rotationTransform.up;
        Vector3 raycastOriginPosition = transform.position;
        float maxDistance = 10f;

        if (Physics.Raycast(raycastOriginPosition, raycastDirection, out hit, maxDistance, groundLayerMask))
        {
            groundDatas.actualGroundObject = hit.transform.gameObject;
            groundDatas.actualGroundNormal = hit.normal;
            groundDatas.actualGroundDistance = hit.distance;
            groundDatas.hitPoint = hit.point;
        }
        else
        {
            groundDatas.actualGroundObject = null;
            groundDatas.actualGroundNormal = Vector3.zero;
            groundDatas.actualGroundDistance = 0;
            groundDatas.hitPoint = Vector3.zero;

            Debug.Log("No ground object detected by the raycast!");
        }
    }

    public void PlaceOnGround()
    {
        if (groundDatas.actualGroundDistance > groundDistance + groundDistanceMargin)
        {
            transform.position -= groundDatas.actualGroundNormal * groundingSpeed * displacements.GetTravelSpeed() * Time.deltaTime;
        }
        else if (groundDatas.actualGroundDistance < groundDistance)
        {
            transform.position += groundDatas.actualGroundNormal * groundingSpeed * displacements.GetTravelSpeed() * Time.deltaTime;
        }
    }



    public GroundDatas GetGroundDatas()
    {
        return groundDatas;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (groundDatas.hitPoint != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rotationTransform.position, groundDatas.hitPoint);
        }
    }
#endif
}
