using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : MonoBehaviour
{
    [SerializeField] private StickToGround stickToGround;
    [SerializeField] private Travel travel;


    private void FixedUpdate()
    {
        stickToGround.Proceed();

        travel.Proceed(stickToGround.GetGroundNormalVector());
    }
}
