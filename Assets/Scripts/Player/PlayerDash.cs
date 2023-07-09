using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDash : MonoBehaviour
{
    [Header("Settings")]
    public float dashDuration = 0.2f;
    public float dashVelocity = 10;

    //Components
    PlayerMovement movement;
    CompositeStateToken isDashingToken;
    InputMap inputMap;
    Rigidbody2D body;
    Health health;

    public UnityEvent onDash = new UnityEvent();
    public UnityEvent onDashEnd = new UnityEvent();
    public static PlayerDash Instance;

    private void Awake()
    {
        Instance = this;

        inputMap = new InputMap();
        inputMap.Gameplay.Dash.performed += OnDashPerformed;

        movement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        isDashingToken = new CompositeStateToken();
    }

    private void Start()
    {
        movement.lockMovementState.Add(isDashingToken);
        health.isInvicibleState.Add(isDashingToken);
    }

    private void OnEnable()
    {
        inputMap.Enable();
    }

    private void OnDisable()
    {
        inputMap.Disable();
    }

    private void OnDashPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (PlayerMovement.Instance.lockMovementState.IsOn || health.isDead)
            return;

        isDashingToken.SetOn(true);
        body.velocity = movement.lastNonZeroMovementInputs * dashVelocity;
        onDash.Invoke();

        //Sound
        SFXManager.PlaySound(GlobalSFX.Dash);

        Invoke("EndDash", dashDuration);
    }

    void EndDash()
    {
        isDashingToken.SetOn(false);
        onDashEnd.Invoke();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(isDashingToken != null && isDashingToken.IsOn)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }    
    }
#endif
}
