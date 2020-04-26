using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    [SerializeField]
    private InputField playerName;

    void Start()
    {
        playerName.text = "Player";
    }

    public void Changed()
    {
        PlayerInfo.playerName = playerName.text;

        if (string.IsNullOrEmpty(playerName.text))
        {
            playerName.image.color = Color.red;
        } else
        {
            playerName.image.color = Color.white;
        }
    }
}
