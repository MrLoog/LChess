using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{

    [SerializeField]
    GameTile tilePrefab = default;

    [SerializeField]
    Transform board = default;

    public Vector2Int Size { get; private set; }

    [SerializeField]
    Texture2D gridTexture = default;

    bool showGrid;

    public bool ShowGrid
    {
        get => showGrid;
        set
        {
            showGrid = value;
            Material m = board.GetComponent<MeshRenderer>().material;
            if (showGrid)
            {
                m.mainTexture = gridTexture;
                m.SetTextureScale("_MainTex", Size);
            }
            else
            {
                m.mainTexture = null;
            }
        }
    }

    public void Initialize(Vector2Int size)
    {
        Size = size;
        board.localScale = new Vector3(size.x, size.y, 1f);
        Vector2 offset = new Vector2(
            (size.x - 1) * 0.5f, (size.y - 1) * 0.5f
        );
        tiles = new GameTile[size.x * size.y];

        for (int i = 0, y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++, i++)
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(
                    x - offset.x, 0f, y - offset.y
                );
            }
        }

        for (int i = 0; i < (size.x * size.y); i++)
        {
            GameTile tile = tiles[i];
            tile.NTile = (i - size.x > -1) ? tiles[i - size.x] : null;
            // tile.NETile = (i - size.x + 1 > -1) ? tiles[i - size.x + 1] : null;
            // tile.WNTile = (i - size.x - 1 > -1) ? tiles[i - size.x - 1] : null;

            tile.ETile = ((i + 1) % size.y > 0) ? tiles[i + 1] : null;
            tile.WTile = ((i + 1) % size.y > 1) ? tiles[i - 1] : null;

            tile.STile = (i + size.x + 1 < (size.x * size.y)) ? tiles[i + size.x] : null;
            // tile.ESTile = (i + size.x + 2 < (size.x * size.y)) ? tiles[i + size.x + 1] : null;
            // tile.SWTile = (i + size.x < (size.x * size.y)) ? tiles[i + size.x - 1] : null;
        }

        for (int i = 0; i < (size.x * size.y); i++)
        {
            GameTile tile = tiles[i];
            tile.NETile = (tile.NTile != null) ? tile.NTile.ETile : null;
            tile.WNTile = (tile.NTile != null) ? tile.NTile.WTile : null;

            tile.ESTile = (tile.STile != null) ? tile.STile.ETile : null;
            tile.SWTile = (tile.STile != null) ? tile.STile.WTile : null;
        }


        MarkPrepareTile();
    }

    internal GameTile GetTile(int x, int y)
    {
        if (x >= 0 && x < Size.x && y >= 0 && y < Size.y)
        {
            return tiles[x + y * Size.x];
        }
        Debug.Log(string.Format("Wrong x/y {0}/{1}", x, y));
        return null;
    }

    public GameTile GetTile(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            int x = (int)(hit.point.x + Size.x * 0.5f);
            int y = (int)(hit.point.z + Size.y * 0.5f);
            if (x >= 0 && x < Size.x && y >= 0 && y < Size.y)
            {
                return tiles[x + y * Size.x];
            }
        }
        return null;
    }


    GameTile[] tiles;
    public int MaxRowArea = 3;

    internal GameTile GetEmptyTileGroup(int group)
    {
        int count = 0;
        int xFrom = group == 0 ? 0 : (Size.y - 1);
        int reverse = group == 0 ? 1 : -1;
        GameTile emptyTile = null;
        do
        {
            count++;
            for (int i = 0; i < Size.x; i++)
            {
                emptyTile = tiles[xFrom * Size.x + i];
                if (emptyTile.Monster != null || emptyTile.ActionUnit != null)
                {
                    emptyTile = null;
                    continue;
                }
                return emptyTile;
            }
            xFrom = xFrom + reverse * 1;

        } while (emptyTile == null && count < MaxRowArea);
        return null;
    }

    internal GameTile GetRandomEmptyTileGroup(int group)
    {

        int max = Size.x * MaxRowArea;
        int indexMin = group == 0 ? 0 : (Size.y * Size.x - max);
        int indexMax = group == 0 ? max : (Size.y * Size.x);

        List<GameTile> validTiles = tiles.Select((tile, index) => new
        {
            Tile = tile,
            i = index
        }).Where(a => a.i >= indexMin && a.i < indexMax && a.Tile.Monster == null && a.Tile.ActionUnit == null)
        .Select(a => a.Tile)
        .ToList();
        if (validTiles.Count == 0) return null;

        int randomIndex = Random.Range(0, validTiles.Count - 1);
        return validTiles[randomIndex];
    }

    public GameTile GetBattleTileGroup(int group)
    {
        int max = Size.x * MaxRowArea;
        int indexMin = group == 0 ? (Size.x) : (Size.y * Size.x - max - Size.y);
        int indexMax = group == 0 ? (max + Size.x) : (Size.y * Size.x - Size.y);

        List<GameTile> validTiles = GetBattleTilesGroup(group, true);

        if (validTiles.Count == 0) return null;

        int randomIndex = Random.Range(0, validTiles.Count - 1);
        return validTiles[randomIndex];
    }


    public List<GameTile> GetBattleTilesGroup(int group, bool empty = false)
    {
        int max = Size.x * MaxRowArea;
        int indexMin = group == 0 ? (Size.x) : (Size.y * Size.x - max - Size.y);
        int indexMax = group == 0 ? (max + Size.x) : (Size.y * Size.x - Size.y);
        if (group == -1)
        {
            indexMin = Size.x;
            indexMax = Size.y * Size.x;
        }

        return tiles.Select((tile, index) => new
        {
            Tile = tile,
            i = index
        }).Where(a => a.i >= indexMin && a.i < indexMax && (!empty || (a.Tile.Monster == null && a.Tile.ActionUnit == null)))
        .Select(a => a.Tile)
        .ToList();
    }

    private void MarkPrepareTile()
    {
        int max = Size.x * 1;
        int indexMin = 0;
        int indexMax = max;

        tiles.Select((tile, index) => new
        {
            Tile = tile,
            i = index
        }).Where(a => a.i >= indexMin && a.i < indexMax)
        .Select(a => a.Tile)
        .ToList().ForEach(x => x.PrepareTile = true);
    }

    public GameTile GetPrepareTile()
    {
        List<GameTile> validTiles = tiles.Select((tile, index) => new
        {
            Tile = tile,
            i = index
        }).Where(a => a.Tile.PrepareTile && a.Tile.Monster == null && a.Tile.ActionUnit == null)
        .Select(a => a.Tile)
        .ToList();
        if (validTiles.Count == 0) return null;

        int randomIndex = Random.Range(0, validTiles.Count - 1);
        return validTiles[randomIndex];
    }
}
