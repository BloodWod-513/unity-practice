using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerCombat : MonoBehaviour
{
    [Header("Static")]
  //  public Animator animatior;
    public Transform attackPoint;
    public LayerMask enemyPlayer;


    [Header("Range and damage")]
    public float attackRange = 0.5f;
    public int attackDamage = 110;

    [Header("Attack speed")]
    public float attackSpeed = 2f;
    float nextAttackTime = 0f;

    public bool attack = false;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }
    public void Attack(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;

        if (context.performed)
        {
            attack = true;
        }
        else if (context.canceled)
        {
            attack = false;
        }

        if (Time.time >= nextAttackTime)
        {
            FightAttack();
        }
    }


    //////////Attack///////////
    public void FightAttack()
    {
        nextAttackTime = Time.time + 1f / attackSpeed;

      //  animatior.SetTrigger("Attack");

        if (Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyPlayer))
        {

            foreach (Collider2D enemy in Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyPlayer))
            {
                PhotonView pv = enemy.GetComponent<PhotonView>();
                pv.RPC("TakeDamage", RpcTarget.All, attackDamage);
                //enemy.GetComponent<TakingDamdge>().TakeDamage(attackDamage);                   
            }
        }
    }


    //////////OnDrawGizmosSelected///////////
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

