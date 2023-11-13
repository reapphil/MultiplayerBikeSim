//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2023 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

#if RCC_PHOTON && PHOTON_UNITY_NETWORKING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class RCC_PhotonManager : MonoBehaviourPunCallbacks {

    public static RCC_PhotonManager Instance;

    public string gameplaySceneName = "Gameplay Scene Name";

    [Header("UI Menus")]
    public InputField nickPanel;
    public GameObject browseRoomsPanel;
    public GameObject roomsContent;
    public GameObject chatLinesPanel;
    public GameObject chatLinesContent;
    public GameObject noRoomsYet;
    public GameObject createRoomButton;
    public GameObject exitRoomButton;

    [Header("UI Texts")]
    public Text status;
    public Text totalOnlinePlayers;
    public Text totalRooms;
    public Text region;
    public Text informer;

    [Header("UI Prefabs")]
    public RCC_PhotonUIRoom roomPrefab;
    public RCC_PhotonUIChatLine chatLinePrefab;

    //  Dictionaries for cached rooms and players.
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;

    private void Awake() {

        if (Instance == null) {

            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

        } else {

            Destroy(gameObject);
            return;

        }

    }

    private void Start() {

        //  Initializing dictionaries.
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

        status.text = "Ready to connect";
        nickPanel.text = "New Player " + Random.Range(0, 99999).ToString();

    }

    private void Update() {

        if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby) {

            totalOnlinePlayers.text = "Total Online Players: " + PhotonNetwork.CountOfPlayers.ToString();
            totalRooms.text = "Total Online Rooms: " + PhotonNetwork.CountOfRooms.ToString();
            region.text = "Region: " + PhotonNetwork.CloudRegion.ToString();

        } else {

            totalOnlinePlayers.text = "";
            totalRooms.text = "";
            region.text = "";

        }

    }

    public void Connect() {

        if (!PhotonNetwork.IsConnected)
            ConnectToServer();
        else
            nickPanel.gameObject.SetActive(false);

    }

    private void ConnectToServer() {

        Debug.Log(status.text);
        informer.text = status.text = "Connecting to server";
        PhotonNetwork.NickName = nickPanel.text;
        PhotonNetwork.ConnectUsingSettings();
        nickPanel.gameObject.SetActive(false);

        if (RCC_InfoLabel.Instance)
            RCC_InfoLabel.Instance.ShowInfo("Connecting to server");

    }

    public override void OnConnectedToMaster() {

        informer.text = "";
        Debug.Log("Connected to server");
        Debug.Log("Entering to lobby");
        status.text = "Entering to lobby";
        PhotonNetwork.JoinLobby();

        if (RCC_InfoLabel.Instance)
            RCC_InfoLabel.Instance.ShowInfo("Connected to server, Entering to lobby");

    }

    public override void OnJoinedLobby() {

        Debug.Log("Entered to lobby");
        status.text = "Entered to lobby";
        nickPanel.gameObject.SetActive(false);
        browseRoomsPanel.SetActive(true);
        createRoomButton.SetActive(true);
        exitRoomButton.SetActive(false);

        if (RCC_InfoLabel.Instance)
            RCC_InfoLabel.Instance.ShowInfo("Entering to lobby");

    }

    public override void OnJoinedRoom() {

        Debug.Log("Joined room");
        status.text = "Joined room";
        nickPanel.gameObject.SetActive(false);
        browseRoomsPanel.SetActive(false);
        createRoomButton.SetActive(false);
        exitRoomButton.SetActive(true);
        chatLinesPanel.SetActive(true);

        if (RCC_InfoLabel.Instance)
            RCC_InfoLabel.Instance.ShowInfo("Joined room, You can spawn your vehicle from 'Options' menu");

        LoadLevel(gameplaySceneName);

    }

    public override void OnCreatedRoom() {

        Debug.Log("Created room");
        status.text = "Created room";
        nickPanel.gameObject.SetActive(false);
        browseRoomsPanel.SetActive(false);
        createRoomButton.SetActive(false);
        exitRoomButton.SetActive(true);
        chatLinesPanel.SetActive(true);

        if (RCC_InfoLabel.Instance)
            RCC_InfoLabel.Instance.ShowInfo("Created room, You can spawn your vehicle from 'Options' menu");

        LoadLevel(gameplaySceneName);

    }

    /// <summary>
    /// When updated room list in the lobby.
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList) {

        ClearRoomListView();
        UpdateCachedRoomList(roomList);
        UpdateRoomListView(roomList);

        if (roomListEntries != null && roomListEntries.Count > 0)
            noRoomsYet.SetActive(false);
        else
            noRoomsYet.SetActive(true);

    }

    /// <summary>
    /// Updates cached room list.
    /// </summary>
    /// <param name="roomList"></param>
    private void UpdateCachedRoomList(List<RoomInfo> roomList) {

        foreach (RoomInfo info in roomList) {

            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList) {

                if (cachedRoomList.ContainsKey(info.Name))
                    cachedRoomList.Remove(info.Name);

                continue;

            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
                cachedRoomList[info.Name] = info;
            else
                cachedRoomList.Add(info.Name, info);

        }

    }

    /// <summary>
    /// Listing all rooms.
    /// </summary>
    /// <param name="roomList"></param>
    private void UpdateRoomListView(List<RoomInfo> roomList) {

        foreach (RoomInfo info in cachedRoomList.Values) {

            GameObject entry = Instantiate(roomPrefab.gameObject);
            entry.transform.SetParent(roomsContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RCC_PhotonUIRoom>().Check(info.Name, info.PlayerCount.ToString() + " / " + info.MaxPlayers.ToString());
            roomListEntries.Add(info.Name, entry);

        }

    }

    /// <summary>
    /// Clears the cache for room list.
    /// </summary>
    private void ClearRoomListView() {

        foreach (GameObject entry in roomListEntries.Values)
            Destroy(entry.gameObject);

        roomListEntries.Clear();

    }

    public void CreateRoom() {

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 4;

        PhotonNetwork.JoinOrCreateRoom("New Room " + Random.Range(0, 999), roomOptions, TypedLobby.Default);

    }

    public void JoinSelectedRoom(RCC_PhotonUIRoom room) {

        PhotonNetwork.JoinRoom(room.roomName.text);

    }

    public void Chat(InputField inputField) {

        photonView.RPC("RPCChat", RpcTarget.AllBuffered, PhotonNetwork.NickName, inputField.text);

    }

    [PunRPC]
    public void RPCChat(string nickName, string text) {

        RCC_PhotonUIChatLine newChatLine = Instantiate(chatLinePrefab.gameObject, chatLinesContent.transform).GetComponent<RCC_PhotonUIChatLine>();
        newChatLine.Line(nickName + " : " + text);

        RCC_PhotonUIChatLine[] chatLines = chatLinesContent.GetComponentsInChildren<RCC_PhotonUIChatLine>();

        if (chatLines.Length > 7)
            Destroy(chatLines[0].gameObject);

    }

    public void ExitRoom() {

        PhotonNetwork.LeaveRoom();

    }

    public override void OnLeftRoom() {

        Debug.Log("Exited room");
        status.text = "Exited room";
        nickPanel.gameObject.SetActive(false);
        browseRoomsPanel.SetActive(true);
        createRoomButton.SetActive(true);
        exitRoomButton.SetActive(false);
        chatLinesPanel.SetActive(false);

    }

    public void ExitLobby() {

        PhotonNetwork.LeaveLobby();

    }

    public override void OnLeftLobby() {

        Debug.Log("Exited to lobby");
        status.text = "Exited to lobby";
        nickPanel.gameObject.SetActive(true);
        browseRoomsPanel.SetActive(false);
        createRoomButton.SetActive(false);
        exitRoomButton.SetActive(false);
        chatLinesPanel.SetActive(false);

    }

    public override void OnDisconnected(DisconnectCause cause) {

        Debug.Log("Disconnected");
        status.text = "Disconnected";
        nickPanel.gameObject.SetActive(true);
        browseRoomsPanel.SetActive(false);
        createRoomButton.SetActive(false);
        exitRoomButton.SetActive(false);
        chatLinesPanel.SetActive(false);

    }

    public void LoadLevel(string level) {

        PhotonNetwork.LoadLevel(level);

    }

}
#endif