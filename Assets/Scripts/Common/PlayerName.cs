using System;
using Network;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Common
{
    public class PlayerName : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _txtPlayerName;

        public void SetPlayerName(string playerName)
        {
            _txtPlayerName.text = playerName;
        }
        

        private void Update()
        {
            if (PlayerManager.LocalVehicle)
            {
                transform.LookAt(PlayerManager.LocalVehicle.ActiveCameraGO.transform);
                Vector3 rot = transform.eulerAngles;
                rot.x = rot.z = 0;
                transform.eulerAngles = rot;
            }
        }
    }
}