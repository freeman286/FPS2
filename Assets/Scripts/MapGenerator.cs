using UnityEngine;
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

    private GameObject[] allTerrain;

    private Transform mapHolder;

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

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;       



        emptyCoords = new List<Coord>();
        Coord _randomCoord = new Coord(0, 0);

        int _esc = 0;

        //Create Main

        while (emptyCoords.Count < mapFill && _esc < 10000)
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

                        if (!InPlayArea(new Coord(x, y)))
                            blockMap[x, y, z] = -1; // So terrain can't spawn at edge of map


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

        InsertTerrain();

    }

    private void InsertTerrain()
    {

        for (int i = 0; i < allTerrain.Length; i++)
        {

            TerrainTarget terrainTarget = allTerrain[i].GetComponent<TerrainTarget>();

            int[,,] targetIds = terrainTarget.targetIds;
            int[,,] ids = terrainTarget.ids;

            Debug.Log(allTerrain[i].transform.name);
            Debug.Log(Util.FormatMatrix(targetIds));

            for (int r = 0; r < 4; r++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    for (int y = 0; y < mapSize.y; y++)
                    {
                        for (int z = 0; z < mapSize.z; z++)
                        {

                            if (Random.Range(0f, 1.0f) < terrainTarget.spawnProbability && CheckLocation(x, y, z, targetIds))
                            {
                                InsertLocation(x, y, z, r, targetIds, ids, allTerrain[i]);
                            }

                        }
                    }
                }


                targetIds = Util.RotateMatrix(targetIds);
                ids = Util.RotateMatrix(ids);
            }
            

        }
    }

    private bool CheckLocation(int _x, int _y, int _z, int[,,]  _targetIds)
    {
        for (int i = 0; i < _targetIds.GetLength(0); i++)
        {
            for (int j = 0; j < _targetIds.GetLength(1); j++)
            {
                for (int k = 0; k < _targetIds.GetLength(2); k++)
                {

                    if (!InPlayArea(new Coord(_x + i, _y + j)) || _z + k > mapSize.z-1 || (_targetIds[i, j, k] != blockMap[_x + i, _y + j, _z + k] && _targetIds[i, j, k] != -3))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void InsertLocation(int _x, int _y, int _z, int r, int[,,] _targetIds, int[,,] _ids, GameObject _terrain)
    {

        TerrainTarget _terrainTarget = _terrain.GetComponent<TerrainTarget>();

        Coord _pos = OfsetPostion(new Coord(_x, _y), r, _targetIds);

        GameObject newTerrain = (GameObject)Instantiate(_terrain, CoordToPosition(_pos.x, _pos.y, _z), Quaternion.Euler(0, r * -90f, 0));
        newTerrain.transform.parent = mapHolder;
        newTerrain.transform.SetAsFirstSibling();

        _terrainTarget.SetTerrainTarget(tilePrefab.localScale.x);

        GameObject[,,] newBlocks = new GameObject[_terrainTarget.ids.GetLength(0), _terrainTarget.ids.GetLength(1), _terrainTarget.ids.GetLength(2)];

        foreach (Transform child in newTerrain.transform)
        {
            newBlocks[(int)(child.localPosition.x / tilePrefab.localScale.x), (int)(child.localPosition.z / tilePrefab.localScale.x), (int)(child.localPosition.y / tilePrefab.localScale.x)] = child.gameObject;
        }

        for (int a = 1; a < r + 1; a++)
        {
            newBlocks = Util.RotateGameObjectMatrix(newBlocks);
        }

        for (int i = 0; i < _targetIds.GetLength(0); i++)
        {
            for (int j = 0; j < _targetIds.GetLength(1); j++)
            {
                for (int k = 0; k < _targetIds.GetLength(2); k++)
                {
                    if (_ids[i, j, k] != -2)
                    {

                        blockMap[_x + i, _y + j, _z + k] = _ids[i, j, k];

                        GameObject oldBlock = blocks[_x + i, _y + j, _z + k];

                        DestroyImmediate(oldBlock);

                        blocks[_x + i, _y + j, _z + k] = newBlocks[i, j, k];
                    }
                }
            }
        }
    }

    private Coord OfsetPostion(Coord _pos, int r, int[,,] _targetIds)
    {
        return _pos + new Vector2(((r == 1 || r == 2) ? _targetIds.GetLength(0) - 1 : 0), ((r == 2 || r == 3) ? _targetIds.GetLength(1) - 1 : 0));
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

            if (!InPlayArea(_currentCoord + 6 * _dir))
            {
                _dir = Util.SnapTo(mapCentre.ToVector2() - _currentCoord.ToVector2());
                if (emptyCoords.Contains(_currentCoord + 3 * _dir))
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
