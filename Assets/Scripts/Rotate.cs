using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float speed;

    void Update()
    {
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + new Vector3(0, Time.deltaTime * speed, 0));
    }
}
