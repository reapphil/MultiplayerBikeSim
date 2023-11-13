using System.Collections.Generic;
using BaseClasses;
using Common;
using Enums;
using Fusion;
using UnityEngine;

namespace Network
{
    public class PlayerManager : MonoBehaviour
    {
        private static List<VehicleRider> _players = new List<VehicleRider>();
        public static List<VehicleRider> Players => _players;
        public static int Count => Players.Count;
        public static int HumanPlayerCount {
            get
            {
                int count = 0;
                for (int i = 0; i < Players.Count ; i++)
                {
                    if (Players[i].InputMode != InputMode.AI)
                        count++;
                }

                return count;
            }
        }
        public static BaseClasses.BaseVehicleController LocalVehicle;
        public static VehicleRider LocalVehicleRider;
        public static PlayerRef LocalPlayerRef;

        public static bool IsLocalPlayerReady => LocalVehicle && GameController.Instance;

        

        public static void AddPlayer(VehicleRider vehicleRider)
        {
            UnityEngine.Debug.Log($"Rider {vehicleRider.name} Added");
            // Sort the player list when adding players
            _players.Add(vehicleRider);
            if (vehicleRider.HasInputAuthority && vehicleRider.InputMode != InputMode.AI)
            {
                LocalVehicleRider = vehicleRider;
                LocalPlayerRef = vehicleRider.Object.InputAuthority;
            }
        }

        public static void RemovePlayer(VehicleRider player)
        {
            if (player==null || !_players.Contains(player))
                return;

            UnityEngine.Debug.Log("Rider Removed " + player.name);

            _players.Remove(player);
        }
        
        public static VehicleRider Get(PlayerRef playerRef)
        {
            for (int i = _players.Count - 1; i >= 0; i--)
            {
                if (_players[i] == null || _players[i].Object == null)
                {
                    _players.RemoveAt(i);
                    UnityEngine.Debug.Log("Removing null rider");
                }
                else if (_players[i].Object.InputAuthority == playerRef)
                    return _players[i];
            }

            return null;
        }

        public static int GetIndexOfPlayer(VehicleRider player)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i] == player)
                    return i;
            }

            return -1;
        }

    }
}