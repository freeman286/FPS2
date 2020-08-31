using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

[System.Serializable]
public class DamageEvent : UnityEvent<string>{}

public class Health : NetworkBehaviour
{
    [SyncVar]
    public string playerID;

    [SyncVar]
    public bool isDead = false;

    public DamageEvent dieEvent;

    public DamageEvent damageEvent;

    [SerializeField]
    public int maxHealth = 100;

    [SyncVar]
    private float currentHealth;

    [SerializeField]
    private float healthRegenTime = 10f;

    [SerializeField]
    private float healthRegenSpeed = 10f;

    private float timeSinceDamaged = 0f;

    [HideInInspector]
    public Player lastDamagedPlayer;

    public float GetHealthPct()
    {
        return Mathf.Clamp((float)currentHealth / maxHealth, 0, 1);
    }

    void Start()
    {
        
        Player _player = GetComponent<Player>();

        if (_player != null)
        {
            _player.onPlayerSetDefaultsCallback += SetDefaults;
        } else
        {
            SetDefaults();
        }

    }

    [ServerCallback]
    void Update()
    {
        timeSinceDamaged += Time.deltaTime;

        if (healthRegenTime != -1f && timeSinceDamaged > healthRegenTime && currentHealth < maxHealth)
        {
            currentHealth += healthRegenSpeed * Time.deltaTime;
        }
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _sourceID, string _damageType)
    {
        if (isDead)
            return;

        lastDamagedPlayer = GameManager.GetPlayer(_sourceID);

        if (lastDamagedPlayer != null)
            _amount = (int)(_amount * lastDamagedPlayer.GetComponent<PlayerStats>().GetDamageMultiplier(_damageType, false));

        Stats _stats = GetComponent<Stats>();

        if (_stats != null)
            _amount = (int)(_amount * _stats.GetDamageMultiplier(_damageType, true));

        currentHealth = Mathf.Max(currentHealth - _amount, 0f);

        timeSinceDamaged = 0f;

        damageEvent.Invoke(_sourceID);

        if (currentHealth == 0)
        {
            isDead = true;

            if (dieEvent != null)
                dieEvent.Invoke(_sourceID);
        }
    }

    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;
    }
}
