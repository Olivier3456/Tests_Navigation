using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderEdgesDetection : MonoBehaviour
{
    [SerializeField] private GameObject visualClueForEdgeCollider;
    [SerializeField] private GameObject visualClueForSphereCheck;


    [SerializeField] private Material inColliderMaterial;
    [SerializeField] private Material notInColliderMaterial;


    private float gridPrecision = 0.1f;
    private float gridSize = 2f;
    private Vector3 gridOrigin = Vector3.zero;


    private class Cell
    {
        public Vector3 cellPosition;
        public bool isColliderEdge;
    }

    private Cell[,,] grid;
    private bool[,,] cellCornersInColliders;


    private void Awake()
    {
        int numberOfCellsForEachSide = (int)Mathf.Ceil(gridSize / gridPrecision);
        grid = new Cell[numberOfCellsForEachSide, numberOfCellsForEachSide, numberOfCellsForEachSide];

        int numberOfSpheresForEachSide = numberOfCellsForEachSide + 1;
        cellCornersInColliders = new bool[numberOfSpheresForEachSide, numberOfSpheresForEachSide, numberOfSpheresForEachSide];
    }


    private void Start()
    {
        CheckAllCorners();
        CheckAllCells();
    }

    private void CheckAllCells()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                for (int k = 0; k < grid.GetLength(2); k++)
                {
                    grid[i, j, k] = new Cell();

                    int cornersInColliders = 0;

                    if (cellCornersInColliders[i, j, k]) cornersInColliders++;
                    if (cellCornersInColliders[i + 1, j, k]) cornersInColliders++;
                    if (cellCornersInColliders[i + 1, j + 1, k]) cornersInColliders++;
                    if (cellCornersInColliders[i + 1, j + 1, k + 1]) cornersInColliders++;
                    if (cellCornersInColliders[i, j + 1, k + 1]) cornersInColliders++;
                    if (cellCornersInColliders[i, j, k + 1]) cornersInColliders++;
                    if (cellCornersInColliders[i, j + 1, k]) cornersInColliders++;
                    if (cellCornersInColliders[i + 1, j, k + 1]) cornersInColliders++;

                    if (cornersInColliders > 0 && cornersInColliders < 8)
                    {
                        // This cell is on the edge of collider(s).

                        grid[i, j, k].isColliderEdge = true;

                        float x = (gridOrigin.x + (i * gridPrecision));
                        float y = (gridOrigin.y + (j * gridPrecision));
                        float z = (gridOrigin.z + (k * gridPrecision));
                        Vector3 position = new Vector3(x, y, z);

                        GameObject cube = Instantiate(visualClueForEdgeCollider, position, Quaternion.identity);
                        float cubeScale = gridPrecision * 0.5f;
                        cube.transform.localScale = new Vector3(cubeScale, cubeScale, cubeScale);
                    }
                    else
                    {
                        grid[i, j, k].isColliderEdge = false;
                    }
                }
            }
        }
    }

    private void CheckAllCorners()
    {
        float offset = gridPrecision * 0.5f;

        for (int i = 0; i < cellCornersInColliders.GetLength(0); i++)
        {
            for (int j = 0; j < cellCornersInColliders.GetLength(1); j++)
            {
                for (int k = 0; k < cellCornersInColliders.GetLength(2); k++)
                {
                    float x = (gridOrigin.x + (i * gridPrecision)) - offset;
                    float y = (gridOrigin.y + (j * gridPrecision)) - offset;
                    float z = (gridOrigin.z + (k * gridPrecision)) - offset;
                    Vector3 position = new Vector3(x, y, z);


                    //Transform sphere = Instantiate(visualClueForSphereCheck, position, Quaternion.identity).transform;
                    //float sphereScale = gridPrecision * 0.1f;
                    //sphere.localScale = new Vector3(sphereScale, sphereScale, sphereScale);

                    if (CheckThisPositione(position))
                    {
                        //sphere.GetComponent<Renderer>().sharedMaterial = inColliderMaterial;
                        cellCornersInColliders[i, j, k] = true;
                    }
                    else
                    {
                        //sphere.GetComponent<Renderer>().sharedMaterial = notInColliderMaterial;
                        cellCornersInColliders[i, j, k] = false;
                    }
                }
            }
        }
    }

    private bool CheckThisPositione(Vector3 position)
    {
        float sphereRadius = gridPrecision * 0.1f;

        if (Physics.CheckSphere(position, sphereRadius))
        {
            return true;
        }

        return false;
    }
}
