using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameplate : MonoBehaviour
{

    [SerializeField]
    private Text usernameText;

    [SerializeField]
    private RectTransform healthBarFill;

    [SerializeField]
    private Player player;

    void Update()
    {
        usernameText.text = player.username;
        healthBarFill.localScale = new Vector3(Mathf.Lerp(healthBarFill.localScale.x, player.GetHealthPct(), 10f * Time.deltaTime), 1f, 1f);
    }

}