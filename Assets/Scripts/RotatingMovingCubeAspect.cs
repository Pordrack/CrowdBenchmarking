using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct RotatingMovingCubeAspect : IAspect
{
    public readonly RefRW<LocalTransform> LocalTransform;
    public readonly RefRO<RotateSpeed> RotateSpeed;
    public readonly RefRO<Movement> Movement;

    [BurstCompile]
    public void MoveAndRotate(float deltaTime)
    {
        LocalTransform.ValueRW=LocalTransform.ValueRO.Translate(deltaTime*Movement.ValueRO.MovementVector);
        LocalTransform.ValueRW = LocalTransform.ValueRO.RotateY(deltaTime * RotateSpeed.ValueRO.Value);
    }
}
