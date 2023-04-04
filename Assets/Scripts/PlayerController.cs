using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public int value;
    public GameObject bulletSpawn;
    public GameObject bullets;
    PhotonView PV;
    Rigidbody2D rigid;
    BoxCollider2D boxCol;
    Animator ani;
    SpriteRenderer spriteRenderer;
    RaycastHit2D hit;
    bool isJump;

    //Text clickValueText;
    void Start()
    {
        PV = photonView;
        rigid = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }
        Move();
        Jump();
        Fire();
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        bool isMoving = x != 0;

        if (isMoving)
        {
            Follow();
            rigid.velocity = new Vector2(4 * x, rigid.velocity.y);
            ani.SetBool("Walk", true);

            if(x == -1 && !spriteRenderer.flipX)
            {
                spriteRenderer.flipX = true;
                bulletSpawn.transform.localPosition = new Vector2(-0.4f, -0.05f);
            }
            else if(x == 1 && spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
                bulletSpawn.transform.localPosition = new Vector2(0.4f, -0.05f);
            }
        }
        else
        {
            ani.SetBool("Walk", false);
        }
    }

    void Jump()
    {
        // 지면에서 떨어지고 있는 경우만 체크
        if (rigid.velocity.y >= 0) return;

        // 아래쪽에 땅인지 체크
        hit = Physics2D.BoxCast(boxCol.bounds.center, boxCol.bounds.size, 0f, Vector2.down, 0.2f, LayerMask.GetMask("Ground"));

        // 땅과 충돌한 경우
        if(hit.collider != null && hit.distance < 0.45f)
        {
            ani.SetBool("Jump", false);
            isJump = false;
        }

        // 스페이스바 입력 시 점프
        if(Input.GetKeyDown(KeyCode.Space) && !isJump)
        {
            isJump = true;
            rigid.velocity = Vector2.zero;
            rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
            ani.SetBool("Jump", true);
        }
    }

    void Fire()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(FireRoution());
        }
    }

    IEnumerator FireRoution()
    {
        ani.SetBool("Fire", true);
        GameObject bullet = Instantiate(bullets, bulletSpawn.transform.position, Quaternion.identity);
        bullet.GetComponent<BulletController>().dir = spriteRenderer.flipX ? -1 : 1;
        yield return new WaitForSeconds(0.1f);
        ani.SetBool("Fire", false);
    }

    void Follow()
    {
        GameManager.instance.cam.transform.
        transform.localPosition = transform.position;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            value = (int)stream.ReceiveNext();
        }
    }

    [PunRPC]
    void FlipXRPC(float x)
    {
        spriteRenderer.flipX = x == -1;
    }

    [PunRPC]
    void JumpRPC()
    {
        rigid.velocity = Vector2.zero;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    }

    [PunRPC]
    void DestroyRPC()
    {
        Destroy(gameObject);
    }
}
