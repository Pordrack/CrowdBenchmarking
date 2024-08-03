using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

//public partial class RotateCubeSystem : SystemBase if you ever need to use managed types
public partial struct RotateCubeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        //Good practive, don't start updating until there's thing to rotate
        state.RequireForUpdate<RotateSpeed>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (localTransform, rotateSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotateSpeed>>())
        {
            localTransform.ValueRW = localTransform.ValueRO.RotateY(SystemAPI.Time.DeltaTime * rotateSpeed.ValueRO.Value);
        }
    }
}
