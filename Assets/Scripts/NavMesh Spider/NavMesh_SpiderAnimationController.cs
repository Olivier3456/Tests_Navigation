using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMesh_SpiderAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NavMesh_Spider spider;
    [SerializeField] private float speedFactor;
   

    void Update()
    {
        animator.speed = spider.GetActualSpeed() * speedFactor;
    }
}
