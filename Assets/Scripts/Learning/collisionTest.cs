using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionTest : MonoBehaviour {

	public void onCollisionEnter(Collision other){
		int a = 1;
		a-=1;
		int b = 1/a;

		string otherObject = other.gameObject.name;
		Debug.Log(otherObject + " just collided!");
	}
}
