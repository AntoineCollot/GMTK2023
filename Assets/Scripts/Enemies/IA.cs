using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.AI;
#endif

public class IA : MonoBehaviour, IKnockbackable, IMoveSpeedBonusable
{
    [Header("Pathfinding")]
    public float moveSpeed = 3;
    public float maxSpeedChange = 100;
    Vector3 targetPosition;
    Vector2 desiredVelocity;
    NavMeshPath path;
    Rigidbody2D body;

    //movespeed Bonus
    float lastMoveSpeedBonusTime;
    float moveSpeedBonusMult;

    [Header("Behaviour")]
    public float projectileIdealRange;
    public float cacIdealRange;
    Transform player;
    public enum State { MoveInSpellRange, Casting, WaitForCooldown }
    State state;
    const float AFTER_CASTING_MOVEMENT_COOLDOWN = 1;

    [Header("Spells")]
    public List<SpellData> spells = new List<SpellData>();
    int currentSpell;
    float outOfCooldownTime;

    Vector3[] corners;
    Health health;
    SpellData NextSpellData => spells[currentSpell % spells.Count];

    private void Start()
    {
        path = new NavMeshPath();
        corners = new Vector3[10];

        health = GetComponent<Health>();
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
    }

    void AIBehaviourUpdate()
    {
        switch (state)
        {
            case State.MoveInSpellRange:
                MoveInSpellRangeBehaviour();
                break;
            case State.Casting:
                targetPosition = transform.position;
                break;
            case State.WaitForCooldown:
                WaitForCooldownBehaviour();
                break;
            default:
                break;
        }
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
                spellRange = NextSpellData.size * 0;
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
        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
    }

    public void ApplyKnockback(Vector2 direction, float amount)
    {
        body.AddForce(direction * amount, ForceMode2D.Impulse);
    }

    public void UseSpell()
    {
        int spellId = currentSpell % spells.Count;
        SpellData data = spells[spellId];
        outOfCooldownTime = Time.time + data.Cooldown;

        Vector2 direction = player.position - transform.position;
        direction.Normalize();
        SpellInstance spell = SpellGenerator.Instance.CastSpell(transform.position, Source.Enemy, direction, in data, OnHitCallback);

        if (data.movespeedBonus > 0)
            GainMoveSpeedBonus(data.MoveSpeedBonusMult);
        StartCoroutine(Casting(spell));

        currentSpell++;
    }

    IEnumerator Casting(SpellInstance spell)
    {
        state = State.Casting;
        yield return new WaitForSeconds(spell.AnticipationTime);
        //The spell is performing
        yield return new WaitForSeconds(AFTER_CASTING_MOVEMENT_COOLDOWN);
        state = State.WaitForCooldown;
    }

    void OnHitCallback(Health hitHealth, SpellData data)
    {
        float damages = data.damages;
        if (data.type == SpellType.Laser)
            damages *= 1 / SpellInstanceLaser.LASER_HIT_COUNT;

        if (data.heal > 0)
            health.Heal(damages * data.heal * SpellData.HEAL_PER_DAMAGE);
    }

    public void GainMoveSpeedBonus(float mult)
    {
        lastMoveSpeedBonusTime = Time.time;
        moveSpeedBonusMult = mult;
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
