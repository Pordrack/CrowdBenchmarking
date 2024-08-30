using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CrowdNPC.Kinemation
{
    public partial class PhysicsNPCRandomizingSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<PhysicNPC>();
            RequireForUpdate<PhysicNPCRandomConstraints>();
        }

        public static float2 GetRandomDirection()
        {
            float angle = UnityEngine.Random.Range(0, 360);
            return new float2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        protected override void OnUpdate()
        {
            foreach ((var physicNPC, var physicNPCRandomConstraints, Entity entity) in SystemAPI.Query<RefRW<PhysicNPC>, PhysicNPCRandomConstraints>().WithEntityAccess())
            {
                float velocityAmplitude=physicNPCRandomConstraints.MinVelocityAmplitude+
                    physicNPCRandomConstraints.VelocityAmplitudeCurve.Evaluate(UnityEngine.Random.value)*(physicNPCRandomConstraints.MaxVelocityAmplitude-physicNPCRandomConstraints.MinVelocityAmplitude);
                physicNPC.ValueRW.Velocity= velocityAmplitude * GetRandomDirection();

                float dampeningCurveResult=UnityEngine.Random.value;
                float dampening=physicNPCRandomConstraints.MinDampening+
                    physicNPCRandomConstraints.DampeningCurve.Evaluate(dampeningCurveResult)*(physicNPCRandomConstraints.MaxDampening-physicNPCRandomConstraints.MinDampening);
                physicNPC.ValueRW.Dampening=dampening;

                float dampeningToRadiusRatio=physicNPCRandomConstraints.MinDampToFavDistRatio+
                    physicNPCRandomConstraints.DampToFavDistRatioCurve.Evaluate(dampeningCurveResult)*(physicNPCRandomConstraints.MaxDampToFavDistRatio-physicNPCRandomConstraints.MinDampToFavDistRatio);
                float preferredRadiusCurveX=dampeningCurveResult*dampeningToRadiusRatio;
                float preferredRadius=physicNPCRandomConstraints.MinFavDist+
                    physicNPCRandomConstraints.FavDistCurve.Evaluate(preferredRadiusCurveX)*(physicNPCRandomConstraints.MaxFavDist-physicNPCRandomConstraints.MinFavDist);
                physicNPC.ValueRW.PreferredRadius=preferredRadius;

                float dampeningToWeightRatio = physicNPCRandomConstraints.MinDampToWeightRatio +
                    physicNPCRandomConstraints.DampToWeightRatioCurve.Evaluate(dampeningCurveResult) * (physicNPCRandomConstraints.MaxDampToWeightRatio - physicNPCRandomConstraints.MinDampToWeightRatio);
                float weightCurveX = dampeningCurveResult * dampeningToWeightRatio;
                float weight=physicNPCRandomConstraints.MinWeight+
                    physicNPCRandomConstraints.WeightCurve.Evaluate(weightCurveX)*(physicNPCRandomConstraints.MaxWeight-physicNPCRandomConstraints.MinWeight);
                physicNPC.ValueRW.Weight=weight;

                float accelerationToDesiredLocation=physicNPCRandomConstraints.MinAccelToDesiredLocation+
                    physicNPCRandomConstraints.AccelToDesiredLocationCurve.Evaluate(UnityEngine.Random.value)*(physicNPCRandomConstraints.MaxAccelToDesiredLocation-physicNPCRandomConstraints.MinAccelToDesiredLocation);
                physicNPC.ValueRW.AccelerationToDesiredLocation=accelerationToDesiredLocation;

                string newBackstageItemID = System.Guid.NewGuid().ToString();
                EntityManager.SetComponentEnabled<PhysicNPCRandomConstraints>(entity, false);
            }    
        }
    }
}
