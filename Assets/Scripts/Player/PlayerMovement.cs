using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IKnockbackable, IMoveSpeedBonusable, IAnimable
{
    InputMap inputMap;
    Rigidbody2D body;
    public Vector2 movementInputs { get; private set; }
    public Vector2 lastNonZeroMovementInputs { get; private set; }

    //Animations
    Direction animationDirection;
    public Direction AnimationDirection => animationDirection;
    public float AnimationMoveSpeed => desiredVelocity.magnitude;

    Vector2 desiredVelocity;

    [Header("Settings")]
    public float moveSpeed = 5;
    public float maxSpeedChange = 100;

    //move speed bonus
    float lastMoveSpeedBonusTime;
    float moveSpeedBonusMult;

    public CompositeState lockMovementState;
    CompositeStateToken isDashingToken;

    public static PlayerMovement Instance;

    private void Awake()
    {
        Instance = this;

        inputMap = new InputMap();

        lockMovementState = new CompositeState();
        isDashingToken = new CompositeStateToken();
        lockMovementState.Add(isDashingToken);

        body = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GetComponent<Health>().onDie.AddListener(OnDie);
    }

    private void OnDie()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        inputMap.Enable();
    }

    private void OnDisable()
    {
        inputMap.Disable();
    }

    private void Update()
    {
        //Movement bonus
        float currentMoveSpeed = moveSpeed;
        if (Time.time <= lastMoveSpeedBonusTime + SpellData.MOVE_SPEED_BONUS_DURATION)
            currentMoveSpeed *= moveSpeedBonusMult;

        if (lockMovementState.IsOn)
            return;

        Vector2 rawInputs = inputMap.Gameplay.Movement.ReadValue<Vector2>();
        if (rawInputs.magnitude > 0.7f)
        {
            lastNonZeroMovementInputs = rawInputs.normalized;
            animationDirection = rawInputs.ToDirection();
        }

        movementInputs = rawInputs.normalized;
        desiredVelocity = movementInputs * currentMoveSpeed;
    }

    private void FixedUpdate()
    {
        if (lockMovementState.IsOn)
            desiredVelocity = Vector2.zero;

        Vector2 velocity = body.velocity;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange * Time.fixedDeltaTime);
        velocity.y = Mathf.MoveTowards(velocity.y, desiredVelocity.y, maxSpeedChange * Time.fixedDeltaTime);

        body.velocity = velocity;
    }

    public void ApplyKnockback(Vector2 direction, float amount)
    {
        body.AddForce(direction * amount, ForceMode2D.Impulse);
    }

    public void GainMoveSpeedBonus(float mult)
    {
        lastMoveSpeedBonusTime = Time.time;
        moveSpeedBonusMult = mult;
        Debug.Log(lastMoveSpeedBonusTime + " | " + moveSpeedBonusMult);
    }

    public void SetAnimationDirection(Direction direction)
    {
        animationDirection = direction;
    }
}
