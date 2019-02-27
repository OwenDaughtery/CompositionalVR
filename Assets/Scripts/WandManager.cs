using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;



public class WandManager : MonoBehaviour {
    /*
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
		//pressing trigger down, call pick up on interaction manager
		if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)){
			interactionManager.pickUp();
		}

		//call let go of interaction manager, releasing trigger
		if(device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger)){
			interactionManager.letGo();
		}
		

		//Value
		Vector2 triggerValue = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger);
		#endregion

		#region grip
		//when grip is activated, get time it was activated.
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Grip)){
			timePressed = Time.time;
			
		}
		//when grip is released, get time it was released, then either call add new vertex or remove vertex.
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
		//if the vertex has been tapped
		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)){
			Vector2 touchpad = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
            //if upper region of vertex tapped.
			if (touchpad.y > 0.6f){
				interactionManager.moveVertexUp();
            //if lower region of vertex tapped.
			}else if (touchpad.y < -0.6f){
				interactionManager.moveVertexDown();
				//interactionManager.removeVertex();
			}

            //if right
			if (touchpad.x > 0.6f){
				
				interactionManager.increaseVertexSize();
            //if left.
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
    */
}
