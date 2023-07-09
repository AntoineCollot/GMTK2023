using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpells : MonoBehaviour, ICastSpell
{
    const int SPELL_COUNT = 3;
    public SpellData[] spells;
    bool[] hasSpell;
    float[] spellUsedTime;

    Direction lastInputDirection = Direction.Up;

    CompositeState isCastingState;

    Health health;
    InputMap inputMap;
    CharacterAnimations characterAnimations;

    public UnityEvent onSpellCastFinished = new UnityEvent();

    public static PlayerSpells Instance;

    public Source Source => Source.Player;
    public Transform SourceTransform => transform;

    private void Awake()
    {
        Instance = this;

        inputMap = new InputMap();
        isCastingState = new CompositeState();
        characterAnimations = GetComponentInChildren<CharacterAnimations>();
        inputMap.Gameplay.Spell1.performed += Spell1Performed;
        inputMap.Gameplay.Spell2.performed += Spell2Performed;
        inputMap.Gameplay.Spell3.performed += Spell3Performed;
        health = GetComponent<Health>();

        spells = new SpellData[SPELL_COUNT];
        spellUsedTime = new float[SPELL_COUNT];
        hasSpell = new bool[SPELL_COUNT];
    }

    private void Start()
    {
        PlayerMovement.Instance.lockMovementState.Add(isCastingState);

        //Getspells
        if (LoopManager.currentSpells != null)
        {
            for (int i = 0; i < LoopManager.currentSpells.Count; i++)
            {
                spells[i] = LoopManager.currentSpells[i];
            }
        }
    }

    private void Spell1Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        TryUseSpell(0);
    }
    private void Spell2Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        TryUseSpell(1);
    }

    private void Spell3Performed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        TryUseSpell(2);
    }

    private void OnEnable()
    {
        inputMap.Enable();
    }

    private void OnDisable()
    {
        inputMap.Disable();
    }

    private void Update()
    {
        Vector2 inputs = inputMap.Gameplay.Movement.ReadValue<Vector2>();
        if (inputs.magnitude > 0.7f)
        {
            lastInputDirection = inputs.ToDirection();
        }
    }

    public void TryUseSpell(int id)
    {
        if (!HasSpell(id))
            return;

        if (!CanUseSpell(id))
            return;

        if (PlayerMovement.Instance.lockMovementState.IsOn)
            return;

        spellUsedTime[id] = Time.time;
        CastSpell(in spells[id]);
    }

    public void CastSpell(in SpellData data)
    {
        Vector2 direction = lastInputDirection.ToVector2();
        SpellGenerator.Instance.CastSpell(transform.position, this, direction, in data, OnHitCallback, isCastingState);

        if (data.movespeedBonus > 0)
            PlayerMovement.Instance.GainMoveSpeedBonus(data.MoveSpeedBonusMult);
    }
    void OnHitCallback(Health hitHealth, SpellData data)
    {
        float damages = data.damages;
        if (data.type == SpellType.Laser)
            damages *= 1 / SpellInstanceLaser.LASER_HIT_COUNT;

        if (data.heal>0)
            health.Heal(damages * data.heal * SpellData.HEAL_PER_DAMAGE);
    }

    //1 full cooldown, <0 spell available
    public float SpellCurrentCooldown01(int id)
    {
        return 1-((Time.time - spellUsedTime[id]) / spells[id].Cooldown);
    }

    public bool CanUseSpell(int id)
    {
        return SpellCurrentCooldown01(id) <= 0;
    }

    public bool HasSpell(int id)
    {
        return hasSpell[id];
    }

    public void AddSpell(SpellData data, int id)
    {
        hasSpell[id] = true;
        spells[id] = data;

        UpdateLoopSpells();
    }

    public void ApplyUpgrade(int toSpellID, in SpellUpgradeData upgradeData)
    {
        spells[toSpellID].ApplyUpgrades(upgradeData);
        UpdateLoopSpells();
    }

    void UpdateLoopSpells()
    {
        LoopManager.currentSpells = spells.ToList();
    }

    public void OnSpellCastStarted()
    {
        characterAnimations.StartCast();
    }

    public void OnSpellCastFinished()
    {
        onSpellCastFinished.Invoke();
        characterAnimations.EndCast();
    }
}
