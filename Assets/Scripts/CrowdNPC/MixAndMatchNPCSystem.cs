using Latios.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace CrowdNPC
{
    public partial class MixAndMatchNPCSystem : SystemBase
    {
        private Dictionary<Material, BatchMaterialID> _materialMapping;
        private EntitiesGraphicsSystem _hybridRenderer;

        private BatchMaterialID RegisterMaterial(Material material)
        {
            BatchMaterialID materialID;
            // Only register each material once
            if (!_materialMapping.ContainsKey(material))
            {
                materialID = _hybridRenderer.RegisterMaterial(material);
                _materialMapping[material] = materialID;
                return materialID;
            }
            materialID = _materialMapping[material];
            return materialID;
        }

        protected override void OnCreate()
        {
            RequireForUpdate<MixAndMatchNPC>();
        }

        protected override void OnStartRunning()
        {
            _hybridRenderer = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
            _materialMapping = new Dictionary<Material, BatchMaterialID>();
        }

        protected override void OnUpdate()
        {
            Enabled = false;
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            bool hasChanges = false;
            foreach ((var mixAndMatchNpc, Entity entity) in SystemAPI.Query<MixAndMatchNPC>().WithEntityAccess())
            {
                
                hasChanges = true;
                foreach(var layer in mixAndMatchNpc.Layers)
                {
                    int chosenElementIndex = UnityEngine.Random.Range(0, layer.Elements.Length + (layer.IncludeEmptyPossibility ? 1 : 0));
                    int chosenMaterialIndex = UnityEngine.Random.Range(0, layer.Materials.Length);
                    List<int> disabledLayers = new List<int>();
                    var chosenMaterialBatchMaterialId = RegisterMaterial(layer.Materials[chosenMaterialIndex]);

                    for(int i=0; i<layer.Elements.Length;i++)
                    {
                        var element = layer.Elements[i];
                        if(i != chosenElementIndex)
                        {
                            ecb.AddComponent<DisableRendering>(element.RendererEntity);
                            continue;
                        }
                        MaterialMeshInfo mmi= EntityManager.GetComponentData<MaterialMeshInfo>(element.RendererEntity);
                        mmi.MaterialID = chosenMaterialBatchMaterialId;
                        ecb.SetComponent(element.RendererEntity,mmi);
                        disabledLayers.AddRange(layer.Elements[chosenElementIndex].DisabledLayers);
                    }

                    foreach(var disabledLayerIndex in disabledLayers)
                    {
                        var disabledLayerElements = mixAndMatchNpc.Layers[disabledLayerIndex].Elements;
                        foreach(var element in disabledLayerElements)
                        {
                            ecb.AddComponent<DisableRendering>(element.RendererEntity);
                        }
                    }

                    foreach(var sharedMaterialEntity in layer.ShareMaterialWith)
                    {
                        MaterialMeshInfo mmi= EntityManager.GetComponentData<MaterialMeshInfo>(sharedMaterialEntity);
                        mmi.MaterialID = chosenMaterialBatchMaterialId;
                        ecb.SetComponent(sharedMaterialEntity,mmi);
                    }
                }
                
                //The mix and match operation is done only once on each npc
                EntityManager.SetComponentEnabled<MixAndMatchNPC>(entity,false);
            }
            if(!hasChanges) return;
            ecb.Playback(EntityManager);
        }
    }
}
