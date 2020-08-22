using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillStreakSpawnManager : MonoBehaviour
{
    public static KillStreakSpawnManager instance;

    private static Dictionary<KillStreak, Transform> spawnPoints = new Dictionary<KillStreak, Transform>();

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one KillStreakSpawnManager in scene.");
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        foreach (Transform _child in transform)
        {
            KillStreakSpawnPoint _killStreakSpawnPoint = _child.GetComponent<KillStreakSpawnPoint>();

            if (_killStreakSpawnPoint != null)
            {
                spawnPoints.Add(_killStreakSpawnPoint.killStreak, _child.transform);
            }

        }
    }

    public static Vector3 GetKillStreakSpawnPoint(KillStreak _killStreak)
    {
        return spawnPoints[_killStreak].position;
    }
}
