using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpiderV2 spider;
    [SerializeField] private float speedFactor;
   


    void Update()
    {
        animator.speed = spider.GetActualTravelSpeed() * speedFactor;
    }
}
