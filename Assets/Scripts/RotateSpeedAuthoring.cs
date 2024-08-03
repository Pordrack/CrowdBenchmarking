using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RotateSpeedAuthoring : MonoBehaviour
{
    public float Value;

    public class Baker : Baker<RotateSpeedAuthoring>
    {
        public override void Bake(RotateSpeedAuthoring authoring)
        {
            var entity=GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,new RotateSpeed 
            { 
                Value = authoring.Value 
            }
            );
        }
    }
}
