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

    private bool loading = false;

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

        Loading(true);

        StartCoroutine(Host_Coroutine());

    }

    void Update()
    {
        if (NetworkClient.active && ClientScene.ready)
            LevelLoader.instance.DoTransition();

        if (loading)
            loadingRectTransform.Rotate(Vector3.forward * Time.deltaTime * 50f);
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
            networkManager.networkAddress = PlayerInfo.ipAddress;
            networkManager.StartClient();
            ipValueChanged = false;

            Loading(true);
        }
        
    }

    void Loading(bool _enabled)
    {
        loading = _enabled;
        loadingImage.enabled = _enabled;
    }

    public void IpValueChange()
    {
        ipValueChanged = true;
        PlayerInfo.ipAddress = ipAddress.text;
    }
}
