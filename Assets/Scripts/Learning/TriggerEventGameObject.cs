using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEventGameObject : MonoBehaviour {

	public int health = 50;
	public GameObject GameObject;
	public GameObject GameObject2;
	public GameObject thePlayer;
	public GameObject floor;
	private GameObject player2;

	void Start(){
		player2 = GameObject.FindGameObjectWithTag("Player");
	}

	void OnTriggerEnter(Collider other){
		if(other.CompareTag("Player")){
			floor.GetComponent<MeshRenderer>().enabled = false;
			player2.GetComponent<FirstScript>().enabled = false;
			GameObject.SetActive(false);
			GameObject2.SetActive(true);
		}
	}
/*
	public GameObject cubeObject;

	void OnTriggerEnter(Collider other){
		if(other.CompareTag("Player")){
			cubeObject.SetActive(false);
		}
	}
 */
}
