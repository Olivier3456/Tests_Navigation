using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Spider spider;
    [SerializeField] private float speedFactor;
   

    void Update()
    {
        animator.speed = spider.ActualSpeed * speedFactor;
    }
}
