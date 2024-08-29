using Latios.Psyshock;
using Latios.Transforms;
using System.ComponentModel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CrowdNPC
{
    //Inspired by https://www.youtube.com/watch?v=ThhdlMbGT5g
    public partial class PhysicsNPCSystem : SystemBase
    {
        static float elasticity = 0f;

        //Take self npc, other npc, elasticity, and returns new velocity for self ball
        public static void HandleBallCollision(ref PhysicNPC selfBallData, ref float2 selfBallPosition, in PhysicNPC otherBallData, in float2 otherBallPosition, float restitution)
        {
            var distance = math.distance(selfBallPosition, otherBallPosition);
            var direction = (1 / distance) * (otherBallPosition - selfBallPosition);
            var correction=(selfBallData.Radius + otherBallData.Radius - distance) / 2;
            selfBallPosition -= direction * correction;

            var v1 = math.dot(selfBallData.Velocity, direction);
            var v2 = math.dot(otherBallData.Velocity, direction);

            var newV1 = (selfBallData.Weight * v1 + otherBallData.Weight * v2 - otherBallData.Weight * (v1 - v2) * restitution) / (selfBallData.Weight + otherBallData.Weight);

            selfBallData.Velocity += ((newV1 - v1));
        }

        protected override void OnUpdate()
        {
            var crowdSpawner = SystemAPI.GetSingleton<CrowdSpawner>();
            var deltaTime = SystemAPI.Time.DeltaTime;

            var query=new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PhysicNPC>(), ComponentType.ReadOnly<Collider>(), ComponentType.ReadOnly<WorldTransform>() }
            };
            var otherNPCs = GetEntityQuery(query);
            var otherNPCsDatas=otherNPCs.ToComponentDataArray<PhysicNPC>(Allocator.TempJob);
            var otherNPCsColliders=otherNPCs.ToComponentDataArray<Collider>(Allocator.TempJob);
            var otherNPCsTransforms=otherNPCs.ToComponentDataArray<WorldTransform>(Allocator.TempJob);
            var otherNPCsEntities=otherNPCs.ToEntityArray(Allocator.TempJob);

            var npcUpdateJob = new PhysicNPCUpdateJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                CrowdSpawner = crowdSpawner,
                Elasticity=elasticity,
                OtherNPCsData = otherNPCsDatas,
                OtherNPCsColliders = otherNPCsColliders,
                OtherNPCsTransforms = otherNPCsTransforms,
                OtherNPCsEntities = otherNPCsEntities
            };
            npcUpdateJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct PhysicNPCUpdateJob : IJobEntity
    {
        public float DeltaTime;
        public CrowdSpawner CrowdSpawner;
        [NativeDisableParallelForRestriction]
        public NativeArray<PhysicNPC> OtherNPCsData;
        [NativeDisableParallelForRestriction]
        public NativeArray<Collider> OtherNPCsColliders;
        [NativeDisableParallelForRestriction]
        public NativeArray<WorldTransform> OtherNPCsTransforms;
        [NativeDisableParallelForRestriction]
        public NativeArray<Entity> OtherNPCsEntities;

        public float Elasticity;


        public void Execute(ref PhysicNPC npcData, in Collider npcCollider, TransformAspect npcTransformAspect, in Entity npcEntity)
        {
            float2 currentVelocity = npcData.Velocity;
            float2 selfPosition = new float2(npcTransformAspect.worldPosition.x, npcTransformAspect.worldPosition.z);
            //We handle change of velocity due to collision with other npcs
            for(int i=0;i<OtherNPCsData.Length;i++)
            {
                var otherEntity = OtherNPCsEntities[i];
                if (npcEntity == otherEntity) continue;
;                var otherHitbox = OtherNPCsColliders[i];
                // DistanceBetween() returns true if the found distance is less than the maximum distance. 
                // Intersections will have negative distances, so a maximum distance of 0f will only return true if the colliders intersect.
                var selfWorldTransform = npcTransformAspect.worldTransform;
                var otherTransform = OtherNPCsTransforms[i];
                if (Physics.DistanceBetween(in npcCollider, in selfWorldTransform, in otherHitbox, in otherTransform.worldTransform, 0f, out _))
                {
                    var otherData = OtherNPCsData[i];
                    float2 otherPosition = new float2(otherTransform.worldTransform.position.x, otherTransform.worldTransform.position.z);
                    PhysicsNPCSystem.HandleBallCollision(ref npcData,ref selfPosition, otherData, otherPosition, 1-Elasticity);
                    break;
                }
            }

            //We handle change of velocity due to collision with the external walls
            if (selfPosition.x - npcData.Radius < CrowdSpawner.BottomLeftCorner.x
                || selfPosition.x + npcData.Radius > CrowdSpawner.TopRightCorner.x)
            {
                selfPosition.x = UnityEngine.Mathf.Clamp(selfPosition.x,CrowdSpawner.BottomLeftCorner.x + npcData.Radius, CrowdSpawner.TopRightCorner.x - npcData.Radius);
                currentVelocity.x = -currentVelocity.x;
            }

            if (selfPosition.y - npcData.Radius < CrowdSpawner.BottomLeftCorner.y
                || selfPosition.y + npcData.Radius > CrowdSpawner.TopRightCorner.y)
            {
                selfPosition.y = UnityEngine.Mathf.Clamp(selfPosition.y, CrowdSpawner.BottomLeftCorner.y + npcData.Radius, CrowdSpawner.TopRightCorner.y - npcData.Radius);
                currentVelocity.y = -currentVelocity.y;
            }

            //Now we apply velocity, dampening etc. To each npc
            
            selfPosition += currentVelocity * DeltaTime;

            npcData.Velocity = currentVelocity;
            //UnityEngine.Debug.Log(currentVelocity);
            npcTransformAspect.worldPosition = new float3(selfPosition.x, npcTransformAspect.worldPosition.y, selfPosition.y);
        }
    }
}
