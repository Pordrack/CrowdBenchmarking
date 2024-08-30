using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace CrowdNPC
{
    //Describes an obstacle that is bound to a gameobject in the regular world
    public class MonoBehaviourObstacle : MonoBehaviour
    {
        [SerializeField] private float _radius = 1;
        private Guid _id = Guid.NewGuid();
        private Vector2 _cachedPosition;

        PhysicsNPCSystem _physicsNPCSystem;

        private void Start()
        {
            _physicsNPCSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PhysicsNPCSystem>();
            var twoDPosition=new Vector2(transform.position.x, transform.position.z);
            _physicsNPCSystem.RegisterObstacle(_id, twoDPosition, _radius);

            if (gameObject.isStatic) enabled = false;
            return;
        }

        private void Update()
        {
            Vector2 newPosition=new Vector2(transform.position.x,transform.position.z);
            if (_cachedPosition != newPosition)
            {
                _physicsNPCSystem.UpdateObstacle(_id, newPosition);
                _cachedPosition = newPosition;
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
#endif
    }
}
