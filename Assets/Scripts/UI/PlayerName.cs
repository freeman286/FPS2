using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    [SerializeField]
    private InputField playerName = null;

    void Start()
    {
        playerName.text = PlayerPrefs.GetString(PlayerUtil.PLAYER_NAME_KEY, "Player");
    }

    public void Changed()
    {
        PlayerPrefs.SetString(PlayerUtil.PLAYER_NAME_KEY, playerName.text);

        if (string.IsNullOrEmpty(playerName.text))
        {
            playerName.image.color = Color.red;
        } else
        {
            playerName.image.color = Color.white;
        }
    }
}
