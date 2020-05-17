using UnityEngine;
using UnityEngine.UI;
using Mirror;

using System.Net;
using System.Net.Sockets;

public class PlayerUI : MonoBehaviour
{

    [SerializeField]
    private GameObject crosshair;

    [SerializeField]
    private GameObject healthBar;

    [SerializeField]
    private RectTransform healthBarFill;

    [SerializeField]
    private GameObject ammo;

    [SerializeField]
    private Text ammoText;

    [SerializeField]
    private GameObject scoreboard;

    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private GameObject ipAddress;

    [SerializeField]
    private Text IpAddressText;

    [SerializeField]
    private Slider lookSensitivitySlider;

    [SerializeField]
    private Text lookSensitivityText;

    public float lookSensitivity;

    private Player player;
    private PlayerController controller;
    private WeaponManager weaponManager;

    [HideInInspector]
    public NetworkManager networkManager;
    [HideInInspector]
    public NetworkManagerHUD networkManagerHUD;

    public void SetPlayer(Player _player)
    {
        player = _player;
        controller = player.GetComponent<PlayerController>();
        weaponManager = player.GetComponent<WeaponManager>();

        networkManager = NetworkManager.singleton;
        networkManagerHUD = networkManager.GetComponent<NetworkManagerHUD>();
        networkManagerHUD.enabled = false;

        IpAddressText.text = "IP Address: " + Util.LocalIPAddress();
    }

    void Update()
    {

        if (Pause.IsOn)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        SetHealthAmount(player.GetHealthPct());
        SetAmmoAmount(weaponManager.GetCurrentWeapon().bullets);

        if (Input.GetButtonDown("Cancel"))
        {
            TogglePauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboard.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboard.SetActive(false);
        }

        ipAddress.SetActive(scoreboard.activeSelf || pauseMenu.activeSelf);
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        Pause.IsOn = pauseMenu.activeSelf;
    }

    void SetHealthAmount(float _amount)
    {
        healthBarFill.localScale = new Vector3(1f, Mathf.Lerp(healthBarFill.localScale.y, _amount, 20f * Time.deltaTime), 1f);
    }

    void SetAmmoAmount(int _amount)
    {
        ammoText.text = _amount.ToString();
    }

    public void Alive()
    {
        crosshair.SetActive(true);
        healthBar.SetActive(true);
        ammo.SetActive(true);
    }

    public void Death()
    {
        crosshair.SetActive(false);
        healthBar.SetActive(false);
        ammo.SetActive(false);
    }

    public void UpdateLookSensitivity()
    {
        lookSensitivity = (float)System.Math.Round(lookSensitivitySlider.value, 1);
        controller.lookSensitivity = lookSensitivity;
        lookSensitivityText.text = string.Format("Sensitivity: {0:F1}", lookSensitivity); ;
    }


}