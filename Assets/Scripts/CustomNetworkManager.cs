using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public class CustomNetworkManager : NetworkManager
{

    private const string PLAYER_TAG = "Player";

    [SerializeField]
    private LayerMask mask = -1;

    [SerializeField]
    private float spawnDistance = 15f;

    [SerializeField]
    private float spawnRange = 30f;

    public override Transform GetStartPosition()
    {
        Player[] players = GameManager.GetAllPlayers();

        if (players.Length == 1)
            return startPositions[Random.Range(0, startPositions.Count)];
        
        List<Transform> usableStartPositions = new List<Transform>(startPositions);

        foreach (Player player in players)
        {
            usableStartPositions.RemoveAll(t => Mathf.Abs(Vector3.Distance(t.position, player.transform.position) - spawnDistance) > spawnRange);
        }

        usableStartPositions = LineOfSight(usableStartPositions, players);

        if (usableStartPositions.Count > 0)
            return usableStartPositions[Random.Range(0, usableStartPositions.Count)];


        usableStartPositions = new List<Transform>(startPositions);
        usableStartPositions = LineOfSight(usableStartPositions, players);

        if (usableStartPositions.Count > 0)
            return usableStartPositions[Random.Range(0, usableStartPositions.Count)];


        return startPositions[Random.Range(0, startPositions.Count)];
    }

    private List<Transform> LineOfSight(List<Transform> positions, Player[] players)
    {
        List<Transform> usablePositions = new List<Transform>(positions);

        foreach (Transform startPos in usablePositions.ToList())
        {
            foreach (Player player in players)
            {
                RaycastHit _hit;
                if (Physics.Raycast(startPos.position + (2 * Vector3.up), player.transform.position - startPos.position - (2 * Vector3.up), out _hit, spawnDistance + spawnRange, mask))
                {
                    if (_hit.collider.tag == PLAYER_TAG)
                    {
                        usablePositions.Remove(startPos);
                    }
                }
            }
        }

        return usablePositions;
    }

    public override void OnStartClient() {
        //Stuff that happens when the client connects
        LevelLoader.instance.DoTransition();
    }

    public override void OnStartHost() {
        //Stuff that happens when the host starts
    }
}

