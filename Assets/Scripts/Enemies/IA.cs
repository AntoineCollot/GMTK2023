using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.AI;
#endif

public class IA : MonoBehaviour, IKnockbackable, IMoveSpeedBonusable, IAnimable, ICastSpell
{
    [Header("Pathfinding")]
    public float moveSpeed = 3;
    public float maxSpeedChange = 100;
    Vector3 targetPosition;
    Vector2 desiredVelocity;
    UnityEngine.AI.NavMeshPath path;
    Rigidbody2D body;

    //movespeed Bonus
    float lastMoveSpeedBonusTime;
    float moveSpeedBonusMult;

    [Header("Behaviour")]
    public float projectileIdealRange;
    public float cacIdealRange;
    public float laserIdealRange = 2.5f;
    Transform player;
    public enum State { AfterCastFreeze, MoveInSpellRange, Casting, WaitForCooldown }
    State state = State.MoveInSpellRange;
    const float AFTER_CASTING_MOVEMENT_COOLDOWN = 1;

    [Header("Spells")]
    public List<SpellData> spells = new List<SpellData>();
    int currentSpell;
    float outOfCooldownTime;
    CompositeState isCastingState;

    Vector3[] corners;
    Health health;
    SpellData NextSpellData => spells[currentSpell % spells.Count];

    //Animations
    Direction animationDirection;
    public Direction AnimationDirection => animationDirection;
    public float AnimationMoveSpeed => desiredVelocity.magnitude;
    CharacterAnimations characterAnimations;

    public Source Source => Source.Enemy;
    public Transform SourceTransform => transform;

    private void Start()
    {
        path = new UnityEngine.AI.NavMeshPath();
        corners = new Vector3[10];

        characterAnimations = GetComponentInChildren<CharacterAnimations>();

        isCastingState = new CompositeState();
        health = GetComponent<Health>();
        health.onDie.AddListener(OnDie);
        body = GetComponent<Rigidbody2D>();
        player = PlayerMovement.Instance.transform;
    }

    private void Update()
    {
            AIBehaviourUpdate();

            ComputeDesiredVelocity();
    }

    private void FixedUpdate()
    {
        Vector2 velocity = body.velocity;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange * Time.fixedDeltaTime);
        velocity.y = Mathf.MoveTowards(velocity.y, desiredVelocity.y, maxSpeedChange * Time.fixedDeltaTime);

