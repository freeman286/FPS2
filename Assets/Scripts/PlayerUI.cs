using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [SerializeField]
    GameObject crosshair;

    [SerializeField]
    GameObject healthBar;

    [SerializeField]
    RectTransform healthBarFill;

    [SerializeField]
    GameObject ammo;

    [SerializeField]
    Text ammoText;

    [SerializeField]
    GameObject scoreboard;

    [SerializeField]
    GameObject pauseMenu;

    private Player player;
    private PlayerController controller;
    private WeaponManager weaponManager;

    public void SetPlayer(Player _player)
    {
        player = _player;
        controller = player.GetComponent<PlayerController>();
        weaponManager = player.GetComponent<WeaponManager>();
    }

    void Update()
    {

        SetHealthAmount(player.GetHealthPct());
        SetAmmoAmount(weaponManager.GetCurrentWeapon().bullets);

        if (Input.GetKeyDown(KeyCode.Escape))
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
        Debug.Log("Alive");
        crosshair.SetActive(true);
        healthBar.SetActive(true);
        ammo.SetActive(true);
    }

    public void Death()
    {
        Debug.Log("Death");
        crosshair.SetActive(false);
        healthBar.SetActive(false);
        ammo.SetActive(false);
    }


}