using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;

//The advantage of having enableable component is that enabling/disabling isn't a structural change and is way less costly
public class StunnedAuthoring : MonoBehaviour
{
    public class Baker : Baker<StunnedAuthoring>
    {
        public override void Bake(StunnedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Stunned());
            SetComponentEnabled<Stunned>(entity, false);
        }
    }
}

public partial struct Stunned : IComponentData, IEnableableComponent
{

}
