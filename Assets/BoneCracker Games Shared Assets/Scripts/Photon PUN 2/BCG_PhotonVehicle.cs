#if BCG_ENTEREXITPHOTON && PHOTON_UNITY_NETWORKING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class BCG_PhotonVehicle : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks {

    private RCC_CarControllerV3 carController;
    private BCG_EnterExitVehicle enterExitVehicle;

    void Awake() {

        carController = GetComponent<RCC_CarControllerV3>();
        enterExitVehicle = GetComponent<BCG_EnterExitVehicle>();

        PhotonNetwork.AddCallbackTarget(this);

    }

    void OnDestroy() {

        PhotonNetwork.RemoveCallbackTarget(this);

    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) {

        if (targetView != base.photonView)
            return;

        base.photonView.TransferOwnership(requestingPlayer);

    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner) {

        if (targetView != base.photonView)
            return;

    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest) {

        throw new System.NotImplementedException();

    }

    void Update() {

        if (photonView.Owner == null)
            return;

        if (PhotonNetwork.LocalPlayer == null)
            return;

        if (enterExitVehicle) {

            if (enterExitVehicle.driver) {

                if (enterExitVehicle.driver.GetComponent<PhotonView>().OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber) {

                    carController.canControl = true;
                    carController.externalController = false;

                } else {

                    carController.canControl = false;
                    carController.externalController = true;

                }

            } else {

                carController.canControl = false;
                carController.externalController = true;

            }

        }

    }

}
#endif