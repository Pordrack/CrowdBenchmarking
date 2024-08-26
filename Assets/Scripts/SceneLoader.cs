using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

        //Reset the ECS world
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        World.DisposeAllWorlds();
        DefaultWorldInitialization.Initialize("Base World", false);
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld);
    }
}
