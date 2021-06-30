using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class TakingDamdge : MonoBehaviour, IPunObservable
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

    private bool isLive = true;
    private PhotonView photonView;
    void Start()
    {
        //animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            isLive = false;
            Die();
        }
    }

    ////////Die//////////
    void Die()
    {
        _moveState = MoveState.Dead;
        rb.velocity = new Vector2(0, 0);
        //animator.Play("Dead");
       // Destroy(ThisPlayer, _walkTimeToDead);
        this.enabled = false;
        PhotonNetwork.Destroy(ThisPlayer);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().gravityScale = 0f;

    }

    private void Update()
    {
        if (!isLive && !photonView.IsMine) 
        {
            Debug.Log("Die");
            Die();

        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isLive);

        }
        else
        {
            isLive = (bool) stream.ReceiveNext();
        }    
    }
}
