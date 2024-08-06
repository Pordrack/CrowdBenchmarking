using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class RotatngMovingCubeNonDotLogger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var handleCubeSystem=World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<HandleCubesSystem>();
        handleCubeSystem.OnEntityRotateAndMove += OnEntityRotateAndMove;
    }

    private void OnEntityRotateAndMove(Entity entity)
    {
        Debug.Log("Rotate and move "+entity.Index);
        Debug.Log("Position " + World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalTransform>(entity));
    }
}
