using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainId : MonoBehaviour
{
    public int id; // -1 null, 0 block, 1 void, 2 floor, 3 open terrain, 4 closed terrain
    public int targetId;
}
