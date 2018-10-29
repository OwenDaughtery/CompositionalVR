using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;



public class WandManager : MonoBehaviour {

	private SteamVR_TrackedObject trackedObject = null;
	private SteamVR_Controller.Device device;

	//the interactionManager manager script
	private InteractionManager interactionManager = null;

	// Use this for initialization
	void Start () {
		trackedObject = GetComponent<SteamVR_TrackedObject>();
		interactionManager = GetComponent<InteractionManager>();
	}
	
	void Update () {
		device = SteamVR_Controller.Input((int)trackedObject.index);
		
		#region trigger
		//Down
		if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)){
			interactionManager.pickUp();
		}

		//Up
		if(device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger)){
			interactionManager.letGo();
		}
		

		//Value
		Vector2 triggerValue = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger);
		#endregion

		#region grip
		//Down
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Grip)){
			interactionManager.addNewVertex();
		}

		//Up
		if(device.GetPressUp(SteamVR_Controller.ButtonMask.Grip)){
			
		}
		#endregion

		#region touchpad
		//Down
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)){
			Vector2 touchpad = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
			if (touchpad.y > 0.6f){
				print("Moving Up");
			}else if (touchpad.y < -0.6f){
				print("Moving Down");
				interactionManager.removeVertex();
			}

			if (touchpad.x > 0.6f){
				print("Moving Right");
				interactionManager.increaseVertexSize();
			}else if (touchpad.x < -0.6f){
				print("Moving left");
				interactionManager.decreaseVertexSize();
			}
		}

		//Up
		if(device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad)){
			
		}

		//Value
		Vector2 touchValue = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
		#endregion
	}
}
