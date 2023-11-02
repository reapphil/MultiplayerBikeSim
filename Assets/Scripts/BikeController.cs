using System;
using System.Collections.Generic;
using BaseClasses;
using Common;
using Enums;
using Fusion;
using Fusion.Sockets;
using Network;
using UnityEngine;
using UnityEngine.Serialization;

/*
This script handles the transformation of the bike model and VR player.
speed and steering angle are public and set externally.

this script is meant to be used with the 'City Bike with LOD' asset, but should also work fine with other models.
https://assetstore.unity.com/packages/3d/vehicles/land/city-bike-with-lod-54310

LODs have been removed

!the bike body transform (child of the bike prefab, containing all parts and meshes) has a rotation of (-90, 0, 0)!
this transform is not used directly but beware of the affect on the local rotation of the wheels and handle bar
*/

[RequireComponent(typeof(Rigidbody))]
public class BikeController : NetworkBehaviour, INetworkRunnerCallbacks
{
    [Networked]
    public BikeRider Rider { get; set; }
    
    [Networked]
    private float _speedInMetersPerSecond { get; set; }
    
    [Networked]
    private float _steeringAngle { get; set; }
    
    private Camera _thirdPersonCamera;
    private Camera _firstPersonCamera;
    [SerializeField] private Transform vRPlayerPositionTransform; //empty to use for adjusting the VR player position
    [SerializeField] private Transform handlebar;
    [SerializeField] private Transform frontWheel;
    [SerializeField] private Transform rearWheel;
    [SerializeField] private float wheelRadius;
    [SerializeField] private float wheelbase = 1.0f; //distance between the center of both the front and rear axle of the bike. just play around until the turning radius looks good
    [Range(0.0f, 1.0f)][SerializeField] private float tiltFactor = 1.0f;
    private Quaternion _initialHandlebarRotation;
    private Rigidbody bikeRigidbody;
    private float lastBikeRotationY;
    private Vector3 lastRearWheelPosition;
    private Vector3 lastFrontWheelPosition;
    private Quaternion smoothBikeRotation;
    private float smoothTilt = 0.0f;
    //private BikeGamePadInput gamePadInput;
    //private BikeVRInput vrInput;
    private BikeInputBase _bikeInputController;
    
    private GameObject bike;
    private Vector3 initialBikePosition;
    private Quaternion initialBikeRotation;

    public InputMode InputMode => Rider.InputMode;

    public CameraMode CameraMode => Rider.CameraMode;

    //debug
    [SerializeField] private bool drawDebug = false;
    private float smoothLeftWheelTravelDistance = 0.0f;
    private float smoothRightWheelTravelDistance = 0.0f;
    public Transform HandleBar => handlebar;

    private BikeInputData _bikeInputData;
    private NetworkTransform _networkRb;

    [SerializeField] private GameObject _avatar;
    
    //Animation Stuff
    private int _speedHash;
    [SerializeField] private Animator _animatorBike;
    [SerializeField] private Animator _animatorRider;
    

    private void Awake()
    {
        _speedHash = Animator.StringToHash("Speed");
    }

    public override void Spawned()
    {
        base.Spawned();
        _networkRb = GetComponent<NetworkTransform>();
        if (HasInputAuthority)
        {
            _networkRb.InterpolationDataSource = InterpolationDataSources.NoInterpolation;
            Runner.AddCallbacks(this);
            _avatar.SetActive(false);
        }

    }

    public void Initialize(BikeRider rider, BikeInputBase inputController)
    {
        Rider = rider;
        _bikeInputController = inputController;
        bike = gameObject;
        initialBikePosition = bike.transform.position;
        initialBikeRotation = bike.transform.rotation;
        bikeRigidbody = GetComponent<Rigidbody>();
        lastRearWheelPosition = rearWheel.transform.position;
        lastFrontWheelPosition = frontWheel.transform.position;
        _initialHandlebarRotation = handlebar.localRotation;
        lastBikeRotationY = bike.transform.localEulerAngles.y;
        smoothBikeRotation = bike.transform.rotation;
    }
    

    private void setCameraMode()
    {
        switch (CameraMode)
        {
            case CameraMode.ThirdPerson:
                _thirdPersonCamera.gameObject.SetActive(true);
                _firstPersonCamera.gameObject.SetActive(false);
                break;
            case CameraMode.FirstPerson:
                _thirdPersonCamera.gameObject.SetActive(false);
                _firstPersonCamera.gameObject.SetActive(true);
                break;
        }
    }

    public void DeactivateCamera()
    {
        if(_thirdPersonCamera)
            _thirdPersonCamera.gameObject.SetActive(false);
        if(_firstPersonCamera)
            _firstPersonCamera.gameObject.SetActive(false);
    }


