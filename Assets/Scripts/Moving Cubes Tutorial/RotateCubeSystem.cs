using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct CubeRotateUpdateJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(ref LocalTransform localTransform, in RotateSpeed rotateSpeed)
    {
        localTransform = localTransform.RotateY(DeltaTime * rotateSpeed.Value);
    }
}

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
        state.Enabled=false;
        return;
        //foreach(var (localTransform, rotateSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotateSpeed>>())
        //{
        //    localTransform.ValueRW = localTransform.ValueRO.RotateY(SystemAPI.Time.DeltaTime * rotateSpeed.ValueRO.Value);
        //}
        var rotateJob = new CubeRotateUpdateJob()
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
        };
        rotateJob.ScheduleParallel();
    }

   
}
