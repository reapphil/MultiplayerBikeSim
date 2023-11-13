using Fusion;

namespace Network
{
    public struct VehicleInputData : INetworkInput
    {
        public float SteeringAngle;
        public float SteeringAngleRaw;
        public float Speed;
        public float Throttle;
        public float Brake;
    }
}