using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class SpiderTriggerZone : MonoBehaviour
{
    [SerializeField] private Spider spider;

    [SerializeField] private SphereCollider sphereCollider;

    [SerializeField] private LayerMask groundLayerMask;

    [SerializeField] private int maxColliders = 4;


    public Vector3 GetClosestGroundPoint(out float distance)
    {
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, sphereCollider.radius, hitColliders, groundLayerMask);


        List<Vector3> closestPoints = new List<Vector3>();


        for (int i = 0; i < numColliders; i++)
        {
            if (CheckSphereExtra(hitColliders[i], sphereCollider, out Vector3 closest_point, out Vector3 surface_normal))
            {
                closestPoints.Add(closest_point);
            }
        }


        Vector3 closestPointOfAllColliders = GetClosestPoint(closestPoints, out distance);

        //Debug.Log("closest point: " + closestPointOfAllColliders);

        return closestPointOfAllColliders;
    }


    /// <summary>
    /// ClosestPoint() doesn't work with non convex colliders, and will fail silently.
    /// Use this function instead to find the closest point of a non convex collider.
    /// (https://gamedev.stackexchange.com/questions/154676/finding-the-closest-point-on-a-concave-mesh)
    /// </summary>
    public static bool CheckSphereExtra(Collider target_collider, SphereCollider sphere_collider, out Vector3 closest_point, out Vector3 surface_normal)
    {
        closest_point = Vector3.zero;
        surface_normal = Vector3.zero;
        float surface_penetration_depth = 0;

        Vector3 sphere_pos = sphere_collider.transform.position;
        if (Physics.ComputePenetration(target_collider, target_collider.transform.position, target_collider.transform.rotation, sphere_collider, sphere_pos, Quaternion.identity, out surface_normal, out surface_penetration_depth))
        {
            closest_point = sphere_pos + (surface_normal * (sphere_collider.radius - surface_penetration_depth));

            surface_normal = -surface_normal;

            return true;
        }

        return false;
    }


    private Vector3 GetClosestPoint(List<Vector3> closestPoints, out float minDistance)
    {
        Vector3 closestPoint = Vector3.zero;
        minDistance = Mathf.Infinity;

        foreach (Vector3 point in closestPoints)
        {
            float distance = Vector3.Distance(point, transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = point;
            }
        }
        
        return closestPoint;
    }
}
