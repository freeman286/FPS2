using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkMapGenerator : NetworkBehaviour
{

    [SyncVar]
    int networkSeed = 0;

    private MapGenerator mapGenerator;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
            networkSeed = (int)System.DateTime.Now.Ticks;

        mapGenerator = GetComponent<MapGenerator>();
        mapGenerator.seed = networkSeed;
        mapGenerator.GenerateMap();
    }

}
