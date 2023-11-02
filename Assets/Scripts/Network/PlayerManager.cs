using System.Collections.Generic;
using Common;
using Enums;
using Fusion;
using UnityEngine;

namespace Network
{
    public class PlayerManager : MonoBehaviour
    {
        private static List<BikeRider> _players = new List<BikeRider>();
        public static List<BikeRider> Players => _players;
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
        public static BikeController LocalBike;
        public static BikeRider LocalRider;
        public static PlayerRef LocalPlayerRef;

        public static bool IsLocalPlayerReady => LocalBike && GameController.Instance;

        

        public static void AddPlayer(BikeRider rider)
        {
            UnityEngine.Debug.Log($"Rider {rider.name} Added");
            // Sort the player list when adding players
            _players.Add(rider);
            if (rider.HasInputAuthority && rider.InputMode != InputMode.AI)
            {
                LocalRider = rider;
                LocalPlayerRef = rider.Object.InputAuthority;
            }
        }

        public static void RemovePlayer(BikeRider player)
        {
            if (player==null || !_players.Contains(player))
                return;

            UnityEngine.Debug.Log("Rider Removed " + player.name);

            _players.Remove(player);
        }
        
        public static BikeRider Get(PlayerRef playerRef)
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

        public static int GetIndexOfPlayer(BikeRider player)
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