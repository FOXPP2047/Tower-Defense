using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField]
    Transform ground = default;

    Vector2Int size;

    [SerializeField]
    GameTile tilePrefab = default;
    GameTile[] tiles;

    GameTileContentFactory content_factory;

    bool show_grid, show_paths;

    Queue<GameTile> search_frontier = new Queue<GameTile>();

    [SerializeField]
    Texture2D grid_texture = default;

    List<GameTile> spawn_points = new List<GameTile>();
    public void Initialize(Vector2Int size, GameTileContentFactory content_factory)
    {
        this.size = size;
        this.content_factory = content_factory;
        this.ground.localScale = new Vector3(this.size.x, this.size.y, 1.0f);

        Vector2 offset = new Vector2((this.size.x - 1) * 0.5f, (this.size.y - 1) * 0.5f);

        tiles = new GameTile[this.size.x * this.size.y];

        for(int i = 0, y = 0; y < this.size.y; ++y)
        {
            for(int x = 0; x < this.size.x; ++x, ++i)
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);

                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0.0f, y - offset.y);

                if(x > 0)
                {
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                }

                if(y > 0)
                {
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - this.size.x]);
                }

                tile.IsAlternative = (x & 1) == 0;

                if((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }

                tile.Content = content_factory.Get(GameTileContentType.Empty);
            }
        }
        ToggleDestination(tiles[tiles.Length / 2]);
        ToggleSpawnPoint(tiles[0]);
    }

    bool FindPaths()
    {
        foreach(GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                search_frontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }
        if(search_frontier.Count == 0)
        {
            return false;
        }

        //tiles[tiles.Length / 2].BecomeDestination();
        //search_frontier.Enqueue(tiles[tiles.Length / 2]);
        GameTile temp_tile = search_frontier.Dequeue();

        search_frontier.Enqueue(temp_tile.GrowPathNorth());
        search_frontier.Enqueue(temp_tile.GrowPathEast());
        search_frontier.Enqueue(temp_tile.GrowPathSouth());
        search_frontier.Enqueue(temp_tile.GrowPathWest());


        while (search_frontier.Count > 0)
        {
            GameTile tile = search_frontier.Dequeue();
            if (tile != null)
            {
                if (tile.IsAlternative)
                {
                    search_frontier.Enqueue(tile.GrowPathNorth());
                    search_frontier.Enqueue(tile.GrowPathSouth());
                    search_frontier.Enqueue(tile.GrowPathEast());
                    search_frontier.Enqueue(tile.GrowPathWest());
                }
                else
                {
                    search_frontier.Enqueue(tile.GrowPathWest());
                    search_frontier.Enqueue(tile.GrowPathEast());
                    search_frontier.Enqueue(tile.GrowPathSouth());
                    search_frontier.Enqueue(tile.GrowPathNorth());
                }
            }
        }

        foreach (GameTile tile in tiles)
        {
            if(!tile.HasPath)
            {
                return false;
            }
        }

        if(show_paths)
        {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }
        
        return true;
    }

    public void ToggleDestination(GameTile tile)
    {
        if(tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = content_factory.Get(GameTileContentType.Empty);
            if(!FindPaths())
            {
                tile.Content = content_factory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = content_factory.Get(GameTileContentType.Destination);
            FindPaths();
        }
        else
        {
            tile.Content = content_factory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }
    public GameTile GetTile(Ray ray)
    {
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            int x = (int)(hit.point.x + this.size.x * 0.5f);
            int y = (int)(hit.point.z + this.size.y * 0.5f);

            if(x >= 0 && x < this.size.x && y >= 0 && y < this.size.y)
            {
                return tiles[x + y * this.size.x];
            }
            return null;
        }
        return null;
    }

    public void ToggleWall(GameTile tile)
    {
        if(tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = content_factory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = content_factory.Get(GameTileContentType.Wall);
            if(!FindPaths())
            {
                tile.Content = content_factory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
        else
        {
            tile.Content = content_factory.Get(GameTileContentType.Wall);
            FindPaths();
        }
    }

    public void ToggleSpawnPoint(GameTile tile)
    {
        if(tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if(spawn_points.Count > 1)
            {
                spawn_points.Remove(tile);
                tile.Content = content_factory.Get(GameTileContentType.Empty);
            }
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = content_factory.Get(GameTileContentType.SpawnPoint);
            spawn_points.Add(tile);
        }
    }
    public bool ShowPaths
    {
        get => show_paths;
        set
        {
            show_paths = value;
            if(show_paths)
            {
                foreach(GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach(GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    public bool ShowGrid
    {
        get => show_grid;
        set
        {
            show_grid = value;
            Material m = ground.GetComponent<MeshRenderer>().material;

            if (show_grid)
            {
                m.mainTexture = grid_texture;
                m.SetTextureScale("_MainTex", this.size);
            }
            else
            {
                m.mainTexture = null;
            }
        }
    }

    public GameTile GetSpawnPoint(int index)
    {
        return spawn_points[index];
    }

    public int SpawnPointCount => spawn_points.Count;
}
