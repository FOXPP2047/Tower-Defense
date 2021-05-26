using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField]
    Transform arrow = default;

    GameTile north, east, south, west, next_on_path;

    int distance;

    static Quaternion north_rotation = Quaternion.Euler(90f, 0f, 0f),
                      east_rotation = Quaternion.Euler(90f, 90f, 0f),
                      south_rotation = Quaternion.Euler(90f, 180f, 0f),
                      west_rotation = Quaternion.Euler(90f, 270f, 0f);

    GameTileContent content;
    public static void MakeEastWestNeighbors(GameTile east, GameTile west)
    {
        Debug.Assert(
            west.east == null && east.west == null, "Redefined neighbors!"
            );
        west.east = east;
        east.west = west;
    }

    public static void MakeNorthSouthNeighbors(GameTile north, GameTile south)
    {
        Debug.Assert(
            south.north == null && north.south == null, "Redefined neighbors!"
            );
        south.north = north;
        north.south = south;
    }

    public void ClearPath()
    {
        distance = int.MaxValue;
        next_on_path = null;
    }

    public void BecomeDestination()
    {
        distance = 0;
        next_on_path = null;
    }

    public bool HasPath => distance != int.MaxValue;

    //void GrowPathTo(GameTile neighbor)
    //{
    //    Debug.Assert(HasPath, "No Path!");
    //    neighbor.distance = distance + 1;
    //    neighbor.next_on_path = this;
    //}

    GameTile GrowPathTo(GameTile neighbor)
    {
        if(!HasPath || neighbor == null || neighbor.HasPath)
        {
            return null;
        }
        neighbor.distance = distance + 1;
        neighbor.next_on_path = this;

        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }

    public GameTile GrowPathNorth() => GrowPathTo(north);
    public GameTile GrowPathEast() => GrowPathTo(east);
    public GameTile GrowPathSouth() => GrowPathTo(south);
    public GameTile GrowPathWest() => GrowPathTo(west);

    public void ShowPath()
    {
        if(distance == 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            next_on_path == north ? north_rotation :
            next_on_path == east ? east_rotation :
            next_on_path == south ? south_rotation :
            west_rotation;
    }

    public bool IsAlternative { get; set; }

    public GameTileContent Content
    {
        get => content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content!");
            if(content != null)
            {
                content.Recycle();
            }
            content = value;
            content.transform.localPosition = transform.localPosition;
        }
    }

    public void HidePath()
    {
        arrow.gameObject.SetActive(false);
    }
}
