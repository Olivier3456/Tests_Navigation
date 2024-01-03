using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderTriggerZone : MonoBehaviour
{
    [SerializeField] private Spider spider;

    [SerializeField] private SphereCollider sphereCollider;

    [SerializeField] private LayerMask groundLayerMask;

    [SerializeField] private int maxColliders = 4;

    [SerializeField] private float maxIterationsForExtraChecks = 5f;
    [SerializeField] private float radiusFactorForExtraChecks = 2f;

    private bool firstCheck = true;

    public Vector3 GetClosestGroundPoint(out float distance)
    {
        Vector3 closestPointOfAllColliders = Vector3.zero;
        distance = Mathf.Infinity;


        if (firstCheck)
        {
            firstCheck = false;
            float initialSphereColliderRadius = sphereCollider.radius;
            int iteration = 0;

            while (closestPointOfAllColliders == Vector3.zero && iteration <= maxIterationsForExtraChecks)
            {
                iteration++;

                if (iteration > 1)
                {
                    sphereCollider.radius *= radiusFactorForExtraChecks;
                    Debug.Log($"No collider in range: extra check necessary. Iteration {iteration}. Sphere radius = {sphereCollider.radius}m.");
                }

                closestPointOfAllColliders = GetClosestGroundPoint_Iteration(out distance);
            }

            sphereCollider.radius = initialSphereColliderRadius;

            if (closestPointOfAllColliders == Vector3.zero)
            {
                Debug.Log("No ground surface detected by the new spawned spider!");
                //Destroy(spider.gameObject);
            }
        }
        else
        {
            closestPointOfAllColliders = GetClosestGroundPoint_Iteration(out distance);
        }

        return closestPointOfAllColliders;
    }


    private Vector3 GetClosestGroundPoint_Iteration(out float distance)
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

        return closestPointOfAllColliders;
    }


    /// <summary>
    /// ClosestPoint() doesn't work with non convex colliders, and will fail silently.
    /// This function finds the closest point of all types of colliders.
    /// (https://gamedev.stackexchange.com/questions/154676/finding-the-closest-point-on-a-concave-mesh)
    /// </summary>
    private static bool CheckSphereExtra(Collider target_collider, SphereCollider sphere_collider, out Vector3 closest_point, out Vector3 surface_normal)
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
