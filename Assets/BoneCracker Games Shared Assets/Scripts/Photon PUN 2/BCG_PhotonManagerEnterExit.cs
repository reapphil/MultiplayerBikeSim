//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2021 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

#if BCG_ENTEREXITPHOTON && PHOTON_UNITY_NETWORKING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// A simple manager script for photon demo scene. It has an array of networked spawnable player vehicles, public methods, restart, and quit application.
/// </summary>
public class BCG_PhotonManagerEnterExit : MonoBehaviourPunCallbacks{

    public BCG_FPSController player;
    public Transform spawnPos;

    public GameObject[] enableOnJoin;
    public GameObject[] disableOnJoin;


    private void Start(){

        for (int i = 0; i < enableOnJoin.Length; i++)
            enableOnJoin[i].SetActive(false);

        for (int i = 0; i < disableOnJoin.Length; i++)
            disableOnJoin[i].SetActive(true);

        ConnectToServer();

    }

    private void ConnectToServer() {

        if (!PhotonNetwork.IsConnectedAndReady)
            ConnectToPhoton();

    }

    private void ConnectToPhoton() {

        Debug.Log("Connecting to server");
        RCC_InfoLabel.Instance.ShowInfo("Entering to lobby");
        PhotonNetwork.NickName = "New Player " + Random.Range(0, 99999).ToString();
        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster() {

        Debug.Log("Connected to server");
        Debug.Log("Entering to lobby");
        RCC_InfoLabel.Instance.ShowInfo("Entering to lobby");
        PhotonNetwork.JoinLobby();

    }

    public override void OnJoinedLobby() {

        Debug.Log("Entered to lobby");
        RCC_InfoLabel.Instance.ShowInfo("Joining Random Room");
        PhotonNetwork.JoinRandomRoom();

    }

    public override void OnJoinRandomFailed(short returnCode, string message) {

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 4;

        Debug.Log("Entered to lobby");
        RCC_InfoLabel.Instance.ShowInfo("Creating New Room");
        PhotonNetwork.JoinOrCreateRoom("New Room (Enter-Exit) " + Random.Range(0, 999), roomOptions, TypedLobby.Default);

    }

    public override void OnJoinedRoom() {

        for (int i = 0; i < enableOnJoin.Length; i++)
            enableOnJoin[i].SetActive(true);

    }

    public override void OnCreatedRoom() {

        for (int i = 0; i < enableOnJoin.Length; i++)
            enableOnJoin[i].SetActive(true);

    }

    public override void OnLeftLobby() {

        base.OnLeftLobby();

    }

    public void Spawn() {

        BCG_EnterExitPlayer newPlayer = PhotonNetwork.Instantiate(player.name, spawnPos.position, spawnPos.rotation).gameObject.GetComponent<BCG_EnterExitPlayer>();
        BCG_EnterExitManager.Instance.activePlayer = newPlayer;

        for (int i = 0; i < disableOnJoin.Length; i++)
            disableOnJoin[i].SetActive(false);

    }

    public void TogglePlayer() {



    }

}
#endif