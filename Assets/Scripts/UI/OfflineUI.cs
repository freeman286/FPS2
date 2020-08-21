using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Mirror;

public class OfflineUI : MonoBehaviour
{
    [HideInInspector]
    public NetworkManager networkManager;
    [HideInInspector]
    public NetworkManagerHUD networkManagerHUD;

    [SerializeField]
    private InputField ipAddress = null;

    [SerializeField]
    private Image loadingImage = null;

    [SerializeField]
    private RectTransform loadingRectTransform = null;

    private bool ipValueChanged = true;

    public void Start()
    {
        networkManager = NetworkManager.singleton;
        networkManagerHUD = networkManager.GetComponent<NetworkManagerHUD>();
        networkManagerHUD.enabled = false;

        ipAddress.text = PlayerInfo.ipAddress;

        loadingImage.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Host()
    {
        if (string.IsNullOrEmpty(PlayerInfo.playerName))
            return;

        loadingImage.enabled = true;
        InvokeRepeating("Loading", 0f, 0.01f);

        StartCoroutine(Host_Coroutine());

    }

    private IEnumerator Host_Coroutine()
    {
        LevelLoader.instance.DoTransition();
        yield return new WaitForSeconds(1f);
        networkManager.StartHost();
    }

    public void Client()
    {
        if (string.IsNullOrEmpty(PlayerInfo.playerName))
            return;

        if (ipValueChanged && !string.IsNullOrEmpty(PlayerInfo.ipAddress))
        {
            loadingImage.enabled = true;
            InvokeRepeating("Loading", 0f, 0.01f);
            networkManager.networkAddress = PlayerInfo.ipAddress;
            networkManager.StartClient();
            ipValueChanged = false;
        }
        
    }

    void Loading()
    {
        loadingRectTransform.Rotate(new Vector3(0, 0, 1));
    }

    public void IpValueChange()
    {
        ipValueChanged = true;
        PlayerInfo.ipAddress = ipAddress.text;
    }
}
