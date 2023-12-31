using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Source { Player, Enemy}
public abstract class SpellInstance : MonoBehaviour
{
    protected SpellData data;
    protected ICastSpell source;
    protected Vector2 direction;
    protected Action<Health, SpellData> hitCallback;
    protected CompositeStateToken isCastingToken;
    CompositeState isCastingState;

    [Header("Settings")]
    public float anticipationTimePlayer = 0.2f;
    public float anticipationTimeEnemy = 0.5f;
    public float minKnockback = 5;
    public float maxKnockback = 30;

    public const float MAX_AMELIORATION_VALUE = 10;

    public float AnticipationTime
    {
        get
        {
            if (source.Source == Source.Player)
                return anticipationTimePlayer;

            return anticipationTimeEnemy;
        }
    }
    public float Knockback => Curves.QuadEaseOut(minKnockback, maxKnockback, Mathf.Clamp01(data.knockback / MAX_AMELIORATION_VALUE));

    public virtual LayerMask LayerMask
    {
        get
        {
            if (source.Source == Source.Player)
                return LayerMask.GetMask("Enemies");
            else
                return LayerMask.GetMask("Player");
        }
    }

    public virtual void Init(in SpellData data, ICastSpell source, Vector2 direction, Action<Health, SpellData> hitCallback, CompositeState isCastingState)
    {
        this.data = data;
        this.source = source;
        this.hitCallback = hitCallback;
        this.direction = direction;

        this.isCastingState = isCastingState;
        isCastingToken = new CompositeStateToken();
        isCastingState.Add(isCastingToken);
        isCastingToken.SetOn(true);
        source.OnSpellCastStarted();
    }

    private void OnDestroy()
    {
        if(isCastingState !=null)
            isCastingState.Remove(isCastingToken);
    }
}
