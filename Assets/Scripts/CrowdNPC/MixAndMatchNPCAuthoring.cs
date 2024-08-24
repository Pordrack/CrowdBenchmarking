using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace CrowdNPC
{
    [System.Serializable]
    public struct MixAndMatchElementAuthoring
    {
        [Tooltip("Leave empty if you want an option where no element of this layer is present (hair for exemple)")]
        public SkinnedMeshRenderer Renderer;
        [Tooltip("If this element is chosen, the elements of these layers will all be deactivated. Layers must come after this element's one in the array for the feature to work")]
        public int[] DisabledLayers;
    }

    public struct MixAndMatchElement
    {
        public Entity RendererEntity;
        public NativeArray<int> DisabledLayers;
    }

    [System.Serializable]
    public struct MixAndMatchLayerAuthoring
    {
        [Tooltip("The name of the layer, only here for the dev's confort, not used in the code")]
        public String LayerName;
        public MixAndMatchElementAuthoring[] Elements;
        public Material[] Materials;
        public SkinnedMeshRenderer[] ShareMaterialWith;

        public MixAndMatchElementAuthoring Compute()
        {
            int chosenElementIndex = UnityEngine.Random.Range(0, Elements.Length);
            int chosenMaterialIndex = UnityEngine.Random.Range(0, Materials.Length);
            List<int> disabledLayers = new List<int>();

            foreach (var sharedMaterialRenderers in ShareMaterialWith)
            {
                sharedMaterialRenderers.sharedMaterial = Materials[chosenMaterialIndex];
            }

            for (int i = 0; i < Elements.Length; i++)
            {
                if (Elements[i].Renderer == null) continue;

                Elements[i].Renderer.gameObject.SetActive(chosenElementIndex == i);
                if (chosenElementIndex != i) continue;
                Elements[i].Renderer.sharedMaterial = Materials[chosenMaterialIndex];
            }

            return Elements[chosenElementIndex];
        }
    }

    public struct MixAndMatchLayer
    {
        public NativeArray<MixAndMatchElement> Elements;
        public NativeArray<BatchMaterialID> Materials;
        public NativeArray<Entity> ShareMaterialWith;
    }
    //Like CrowdSpawnerSystem, this authoring component can also be used as a regular component
    public class MixAndMatchNPCAuthoring : MonoBehaviour
    {
        public MixAndMatchLayerAuthoring[] Layers;

        public void Start()
        {
            List<int> excludedLayers = new List<int>();
            foreach (var layer in Layers)
            {
                var chosenElement = layer.Compute();
                excludedLayers.AddRange(chosenElement.DisabledLayers);
            }
            foreach (var excludedLayerIndex in excludedLayers)
            {
                foreach (var element in Layers[excludedLayerIndex].Elements)
                {
                    element.Renderer?.gameObject.SetActive(false);
                }
            }
        }

        public class Baker : Baker<MixAndMatchNPCAuthoring>
        {
            public override void Bake(MixAndMatchNPCAuthoring authoring)
            {
                List<MixAndMatchLayer> bakedLayers = new List<MixAndMatchLayer>();
                foreach (var layer in authoring.Layers)
                {
                    List<MixAndMatchElement> bakedElements = new List<MixAndMatchElement>();
                    foreach (var element in layer.Elements)
                    {
                        MixAndMatchElement bakedElement = new MixAndMatchElement();
                        bakedElement.DisabledLayers = new NativeArray<int>(element.DisabledLayers, Allocator.Persistent);
                        bakedElement.RendererEntity = Entity.Null;//(element.Renderer.gameObject!=null?GetEntity(element.Renderer.gameObject, TransformUsageFlags.Dynamic):Entity.Null);
                        bakedElements.Add(bakedElement);
                    }

                    List<Entity> bakedSharedWith = new List<Entity>();
                    foreach (var sharedWith in layer.ShareMaterialWith)
                    {
                        bakedSharedWith.Add(GetEntity(sharedWith.gameObject, TransformUsageFlags.Dynamic));
                    }
                    MixAndMatchLayer bakedLayer = new MixAndMatchLayer();
                    //bakedLayer.Materials = new NativeArray<BatchMaterialID>(layer.Materials);
                    bakedLayer.Elements = new NativeArray<MixAndMatchElement>(bakedElements.ToArray(),Allocator.Persistent);
                    bakedLayer.ShareMaterialWith = new NativeArray<Entity>(bakedSharedWith.ToArray(),Allocator.Persistent);
                    bakedLayers.Add(bakedLayer);
                }

                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new MixAndMatchNPC()
                {
                    Layers = new NativeArray<MixAndMatchLayer>(bakedLayers.ToArray(),Allocator.Persistent)
                });
                SetComponentEnabled<MixAndMatchNPC>(entity, true);
            }
        }
    }

    public partial struct MixAndMatchNPC : IComponentData, IEnableableComponent
    {
        public NativeArray<MixAndMatchLayer> Layers;
    } 
}
