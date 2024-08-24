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
    }

    public struct SingleClip : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> blob;
    }

    [TemporaryBakingType]
    struct SingleClipSmartBakeItem : ISmartBakeItem<SingleClipAuthoring>
    {
        SmartBlobberHandle<SkeletonClipSetBlob> blob;

        public bool Bake(SingleClipAuthoring authoring, IBaker baker)
        {
            baker.AddComponent<SingleClip>(baker.GetEntity(TransformUsageFlags.Dynamic));
            var clips = new NativeArray<SkeletonClipConfig>(1, Allocator.Temp);
            clips[0] = new SkeletonClipConfig { clip = authoring.clip, settings = SkeletonClipCompressionSettings.kDefaultSettings };
            blob = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);
            return true;
        }

        public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
        {
            entityManager.SetComponentData(entity, new SingleClip { blob = blob.Resolve(entityManager) });
        }
    }

    class SingleClipBaker : SmartBaker<SingleClipAuthoring, SingleClipSmartBakeItem>
    {
    }
}
