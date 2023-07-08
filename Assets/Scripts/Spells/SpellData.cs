using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum SpellType { Cac, Projectile, Laser, AOE}
[System.Serializable]
public struct SpellData
{
    public const float HEAL_PER_DAMAGE = 0.1f;
    public const float MOVE_SPEED_BONUS_DURATION = 1;
    public const float MOVE_SPEED_BONUS_MIN = 1.25f;
    public const float MOVE_SPEED_BONUS_MAX = 3f;
    public const float COOLDOWN_REDUCTION_RATIO = 0.1f;

    public float MoveSpeedBonusMult => Curves.QuadEaseOut(MOVE_SPEED_BONUS_MIN, MOVE_SPEED_BONUS_MAX, movespeedBonus);
    public float Cooldown => baseCooldown * Mathf.Max(0.25f, (1 - cooldownReduction * COOLDOWN_REDUCTION_RATIO));

    //Base
    public SpellType type;
    public float baseCooldown;
    public float baseProjectileSpeed;

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
