using UnityEngine;
using UnityEngine.UIElements;

public struct HeightData
{
    public float length;
    public Vector3 direction;
    public Vector3 intersection;
}

public class LineRectangleIntersection : MonoBehaviour
{
    public Transform rectangleTransform;
    public Vector2 rectangleSize;

    private HeightData[] heightData = null;
    public HeightData[] HeightDatas { get { return heightData; } }


    void Update()
    {
        heightData = GetHeightData();
    }


    private HeightData[] GetHeightData()
    {
        Vector3[] corners = GetCornersPosition();

        HeightData[] result = GetTrianglesHeight(corners);

        GetIntersection(ref result);

        return result;
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


    private void GetIntersection(ref HeightData[] heightData)
    {
        for (int i = 0; i < heightData.Length; i++)
        {
            heightData[i].intersection = transform.position + heightData[i].direction * heightData[i].length;
            Debug.DrawLine(transform.position, heightData[i].intersection, Color.red);
        }
    }




    //private HeightData GetLongestHeight(HeightData[] heightData)
    //{
    //    HeightData longestHeight = new HeightData();
    //    float maxDistance = 0;
    //    foreach (var item in heightData)
    //    {
    //        if (item.length > maxDistance)
    //        {
    //            maxDistance = item.length;
    //            longestHeight = item;
    //        }
    //    }

    //    return longestHeight;
    //}
}
