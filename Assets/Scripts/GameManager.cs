using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks, IChatClientListener
{
    public static GameManager instance;
    private ChatClient chatClient;
    public GameObject ConnectedPanel;
    public GameObject LobbyPanel;
    public GameObject roomPanel;
    public GameObject contentBox;
    public InputField inputName;
    public Button[] cellBtns;
    public Button prevBtn, nextBtn, createRoomButton;
    public InputField inputRoomName, chatInput, lobbyInput;
    public Text lobbyStatus;
    public Text roomStatus;
    public Text lobbyChatText, channelText;
    public List<RoomInfo> list = new List<RoomInfo>();
    string currentChannelName;
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

        Application.runInBackground = true;
        chatClient = new ChatClient(this);
        chatClient.UseBackgroundWorkerForSending = true;
        currentChannelName = "lobby1";
    }

    // Update is called once per frame
    void Update()
    {
        chatClient.Service();
        lobbyStatus.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";
    }

    public void Connect()
    {
        PhotonNetwork.LocalPlayer.NickName = inputName.text;
        PhotonNetwork.ConnectUsingSettings();
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, "1.0", new Photon.Chat.AuthenticationValues(PhotonNetwork.LocalPlayer.NickName));
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, "1.0", new Photon.Chat.AuthenticationValues(PhotonNetwork.LocalPlayer.NickName));
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
        //AddLine(string.Format("연결시도 : ({0})", PhotonNetwork.LocalPlayer.NickName));
        SetPanel(LobbyPanel);
    }

    [ContextMenu("방생성")]
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(inputRoomName.text == "" ? "Room" + Random.Range(0,100) : inputRoomName.text, roomOptions, null);
        inputRoomName.text = "";
    }

    public void JoinRoom(string name)
    {
        if (name == null || name == "") name = inputRoomName.text;
        PhotonNetwork.JoinRoom(name);
    }

    public override void OnJoinedRoom()
    {
        chatClient.Disconnect();
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

    public void AddLine(Text txt, string lineString)
    {
        txt.text = lineString;
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
        {
            Debug.LogError(message);
        }
        else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
        {
            Debug.LogWarning(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    public void OnConnected()
    {
        chatClient.Subscribe(new string[] { currentChannelName }, 14);
    }


    public void OnDisconnected()
    {
        AddLine(channelText, "현재 채널 : 접속중...");
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log("OnChatStateChange = " + state);
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName.Equals(currentChannelName))
        {
            ShowChannel(currentChannelName);
        }
        SizeDelta();
    }

    public void ShowChannel(string channelName)
    {
        if (string.IsNullOrEmpty(channelName)) return;
        ChatChannel channel = null;
        bool found = chatClient.TryGetChannel(channelName, out channel);
        if (!found)
        {
            Debug.Log("ShowChannel failed to find channel : " + channelName);
            return;
        }
        currentChannelName = channelName;
        lobbyChatText.text = channel.ToStringMessages();
        Debug.Log("ShowChannel : " + currentChannelName);
    }

    public void OnEnterSend()
    {
        SendChatMessage(lobbyInput.text);
        lobbyInput.text = "";
        lobbyInput.Select();
        contentBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, contentBox.GetComponent<RectTransform>().sizeDelta.y - 266);
    }

    public void SendChatMessage(string inputLine)
    {
        if (string.IsNullOrEmpty(inputLine)) return;

        chatClient.PublishMessage(currentChannelName, inputLine);
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new System.NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        AddLine(channelText, string.Format("현재 채널 : {0}", string.Join(",", channels)));
        SizeDelta();
    }

    public void OnUnsubscribed(string[] channels)
    {
        AddLine(channelText, string.Format("채널 퇴장 ({0})", string.Join(",", channels)));
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void SizeDelta()
    {
        if(lobbyChatText.rectTransform.sizeDelta.y < 250)
        {
            return;
        }
        contentBox.GetComponent<RectTransform>().sizeDelta = new Vector2(contentBox.GetComponent<RectTransform>().sizeDelta.x, lobbyChatText.rectTransform.sizeDelta.y+16);
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