        body.velocity = velocity;
    }

    void ComputeDesiredVelocity()
    {
        if (isCastingState.IsOn)
        {
            desiredVelocity = Vector2.zero;
            return;
        }

        Vector2 targetDirection = targetPosition - transform.position;
        if (targetDirection.magnitude < Time.deltaTime * moveSpeed)
        {
            desiredVelocity = Vector2.zero;
            return;
        }

        targetDirection.Normalize();

        if (!HasLineOfSightToPosition(targetPosition))
        {
            UpdatePath();
            targetDirection = GetMoveAlongPath();
        }

        float currentMoveSpeed = moveSpeed;
        if (Time.time <= lastMoveSpeedBonusTime + SpellData.MOVE_SPEED_BONUS_DURATION)
            currentMoveSpeed *= moveSpeedBonusMult;

        desiredVelocity = targetDirection * currentMoveSpeed;
        animationDirection = desiredVelocity.ToDirection();
    }

    void AIBehaviourUpdate()
    {
        switch (state)
        {
            case State.AfterCastFreeze:

                break;
            case State.MoveInSpellRange:
                MoveInSpellRangeBehaviour();
                break;
            case State.Casting:
                targetPosition = transform.position;
                if (!isCastingState.IsOn)
                {
                    state = State.AfterCastFreeze;
                    Invoke("EndAfterCastFreeze", AFTER_CASTING_MOVEMENT_COOLDOWN);
                }
                break;
            case State.WaitForCooldown:
                WaitForCooldownBehaviour();
                break;
            default:
                break;
        }
    }

    void EndAfterCastFreeze()
    {
        state = State.WaitForCooldown;
    }

    void WaitForCooldownBehaviour()
    {
        Vector3 fromPlayer = transform.position - player.position;
        float idealRange;
        switch (NextSpellData.type)
        {
            case SpellType.Cac:
            case SpellType.AOE:
            default:
                idealRange = cacIdealRange;
                break;
            case SpellType.Projectile:
                idealRange = projectileIdealRange;
                break;
            case SpellType.Laser:
                idealRange = NextSpellData.size * 0.8f;
                break;
        }
        targetPosition = player.position + fromPlayer.normalized * idealRange;

        if (Time.time > outOfCooldownTime)
            state = State.MoveInSpellRange;
    }

    void MoveInSpellRangeBehaviour()
    {
        float spellRange;
        switch (NextSpellData.type)
        {
            case SpellType.Cac:
            case SpellType.AOE:
            default:
                spellRange = cacIdealRange;
                break;
            case SpellType.Projectile:
                spellRange = projectileIdealRange;
                break;
            case SpellType.Laser:
                spellRange = laserIdealRange;
                break;
        }

        //Check if in range for spell
        if (Vector2.Distance(transform.position, player.position) <= spellRange)
        {
            //Use a spell
            UseSpell();
            return;
        }

        //Move into range
        targetPosition = player.position;
    }

    Vector2 GetMoveAlongPath()
    {
        path.GetCornersNonAlloc(corners);
        Vector2 toNextCorner = corners[1] - transform.position;
        toNextCorner.Normalize();

        return toNextCorner;
    }

    public bool HasLineOfSightToPosition(Vector3 position)
    {
        Vector2 toPlayer = position - transform.position;
        LayerMask layer = LayerMask.GetMask("Level");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, toPlayer.magnitude, layer);

        return hit.collider == null;
    }

    void UpdatePath()
    {
        UnityEngine.AI.NavMesh.CalculatePath(transform.position, targetPosition, UnityEngine.AI.NavMesh.AllAreas, path);
    }

    public void ApplyKnockback(Vector2 direction, float amount)
    {
        body.AddForce(direction * amount, ForceMode2D.Impulse);
    }

    private void OnDie()
    {
        enabled = false;
    }

    public void UseSpell()
    {
        Vector2 direction = player.position - transform.position;
        direction.Normalize();

        int spellId = currentSpell % spells.Count;
        SpellData data = spells[spellId];
        outOfCooldownTime = Time.time + data.Cooldown;
        state = State.Casting;

        //Test IA boss - use all
        foreach (SpellData spellData in spells)
        {
            SpellGenerator.Instance.CastSpell(transform.position, this, direction, in spellData, OnHitCallback, isCastingState);

            if (spellData.movespeedBonus > 0)
                GainMoveSpeedBonus(spellData.MoveSpeedBonusMult);
        }
        currentSpell++;

        animationDirection = direction.ToDirection();
    }

    void OnHitCallback(Health hitHealth, SpellData data)
    {
        Debug.Log("hit");
        float damages = data.damages;
        if (data.type == SpellType.Laser)
            damages *= 1 / SpellInstanceLaser.LASER_HIT_COUNT;

        if (data.heal > 0)
            //  health.Heal(damages * data.heal * SpellData.HEAL_PER_DAMAGE);
            health.Heal(data.heal * SpellData.HEAL_PER_DAMAGE);
    }

    public void GainMoveSpeedBonus(float mult)
    {
        lastMoveSpeedBonusTime = Time.time;
        moveSpeedBonusMult = mult;
    }

    public void OnSpellCastStarted()
    {
        characterAnimations.StartCast();
    }

    public void OnSpellCastFinished()
    {
        characterAnimations.EndCast();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, projectileIdealRange);
        Gizmos.DrawWireSphere(transform.position, cacIdealRange);

        Handles.Label(transform.position + Vector3.down * 0.3f, state.ToString());

        //Path
        if (path == null || path.corners == null || path.corners.Length < 2)
            return;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, path.corners[1]);

    }
#endif
}
