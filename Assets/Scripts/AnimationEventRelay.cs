using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{

    private Tower _tower;

    private void Awake()
    {
        _tower = GetComponentInParent<Tower>();

        if (_tower == null)
        {
            Debug.LogError("Tower script no encontrado en el padre.");
        }
    }

    public void SpawnProjectileRelay()
    {
        if (_tower != null)
        {
            _tower.ApplyDamageToTarget();
        }
    }
}