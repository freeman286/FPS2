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

    private Health health;

    void Start()
    {
        health = player.GetComponent<Health>();
    }

    void Update()
    {
        usernameText.text = player.username;
        healthBarFill.localScale = new Vector3(Mathf.Lerp(healthBarFill.localScale.x, health.GetHealthPct(), 10f * Time.deltaTime), 1f, 1f);
    }

}