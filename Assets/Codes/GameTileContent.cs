using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameTileContentType
{
    Empty, Destination, Wall, SpawnPoint
}
public class GameTileContent : MonoBehaviour
{
    [SerializeField]
    GameTileContentType type = default;

    public GameTileContentType Type => type;

    GameTileContentFactory origin_factory;

    public GameTileContentFactory OriginFactory
    {
        get => origin_factory;
        set
        {
            Debug.Assert(origin_factory == null, "Redefined origin factory!");
            origin_factory = value;
        }
    }

    public void Recycle()
    {
        origin_factory.Reclaim(this);
    }
}
