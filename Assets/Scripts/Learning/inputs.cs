using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputs : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("TB")){
			Debug.Log("Do Something Here");
		}
	}
}
