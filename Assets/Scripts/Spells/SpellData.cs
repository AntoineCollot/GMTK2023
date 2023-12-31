using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum SpellType { Cac, Projectile, Laser, AOE}
[System.Serializable]
public struct SpellData
{
    public const float HEAL_PER_DAMAGE = 0.5f;
    public const float MOVE_SPEED_BONUS_DURATION = 3;
    public const float MOVE_SPEED_BONUS_MIN = 1.25f;
    public const float MOVE_SPEED_BONUS_MAX = 3f;
    public const float COOLDOWN_REDUCTION_RATIO = 0.1f;

    public float MoveSpeedBonusMult => Curves.QuadEaseOut(MOVE_SPEED_BONUS_MIN, MOVE_SPEED_BONUS_MAX, movespeedBonus);
    public float Cooldown => baseCooldown * Mathf.Max(0.25f, (1 - cooldownReduction * COOLDOWN_REDUCTION_RATIO));

    [Header("Base (Not Upgradable)")]
    public Sprite icon;
    public string description;
    public SpellType type;
    public float baseCooldown;
    public float baseProjectileSpeed;

    [Header("Character buff")]
    public int heal;
    public int movespeedBonus;

    [Header("Effects on character")]
    public int repeat;
    public int discharge;
    public int cooldownReduction;

    [Header("Effects on spell")]
    public int damages;
    public int size;
    public int knockback;

    public void ApplyUpgrades(in SpellUpgradeData data)
    {
        heal += heal.AddOrMultiply(data.heal);
        movespeedBonus += movespeedBonus.AddOrMultiply(data.movespeedBonus);

        repeat += repeat.AddOrMultiply(data.repeat);
        discharge += discharge.AddOrMultiply(data.discharge);
        cooldownReduction += cooldownReduction.AddOrMultiply(data.cooldownReduction);

        damages += damages.AddOrMultiply(data.damages);
        size += size.AddOrMultiply(data.size);
        knockback += knockback.AddOrMultiply(data.knockback);
    }
}

[System.Serializable]
public struct SpellUpgradeData
{
    [Header("Character buff")]
    public int heal;
    public int movespeedBonus;

    [Header("Effects on character")]
    public int repeat;
    public int discharge;
    public int cooldownReduction;

    [Header("Effects on spell")]
    public int damages;
    public int size;
    public int knockback;
}
