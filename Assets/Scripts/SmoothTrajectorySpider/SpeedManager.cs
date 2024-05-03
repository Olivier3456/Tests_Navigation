using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public float speed = 1f;
    //public AnimationCurve changeSpeedCurve;

    [HideInInspector] public float currentSpeed = 1f;

    //private bool isChangingSpeed = false;
    //private bool isWaiting = false;
    //private bool isStopped = false;
    //private bool CanChangeSpeed { get { return !isChangingSpeed && !isWaiting; } }



    private void Start()
    {
        //StartCoroutine(StopSpiderRepetitively());
    }


    void Update()
    {
        //if (CanChangeSpeed)
        //{
        //    if (!isStopped)
        //    {
        //        int random = Random.Range(0, 4);

        //        if (random == 0)
        //        {
        //            float targetSpeed = 0;
        //            float transitionLength = Mathf.Abs(targetSpeed - currentSpeed);
        //            StartCoroutine(ChangeSpeedCoroutine(targetSpeed, transitionLength));
        //        }
        //        else
        //        {
        //            float waitTime = Random.Range(2, 5);
        //            StartCoroutine(SetConstantSpeedTimeCoroutine(waitTime));
        //        }
        //    }
        //    else
        //    {
        //        StartCoroutine(ChangeSpeedCoroutine(speed, speed));
        //    }
        //}        
    }



    private IEnumerator StopSpiderRepetitively()
    {
        while (true)
        {
            float stoppedTimer = 0;
            float maxStoppedTime = Random.Range(1, 5);

            currentSpeed = 0;

            while (stoppedTimer < maxStoppedTime)
            {
                yield return null;
                stoppedTimer += Time.deltaTime;
                Debug.Log($"stoppedTimer = {stoppedTimer}. MaxStoppedTimer = {maxStoppedTime}.");
            }

            currentSpeed = speed;

            float walkTimer = 0;
            float maxWalkTime = Random.Range(2, 10);

            while (walkTimer < maxWalkTime)
            {
                yield return null;
                walkTimer += Time.deltaTime;
                Debug.Log($"walkTimer = {walkTimer}. maxWalkTime = {maxWalkTime}.");
            }
        }
    }







    //private IEnumerator ChangeSpeedCoroutine(float targetSpeed, float length)
    //{
    //    isChangingSpeed = true;

    //    float originalSpeed = currentSpeed;
    //    float speedDifference = targetSpeed - currentSpeed;
    //    float status = 0;

    //    while (status < 1)
    //    {
    //        yield return null;
    //        status += Time.deltaTime / length;
    //        currentSpeed = originalSpeed + (changeSpeedCurve.Evaluate(status) * speedDifference);
    //    }

    //    currentSpeed = targetSpeed;

    //    if (currentSpeed == 0) isStopped = true;
    //    else isStopped = false;

    //    isChangingSpeed = false;

    //    float waitTime = Random.Range(1, 2f);
    //    StartCoroutine(SetConstantSpeedTimeCoroutine(waitTime));
    //}

    //private IEnumerator SetConstantSpeedTimeCoroutine(float length)
    //{
    //    isWaiting = true;
    //    yield return new WaitForSeconds(length);
    //    isWaiting = false;
    //}
}
