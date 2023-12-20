using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSpider : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);

            if (obj.TryGetComponent(out Spider spider))
            {
                spider.SetPosition(transform.position);
            }
        }
    }
}
