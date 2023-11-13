//----------------------------------------------
//            BCG Shared Assets
//
// Copyright © 2014 - 2021 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enter Exit for BCG Vehicles.
/// </summary>
[AddComponentMenu("BoneCracker Games/Shared Assets/Enter-Exit/BCG Enter Exit Script For Vehicle")]
public class BCG_EnterExitVehicle : MonoBehaviour {

#if BCG_RCC
	internal RCC_CarControllerV3 carController;
#endif
#if BCG_RTC
    internal RTC_TankController tankController;
#endif
#if BCG_RHOC
	internal RHOC_HovercraftController hoverController;
#endif

    private Rigidbody rigid;
    public BCG_EnterExitPlayer driver;

    public GameObject correspondingCamera;
    public Transform getOutPosition;

    internal float speed = 0f;

    public delegate void onBCGVehicleSpawned(BCG_EnterExitVehicle player);
    public static event onBCGVehicleSpawned OnBCGVehicleSpawned;

    void Awake() {

        rigid = GetComponent<Rigidbody>();

        gameObject.SendMessage("SetCanControl", false, SendMessageOptions.DontRequireReceiver);

    }

    void OnEnable() {

        Reset();

        if (OnBCGVehicleSpawned != null)
            OnBCGVehicleSpawned(this);

    }

    void FindCamera() {

        if (correspondingCamera)
            return;

#if BCG_RCC

		if(carController){
			
			correspondingCamera = FindObjectOfType<RCC_Camera> ().gameObject;
			return;

		}

#endif

#if BCG_RTC

        if (tankController) {

            correspondingCamera = FindObjectOfType<RTC_Camera>().gameObject;
            return;

        }

#endif

#if BCG_RHOC

//		if(hoverController){
//
//		correspondingCamera = FindObjectOfType<RCC_Camera> ().gameObject;
//		return;
//
//		}

#endif

    }

    IEnumerator BCGVehicleSpawned() {

        yield return new WaitForEndOfFrame();

        if (OnBCGVehicleSpawned != null)
            OnBCGVehicleSpawned(this);

    }

    private void Update() {

        //if (driver != null && Input.GetKeyDown(BCG_EnterExitSettings.Instance.enterExitVehicleKB))
        //	GetOut();

    }

    void FixedUpdate() {

        //Speed.
        speed = rigid.velocity.magnitude * 3.6f;

    }

    void OnDisable() {



    }

    public void GetOut() {

        driver.GetOut();

    }

    public void Reset() {

#if BCG_RCC
carController = GetComponent<RCC_CarControllerV3>();
#endif
#if BCG_RTC
        tankController = GetComponent<RTC_TankController>();
#endif
#if BCG_RHOC
hoverController = GetComponent<RHOC_HovercraftController>();
#endif

        FindCamera();

        if (transform.Find("Get Out Pos")) {

            getOutPosition = transform.Find("Get Out Pos");

        } else {

            GameObject getOut = new GameObject("Get Out Pos");
            getOut.transform.SetParent(transform, false);
            getOut.transform.rotation = transform.rotation;
            getOut.transform.localPosition = new Vector3(-1.5f, 0f, 0f);
            getOutPosition = getOut.transform;

        }

    }

}
