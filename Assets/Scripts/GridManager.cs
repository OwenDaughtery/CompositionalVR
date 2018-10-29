﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

	#region variables
	//A list for storing every Vertex (sphere) in the environment
	private GameObject[] allVertices;
	
	//3 variables that will hold the distance of a vertex (sphere) in relation to x, y, and z
	private float xDist;
	private float yDist;
	private float zDist;

	//3 vectors that will hold the single x, y, and z cordinate of a vertex (sphere)
	Vector3 xVector;
	Vector3 yVector;
	Vector3 zVector;

	//The vertexManager for any specific vertex (sphere) at any given point
	private VertexManager vertexManager;

	//The list of possible notes to play, always includes none at start.
	public enum Notes {none, C4, D4, E4, F4, G4, A4, B4, C5, D5, E5, F5, G5, A5, B5};

	#endregion

	void Start () {
		xVector = new Vector3(0f, 0f, 0f);
		yVector = new Vector3(0f, 0f, 0f);
		zVector = new Vector3(0f, 0f, 0f);
	}
	
	void Update () {
		//every frame, call the method that will update the variables of all the vertices in the environment.
		getVertexStats();
	}

	#region main

	//the main method for this script, used to go through all vertices (spheres) in the environment and update their positions
	private void getVertexStats(){
		//get all vertices (spheres)
		allVertices = GameObject.FindGameObjectsWithTag("Vertex");

		foreach (GameObject vertex in allVertices){
			//get each vertexManager attached to current vertex (sphere)
			vertexManager = vertex.GetComponent<VertexManager>();

			//Set the vector variables according to the vertice (sphere) position. To do maths on later.
			xVector.Set(vertex.transform.position.x, 0f, 0f);
			yVector.Set(0f, vertex.transform.position.y, 0f);
			zVector.Set(0f, 0f, vertex.transform.position.z);

			//calculate the distance between the vertices (sphere) x, y, and z cords from the center.
			xDist = Vector3.Distance(gameObject.transform.position, xVector);
			yDist = Vector3.Distance(gameObject.transform.position, yVector);
			zDist = Vector3.Distance(gameObject.transform.position, zVector);
		
			if(vertexManager!=null){
				//setting vertex volume by passing x^2 + z^2 to convert volume
				vertexManager.setVertexVolume(convertVolume((xDist*xDist) + (zDist*zDist)));

				//setting vertex timing by passing y distance.
				vertexManager.setVertexTiming(convertTiming(yDist));

				//calling calculateAngle with the vertices (sphere) x and z cordinates, and then setting the vertices note accordingly.
				float vertexAngle = calculateAngle(vertexManager.transform.position.x, vertexManager.transform.position.z);
				vertexManager.setVertexAngle(vertexAngle);
				vertexManager.setVertexNote(convertAngle(vertexAngle));
			}
		}
	}

	#endregion

	#region volume related

	//method used to "clamp" volume to range 0-1
	private float convertVolume(float oldVolume){
		return (1-((oldVolume - 0) / (17 - 0) * (1 - 0) + 0));
	}

	#endregion

	#region timing related

	//method used to "clamp" volume to range 0-16
	private float convertTiming(float oldTiming){
		//0.1623
		return Mathf.Floor((16-((oldTiming - 0.1623f) / (3.2f - 0.1623f) * (16 - 0) + 0)));
	}

	#endregion

	#region note related

	//method used to perform trig on x and z cordinate of vertex (sphere) and to call calculateQuad
	static public float calculateAngle(float x, float z){
		int quad = calculateQuad(x, z);
		//converting to degrees
		float angle = (Mathf.Atan(z/x)* Mathf.Rad2Deg);

		//applying maths depending on which quarter the vertex (sphere) is in
		if(quad==0){
			angle = ((angle-90)*-1)+270;
		}else if(quad==1){
			angle = angle*-1;
		}else if(quad==2){
			angle = ((angle-90)*-1)+90;
		}else if(quad==3){
			angle = (angle*-1)+180;
		}
		return angle;
	}

	//method that takes a float 0-360, and uses enum Notes to decide which enum to return.
	private Notes convertAngle(float angle){
		Notes note = Notes.none;
		float segmentIncrement = 360/(Notes.GetNames(typeof(Notes)).Length-2);
		float currentSegment=0;
		int index =1;
		while(index<(Notes.GetNames(typeof(Notes)).Length)){
			if(angle>=currentSegment && angle<currentSegment+segmentIncrement){
				note = (Notes)index;
			}
			index+=1;
			currentSegment+=segmentIncrement;
		}
		return note;
	}

	//method used to find out which quarter ("quadrant") a given cordinate is in.
	static public int calculateQuad(float x, float z){
		if(x>=0 && z>=0){
			return 0;
		}else if(x>=0 && z<0){
			return 1;
		}else if(x<0 && z<0){
			return 2;
		}else{
			return 3;
		}
	}
	#endregion


}