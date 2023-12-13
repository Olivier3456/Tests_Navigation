using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetApplicationTargetFramerate : MonoBehaviour
{
    [SerializeField] private int targetFramerate = 120;

    void Start()
    {
        Application.targetFrameRate = targetFramerate;
    }    
}
