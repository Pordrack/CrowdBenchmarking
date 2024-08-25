using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace CrowdNPC.Kinemation
{
    public partial struct AnimationRandomizingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CrowdSpawner>();
            state.RequireForUpdate<SingleClipRandomConstraints>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach ((var singleclip, var singleClipRandomConstraints, Entity entity) in SystemAPI.Query<RefRW<SingleClip>, RefRO<SingleClipRandomConstraints>>().WithEntityAccess())
            {
                singleclip.ValueRW.Offset = Random.Range(singleClipRandomConstraints.ValueRO.MinOffset, singleClipRandomConstraints.ValueRO.MaxOffset);
                singleclip.ValueRW.SpeedMultiplier = Random.Range(singleClipRandomConstraints.ValueRO.MinSpeedMultiplier, singleClipRandomConstraints.ValueRO.MaxSpeedMultiplier);
                singleclip.ValueRW.HasBeenRandomized = true;
                state.EntityManager.SetComponentEnabled<SingleClipRandomConstraints>(entity, false);
            }
        }
    }
}
