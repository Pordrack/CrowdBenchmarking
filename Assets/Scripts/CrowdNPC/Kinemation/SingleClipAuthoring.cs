using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CrowdNPC.Kinemation
{
    [DisallowMultipleComponent]
    public class SingleClipAuthoring : MonoBehaviour
    {
        public AnimationClip clip;

        [Header("Speed Range")]
        public float MinSpeedMultiplier;
        public float MaxSpeedMultiplier;

        [Header("Offset")]
        [Range(0, 1)]
        public float MinOffset;
        [Range(0, 1)]
        public float MaxOffset;
    }

    public struct SingleClip : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> Blob;
        public bool HasBeenRandomized;

        public float SpeedMultiplier;
        public float Offset;
    }

    //We store the boundaries of the randomization in a separate component to be allowed to deactivate the randomness once done, this way a separate system can handle randomization once
    public struct SingleClipRandomConstraints : IComponentData, IEnableableComponent
    {
        public float MinSpeedMultiplier;
        public float MaxSpeedMultiplier;

        public float MinOffset;
        public float MaxOffset;
    }

    [TemporaryBakingType]
    struct SingleClipSmartBakeItem : ISmartBakeItem<SingleClipAuthoring>
    {
        SmartBlobberHandle<SkeletonClipSetBlob> blob;
        private float _minSpeedMultiplier;
        private float _maxSpeedMultiplier;
        private float _minOffset;
        private float _maxOffset;

        public bool Bake(SingleClipAuthoring authoring, IBaker baker)
        {
            baker.AddComponent<SingleClip>(baker.GetEntity(TransformUsageFlags.Dynamic));
            var clips = new NativeArray<SkeletonClipConfig>(1, Allocator.Temp);
            clips[0] = new SkeletonClipConfig { clip = authoring.clip, settings = SkeletonClipCompressionSettings.kDefaultSettings };
            blob = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);
            _minSpeedMultiplier = authoring.MinSpeedMultiplier;
            _maxSpeedMultiplier = authoring.MaxSpeedMultiplier;
            _minOffset = authoring.MinOffset;
            _maxOffset = authoring.MaxOffset;
            return true;
        }

        public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
        {
            entityManager.SetComponentData(entity, new SingleClip 
            { 
                Blob = blob.Resolve(entityManager), 
                HasBeenRandomized=false
            });

            entityManager.AddComponentData(entity, new SingleClipRandomConstraints
            {
                MinSpeedMultiplier = _minSpeedMultiplier,
                MaxSpeedMultiplier = _maxSpeedMultiplier,
                MinOffset = _minOffset,
                MaxOffset = _maxOffset
            });
            entityManager.SetComponentEnabled<SingleClipRandomConstraints>(entity, true);
        }
    }

    class SingleClipBaker : SmartBaker<SingleClipAuthoring, SingleClipSmartBakeItem>
    {
    }
}
