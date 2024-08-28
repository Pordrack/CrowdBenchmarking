using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TestStringAuthoring : MonoBehaviour
{
    public int Value;
    public class Baker : Baker<TestStringAuthoring> 
    {
        public override void Bake(TestStringAuthoring authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent(entity, new TestString()
            {
                Value = authoring.Value,
            });
        }
    }

}

public partial struct TestString : IComponentData
{
    public int Value;
}