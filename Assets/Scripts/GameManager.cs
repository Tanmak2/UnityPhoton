using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    public GameObject ConnectedPanel;
    public GameObject LobbyPanel;
    public GameObject roomPanel;
    public InputField inputName;
    public Button[] cellBtns;
    public Button prevBtn, nextBtn, createRoomButton;
    public InputField inputRoomName, chatInput;
    public Text lobbyStatus;
    public Text roomStatus;
    public List<RoomInfo> list = new List<RoomInfo>();
    float refreshTime = 0;
    RoomOptions roomOptions;
    public Text[] chatTexts;
    PhotonView PV;

    int currentPage = 1, maxPage, multiple;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            PV = photonView;
            DontDestroyOnLoad(this);
        }
        else Destroy(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(960, 540, false);
        SetPanel(ConnectedPanel);
        roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;
        RefreshRoomList();
    }

    // Update is called once per frame
    void Update()
    {
        lobbyStatus.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";
    }
    public void Connect()
    {
        PhotonNetwork.LocalPlayer.NickName = inputName.text;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SetPanel(LobbyPanel);
    }

    public override void OnConnectedToMaster()
    {
        print("서버 접속 완료");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        print("로비 접속 완료");
        SetPanel(LobbyPanel);
    }
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(inputRoomName.text == "" ? "Room" + Random.Range(0,100) : inputRoomName.text, roomOptions, null);
    }

    public void JoinRoom(string name)
    {
        if (name == null || name == "") name = inputRoomName.text;
        PhotonNetwork.JoinRoom(name);
    }

    public override void OnJoinedRoom()
    {
        SetPanel(roomPanel);
        RefreshRoom();
        chatInput.text = "";
        for (int i = 0; i < chatTexts.Length; i++)
        {
            chatTexts[i].text = "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!list.Contains(roomList[i])) list.Add(roomList[i]);
                else list[list.IndexOf(roomList[i])] = roomList[i];
            }
            else if (list.IndexOf(roomList[i]) != -1) list.RemoveAt(list.IndexOf(roomList[i]));
        }
        RefreshRoomList();
    }

    public void RefreshRoomList()
    {
        //최대 페이지 
        maxPage = (list.Count % cellBtns.Length == 0) ? list.Count / cellBtns.Length : list.Count / cellBtns.Length + 1;


        //이전, 다음버튼 활성화 또는 비활성화
        prevBtn.interactable = (currentPage <= 1) ? false : true;
        nextBtn.interactable = (currentPage >= maxPage) ? false : true;

        multiple = (currentPage - 1) * cellBtns.Length;
        for (int i = 0; i < cellBtns.Length; i++)
        {
            cellBtns[i].interactable = (multiple + i < list.Count) ? true : false;
            cellBtns[i].GetComponentInChildren<Text>().text = (multiple + i < list.Count) ? list[multiple + i].Name : "";
        }
    }

    public void RefreshRoom()
    {
        roomStatus.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + "\n현재 접속중인 유저\n";
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            roomStatus.text += PhotonNetwork.PlayerList[i].NickName + ((i + i == PhotonNetwork.PlayerList.Length) ? "" : "\n");
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshRoom();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다." + "</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshRoom();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다." + "</color>");
    }


    public void ButtonClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else JoinRoom(list[multiple + num].Name);

        RefreshRoomList();
    }

    public void SetPanel(GameObject panel)
    {
        ConnectedPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
        panel.SetActive(true);
    }

    public void SendMessage()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + chatInput.text);
        chatInput.text = "";
    }

    [PunRPC]
    public void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < chatTexts.Length; i++)
        {
            if(chatTexts[i].text == "")
            {
                isInput = true;
                chatTexts[i].text = msg;
                break;
            }
        }
        if (!isInput)
        {
            for(int i = 1; i < chatTexts.Length; i++)
            {
                chatTexts[i - 1].text = chatTexts[i].text;
            }
            chatTexts[chatTexts.Length - 1].text = msg;
        }
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(list);
    //    }
    //    else
    //    {
    //        list = (List<string>)stream.ReceiveNext();
    //    }
    //}
}
