using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class PhysicNPCAuthoring : MonoBehaviour
{
    public float Weight = 1;
    public float Radius  =1;
    public float MinVelocityAmplitude = 0;
    public float MaxVelocityAmplitude = 1;
    public AnimationCurve VelocityAmplitudeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float MinWeight = 1;
    public float MaxWeight = 1;
    public AnimationCurve WeightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public class Baker: Baker<PhysicNPCAuthoring>
    {
        public override void Bake(PhysicNPCAuthoring authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PhysicNPC
            {
                Weight = authoring.Weight,
                Radius = authoring.Radius,
                Velocity= new float2(0,0)
            });
            AddComponent(entity, new PhysicNPCRandomConstraints
            {
                MinVelocityAmplitude = authoring.MinVelocityAmplitude,
                MaxVelocityAmplitude = authoring.MaxVelocityAmplitude,
                VelocityAmplitudeCurve = authoring.VelocityAmplitudeCurve,
                MinWeight = authoring.MinWeight,
                MaxWeight = authoring.MaxWeight,
                WeightCurve = authoring.WeightCurve
            });
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
#endif
}

public partial struct PhysicNPC : IComponentData
{
    public float Weight;
    public float Radius;
    public float2 Velocity;
}

public class PhysicNPCRandomConstraints : IComponentData, IEnableableComponent
{
    public float MinVelocityAmplitude;
    public float MaxVelocityAmplitude;
    public BakedAnimationCurve VelocityAmplitudeCurve;
    public float MinWeight;
    public float MaxWeight;
    public BakedAnimationCurve WeightCurve;
}
