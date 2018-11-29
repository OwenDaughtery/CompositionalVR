using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour {

	#region variables
	public int startID;
	public int endID;
	#endregion

	#region getters and setters
	//simple method used to set the start and end ids of the object.
	public void setIDs(int newStartID, int newEndID){
		startID = newStartID;
		endID = newEndID;
	}

	//simple method to get the start ID
	public int getStartID(){
		return startID;
	}

	//simple method to get the end ID
	public int getEndID(){
		return endID;
	}

	#endregion

	#region utilities
	//for every frame that a collider is in the box collider, call setHoveringBoxColliders on interaction manager.
	public void OnTriggerStay(Collider other){
		if(other.tag=="GameController"){
			other.GetComponent<InteractionManager>().setHoveringBoxColliders(gameObject);
		}
	}
	#endregion
}
