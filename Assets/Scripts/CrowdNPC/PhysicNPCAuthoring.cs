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

    public float MinDampening;
    public float MaxDampening;
    public AnimationCurve DampeningCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float MinAccelToDesiredLocation;
    public float MaxAccelToDesiredLocation;
    public AnimationCurve AccelToDesiredLocationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float MinMaxVelocityToDesiredLocation;
    public float MaxMaxVelocityToDesiredLocation;
    public AnimationCurve MaxVelocityToDesiredLocationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("The favorite distance from the interest point")]
    public float MinFavDist;
    public float MaxFavDist;
    public AnimationCurve FavDistCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("The system will generate a number x between 0 and 1, use it to get the dampening value on the curve, multiply x by the randomized ratio to get y, and use y to sample from the preferred ratio curve")]
    public float MinDampToFavDistRatio=0.9f;
    public float MaxDampToFavDistRatio=1.1f;
    public AnimationCurve DampToFavDistRatioCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float MinWeight = 1;
    public float MaxWeight = 1;
    public AnimationCurve WeightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("Same things with the weight")]
    public float MinDampToWeightRatio = 0.5f;
    public float MaxDampToWeightRatio = 1.5f;
    public AnimationCurve DampToWeightRatioCurve= AnimationCurve.Linear(0, 0,1,1);


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
                WeightCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.WeightCurve),
                MinDampening = authoring.MinDampening,
                MaxDampening = authoring.MaxDampening,
                DampeningCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.DampeningCurve),
                MinFavDist = authoring.MinFavDist,
                MaxFavDist = authoring.MaxFavDist,
                FavDistCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.FavDistCurve),
                MinDampToFavDistRatio = authoring.MinDampToFavDistRatio,
                MaxDampToFavDistRatio = authoring.MaxDampToFavDistRatio,
                DampToFavDistRatioCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.DampToFavDistRatioCurve),
                MinDampToWeightRatio = authoring.MinDampToWeightRatio,
                MaxDampToWeightRatio = authoring.MaxDampToWeightRatio,
                DampToWeightRatioCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.DampToWeightRatioCurve),
                MinAccelToDesiredLocation = authoring.MinAccelToDesiredLocation,
                MaxAccelToDesiredLocation = authoring.MaxAccelToDesiredLocation,
                AccelToDesiredLocationCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.AccelToDesiredLocationCurve),
                MinMaxVelocityToDesiredLocation = authoring.MinMaxVelocityToDesiredLocation,
                MaxMaxVelocityToDesiredLocation = authoring.MaxMaxVelocityToDesiredLocation,
                MaxVelocityToDesiredLocationCurve = BakedAnimationCurve.BakeAnimationCurve(authoring.MaxVelocityToDesiredLocationCurve)
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
    public float AccelerationToDesiredLocation;
    public float MaxVelocityToDesiredLocation;
}

public class PhysicNPCRandomConstraints : IComponentData, IEnableableComponent
{
    public float MinVelocityAmplitude;
    public float MaxVelocityAmplitude;
    public BakedAnimationCurve VelocityAmplitudeCurve;
    public float MinWeight;
    public float MaxWeight;
    public BakedAnimationCurve WeightCurve;

    public float MinDampening;
    public float MaxDampening;
    public BakedAnimationCurve DampeningCurve;

    public float MinFavDist;
    public float MaxFavDist;
    public BakedAnimationCurve FavDistCurve;

    public float MinDampToFavDistRatio;
    public float MaxDampToFavDistRatio;
    public BakedAnimationCurve DampToFavDistRatioCurve;

    public float MinDampToWeightRatio;
    public float MaxDampToWeightRatio;
    public BakedAnimationCurve DampToWeightRatioCurve;

    public float MinAccelToDesiredLocation;
    public float MaxAccelToDesiredLocation;
    public BakedAnimationCurve AccelToDesiredLocationCurve;

    public float MinMaxVelocityToDesiredLocation;
    public float MaxMaxVelocityToDesiredLocation;
    public BakedAnimationCurve MaxVelocityToDesiredLocationCurve;
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
