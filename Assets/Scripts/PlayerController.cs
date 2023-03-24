using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public int value;
    PhotonView PV;
    //Text clickValueText;
    void Start()
    {
        PV = photonView;
        //clickValueText = GameObject.Find("ClickText").GetComponent<Text>();
    }

    void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }
        //clickValueText.text = value.ToString();
    }

    [PunRPC]
    void TestRPC(string str1, string str2)
    {
        Debug.Log("RPC ½ÇÇà");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(value);
        }
        else
        {
            value = (int)stream.ReceiveNext();
        }
    }
}
