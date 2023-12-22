using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnSpider : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    [SerializeField] private InputActionReference spawnSpider;


    private void OnEnable()
    {
        spawnSpider.action.performed += SpawnASpider;
        spawnSpider.action.Enable();
    }

    private void SpawnASpider(InputAction.CallbackContext obj)
    {
        GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        if (go.TryGetComponent(out Spider spider))
        {
            spider.SetSpiderSize(0.05f);
            spider.SetPosition(transform.position);
        }
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))

    //    {
    //        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);

    //        if (obj.TryGetComponent(out Spider spider))
    //        {
    //            spider.SetSpiderSize(0.05f);
    //            spider.SetPosition(transform.position);
    //        }
    //    }
    //}
}
