using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTarget : MonoBehaviour
{

    public int[,,] ids;
    public int[,,] targetIds;
    public float spawnProbability = 1f;

    public void SetTerrainTarget(float _gridSize)
    {
        SetTargetBounds(_gridSize);

        foreach (Transform child in transform)
        {
            TerrainId terrainId = child.GetComponent<TerrainId>();
            targetIds[(int)(child.localPosition.x / _gridSize), (int)(child.localPosition.z / _gridSize), (int)(child.localPosition.y / _gridSize)] = terrainId.targetId;
            ids[(int)(child.localPosition.x / _gridSize), (int)(child.localPosition.z / _gridSize), (int)(child.localPosition.y / _gridSize)] = terrainId.id;
        }
    }
    
    private void SetTargetBounds(float _gridSize)
    {

        int _maxX = 0;
        int _maxY = 0;
        int _maxZ = 0;

        foreach (Transform child in transform)
        {
            if (child.localPosition.x > _maxX * _gridSize)
            {
                _maxX = (int)(child.localPosition.x / _gridSize);
            }

            if (child.localPosition.z > _maxY * _gridSize)
            {
                _maxY = (int)(child.localPosition.z / _gridSize);
            }

            if (child.localPosition.y > _maxZ * _gridSize)
            {
                _maxZ = (int)(child.localPosition.y / _gridSize);
            }
        }

        targetIds = new int[_maxX + 1, _maxY + 1, _maxZ + 1];
        ids = new int[_maxX + 1, _maxY + 1, _maxZ + 1];
    }
}
