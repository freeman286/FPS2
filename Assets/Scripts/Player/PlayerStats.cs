using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class PlayerStats : Stats
{

    private WeaponManager weaponManager;

    private Set[] allSets;

    public List<Set> activeSets;

    [SerializeField]
    private ListType[] listTypes = new ListType[3];

    [SyncVar(hook = nameof(OnPrimaryNameChanged))]
    public string primaryWeaponName;

    [SyncVar(hook = nameof(OnSecondaryNameChanged))]
    public string secondaryWeaponName;

    [SyncVar(hook = nameof(OnEquipmentNameChanged))]
    public string equipmentName;

    [SyncVar(hook = nameof(OnMovementAbilityNameChanged))]
    public string movementAbilityName;
    
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
            CmdSetPlayerInfo(PlayerInfo.GetNameSelected(listTypes[0]), PlayerInfo.GetNameSelected(listTypes[1]), PlayerInfo.GetNameSelected(listTypes[2]), PlayerInfo.GetNameSelected(listTypes[3]));
        }

    }

    void OnPrimaryNameChanged(string _oldName, string _newName)
    {
        primaryWeaponName = _newName;
        UpdateSets(_oldName, _newName);
    }

    void OnSecondaryNameChanged(string _oldName, string _newName)
    {
        secondaryWeaponName = _newName;
        UpdateSets(_oldName, _newName);
    }

    void OnEquipmentNameChanged(string _oldName, string _newName)
    {
        equipmentName = _newName;
        UpdateSets(_oldName, _newName);
    }

    void OnMovementAbilityNameChanged(string _oldName, string _newName)
    {
        movementAbilityName = _newName;
        UpdateSets(_oldName, _newName);
    }

    void UpdateSets(string _oldName, string _newName)
    {
        if (_oldName == _newName)
            return;

        allSets = SetsUtil.AllSets();

        Reset();

        SelectSets();

        AddSetAttributes();

        AddSetScripts();
    }

    [Command]
    void CmdSetPlayerInfo(string _primaryWeaponName, string _secondaryWeaponName, string _equipmentName, string _movementAbilityName)
    {
        primaryWeaponName = _primaryWeaponName;
        secondaryWeaponName = _secondaryWeaponName;
        equipmentName = _equipmentName;
        movementAbilityName = _movementAbilityName;
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
            if (SetsUtil.SetMatch(_set, primaryWeaponName, secondaryWeaponName, equipmentName, movementAbilityName))
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
                foreach (ScriptID _scriptID in _set.scriptsToEnable)
                {

                    Behaviour _newScript = Util.EnableScipt(gameObject, _scriptID, true);
                    if (_newScript != null)
                    {
                        scripts.Add(_newScript);
                    }

                }
            }
        }
    }
}
