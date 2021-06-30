using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class TakingDamdge : MonoBehaviour
{
    enum MoveState
    {
        Live,
        Dead
    }
    [Header("State")]
    public GameObject ThisPlayer;
    // public Animator animator;
    public float _walkTimeToDead;

    private MoveState _moveState = MoveState.Live;
    private Rigidbody2D rb;

    [Header("Healt Point")]
    public int maxHealth = 100;
    public int currentHealth;

    private PhotonView photonView;
    void Start()
    {
        //animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    ////////Die//////////
    void Die()
    {
        if (GetComponent<PhotonView>().InstantiationId == 0)
        {
            Destroy(gameObject);
        }
        else
        {
            if (photonView.IsMine)
            {
                _moveState = MoveState.Dead;
                rb.velocity = new Vector2(0, 0);
                //animator.Play("Dead");
                this.enabled = false;
                PhotonNetwork.Destroy(gameObject);
                GetComponent<Collider2D>().enabled = false;
                GetComponent<Rigidbody2D>().gravityScale = 0f;
            }
        }
    }
}
