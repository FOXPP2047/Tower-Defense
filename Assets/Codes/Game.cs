using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    Vector2Int board_size = new Vector2Int(11, 11);

    [SerializeField]
    GameBoard board = default;

    [SerializeField]
    GameTileContentFactory tile_content_factory = default;

    void Awake()
    {
        board.Initialize(board_size, tile_content_factory);    
    }

    private void OnValidate()
    {
        if(board_size.x < 2)
        {
            board_size.x = 2;
        }

        if(board_size.y < 2)
        {
            board_size.y = 2;
        }
    }

    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
    }
    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            //tile.Content = tile_content_factory.Get(GameTileContentType.Destination);
            board.ToggleDestination(tile);
        }
    }
}
