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
                physicNPC.ValueRW.Weight = physicNPCRandomConstraints.MinWeight+
                    physicNPCRandomConstraints.WeightCurve.Evaluate(UnityEngine.Random.value)*(physicNPCRandomConstraints.MaxWeight-physicNPCRandomConstraints.MinWeight);
                float velocityAmplitude=physicNPCRandomConstraints.MinVelocityAmplitude+
                    physicNPCRandomConstraints.VelocityAmplitudeCurve.Evaluate(UnityEngine.Random.value)*(physicNPCRandomConstraints.MaxVelocityAmplitude-physicNPCRandomConstraints.MinVelocityAmplitude);
                physicNPC.ValueRW.Velocity= velocityAmplitude * GetRandomDirection();
                float dampeningCurveResult=UnityEngine.Random.value;
                float dampening=physicNPCRandomConstraints.MinDampening+
                    physicNPCRandomConstraints.DampeningCurve.Evaluate(dampeningCurveResult)*(physicNPCRandomConstraints.MaxDampening-physicNPCRandomConstraints.MinDampening);
                physicNPC.ValueRW.Dampening=dampening;
                float dampeningToRadiusRatio=physicNPCRandomConstraints.MinDampToFavDistRatio+
                    physicNPCRandomConstraints.DampeningToPreferredRadiusRatioCurve.Evaluate(dampeningCurveResult)*(physicNPCRandomConstraints.MaxDampToFavDist-physicNPCRandomConstraints.MinDampToFavDistRatio);
                float preferredRadiusCurveX=dampeningCurveResult*dampeningToRadiusRatio;
                float preferredRadius=physicNPCRandomConstraints.MinFavDist+
                    physicNPCRandomConstraints.FavDistCurve.Evaluate(preferredRadiusCurveX)*(physicNPCRandomConstraints.MaxFavDist-physicNPCRandomConstraints.MinFavDist);
                physicNPC.ValueRW.PreferredRadius=preferredRadius;
                string newBackstageItemID = System.Guid.NewGuid().ToString();
                EntityManager.SetComponentEnabled<PhysicNPCRandomConstraints>(entity, false);
            }    
        }
    }
}
