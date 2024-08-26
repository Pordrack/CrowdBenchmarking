using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace CrowdNPC
{
    public class SpawnButton : MonoBehaviour
    {
        [SerializeField] private TMP_InputField SpawnNumber;

        public void OnSpawnButtonPressed()
        {
            if(CrowdSpawnerAuthoring.Instance != null)
            {
                CrowdSpawnerAuthoring.Instance.SpawnCount= int.Parse(SpawnNumber.text);
                CrowdSpawnerAuthoring.Instance.Spawn();
                return;
            }
            CrowdSpawnerSystem crowdSpawnerSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CrowdSpawnerSystem>();
            crowdSpawnerSystem.Spawn(int.Parse(SpawnNumber.text));
        }
    }
}
