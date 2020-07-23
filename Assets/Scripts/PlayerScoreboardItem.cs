using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreboardItem : MonoBehaviour
{

    [SerializeField]
    Text usernameText = null;

    [SerializeField]
    Text killsText = null;

    [SerializeField]
    Text deathsText = null;

    public void Setup(string username, int kills, int deaths)
    {
        usernameText.text = username;
        killsText.text = "Kills: " + kills;
        deathsText.text = "Deaths: " + deaths;
    }

}