    public void SetCameras(Camera firstPersonCamera, Camera thirdPersonCamera)
    {
        _firstPersonCamera = firstPersonCamera;
        _thirdPersonCamera = thirdPersonCamera;
        setCameraMode();
    }

    
    /*
    //FixedUpdate would be nicer for physics, but resulted in significant jitter.
    I believe the FixedUpdate rate is determined by the XR toolkit (in our case).
    Maybe interpolation could be a solution? Anyways works fine for now.
    */
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        //enabeling or disabeling input scripts based on selection
        // if (gamePadInput != null) gamePadInput.enabled = inputMode == InputMode.Gamepad;
        // if (vrInput != null) vrInput.enabled = inputMode == InputMode.VR;

        if (GetInput(out BikeInputData _bikeInput))
        {
            _speedInMetersPerSecond = _bikeInput.Speed;
            _steeringAngle = _bikeInput.SteeringAngle;
        }
        Debug.Log($"speed: {_speedInMetersPerSecond}");
         _animatorBike.SetFloat(_speedHash,_speedInMetersPerSecond);
         _animatorRider.SetFloat(_speedHash,_speedInMetersPerSecond);

        //rotating the wheels based on idividual distance traveled in wheel direction
        Vector3 frontWheelPositionDelta = frontWheel.position - lastFrontWheelPosition;
        Vector3 rearWheelPositionDelta = rearWheel.position - lastRearWheelPosition;
        lastRearWheelPosition = rearWheel.position;
        lastFrontWheelPosition = frontWheel.position;
        Vector3 frontWheelForwardVector = Vector3.ProjectOnPlane(-handlebar.up, bike.transform.up).normalized;
        Vector3 rearWheelForwardVector = bike.transform.forward;
        float frontWheelRollDistance = Vector3.Dot(frontWheelPositionDelta, frontWheelForwardVector);
        float rearWheelRollDistance = Vector3.Dot(rearWheelPositionDelta, rearWheelForwardVector);
        float frontWheelRotation = 180 * frontWheelRollDistance / (Mathf.PI * wheelRadius);
        float rearWheelRotation = 180 * rearWheelRollDistance / (Mathf.PI * wheelRadius);
        frontWheel.Rotate(Vector3.right * frontWheelRotation, Space.Self);
        rearWheel.Rotate(Vector3.right * rearWheelRotation, Space.Self);

        //rigid body verlocity is simpy set to the current speedInMetersPerSecond in the direction of travel
        //I don't really like this, should have proper physics handling w
        float downwardsVelocity = bikeRigidbody.velocity.y; //keeping the current downwards velocity
        Vector3 velocity = (bike.transform.forward * _speedInMetersPerSecond) * (Runner.DeltaTime * 2f);
        velocity.y = downwardsVelocity;
        bikeRigidbody.velocity = velocity;
        
        handlebar.localRotation =  _initialHandlebarRotation*Quaternion.Euler(0,0,_steeringAngle);

