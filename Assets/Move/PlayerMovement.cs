using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    Animator animator;
    private string currentState;

    private PhotonView photonView;
    private void Start()
    {
        animator = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();
    }


    public MainMoveLogic controller;
    public float runSpeed = 40f;

    float horizontalMove = 0f;
    public bool jump = false;
    bool crouch = false;
    private bool attackFight = false;
    private float timeLeft = 0.35f;


    const string PlyerIdle = "Idle";
    const string PlayerWalk = "Walk";
    const string PlayerJump = "Jump";
    const string PlayerPunch ="Punch";



    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }


    public void Jump(InputAction.CallbackContext context)
    {
        jump = true;
        Debug.Log(GameManager.PlayerN.Count);

    }

    public void Crouch(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            crouch = true;
        }
        else if (context.canceled)
        {
            crouch = false;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (attackFight == true)
        {
            horizontalMove = context.ReadValue<Vector2>().x * runSpeed * 0.4f;

        }
        else if (attackFight == false && controller.isGroud == true)
        {
            horizontalMove = context.ReadValue<Vector2>().x * runSpeed;
        }
        else if (controller.isGroud == false)
        {
            horizontalMove = context.ReadValue<Vector2>().x * runSpeed * 0.8f;
        }
        else
        {
            horizontalMove = context.ReadValue<Vector2>().x * runSpeed;
        }
    }


    void FixedUpdate()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            attackFight = false;
            timeLeft = 0.35f;
        }
        if (!photonView.IsMine) return;
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        jump = false;

    }
}
