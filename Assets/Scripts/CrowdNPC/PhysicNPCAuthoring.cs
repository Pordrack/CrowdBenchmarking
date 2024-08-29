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
    public float MinVelocityAmplitude = 0;
    public float MaxVelocityAmplitude = 1;
    public AnimationCurve VelocityAmplitudeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float MinWeight = 1;
    public float MaxWeight = 1;
    public AnimationCurve WeightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float MinDampening;
    public float MaxDampening;
    public AnimationCurve DampeningCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float MinPreferredRadius;
    public float MaxPreferredRadius;
    public AnimationCurve PreferredRadiusCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("Instead of being purely random, the preferred radius is linked to the dampening with a randomized ratio")]
    public float MinDampeningToPreferredRadiusRatio;
    public float MaxDampeningToPreferredRadiusRatio;
    public AnimationCurve DampeningToPreferredRadiusRatioCurve = AnimationCurve.Linear(0, 0, 1, 1);


    public class Baker: Baker<PhysicNPCAuthoring>
    {
        public override void Bake(PhysicNPCAuthoring authoring)
        {
            Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PhysicNPC
            {
                Weight = authoring.Weight,
                Velocity= new float2(0,0)
            });
            AddComponentObject(entity, new PhysicNPCRandomConstraints
            {
                MinVelocityAmplitude = authoring.MinVelocityAmplitude,
                MaxVelocityAmplitude = authoring.MaxVelocityAmplitude,
                VelocityAmplitudeCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.VelocityAmplitudeCurve),
                MinWeight = authoring.MinWeight,
                MaxWeight = authoring.MaxWeight,
                WeightCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.WeightCurve)
            });
        }
    }
}

public partial struct PhysicNPC : IComponentData
{
    public float Weight;
    public float2 Velocity;
    public float Dampening;
    public float PreferredRadius;
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

public struct BakedAnimationCurve
{
    public float[] Values; 
    public int Precision;

    public static BakedAnimationCurve BakeAnimationCurve(AnimationCurve curve, int precision=1000)
    {
        var bakedCurve = new BakedAnimationCurve();
        bakedCurve.Values = new float[precision];
        for (int i = 0; i < precision; i++)
        {
            bakedCurve.Values[i] = curve.Evaluate((float)i/(float)precision);
        }
        bakedCurve.Precision = precision;
        return bakedCurve;
    }

    public float Evaluate(float t)
    {
        int index=Mathf.FloorToInt(t*Precision);
        if (index < Precision)
        {
            return Values[index];
        }
        return Values[Precision-1];
    }
}
