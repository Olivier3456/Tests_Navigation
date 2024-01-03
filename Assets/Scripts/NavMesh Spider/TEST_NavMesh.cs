using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class TEST_NavMesh : MonoBehaviour
{
    public Transform destination;
    public NavMeshAgent agent;


    void Start()
    {
        agent.SetDestination(destination.position);
    }


    //void Update()
    //{
        
    //}
}
