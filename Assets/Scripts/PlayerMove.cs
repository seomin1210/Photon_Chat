using Photon.Pun;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView myView;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public TMP_Text nickname;
    public GameObject MainChatPanel;
    public InputField Chat_Input;
    public GameObject ChatBox;
    public TMP_Text ChatBox_Text;

    private bool isJumping = false;

    void Start()
    {
        MainChatPanel = GameObject.Find("Canvas").transform.GetChild(2).gameObject;
        Chat_Input = MainChatPanel.GetComponent<InputField>();

        if (photonView.IsMine)
        {
            nickname.text = PhotonNetwork.NickName;
            nickname.color = Color.white;
        }
        else
        {
            nickname.text = photonView.Owner.NickName;
            nickname.color = Color.red;
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && !MainChatPanel.activeSelf)
        {
            float x = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(4 * x, rb.velocity.y);

            if (x != 0)
            {
                myView.RPC("FlipX", RpcTarget.All, x);
            }
        }

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Return))
        {
            if (MainChatPanel.activeSelf)
            {
                MainChatPanel.SetActive(false);
                if (Chat_Input.text.Length > 0)
                {
                    myView.RPC("OpenChatBox", RpcTarget.All);
                    Chat_Input.text = "";
                    StopAllCoroutines();
                    StartCoroutine(DelayCloseChatBox(3f));
                }
            }
            else
            {
                MainChatPanel.SetActive(true);
                Chat_Input.ActivateInputField();
            }
        }

        if (photonView.IsMine && !MainChatPanel.activeSelf)
        {
            if (!isJumping)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
                    isJumping = true;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if(rb.velocity.y < 0f)
            {
                RaycastHit2D ray = Physics2D.Raycast(rb.position, Vector2.down, 2, LayerMask.GetMask("Plane"));
                if (ray.collider != null)
                {
                    if (ray.distance < 1f)
                    {
                        isJumping = false;
                    }
                }
            }
            else if (rb.velocity.y == 0f)
            {
                isJumping = false;
            }
        }
    }

    private IEnumerator DelayCloseChatBox(float v)
    {
        yield return new WaitForSeconds(v);
        myView.RPC("CloseChatBox", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void CloseChatBox()
    {
        ChatBox.SetActive(false);
    }

    [PunRPC]
    public void OpenChatBox()
    {
        ChatBox.SetActive(true);
        ChatBox_Text.text = "<color=red>"+PhotonNetwork.NickName+"</color> :\n" + Chat_Input.text;
    }

    [PunRPC]
    public void FlipX(float x) => spriteRenderer.flipX = x == -1;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ChatBox_Text.text);
        }
        else
        {
            ChatBox_Text.text = (string)stream.ReceiveNext();
        }
    }
}
