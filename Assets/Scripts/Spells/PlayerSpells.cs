using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpells : MonoBehaviour
{
    public SpellData[] spells;
    const int SPELL_COUNT = 3;

    public float[] spellUsedTime;
    Direction lastInputDirection = Direction.Up;

    CompositeState isCastingState;

    Health health;
    InputMap inputMap;

    private void Awake()
    {
        inputMap = new InputMap();
        isCastingState = new CompositeState();
        inputMap.Gameplay.Spell1.performed += Spell1Performed;
        inputMap.Gameplay.Spell2.performed += Spell2Performed;
        inputMap.Gameplay.Spell3.performed += Spell3Performed;

        spellUsedTime = new float[SPELL_COUNT];
    }

    private void Start()
    {
        PlayerMovement.Instance.lockMovementState.Add(isCastingState);
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
        if (!CanUseSpell(id))
            return;

        spellUsedTime[id] = Time.time;
        CastSpell(in spells[id]);
    }

    public void CastSpell(in SpellData data)
    {
        Vector2 direction = lastInputDirection.ToVector2();
        SpellGenerator.Instance.CastSpell(transform.position, Source.Player, direction, in data, OnHitCallback, isCastingState);

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
}
