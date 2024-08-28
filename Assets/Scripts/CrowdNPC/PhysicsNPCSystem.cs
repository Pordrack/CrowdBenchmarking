using Latios.Psyshock;
using Latios.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace CrowdNPC
{
    //Inspired by https://www.youtube.com/watch?v=ThhdlMbGT5g
    public partial class PhysicsNPCSystem : SystemBase
    {
        static float elasticity = 0.2f;

        //Take self npc, other npc, elasticity, and returns new velocity for self ball
        static float2 HandleBallCollision(in PhysicNPC selfBallData, in float2 selfBallPosition, in PhysicNPC otherBallData, in float2 otherBallPosition, float restitution)
        {
            var distance=math.distance(selfBallPosition,otherBallPosition);
            var direction=(1/distance)*(otherBallPosition-selfBallPosition);

            var v1 = math.dot(selfBallData.Velocity,direction);
            var v2 = math.dot(otherBallData.Velocity,direction);

            var newV1 = (selfBallData.Weight*v1+otherBallData.Weight*v2-otherBallData.Weight*(v1-v2)*restitution)/(selfBallData.Weight+otherBallData.Weight);

            return selfBallData.Velocity+(direction*(newV1-v1));
        }

        protected override void OnUpdate()
        {
            foreach ((var npcData, var npcCollider, var npcTransform, var npcTransformAspect, var npcEntity) in SystemAPI.Query<RefRW<PhysicNPC>, Collider, WorldTransform, TransformAspect>().WithEntityAccess())
            {
                float2 currentVelocity=npcData.ValueRO.Velocity;
                //We handle change of velocity due to collision with other npcs
                foreach ((var otherData, var otherHitbox, var otherTransform, var otherEntity) in SystemAPI.Query< RefRO<PhysicNPC> ,Collider, WorldTransform>().WithEntityAccess())
                {
                    if (npcEntity == otherEntity) continue;
                    // DistanceBetween() returns true if the found distance is less than the maximum distance. 
                    // Intersections will have negative distances, so a maximum distance of 0f will only return true if the colliders intersect.
                    if (Physics.DistanceBetween(in npcCollider, in npcTransform.worldTransform, in otherHitbox, in otherTransform.worldTransform, 0f, out _))
                    {
                        float2 otherPosition=new float2(otherTransform.worldTransform.position.x,otherTransform.worldTransform.position.z);
                        currentVelocity=HandleBallCollision(npcData.ValueRO,new float2(npcTransform.worldTransform.position.x,npcTransform.worldTransform.position.z),otherData.ValueRO,otherPosition,elasticity);
                        break;
                    }
                }

                //Now we apply velocity, dampening etc. To each npc
                float2 selfPosition = new float2(npcTransformAspect.worldPosition.x, npcTransformAspect.worldPosition.z);
                selfPosition += currentVelocity * SystemAPI.Time.DeltaTime;

                npcData.ValueRW.Velocity = currentVelocity;
                npcTransformAspect.worldPosition += new float3(selfPosition.x, npcTransform.position.y, selfPosition.y);
            }
        }
    }
}
