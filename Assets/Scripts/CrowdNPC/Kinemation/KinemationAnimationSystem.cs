using Latios.Kinemation;
using Latios.Transforms;
using Latios.Transforms.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using static Unity.Entities.SystemAPI;

namespace CrowdNPC.Kinemation
{
    [UpdateBefore(typeof(TransformSuperSystem))]
    public partial struct KinemationAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ExposedJob { clipLookup = GetComponentLookup<SingleClip>(true), et = (float)Time.ElapsedTime }.ScheduleParallel();
        }

        [BurstCompile]
        partial struct ExposedJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<SingleClip> clipLookup;
            public float et;

            public void Execute(TransformAspect transform, in BoneIndex boneIndex, in BoneOwningSkeletonReference skeletonRef)
            {
                if (boneIndex.index <= 0 || !clipLookup.HasComponent(skeletonRef.skeletonRoot))
                    return;

                ref var clip = ref clipLookup[skeletonRef.skeletonRoot].Blob.Value.clips[0];
                var roSingleClip= clipLookup[skeletonRef.skeletonRoot];

                float clipTime = 0;

                if (roSingleClip.HasBeenRandomized)
                {
                    clipTime = (roSingleClip.Offset * clip.duration)+ (et * roSingleClip.SpeedMultiplier);
                    clipTime = clipTime % clip.duration;
                }
                else
                {
                    clipTime= clip.LoopToClipTime(et);
                }

                //UnityEngine.Debug.Log($"Offset:{roSingleClip.Offset};SpeedMultiplier:{roSingleClip.SpeedMultiplier};RawET:{et};ClipTime: {clipTime}");
                transform.localTransformQvvs = clip.SampleBone(boneIndex.index, clipTime);
            }
        }
    }
}
