using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonMagager : MonoBehaviourPunCallbacks
{
    public static PhotonMagager instance;
    
    [Header("ConnectedPanel")]
    
    public GameObject currentPanel;
    
    //public Text playersText;
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(this);
    }
    private void Start()
    {
        
    }

    private void Update()
    {
        ShowPlayers();
    }

    PlayerController FindPlayer()
    {
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetPhotonView().IsMine)
            {
                return player.GetComponent<PlayerController>();
            }
        }
        return null;
    }
    [ContextMenu("º§·ù¾÷")]
    public void Click()
    {
        FindPlayer().value++;
    }

    public void ShowPlayers()
    {
        string playersString = "";
        foreach(GameObject Player in GameObject.FindGameObjectsWithTag("Player"))
        {
            playersString += Player.GetPhotonView().Owner.NickName + " / " + Player.GetComponent<PlayerController>().value.ToString() + "\n";
        }
        //playersText.text = playersString;
    }

}
