using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        inputMap = new InputMap();
        inputMap.Gameplay.Dash.performed += OnDashPerformed;

        movement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody2D>();

        isDashingToken = new CompositeStateToken();
    }

    private void Start()
    {
        movement.lockMovementState.Add(isDashingToken);
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
        isDashingToken.SetOn(true);

        body.velocity = movement.movementInputs * dashVelocity;

        Invoke("EndDash", dashDuration);
    }

    void EndDash()
    {
        isDashingToken.SetOn(false);
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
