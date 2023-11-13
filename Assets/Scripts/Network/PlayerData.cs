using Fusion;

namespace Network
{
    public struct PlayerData : INetworkStruct
    {
        public bool Initialized;
        public int InputMode;
        public bool AddCameraController;
        public int CameraMode;
        public int VehiclePrefab;
        public NetworkString<_32> Name;
    }
}