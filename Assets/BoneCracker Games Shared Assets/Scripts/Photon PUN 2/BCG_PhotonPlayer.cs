#if BCG_ENTEREXITPHOTON && PHOTON_UNITY_NETWORKING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class BCG_PhotonPlayer : MonoBehaviourPunCallbacks{

    private BCG_EnterExitPlayer player;

    private void Awake() {

        player = GetComponent<BCG_EnterExitPlayer>();

    }

    private void Update(){

        // If we are the owner of the player, enable controller of the player. Do opposite if we don't own this.
        if (photonView.IsMine)
            player.canControl = true;
        else 
            player.canControl = false;

    }
}
#endif