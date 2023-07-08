using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellGenerator : MonoBehaviour
{
    public SpellInstance cacSpellPrefab;
    public SpellInstance projectileSpellPrefab;
    public SpellInstance laserSpellPrefab;

    [Header("Discharge")]
    public Projectile dischargeProjectilePrefab;
    public float minDischargeKnockback = 3;
    public float maxDischargeKnockback = 30;

    public static SpellGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CastSpell(Vector2 position, ICastSpell source, Vector2 direction, in SpellData data, Action<Health, SpellData> hitCallback, CompositeState isCastingState)
    {
        PerformSpell(position, source, direction, data, hitCallback, isCastingState);

        if (data.repeat > 0)
            StartCoroutine(RepeatSpellLoop(position, source, direction, data, hitCallback, isCastingState));
    }

    IEnumerator RepeatSpellLoop(Vector2 position, ICastSpell source, Vector2 direction, SpellData data, Action<Health, SpellData> hitCallback, CompositeState isCastingState)
    {
        CompositeStateToken isRepeatingToken = new CompositeStateToken();
        isRepeatingToken.SetOn(true);
        isCastingState.Add(isRepeatingToken);
        for (int i = 0; i < data.repeat; i++)
        {
            switch (data.type)
            {
                case SpellType.Laser:
                    //Change the direction
                     GetLaserRepeatDirections(data.repeat, i, direction, out Vector2 leftDir, out Vector2 rightDir);
                    PerformSpell(position, source, leftDir, in data, hitCallback, isCastingState);
                    PerformSpell(position, source, rightDir, in data, hitCallback, isCastingState);

                    break;
                case SpellType.Cac:
                case SpellType.Projectile:
                case SpellType.AOE:
                default:
                    yield return new WaitForSeconds(0.3f);
                    PerformSpell(position, source, direction, in data, hitCallback, isCastingState);
                    break;
            }
        }
        isRepeatingToken.SetOn(false);
        isCastingState.Remove(isRepeatingToken);
    }

    void PerformSpell(Vector2 position, ICastSpell source, Vector2 direction, in SpellData data, Action<Health, SpellData> hitCallback, CompositeState isCastingState)
    {
        SpellInstance newSpell;
        switch (data.type)
        {
            case SpellType.Cac:
            case SpellType.AOE:
            default:
                newSpell = Instantiate(cacSpellPrefab, transform);
                break;
            case SpellType.Projectile:
                newSpell = Instantiate(projectileSpellPrefab, transform);
                break;
            case SpellType.Laser:
                newSpell = Instantiate(laserSpellPrefab, transform);
                break;
        }

        newSpell.transform.position = position;
        newSpell.Init(in data, source, direction, hitCallback, isCastingState);

        if (data.discharge>0)
            StartCoroutine(SpellDischarge(position, source, direction, data, hitCallback));
    }

    void GetLaserRepeatDirections(int repeat, int id, Vector2 direction, out Vector2 leftDirection, out Vector2 rightDirection)
    {
        int angleInterval = 90 / (repeat + 1);

        leftDirection = Quaternion.AngleAxis(angleInterval * (id+1), Vector3.forward) * direction;
        rightDirection = Quaternion.AngleAxis(angleInterval * (id+1), Vector3.back) * direction;
    }

    IEnumerator SpellDischarge(Vector2 position, ICastSpell source, Vector2 direction, SpellData data, Action<Health, SpellData> hitCallback)
    {
        yield return new WaitForSeconds(0.2f);

        float knockback = 0;
        if(data.knockback > 0)
        {
            knockback = Curves.QuadEaseOut(minDischargeKnockback, maxDischargeKnockback, data.knockback);
        }

        int dischargeProjectiles = data.discharge + 3;
        float angleInterval = 360.0f / dischargeProjectiles;
        for (int i = 0; i < dischargeProjectiles; i++)
        {
            Vector2 projDirection = Quaternion.AngleAxis(angleInterval * i, Vector3.forward) * direction;
            Projectile newProj = Instantiate(dischargeProjectilePrefab, null);
            newProj.transform.position = position;
            newProj.Init(data, source.Source, projDirection, hitCallback, knockback);
        }
    }
}
