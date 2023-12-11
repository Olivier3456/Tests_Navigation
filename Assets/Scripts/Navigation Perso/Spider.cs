using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : MonoBehaviour
{
    [SerializeField] private StickToGround stickToGround;
    [SerializeField] private Displacements displacements;


    private void FixedUpdate()
    {
        stickToGround.Proceed();

        displacements.Proceed(stickToGround.GetGroundNormalVector());
    }
}
