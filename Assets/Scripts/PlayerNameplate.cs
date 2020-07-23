using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameplate : MonoBehaviour
{

    [SerializeField]
    private Text usernameText = null;

    [SerializeField]
    private RectTransform healthBarFill = null;

    [SerializeField]
    private Player player = null;

    void Update()
    {
        usernameText.text = player.username;
        healthBarFill.localScale = new Vector3(Mathf.Lerp(healthBarFill.localScale.x, player.GetHealthPct(), 10f * Time.deltaTime), 1f, 1f);
    }

}