using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillStreakUtil : MonoBehaviour
{
    public static KillStreak[] AllKillStreaks()
    {
        KillStreak[] allKillStreaks = Resources.LoadAll<KillStreak>("ScriptableObjects/KillStreaks");

        return allKillStreaks;
    }

    public static KillStreak NameToKillStreak(string _name)
    {
        KillStreak[] allKillStreaks = AllKillStreaks();

        foreach (var killStreak in allKillStreaks)
        {
            if (killStreak.name == _name)
            {
                return killStreak;
            }
        }
        return null;
    }

}
