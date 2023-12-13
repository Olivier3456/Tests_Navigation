using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerZoneSpiderV2 : MonoBehaviour
{
    [SerializeField] SpiderV2 spider;

    private void OnTriggerEnter(Collider other)
    {

        spider.UpdateClosestGroundPoint(other.ClosestPoint(transform.position));

    }

    private void OnTriggerStay(Collider other)
    {

        spider.UpdateClosestGroundPoint(other.ClosestPoint(transform.position));

    }
}


