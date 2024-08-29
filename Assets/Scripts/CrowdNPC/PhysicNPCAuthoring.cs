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
                VelocityAmplitudeCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.VelocityAmplitudeCurve,),
                MinWeight = authoring.MinWeight,
                MaxWeight = authoring.MaxWeight,
                WeightCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.WeightCurve)
            });
            SetComponentEnabled<PhysicNPCRandomConstraints>(entity,true);
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

public struct PhysicNPCRandomConstraints : IComponentData, IEnableableComponent
{
    public float MinVelocityAmplitude;
    public float MaxVelocityAmplitude;
    public BakedAnimationCurve VelocityAmplitudeCurve;
    public float MinWeight;
    public float MaxWeight;
    public BakedAnimationCurve WeightCurve;
}

public struct BakedAnimationCurve
{
    public int Precision;
    public DynamicBuffer<AnimationCurveValue> Values;
    public static BakedAnimationCurve BakeAnimationCurve(AnimationCurve v, EntityManager em,Entity e, int precision=1000)
    {
        var curve = new BakedAnimationCurve();
        curve.Values= em.AddBuffer<AnimationCurveValue>(e);
        for (int i = 0; i < precision; i++)
        {
            curve.Values.Add(new AnimationCurveValue() { Value = v.Evaluate(i * (1 / precision)) });
        }
        curve.Precision = precision;
        return curve;
    }

    public float Evaluate(float t)
    {
        int index = (int)(t * Precision);
        if (index >= Precision)
            return Values[Values.Length - 1].Value;
        return Values[index].Value;
        //return 1;
    }
}

public struct AnimationCurveValue : IBufferElementData
{
    public float Value;
}
