using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor.AI;
using UnityEngine;

public class DynamicNavMeshHandler : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;



    void Update()
    {
        navMeshSurface.BuildNavMesh();
    }
}
