using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderTriggerZone : MonoBehaviour
{
    [SerializeField] Spider spider;

    private void OnTriggerEnter(Collider other)
    {

        spider.UpdateClosestGroundPoint(other.ClosestPoint(transform.position));

    }

    private void OnTriggerStay(Collider other)
    {

        spider.UpdateClosestGroundPoint(other.ClosestPoint(transform.position));

    }
}


