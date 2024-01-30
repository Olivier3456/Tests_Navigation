using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnSpider : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    [SerializeField] private InputActionReference spawnSpider;

    [SerializeField] private Transform destination;


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
            spider.SetSpiderValues(0.05f, transform.position, 150f, 1f);
        }

        if (go.TryGetComponent(out NavMesh_Spider navmesh_spider))
        {
            navmesh_spider.SetSpiderValues(transform.position, 0.05f, 1f, 0f);
            navmesh_spider.SetDestination(destination.position);
        }
    }
}
