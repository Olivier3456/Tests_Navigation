using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsDestination : MonoBehaviour
{

    public void Rotate(Vector3 actualDestination)
    {
        Vector3 directionToDestination = actualDestination - transform.position;
        directionToDestination.y = 0f;

        if (directionToDestination != Vector3.zero)
        {
            // Convertir la direction en rotation
            Quaternion targetRotation = Quaternion.LookRotation(directionToDestination);

            // Rotation seulement sur l'axe Y
            float yRotation = targetRotation.eulerAngles.y;

            // Appliquer la rotation sur l'axe Y à l'objet enfant, tout en préservant la rotation du parent
            transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}
