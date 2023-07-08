using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum SpellType { Cac, Projectile, Laser, AOE}
[System.Serializable]
public struct SpellData
{
    public const float HEAL_PER_DAMAGE = 0.1f;
    public const float MOVE_SPEED_BONUS_DURATION = 1;
    public const float COOLDOWN_REDUCTION_RATIO = 0.1f;

    public float Cooldown => baseCooldown * Mathf.Max(0.25f, (1 - cooldownReduction * COOLDOWN_REDUCTION_RATIO));

    public SpellType type;
    public float baseCooldown;

    //Char Buff
    public int heal;
    public int movespeedBonus;

    //Char Side
    public int repeat;
    public int discharge;
    public int cooldownReduction;

    //Spell Side
    public int damages;
    public int size;
    public int knockback;
}
