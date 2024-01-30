using UnityEngine;
using UnityEngine.UIElements;

public class LineRectangleIntersection : MonoBehaviour
{
    public Transform rectangleTransform; // Assign the transform of the rectangle
    public Vector2 rectangleSize; // Set the size of the rectangle

    public GameObject cornerVisualMarker1;
    public GameObject cornerVisualMarker2;
    public GameObject cornerVisualMarker3;
    public GameObject cornerVisualMarker4;


    private struct HeightData
    {
        public float length;
        public Vector3 direction;
    }


    void Update()
    {
        HeightData[] heightData = GetHeightData();

        foreach (var item in heightData)
        {
            Vector3 intersectionPosition = transform.position + item.direction * item.length;

            Debug.DrawLine(transform.position, intersectionPosition, Color.red);
        }
    }

    private HeightData[] GetHeightData()
    {
        Vector3[] corners = GetCornersPosition();

        return GetTrianglesHeight(corners);
    }

    private Vector3[] GetCornersPosition()
    {
        // Calculating the half extents of the rectangle
        float halfWidth = rectangleSize.x * 0.5f;
        float halfHeight = rectangleSize.y * 0.5f;

        // Get the corners of the rectangle
        Vector3[] corners = new Vector3[4];
        corners[0] = rectangleTransform.TransformPoint(new Vector3(-halfWidth, 0, -halfHeight));
        corners[1] = rectangleTransform.TransformPoint(new Vector3(halfWidth, 0, -halfHeight));
        corners[2] = rectangleTransform.TransformPoint(new Vector3(halfWidth, 0, halfHeight));
        corners[3] = rectangleTransform.TransformPoint(new Vector3(-halfWidth, 0, halfHeight));


        // DEBUG
        //cornerVisualMarker1.transform.position = corners[0];
        //cornerVisualMarker2.transform.position = corners[1];
        //cornerVisualMarker3.transform.position = corners[2];
        //cornerVisualMarker4.transform.position = corners[3];


        return corners;
    }

    

    private HeightData[] GetTrianglesHeight(Vector3[] corners)
    {
        HeightData[] heights = new HeightData[corners.Length];

        int j = 1;

        for (int i = 0; i < corners.Length; i++)
        {
            if (i == corners.Length - 1)
            {
                j = 0;
            }

            float edge1 = Vector3.Distance(transform.position, corners[i]);
            float edge2 = Vector3.Distance(transform.position, corners[j]);
            float edge3 = Vector3.Distance(corners[i], corners[j]); // The base of the triangle.
            float halfPerimeter = (edge1 + edge2 + edge3) * 0.5f;

            // Heron's formula:
            float area = Mathf.Sqrt(halfPerimeter * (halfPerimeter - edge1) * (halfPerimeter - edge2) * (halfPerimeter - edge3));

            // Height's formula:
            float height = (2 * area) / edge3;

            Vector3 direction;
            if (i > 0)
            {
                direction = (corners[i] - corners[i - 1]).normalized;
            }
            else
            {
                direction = (corners[i] - corners[corners.Length - 1]).normalized;
            }

            heights[i] = new HeightData() { length = height, direction = direction };

            j++;
        }

        return heights;
    }
}
