using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CrowdNPC
{
    [System.Serializable]
    public struct MixAndMatchElementAuthoring
    {
        [Tooltip("Dont leave empty ! If you want an empty possibility, use IncludeEmptuPossibility on layer")]
        public SkinnedMeshRenderer Renderer;
        [Tooltip("If this element is chosen, the elements of these layers will all be deactivated. Layers must come after this element's one in the array for the feature to work")]
        public int[] DisabledLayers;
    }

    public struct MixAndMatchElement
    {
        public Entity RendererEntity;
        public int[] DisabledLayers;
    }

    [System.Serializable]
    public struct MixAndMatchLayerAuthoring
    {
        [Tooltip("The name of the layer, only here for the dev's confort, not used in the code")]
        public String LayerName;
        public MixAndMatchElementAuthoring[] Elements;
        public Material[] Materials;
        public SkinnedMeshRenderer[] ShareMaterialWith;
        public bool IncludeEmptyPossibility;

        public MixAndMatchElementAuthoring Compute()
        {
            int chosenElementIndex = UnityEngine.Random.Range(0, Elements.Length + (IncludeEmptyPossibility ? 1 : 0));
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

            if(chosenElementIndex >= Elements.Length)
            {
                return new MixAndMatchElementAuthoring();
            }
            return Elements[chosenElementIndex];
        }
    }

    public struct MixAndMatchLayer
    {
        public MixAndMatchElement[] Elements;
        public Material[] Materials;
        public Entity[] ShareMaterialWith;
        public bool IncludeEmptyPossibility;
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
                if(chosenElement.Renderer == null) continue;
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
                        bakedElement.DisabledLayers = element.DisabledLayers;

                        if (element.Renderer == null)
                            Debug.LogError("Cannot have a null renderer in a MixAndMatchElementAuthoring, if you want an empty possibiluty, use the IncludeEmptyPossibility of Layer !");

                        bakedElement.RendererEntity = GetEntity(element.Renderer.gameObject, TransformUsageFlags.Dynamic);
                        bakedElements.Add(bakedElement);
                    }

                    List<Entity> bakedSharedWith = new List<Entity>();
                    foreach (var sharedWith in layer.ShareMaterialWith)
                    {
                        bakedSharedWith.Add(GetEntity(sharedWith.gameObject, TransformUsageFlags.Dynamic));
                    }
                    MixAndMatchLayer bakedLayer = new MixAndMatchLayer();
                    bakedLayer.Materials = layer.Materials;
                    bakedLayer.Elements = bakedElements.ToArray();
                    bakedLayer.ShareMaterialWith = bakedSharedWith.ToArray();
                    bakedLayer.IncludeEmptyPossibility = layer.IncludeEmptyPossibility;
                    bakedLayers.Add(bakedLayer);
                }

                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new MixAndMatchNPC()
                {
                    Layers = bakedLayers.ToArray()
                });
                //There's no way to make a "SetComponentEnabled" on a managed component AFAIK, but thankfully component starts on enable
            }
        }
    }

    public class MixAndMatchNPC : IComponentData, IEnableableComponent
    {
        public MixAndMatchLayer[] Layers;
    } 
}
