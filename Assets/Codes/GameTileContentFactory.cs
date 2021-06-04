using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : ScriptableObject
{
    Scene content_scene;

    [SerializeField]
    GameTileContent destination_prefab = default;
    [SerializeField]
    GameTileContent empty_prefab = default;
    [SerializeField]
    GameTileContent wall_prefab = default;
    [SerializeField]
    GameTileContent spawn_point_prefab = default;
    
    public void Reclaim (GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }

    GameTileContent Get(GameTileContent prefab)
    {
        GameTileContent instance = Instantiate(prefab);
        instance.OriginFactory = this;
        MoveToFactoryScene(instance.gameObject);
        return instance;
    }

    void MoveToFactoryScene(GameObject o)
    {
        if(!content_scene.isLoaded)
        {
            if(Application.isEditor)
            {
                content_scene = SceneManager.GetSceneByName(name);
                if(!content_scene.isLoaded)
                {
                    content_scene = SceneManager.CreateScene(name);
                }
            }
            else
            {
                content_scene = SceneManager.CreateScene(name);
            }
        }
        SceneManager.MoveGameObjectToScene(o, content_scene);
    }

    public GameTileContent Get(GameTileContentType type)
    {
        switch(type)
        {
            case GameTileContentType.Destination: return Get(destination_prefab);
            case GameTileContentType.Empty: return Get(empty_prefab);
            case GameTileContentType.Wall: return Get(wall_prefab);
            case GameTileContentType.SpawnPoint: return Get(spawn_point_prefab);
        }
        Debug.AssertFormat(false, "Unspported type : ", type);
        return null;
    }
}