        float tilt = 0;
        if (Mathf.Abs(_steeringAngle) < 0.01f) //Going straight
        {
            bike.transform.position = bike.transform.position +
                                      bike.transform.forward * Runner.DeltaTime * _speedInMetersPerSecond;
        }
        else //Curve
        {
            float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(_steeringAngle) * Mathf.Deg2Rad));
            float sign = Mathf.Sign(_steeringAngle);
            Vector3 turningCurveCenter =
                (bike.transform.position + (bike.transform.right.normalized * sign * turnRadius));
            bike.transform.RotateAround(turningCurveCenter, Vector3.up,
                sign * ((_speedInMetersPerSecond * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Runner.DeltaTime);

            tilt =
                Mathf.Atan((_speedInMetersPerSecond * _speedInMetersPerSecond) / (turnRadius * Physics.gravity.y)) *
                Mathf.Rad2Deg * sign * tiltFactor;
        }

        //smoothing tilt is probably unnecessary
        float lastSmoothTilt = smoothTilt;
        smoothTilt = Mathf.Lerp(smoothTilt, tilt, Runner.DeltaTime * 30f);
        bike.transform.localEulerAngles = new Vector3(bike.transform.localEulerAngles.x,
            bike.transform.localEulerAngles.y, smoothTilt);



        updateFirstPersonCamera(lastSmoothTilt);


        //Debug Lines
        if (!drawDebug) return;
        smoothLeftWheelTravelDistance =
            Mathf.Lerp(smoothLeftWheelTravelDistance, frontWheelRotation, Runner.DeltaTime * 5f);
        smoothRightWheelTravelDistance =
            Mathf.Lerp(smoothRightWheelTravelDistance, rearWheelRotation, Runner.DeltaTime * 5f);
        Vector3 rearwheelflorrpos = rearWheel.position;
        Vector3 frontwheelflorrpos = frontWheel.position;
        rearwheelflorrpos.y = bike.transform.position.y;
        frontwheelflorrpos.y = bike.transform.position.y;
        DrawThickDebugLine(frontwheelflorrpos,
            frontwheelflorrpos + frontWheelForwardVector * frontWheelRollDistance * 100f, Color.red, 0.01f);
        DrawThickDebugLine(rearwheelflorrpos,
            rearwheelflorrpos + rearWheelForwardVector * rearWheelRollDistance * 100f, Color.red, 0.01f);
        if (Mathf.Abs(_steeringAngle) > 0.01f)
        {
            float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(_steeringAngle) * Mathf.Deg2Rad));
            float sign = Mathf.Sign(_steeringAngle);
            Vector3 turningCurveCenter =
                (bike.transform.position + (bike.transform.right.normalized * sign * turnRadius));
            DrawThickDebugLine(turningCurveCenter, turningCurveCenter + bike.transform.up * 0.3f, Color.green,
                0.01f);
            DrawThickDebugLine(turningCurveCenter, turningCurveCenter + bike.transform.right * 0.3f, Color.blue,
                0.01f);
            DrawThickDebugLine(turningCurveCenter, turningCurveCenter + bike.transform.forward * 0.3f, Color.red,
                0.01f);
            DrawThickDebugLine(bike.transform.position, turningCurveCenter, Color.yellow, 0.01f);
            turningCurveCenter.y = bike.transform.position.y;
            DrawThickDebugCircle(turningCurveCenter, Vector3.up, turnRadius, 64, Color.yellow, 0.01f);
        }
    }



    private void Update()
    {
        if (HasInputAuthority && _bikeInputController != null)
        {
            _bikeInputData.Speed = _bikeInputController.GetSpeed();
            _bikeInputData.SteeringAngle = _bikeInputController.GetSteeringAngle();
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        input.Set(_bikeInputData);
    }
    

    private void updateFirstPersonCamera(float lastSmoothTilt)
    {
        if (HasInputAuthority && CameraMode == CameraMode.FirstPerson && Rider.IsVR)
        {
            //Moving and turning the VR Player with the bike
            //Smoothing VR Camera transforms slightly to reduce jitter and motion sickness
            _firstPersonCamera.transform.position = Vector3.Lerp(_firstPersonCamera.transform.position,
                vRPlayerPositionTransform.position, Runner.DeltaTime * 30f);
            smoothBikeRotation = Quaternion.Lerp(smoothBikeRotation, bike.transform.rotation, Runner.DeltaTime * 20f);
            float deltaBikeRotationY = smoothBikeRotation.eulerAngles.y - lastBikeRotationY;
            lastBikeRotationY = smoothBikeRotation.eulerAngles.y;
            Vector3 vrRotation = _firstPersonCamera.transform.rotation.eulerAngles;
            float smoothTiltDelta = smoothTilt - lastSmoothTilt;
            Quaternion deltaRotationY = Quaternion.Euler(0, deltaBikeRotationY, 0);
            Quaternion deltaTiltRotation = Quaternion.Euler(0, 0, smoothTiltDelta);
            //VRCamera.transform.rotation = deltaRotationY * deltaTiltRotation * VRCamera.transform.rotation;
            _firstPersonCamera.transform.rotation = Quaternion.Euler(vrRotation.x, vrRotation.y + deltaBikeRotationY,
                vrRotation.z + smoothTiltDelta);
        }
    }

    public void ResetBike() //not tested yet sorry
    {
        bike.transform.position = initialBikePosition;
        bike.transform.rotation = initialBikeRotation;
        bikeRigidbody.velocity = Vector3.zero;
        bikeRigidbody.angularVelocity = Vector3.zero;
        lastRearWheelPosition = rearWheel.transform.position;
        lastFrontWheelPosition = frontWheel.transform.position;
        handlebar.localRotation = _initialHandlebarRotation;
        smoothTilt = 0.0f;
        lastBikeRotationY = bike.transform.localEulerAngles.y;
        smoothBikeRotation = bike.transform.rotation;
    }

    //debug
    public static void DrawThickDebugLine(Vector3 start, Vector3 end, Color color, float thickness)
    {
        Vector3 direction = end - start;
        Vector3 normal = Vector3.Cross(direction, Vector3.up).normalized;
        for (int i = 0; i < 10; i++)
        {
            float angle = i * 36;
            Vector3 offset = Quaternion.AngleAxis(angle, direction) * normal * thickness;
            Debug.DrawLine(start + offset, end + offset, color);
        }
    }
    public static void DrawThickDebugCircle(Vector3 center, Vector3 upVector, float radius, int nSegments, Color color, float thickness)
    {
        Vector3 lastPoint = center + (Quaternion.AngleAxis(360f / nSegments, upVector) * (Vector3.ProjectOnPlane(Vector3.forward, upVector).normalized * radius));
        for (int i = 1; i <= nSegments; i++)
        {
            float angle = i * 360f / nSegments;
            Vector3 point = center + (Quaternion.AngleAxis(angle, upVector) * (Vector3.ProjectOnPlane(Vector3.forward, upVector).normalized * radius));
            DrawThickDebugLine(lastPoint, point, color, thickness);
            lastPoint = point;
        }
    }

    #region Unused Fusion-Callbacks
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    
    #endregion
}

