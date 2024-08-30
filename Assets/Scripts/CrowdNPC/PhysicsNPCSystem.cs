using Latios.Transforms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace CrowdNPC
{
    //Inspired by https://www.youtube.com/watch?v=ThhdlMbGT5g
    public partial class PhysicsNPCSystem : SystemBase
    {
        static float elasticity = 0f;
        public Dictionary<Guid, Obstacle> ObstaclesDict;

        public void RegisterObstacle(Guid guid, Vector2 position,float radius)
        {
            if (ObstaclesDict == null)
            {
                ObstaclesDict = new Dictionary<Guid, Obstacle>();
            }
            ObstaclesDict[guid] = new Obstacle() { Position = position, Radius = radius };
        }

        public void RemoveObstacle(Guid guid)
        {
            if (ObstaclesDict == null) return;
            if (!ObstaclesDict.ContainsKey(guid)) return;
            if (ObstaclesDict.Remove(guid)) return;
        }

        public void UpdateObstacle(Guid guid, Vector2 pos)
        {
            if (ObstaclesDict == null) return;
            if (!ObstaclesDict.ContainsKey(guid)) return;
            Obstacle obstacle = ObstaclesDict[guid];
            ObstaclesDict[guid]= new Obstacle() { Position = pos, Radius = obstacle.Radius };
        }

        //Take self npc, other npc, elasticity, and returns new velocity for self ball
        public static void HandleNPCCollision(ref PhysicNPC selfBallData, ref float2 selfBallPosition, in PhysicNPC otherBallData, in float2 otherBallPosition, float radius, float restitution)
        {
            var distance = math.distance(selfBallPosition, otherBallPosition);
            var direction = (1 / distance) * (otherBallPosition - selfBallPosition);
            var correction=(2* radius - distance) / 2;
            selfBallPosition -= direction * correction;
            var v1 = math.dot(selfBallData.Velocity, direction);
            var v2 = math.dot(otherBallData.Velocity, direction);
 
            var newV1 = (selfBallData.Weight * v1 + otherBallData.Weight * v2 - otherBallData.Weight * (v1 - v2) * restitution) / (selfBallData.Weight + otherBallData.Weight);

            selfBallData.Velocity += (direction * (newV1 - v1));
        }

        public static void HandleObstacleCollision(ref PhysicNPC selfBallData, ref float2 selfBallPosition, in float2 obstaclePosition, in float obstacleRadius, float ballRadius)
        {
            var distance=math.distance(selfBallPosition,obstaclePosition);
            var direction = (1 / distance) * (selfBallPosition - obstaclePosition);
            var perpendicularToDirection = new float2(-direction.y, direction.x);
            var correction=(obstacleRadius+ballRadius - distance) / 2;
            selfBallPosition+= direction * correction;
            var vAlongCollisionDirection=math.dot(selfBallData.Velocity,direction);
            var vAlongCollisionPerpendicular=math.dot(selfBallData.Velocity, perpendicularToDirection);
            //We invert the part of the vector that is along the collision direction, while not touching the other
            //It's effectively the behaviour of the wall collision except the wall can have any "direction"Z
            var newVelocity = vAlongCollisionDirection * -1 * direction + vAlongCollisionPerpendicular * perpendicularToDirection;
            selfBallData.Velocity = newVelocity;
        }

        protected override void OnCreate()
        {
            ObstaclesDict = new Dictionary<Guid, Obstacle>();
        }

        protected override void OnUpdate()
        {
            var crowdSpawner = SystemAPI.GetSingleton<CrowdSpawner>();
            var deltaTime = SystemAPI.Time.DeltaTime;

            var query=new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PhysicNPC>(), ComponentType.ReadOnly<WorldTransform>() }
            };
            var otherNPCs = GetEntityQuery(query);
            var otherNPCsDatas=otherNPCs.ToComponentDataArray<PhysicNPC>(Allocator.TempJob);
            var otherNPCsTransforms=otherNPCs.ToComponentDataArray<WorldTransform>(Allocator.TempJob);
            var otherNPCsEntities=otherNPCs.ToEntityArray(Allocator.TempJob);
            var ObstaclesNativeArray = new NativeArray<Obstacle>(ObstaclesDict.Values.ToArray(), Allocator.TempJob);

            var npcUpdateJob = new PhysicNPCUpdateJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                CrowdSpawner = crowdSpawner,
                Elasticity=elasticity,
                OtherNPCsData = otherNPCsDatas,
                OtherNPCsTransforms = otherNPCsTransforms,
                OtherNPCsEntities = otherNPCsEntities,
                Obstacles= ObstaclesNativeArray,
            };
            npcUpdateJob.ScheduleParallel();
        }
    }

    public struct Obstacle : IComponentData
    {
        public float2 Position;
        public float Radius;
    }

    [BurstCompile]
    public partial struct PhysicNPCUpdateJob : IJobEntity
    {
        public float DeltaTime;
        public CrowdSpawner CrowdSpawner;
        [Unity.Collections.ReadOnly]
        public NativeArray<PhysicNPC> OtherNPCsData;
        [Unity.Collections.ReadOnly]
        public NativeArray<WorldTransform> OtherNPCsTransforms;
        [Unity.Collections.ReadOnly]
        public NativeArray<Entity> OtherNPCsEntities;
        [Unity.Collections.ReadOnly]
        public NativeArray<Obstacle> Obstacles;

        public float Elasticity;

        public static float SqrDistance(float2 a, float2 b)
        {
            float2 c = a - b;
            return (c.x * c.x + c.y * c.y);
        }

        public void Execute(ref PhysicNPC npcData, TransformAspect npcTransformAspect, in Entity npcEntity)
        {
            float2 selfPosition = new float2(npcTransformAspect.worldPosition.x, npcTransformAspect.worldPosition.z);
            float radius=CrowdSpawner.IndividualRadius;
            float sqrCollisionDistance = (radius + radius) * (radius + radius);
            //We handle change of velocity due to collision with other npcs
            for (int i = 0; i < OtherNPCsData.Length; i++)
            {
                var otherEntity = OtherNPCsEntities[i];
                if (npcEntity == otherEntity) continue;
                // DistanceBetween() returns true if the found distance is less than the maximum distance. 
                // Intersections will have negative distances, so a maximum distance of 0f will only return true if the colliders intersect.
                var otherTransform = OtherNPCsTransforms[i];
                float2 otherPosition = new float2(otherTransform.worldTransform.position.x, otherTransform.worldTransform.position.z);
                if (SqrDistance(selfPosition, otherPosition) < sqrCollisionDistance)
                {
                    var otherData = OtherNPCsData[i];
                    PhysicsNPCSystem.HandleNPCCollision(ref npcData, ref selfPosition, otherData, otherPosition, radius, 1 - Elasticity);
                    break;
                }
            }

            //And with external obstacles
            foreach(var obstacle in Obstacles)
            {
                if(SqrDistance(selfPosition, obstacle.Position) >= (radius + obstacle.Radius) * (radius + obstacle.Radius)) continue;
                PhysicsNPCSystem.HandleObstacleCollision(ref npcData, ref selfPosition, obstacle.Position, obstacle.Radius, radius);
            }

            float2 currentVelocity = npcData.Velocity;
            //We handle change of velocity due to collision with the external walls
            if (selfPosition.x - radius < CrowdSpawner.BottomLeftCorner.x
                || selfPosition.x + radius > CrowdSpawner.TopRightCorner.x)
            {
                selfPosition.x = UnityEngine.Mathf.Clamp(selfPosition.x,CrowdSpawner.BottomLeftCorner.x + radius, CrowdSpawner.TopRightCorner.x - radius);
                currentVelocity.x = -currentVelocity.x;
            }

            if (selfPosition.y - radius < CrowdSpawner.BottomLeftCorner.y
                || selfPosition.y + radius > CrowdSpawner.TopRightCorner.y)
            {
                selfPosition.y = UnityEngine.Mathf.Clamp(selfPosition.y, CrowdSpawner.BottomLeftCorner.y + radius, CrowdSpawner.TopRightCorner.y - radius);
                currentVelocity.y = -currentVelocity.y;
            }

            //We also move in relation to our target range
            //Target position
            float2 selfToPointOfInterest= CrowdSpawner.InterestPoint-selfPosition;
            float2 pointOfInterestToSelf = selfPosition - CrowdSpawner.InterestPoint;
            float distanceToPointOfInterest=math.distance(selfPosition, selfToPointOfInterest);
            float2 pointOfInterestToSelfDirection=pointOfInterestToSelf * (1 / distanceToPointOfInterest);
            float2 desiredPosition= CrowdSpawner.InterestPoint + pointOfInterestToSelfDirection * npcData.PreferredRadius;

            float2 desiredDirection = math.normalize(desiredPosition-selfPosition);

            //Do a new system where you dampen to 0, except for the part of the vector that is along the desired velocity, and if you're slower than the approaching speed, you actively accelerate
            float velocityInDesiredDirection=math.dot(currentVelocity, desiredDirection);
            currentVelocity = math.lerp(currentVelocity, float2.zero, math.clamp(DeltaTime * npcData.Dampening,0,1));
            float velocityPerpendicularToDesiredDirection = math.dot(currentVelocity, new float2(-desiredDirection.y, desiredDirection.x));
            if (velocityInDesiredDirection < npcData.MaxVelocityToDesiredLocation)
            {
                velocityInDesiredDirection += DeltaTime * npcData.AccelerationToDesiredLocation;
            }
            if(velocityInDesiredDirection>0)
            {
                currentVelocity = desiredDirection * velocityInDesiredDirection + new float2(-desiredDirection.y, desiredDirection.x) * velocityPerpendicularToDesiredDirection;
            }
            selfPosition += currentVelocity * DeltaTime;
             
            npcData.Velocity = currentVelocity;
            npcTransformAspect.worldPosition = new float3(selfPosition.x, npcTransformAspect.worldPosition.y, selfPosition.y);
        }
    }
}
