using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimatorController : MonoBehaviour
{
    public Animator animator;
    public RandomCurveMovement rcm;
    public float speedFactor = 1.0f;
    


    void Update()
    {
        animator.speed = rcm.velocity * speedFactor;
    }
}
