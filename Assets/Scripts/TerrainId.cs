using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainId : MonoBehaviour
{
    public int id; // -3 don't care, -2 target only, -1 null, 0 block, 1 void, 2 floor, 3 open terrain, 4 closed terrain, 5 bridging terrain, 6 raised terrain
    public int targetId;
}
