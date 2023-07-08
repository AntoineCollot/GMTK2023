using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellGenerator : MonoBehaviour
{
    public SpellInstance cacSpellPrefab;
    public SpellInstance projectileSpellPrefab;
    public SpellInstance laserSpellPrefab;

    public static SpellGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public SpellInstance CastSpell(Vector2 position, Source source, Vector2 direction, in SpellData data, Action<Health, SpellData> hitCallback)
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
        newSpell.Init(in data, source, direction, hitCallback);
        return newSpell;
    }
}
