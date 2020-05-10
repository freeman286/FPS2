﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

    public Transform tilePrefab;
    public Transform blockPrefab;
    public Transform voidPrefab;
    public Vector3 mapSize;
    public int mapFill;

    public float turnLeftThreshold = 0.9f;
    public float turnRightThreshold = 0.9f;


    private List<Coord> allTileCoords;

    private List<Coord> emptyCoords;

    public int seed = 0;
    private Coord mapCentre;

    public int[,,] blockMap;

    public GameObject[,,] blocks;

    public GameObject[] allTerrain;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        Random.seed = seed;

        if (mapSize.z < 2)
            mapSize.z = 2;

        GetAllTerrain();


        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;       



        emptyCoords = new List<Coord>();
        Coord _randomCoord = new Coord(0, 0);

        int _esc = 0;

        //Create Main

        while (emptyCoords.Count < mapFill && _esc < 1000)
        {
            emptyCoords.Clear();
            blockMap = new int[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
            blocks = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];


            while (!InPlayArea(_randomCoord))
            {
                _randomCoord = GetRandomCoord();
            }

            Vector2 _dir = Util.SnapTo(mapCentre.ToVector2() - _randomCoord.ToVector2());

            ClearLoop(_randomCoord, _dir);
            _esc++;
        }

        //Create additional loop to connect map

        ClearLoop(_randomCoord, Util.SnapTo(_randomCoord.ToVector2()) - mapCentre.ToVector2());

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {

                Vector3 tilePosition = CoordToPosition(x, y, 0);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as Transform;
                newTile.parent = mapHolder;

                blockMap[x, y, 0] = 2;

                blocks[x, y, 0] = newTile.gameObject;

                for (int z = 1; z < mapSize.z; z++)
                {
                    Vector3 blockPosition = CoordToPosition(x, y, z);
                    if (blockMap[x, y, z] == 0)
                    {
                        Transform newBlock = Instantiate(blockPrefab, blockPosition, Quaternion.identity) as Transform;
                        newBlock.parent = mapHolder;
                        blocks[x, y, z] = newBlock.gameObject;
                    }
                    else if (blockMap[x, y, z] == 1)
                    {
                        Transform newVoid = Instantiate(voidPrefab, blockPosition, Quaternion.identity) as Transform;
                        newVoid.parent = mapHolder;
                        blocks[x, y, z] = newVoid.gameObject;
                    }
                }
            }
        }

    }

    Vector3 CoordToPosition(int x, int y, int z)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0.5f + z, -mapSize.y / 2 + 0.5f + y) * tilePrefab.localScale.x;
    }

    private void ClearColumn(Coord _coord)
    {
        for (int z = 1; z < mapSize.z; z++)
        {
            blockMap[_coord.x, _coord.y, z] = 1;
            emptyCoords.Add(_coord);
        }
    }

    private void ClearLoop(Coord _start, Vector2 _dir)
    {

        Coord _currentCoord = _start;

        while (InPlayArea(_currentCoord) && !emptyCoords.Contains(_currentCoord + _dir))
        {
            ClearColumn(_currentCoord);

            float _rng = Random.Range(0f, 1.0f);

            if (_rng > turnLeftThreshold)
            {
                _dir = _dir.Rotate(90f);
            } else if (_rng < 1 - turnLeftThreshold)
            {
                _dir = _dir.Rotate(-90f);
            }

            if (!InPlayArea(_currentCoord + _dir))
            {
                _dir = Util.SnapTo(mapCentre.ToVector2() - _currentCoord.ToVector2());
                if (emptyCoords.Contains(_currentCoord + _dir))
                {
                    _dir = _dir.Rotate(90f);
                }
            }

            _currentCoord += _dir;

        }

        if (InPlayArea(_currentCoord))
            ClearColumn(_currentCoord);
    }

    public Coord GetRandomCoord()
    {
        return allTileCoords[Random.Range(0, allTileCoords.Count)];
    }

    private bool InMap(Coord _coord)
    {
        return _coord.x >= 0 && _coord.y >= 0 && _coord.x < mapSize.x && _coord.y < mapSize.y;
    }

    private bool InPlayArea(Coord _coord)
    {
        return _coord.x > 0 && _coord.y > 0 && _coord.x < mapSize.x - 1 && _coord.y < mapSize.y - 1;
    }

    private void GetAllTerrain()
    {
        Object[] allTerrainObjects = Resources.LoadAll("Prefabs/Map/Terrain", typeof(GameObject));

        allTerrain = new GameObject[allTerrainObjects.Length];

        for (int i = 0; i < allTerrainObjects.Length; i++)
        {
            allTerrain[i] = ((GameObject)allTerrainObjects[i]);
            allTerrain[i].GetComponent<TerrainTarget>().SetTerrainTarget(tilePrefab.localScale.x);
        }
    }

    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }

        public static Coord operator +(Coord c1, Vector2 v1)
        {
            return new Coord(c1.x + (int)v1.x, c1.y + (int)v1.y);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

    }
}