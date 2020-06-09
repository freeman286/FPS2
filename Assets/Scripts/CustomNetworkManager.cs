using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public class CustomNetworkManager : NetworkManager
{

    private const string PLAYER_TAG = "Player";

    [SerializeField]
    private LayerMask mask;

    [SerializeField]
    private float spawnDistance;

    [SerializeField]
    private float spawnRange;

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
}

