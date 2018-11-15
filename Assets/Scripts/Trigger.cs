using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour {

	public int startID;
	public int endID;

	public void setIDs(int newStartID, int newEndID){
		startID = newStartID;
		endID = newEndID;
	}

	public int getStartID(){
		return startID;
	}

	public int getEndID(){
		return endID;
	}

	public void OnTriggerEnter(Collider other){
		if(other.tag=="GameController"){
			//print("entering");
			other.GetComponent<InteractionManager>().setHoveringBoxCollider(gameObject);
		}
	}


	public void OnTriggerExit(Collider other){
		if(other.tag=="GameController"){
			other.GetComponent<InteractionManager>().setHoveringBoxCollider(null);
			//print(other.gameObject.name + " is leaving collider " + gameObject.name);
		}
	}
}
