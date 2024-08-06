using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

//We don't use ISystem because we need to have events integrations
public partial class HandleCubesSystem : SystemBase
{
    public Action<Entity> OnEntityRotateAndMove;

    protected override void OnUpdate()
    {
        foreach ((var rotatingMovingCubeAspect, Entity entity) in SystemAPI.Query<RotatingMovingCubeAspect>().WithDisabled<Stunned>().WithEntityAccess())
        {
            rotatingMovingCubeAspect.MoveAndRotate(SystemAPI.Time.DeltaTime);
            OnEntityRotateAndMove?.Invoke(entity);
        }
    }
}
