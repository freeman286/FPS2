using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

    public Transform tilePrefab;
    public Transform blockPrefab;
    public Transform voidPrefab;
    public Vector2 mapSize;

    private List<Coord> allTileCoords;
    private Queue<Coord> shuffledTileCoords;

    public int seed = 10;
    private Coord mapCentre;

    public int[,,] blockMap;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        
        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Util.ShuffleArray(allTileCoords.ToArray(), seed));
        mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);

        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        blockMap = new int[(int)mapSize.x, (int)mapSize.y, 4];


        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y, 0);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as Transform;
                newTile.parent = mapHolder;

                blockMap[x, y, 0] = 2;
            }
        }



        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (Random.Range(0.0f, 1.0f) > 0.5f)
                {

                    for (int z = 1; z < 4; z++)
                    {
                        Vector3 blockPosition = CoordToPosition(x, y, z);

                        Transform newBlock = Instantiate(blockPrefab, blockPosition, Quaternion.identity) as Transform;
                        newBlock.parent = mapHolder;
                        blockMap[x, y, z] = 0;
                    }
                }
                
            }
        }


    }

    Vector3 CoordToPosition(int x, int y, int z)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0.5f + z, -mapSize.y / 2 + 0.5f + y) * tilePrefab.localScale.x;
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
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

    }
}
