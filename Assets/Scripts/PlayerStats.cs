using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class PlayerStats : NetworkBehaviour
{

    private WeaponManager weaponManager;

    private Set[] allSets;

    public List<Set> activeSets;

    [SyncVar]
    public string primaryWeaponName;

    [SyncVar]
    public string secondaryWeaponName;


    [Header("Cumulative effects")]
    public List<DamageAttribute> damageAttributes = new List<DamageAttribute>();
    public float speedMultiplier = 1f;
    public float jumpMultiplier = 1f;

    private List<Behaviour> scripts = new List<Behaviour>();

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
    }

    public void GetSets()
    {

        if (isLocalPlayer)
        {
            CmdSetPlayerInfo(PlayerInfo.primaryWeaponName, PlayerInfo.secondaryWeaponName);
        }

    }

    void UpdateSets()
    {
        allSets = SetsUtil.AllSets();

        Reset();

        SelectSets();

        AddSetAttributes();

        AddSetScripts();
    }

    [Command]
    void CmdSetPlayerInfo(string _primaryWeaponName, string _secondaryWeaponName)
    {
        RpcSetPlayerInfo(_primaryWeaponName, _secondaryWeaponName);
    }

    [ClientRpc]
    void RpcSetPlayerInfo(string _primaryWeaponName, string _secondaryWeaponName)
    {
        primaryWeaponName = _primaryWeaponName;
        secondaryWeaponName = _secondaryWeaponName;
        UpdateSets();
    }

    void Reset()
    {
        foreach (Behaviour _script in scripts)
        {
            _script.enabled = false;
        }

        activeSets.Clear();
        damageAttributes.Clear();
        scripts.Clear();

        speedMultiplier = 1f;
        jumpMultiplier = 1f;
    }

    void SelectSets()
    {
        foreach (Set _set in allSets)
        {
            if (SetsUtil.SetMatch(_set, primaryWeaponName, secondaryWeaponName))
            {
                activeSets.Add(_set);
            }
        }
    }

    void AddSetAttributes()
    {
        foreach (Set _set in activeSets)
        {
            speedMultiplier *= _set.speedMultiplier;
            jumpMultiplier *= _set.jumpMultiplier;

            foreach (DamageAttribute _attribute in _set.attributes)
            {
                DamageAttribute _damageAttribute = damageAttributes.FirstOrDefault(x => x.damageType == _attribute.damageType && x.self == _attribute.self);

                if (_damageAttribute != null)
                {
                    _damageAttribute.multiplier *= _attribute.multiplier;
                }
                else
                {
                    _damageAttribute = _attribute.Copy();
                    damageAttributes.Add(_damageAttribute);
                }
            }
        }
    }

    void AddSetScripts()
    {
        foreach (Set _set in activeSets)
        {
            if (_set.scriptsToEnable != null)
            {
                foreach (string _script in _set.scriptsToEnable)
                {

                    Behaviour _newScript = Util.EnableScipt(gameObject, _script, true);
                    if (_newScript != null)
                    {
                        scripts.Add(_newScript);
                    }

                }
            }
        }
    }
}
