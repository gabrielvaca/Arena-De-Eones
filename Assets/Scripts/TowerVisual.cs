using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerVisual : MonoBehaviour
{
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private float rotationSpeed = 10f;
        

    public void AimAt(Vector3 worldTargetPos)
    {
        if (!rotatingPart) return;

        Vector3 direction = worldTargetPos - rotatingPart.position;
        direction.y = 0f;
        
        if (direction.magnitude < 0.1f) return;
        
        Quaternion targetRot = Quaternion.LookRotation(direction);
        rotatingPart.rotation = Quaternion.Lerp(rotatingPart.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }
}
