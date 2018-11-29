using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;



public class WandManager : MonoBehaviour {

	#region variables
	private SteamVR_TrackedObject trackedObject = null;
	private SteamVR_Controller.Device device;
	//the interactionManager manager script
	private InteractionManager interactionManager = null;
	//variable to hold how long a button has been held down.
	float timePressed = 0f;

	#endregion

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
			timePressed = Time.time;
			
		}
		//Up
		if(device.GetPressUp(SteamVR_Controller.ButtonMask.Grip)){
			timePressed = Time.time - timePressed;
			if(timePressed<=0.5f){
				interactionManager.addNewVertex();
			}else{
				interactionManager.removeVertex();
			}
			
		}
		#endregion

		#region touchpad
		//Down
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)){
			Vector2 touchpad = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
			if (touchpad.y > 0.6f){
				interactionManager.moveVertexUp();
			}else if (touchpad.y < -0.6f){
				interactionManager.moveVertexDown();
				//interactionManager.removeVertex();
			}

			if (touchpad.x > 0.6f){
				
				interactionManager.increaseVertexSize();
			}else if (touchpad.x < -0.6f){
				
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
