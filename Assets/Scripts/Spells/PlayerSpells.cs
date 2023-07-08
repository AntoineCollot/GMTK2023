using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpells : MonoBehaviour
{
    public SpellData[] spells;
    const int SPELL_COUNT = 3;

    public float[] spellUsedTime;

    Health health;

    InputMap inputMap;

    private void Awake()
    {
        inputMap = new InputMap();
        inputMap.Gameplay.Spell1.performed += Spell1Performed;
        inputMap.Gameplay.Spell2.performed += Spell2Performed;
        inputMap.Gameplay.Spell3.performed += Spell3Performed;

        spellUsedTime = new float[SPELL_COUNT];
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

    public void TryUseSpell(int id)
    {
        if (!CanUseSpell(id))
            return;

        spellUsedTime[id] = Time.time;
        CastSpell(in spells[id]);
    }

    public void CastSpell(in SpellData data)
    {
        SpellGenerator.Instance.CastSpell(transform.position, Source.Player, in data, OnHitCallback);

        if (data.repeat > 0)
            StartCoroutine(RepeatSpell(data));
    }

    IEnumerator RepeatSpell(SpellData data)
    {
        for (int i = 0; i < data.repeat; i++)
        {
            switch (data.type)
            {
                case SpellType.Cac:
                case SpellType.Projectile:
                case SpellType.Laser:
                case SpellType.AOE:
                default:
                    yield return new WaitForSeconds(0.3f);
                    break;
            }

            SpellGenerator.Instance.CastSpell(transform.position, Source.Player, in data, OnHitCallback);
        }
    }

    void OnHitCallback(Health hitHealth, SpellData data)
    {
        if(data.heal>0)
            health.Heal(data.damages * data.heal * SpellData.HEAL_PER_DAMAGE);
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
