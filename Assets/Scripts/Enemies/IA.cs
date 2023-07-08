using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IA : MonoBehaviour, IKnockbackable
{
    [Header("Pathfinding")]
    public float moveSpeed = 3;
    public float maxSpeedChange = 100;
    Transform player;
    Vector2 desiredVelocity;
    NavMeshPath path;
    Rigidbody2D body;

    [Header("Behaviour")]
    State state;
    public enum State { MoveInSpellRange, Casting, WaitForCooldown}

    [Header("Spells")]
    public List<SpellData> spells = new List<SpellData>();
    int currentSpell;
    float outOfCooldownTime;

    Vector3[] corners;

    private void Start()
    {
        path = new NavMeshPath();
        corners = new Vector3[10];

        body = GetComponent<Rigidbody2D>();
        player = PlayerMovement.Instance.transform;
    }

    private void Update()
    {
        Vector2 targetDirection = player.position - transform.position;
        targetDirection.Normalize();

        if (!HasLineOfSightToPlayer())
        {
            UpdatePath();
            targetDirection = GetMoveAlongPath();
        }

        desiredVelocity = targetDirection * moveSpeed;
    }

    private void FixedUpdate()
    {
        Vector2 velocity = body.velocity;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange * Time.fixedDeltaTime);
        velocity.y = Mathf.MoveTowards(velocity.y, desiredVelocity.y, maxSpeedChange * Time.fixedDeltaTime);

        body.velocity = velocity;
    }

    Vector2 GetMoveAlongPath()
    {
        path.GetCornersNonAlloc(corners);
        Vector2 toNextCorner = corners[1] - transform.position;
        toNextCorner.Normalize();

        return toNextCorner;
    }

    public bool HasLineOfSightToPlayer()
    {
        Vector2 toPlayer = player.position - transform.position;
        LayerMask layer = LayerMask.GetMask("Player", "Level");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, toPlayer.magnitude, layer);

        return hit.collider.transform == player;
    }

    void UpdatePath()
    {
        NavMesh.CalculatePath(transform.position, player.position, NavMesh.AllAreas, path);
    }

    public void ApplyKnockback(Vector2 direction, float amount)
    {
        body.AddForce(direction * amount, ForceMode2D.Impulse);
    }

    public void UseSpell()
    {
        int spellId = currentSpell % spells.Count;
        SpellData data = spells[spellId];
        outOfCooldownTime = data.Cooldown;

        SpellGenerator.Instance.CastSpell(transform.position, Source.Enemy, in data, OnHitCallback);

        currentSpell++;
    }

    void OnHitCallback(Health hitHealth, SpellData data)
    {

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (path == null)
            return;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, path.corners[1]);
    }
#endif
}
