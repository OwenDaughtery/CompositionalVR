﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour {


	#region variables

	//The attached script line renderer.
	public LineRenderer attachedLR;
	//The pulse manager in the game.
	private PulseManager pulseManager;
	//the actual game object that the script is attached too. (aka a baseline)
	private GameObject attachedObject;

	//an enum of possible voices, along with a variable to hold a voice enum.
	public enum Voices {None, A, B, C};
	private Voices voice;

	//the vertices of the line renderer.
	private Vector3[] vertices;

	//boolean as to whether the baseline is currently tethered to the tether point yet.
	//private bool isTethered = true;
	private bool isLineActive = true;

	//the object pool to get vertices from.
	private ObjectPooler objectPooler;
	
	public Dictionary<float, List<VertexManager>> timingDict;

	//int variable to hold the index of the last vertex that is played.
	[SerializeField]
	private int lastPlayedVertex;
	//the last height of what is essentially the "read head" of the system.
	private int lastHeight;
	//the local rotation of this baseline
	private float localRotation;
	private BoxColliderManager boxColliderManager;

	//possible colours where the letter corressponds to the voice enum.
	Color colourA = new Color(0.89f, 0.12f, 0.05f, 1);
	Color colourB = new Color(0.05f, 0.09f, 0.89f, 1);
	Color colourC = new Color(0.05f, 0.89f, 0.47f, 1);
	Color colourNone = new Color(1f, 1f, 1f, 1);


	#endregion

	void Start () {
		localRotation = gameObject.transform.eulerAngles.y;
		timingDict = new Dictionary<float, List<VertexManager>>();
		for (int i = -1; i <= GridManager.getYSegments(); i++){
			timingDict[i] = new List<VertexManager>();
		}
		lastPlayedVertex = -1;
		objectPooler = ObjectPooler.Instance;
		attachedObject = gameObject;
		Vector3[] currentVerts = getVertices();
		drawVerts(currentVerts);
		GameObject objectPulseManager = GameObject.FindGameObjectWithTag("PulseManager");
		pulseManager = objectPulseManager.GetComponent<PulseManager>();
		chooseVoice();

		boxColliderManager = this.GetComponent<BoxColliderManager>();
		pulseManager.activateLineManager(this);
	}

	void Update(){
	}

	//==MAY REMOVE THIS METHOD==
	public static float FindDegree(float x, float y){
      float value = ((float)((System.Math.Atan2(x, y) / System.Math.PI) * 180f));
      //if (value < 0) value += 360f;
      return value;
  	}	

	#region  getters and setters
	//get the variable local rotation (that represents how this empty object that this line renderer is a child of is rotated)
	public float getLocalRotation(){
		return localRotation;
	}

	//==MAY REMOVE THIS METHOD==
	public Dictionary<float, List<VertexManager>> getTimingDict(){
		return timingDict;

	}

	//update the timing dictionary by removing the vertex manager from the old key and inserting it into the new one.
	public void updateTimingDict(float oldTiming, float newTiming, VertexManager vm){
		timingDict[oldTiming].Remove(vm);
		timingDict[newTiming].Add(vm);
	}

	public int getNumberOfVertices(){
		return attachedLR.positionCount;
	}
	#endregion
	
	//==MAY REMOVE THIS ENTIRE REGION==
	#region tethering
	public bool checkIfTethered(GameObject vertex){
		return checkIfTetheredForY(vertex) && checkIfTetheredForXZ(vertex);
	}


	public bool checkIfTetheredForY(GameObject vertex){
		return vertex.GetComponent<VertexManager>().getVertexTiming()==GridManager.getYSegments();
	}

	public bool checkIfTetheredForXZ(GameObject vertex){
		return vertex.GetComponent<VertexManager>().getVertexVolume()>0.955f;
	}

	#endregion

	#region voice and colour

	//method called once to choose a random voice, and set the attached line renderer to the corresponding colour.
	private void chooseVoice(){
		voice = (Voices)Random.Range(1, (Voices.GetNames(typeof(Voices)).Length));
		setVoiceColour();
	}

	//method used to check the current assigned voice and set the attached line renderer to the corresponding colour.
	private void setVoiceColour(){
		if(voice==Voices.A){
			attachedLR.SetColors(colourA, new Color(0.89f, colourA.g+0.41f, 0.05f, 1));
		}else if(voice==Voices.B){
			attachedLR.SetColors(colourB, new Color(colourB.g+0.18f, colourB.r, 0.89f, 1));
		}else if(voice==Voices.C){
			attachedLR.SetColors(colourC,new Color(0.05f, 0.89f, colourC.b-0.28f, 1));
		}else if(voice==Voices.None){
			attachedLR.SetColors(colourNone, colourNone);
		}
	}

	//get colour of the attached line renderer that corresponds to a voice.
	public Color getColourOfVoice(){
		if(voice==Voices.A){
			return new Color(0.89f, colourA.g+0.41f, 0.05f, 1);
		}else if(voice==Voices.B){
			return new Color(colourB.g+0.18f, colourB.r, 0.89f, 1);
		}else if(voice==Voices.C){
			return new Color(0.05f, 0.89f, colourC.b-0.28f, 1);
		}
		return new Color(0,0,0,1);
	}

	//simple method to get the voice variable of this line manager
	public Voices getVoice(){
		return voice;
	}
	#endregion

	#region getting vertices (spheres and line renderer vertices)

	//Method used to get the current vertices of the LineRenderer and return them as a Vector3[]
	private Vector3[] getVertices(){
		vertices = new Vector3[attachedLR.positionCount];
		attachedLR.GetPositions(vertices);
		return vertices;

	}

	//Method used to get vertices (spheres), and return them as a List<GameObject>
	public List<GameObject> getChildrenVertices(){
		 List<GameObject> childrenvertices = new List<GameObject>();

		 foreach(Transform child in transform){
			 if(child.tag == "Vertex"){
				childrenvertices.Add(child.gameObject);
			}
		 }
		 return childrenvertices;
	}

	//method used to get all vertices (spheres), even ones that used to belong to this line manager but are being held by a controller.
	private List<GameObject> getAllChildrenVerticesForInterpole(){
		List<GameObject> childrenVertices = getChildrenVertices();
		//if the currently tracked children equals the number that's expected, just return those (means controllers don't have this lines children)
		if(childrenVertices.Count==attachedLR.positionCount){
			return childrenVertices;
		}
		//get controllers
		GameObject[] controllers = GameObject.FindGameObjectsWithTag("GameController");
		foreach (GameObject controller in controllers){
			GameObject possibleChild = controller.GetComponent<InteractionManager>().getCurrentGameObject();
			//if controller has child, and child isn't in this list, and childs baseline parent is this gameobject:
			if(possibleChild && !childrenVertices.Contains(possibleChild) && possibleChild.GetComponent<VertexManager>().getBaseLineParent()==gameObject.transform){
				childrenVertices.Add(possibleChild);
			}
		}
		childrenVertices.Sort(sortByVertexID);
		return childrenVertices;
	}
	#endregion

	#region drawing verts (spheres)

	//Method used to call drawVert(...) multiple time with the given Vector3[] of vertices
	private void drawVerts(Vector3[] currentVerts){
		int vertexID=0;
		foreach (var vert in currentVerts){
			VertexManager newVert = drawVert(vert, vertexID, true).GetComponent<VertexManager>();
			if(vertexID==0){
				newVert.setVertexTiming(-1);
			}else if(vertexID==getNumberOfVertices()-1){
				newVert.setVertexTiming(GridManager.getYSegments());
			}
			vertexID++;
		}	
	}

	//Method used to create a new vertex (which is a sphere) by grabbing one from pool
	private GameObject drawVert(Vector3 vert, int vertexID, bool isStatic){
		GameObject vertex = null;
		vertex = objectPooler.spawnFromPool("Vertex", vert, attachedLR.transform);
		
		//Get the object from pool, which will spawn it in, and set it's details to the parameters
		vertex.transform.name = "Vertex";
		vertex.AddComponent<VertexManager>();
		vertex.AddComponent<Rigidbody>();
		vertex.GetComponent<Rigidbody>().useGravity = false;
		vertex.GetComponent<Rigidbody>().isKinematic= true;
		vertex.GetComponent<VertexManager>().setVertexID(vertexID);
		vertex.GetComponent<VertexManager>().setParentsLineManager(this);
		
		return vertex;
	}

	//given the rotation and a position, rotate that position using Quaternions.
	public Vector3 rotateVertex(Vector3 pos, float rotationFloat){
		Quaternion rotation = Quaternion.Euler(0,rotationFloat,0);
		Vector3 myVector = pos;
		Vector3 rotateVector = rotation * myVector;
		return rotateVector;
	}

	//Method used to move the vertex of a linerenderer to a position
	public void moveLineVertex(int index, Vector3 pos, float eugerValueRotation){
		pos = rotateVertex(pos, -getLocalRotation());
		attachedLR.SetPosition(index, pos);
	}

	#endregion

	#region addVertex

	//Method used to add a new line renderer vertex and a new vertex sphere.
	public GameObject addVertex(Vector3 pos, int vertexID, GameObject selectedVertex){
		
		pos = rotateVertex(pos, -getLocalRotation());
		
		//Get the vertex spheres that are children of this object
		List<GameObject> children = getChildrenVertices();
		//Add the selected vertex to that list (as it won't be a child of the object if it's selected), and sort the list.
		if(selectedVertex){
			children.Add(selectedVertex);
		}
		
		children.Sort(sortByVertexID);
		

		//for loop to go through each vertex and update it's vertexID if it's PAST the selected vertex
		foreach(GameObject child in children){
			VertexManager childsVM = child.GetComponent<VertexManager>();
			if(childsVM.getVertexID()>=vertexID){
				childsVM.setVertexID(childsVM.getVertexID()+1);
			}
		}
		
		//create a new vertex sphere
		GameObject newVert = drawVert(pos, vertexID, false);
		VertexManager newVertsVM = newVert.GetComponent<VertexManager>();
		//make the new vertex's size and "length" field equal to the one currently selected
		if(selectedVertex){
			newVert.transform.localScale=selectedVertex.transform.lossyScale;
			newVertsVM.setVertexLength(selectedVertex.GetComponent<VertexManager>().getVertexLength());
		}
		//add this new vertex to the dictionary of vertexmanagers with its current timing
		timingDict[newVertsVM.getVertexTiming()].Add(newVertsVM);
		//add this new vertex to the list of children, and sort the list again.
		children.Add(newVert);
		children.Sort(sortByVertexID);
		//create a Vector3[] to hold the positions that the line renderer will be set to.
		Vector3[] finalPositions = new Vector3[children.Count];
		//translate the position of every child and add it to finalPositions.
		for (int i = 0; i < children.Count; i++){
			Vector3 tempPos = children[i].transform.position;
			tempPos = rotateVertex(tempPos, -getLocalRotation());
			//tempPos = vertexRotationTwo(tempPos, localRotation+270, i);
			finalPositions[i]=tempPos;	
		}
		
		//create a new vertex on the line renderer and set it's positions to finalPositions.
		attachedLR.positionCount+=1;
		attachedLR.SetPositions(finalPositions);
		
		boxColliderManager.addBoxCollider(vertexID);

		return newVert;
	}
	#endregion

	#region removeVertex

	//Method used to remove a vertex from a line and return it back to the pool
	public void removeVertex(Vector3 pos, int vertexID, GameObject selectedVertex){
		boxColliderManager.removeBoxCollider(vertexID);

		//if statement to detect whether the vertex being removed is the last in the line.
		if(vertexID==attachedLR.positionCount-1){
			//Lower the position count by 1 and return the object to the pool.
			attachedLR.positionCount-=1;
			timingDict[selectedVertex.GetComponent<VertexManager>().getVertexTiming()].Remove(selectedVertex.GetComponent<VertexManager>());
			objectPooler.returnToPool("Vertex", selectedVertex);
		}else{
			//removing a vertex from the middle of a list
			//get the children of the lineBase and sort them
			List<GameObject> children = getChildrenVertices();
			children.Sort(sortByVertexID);
			
			//go through each vertex sphere that is a child of lineBase and lower it's vertexID by 1 if it is past the vertex being removed.
			foreach(GameObject child in children){
				VertexManager childsVM = child.GetComponent<VertexManager>();
				if(childsVM.getVertexID()>=vertexID){
					childsVM.setVertexID(childsVM.getVertexID()-1);
				}
			}

			//Translate the vector3 pos to take into account the position of the baseline object.
			pos=rotateVertex(pos, -getLocalRotation());

			//create a list of final positions that the line renderer vertices will be set too
			Vector3[] finalPositions = new Vector3[children.Count];
			for (int i = 0; i < children.Count; i++){
				Vector3 tempPos = children[i].transform.position;
				tempPos = rotateVertex(tempPos, -getLocalRotation());
				finalPositions[i]=tempPos;	
			}

			//lower the number of vertices in the line renderer by 1, and set the remaining vertices to finalPositions
			attachedLR.positionCount-=1;
			timingDict[selectedVertex.GetComponent<VertexManager>().getVertexTiming()].Remove(selectedVertex.GetComponent<VertexManager>());
			attachedLR.SetPositions(finalPositions);
			//return the removed vertex sphere back to the pool
			objectPooler.returnToPool("Vertex", selectedVertex);


		}	
	}
	#endregion

	#region utilities

	//method used to take the voice id of this line manager, and cycle it by 1 along the list of voices.
	public void cycleVoices(){
		int voiceID = (int)voice;
		voiceID = (voiceID + 1)%Voices.GetNames(typeof(Voices)).Length;
		voice = (Voices) voiceID;
		setVoiceColour();
	}

	//simple sorter method to compare 2 vertices by their vertex id's
	static int sortByVertexID(GameObject v1, GameObject v2){
		return v1.GetComponent<VertexManager>().getVertexID().CompareTo(v2.GetComponent<VertexManager>().getVertexID());
	}

	//==MAY REMOVE THIS METHOD==:
	//simple method that translates a given position by the position of the attached gameobject
	private Vector3 VertexTranslation(Vector3 pos){
		Vector3 translation = attachedObject.transform.position;
		
		pos.x-=translation.x;
		pos.y-=translation.y;
		pos.z-=translation.z;

		return pos;
	}
	#endregion

	#region Pulse Methods

	//method used for tranlating a pulse specifically, different from vertex translator.
	private Vector3 PulseTranslation(Vector3 pos){
		Vector3 translation = attachedObject.transform.position;
		
		pos.x+=translation.x;
		pos.z+=translation.z;

		return pos;
	}

	//method called by pulse manager to calculate a pulse should be on this line renderer. (and for creating new lead on pulses)
	public Vector3 interpole(float height, bool playVertex){
		float lowerBoundTiming = float.MinValue;
		float upperBoundTiming = float.MaxValue;
		int lowerBoundIndex = 0;
		int upperBoundIndex = 0;
		int flooredHeight = Mathf.FloorToInt(height);

		//get all of the children, even ones attached to controllers.
		List<GameObject> childrenVertices = getAllChildrenVerticesForInterpole();
		childrenVertices.Sort(sortByVertexID);
		//first 1/3 of method used for deducing which 2 vertices the pulse should be between based on a given height.

		foreach (GameObject Vertex in childrenVertices){
			VertexManager currentVertexManager = Vertex.GetComponent<VertexManager>();
			if(currentVertexManager.getVertexTiming()>=lowerBoundTiming && currentVertexManager.getVertexTiming()<=height){
				lowerBoundTiming = currentVertexManager.getVertexTiming();
				lowerBoundIndex=currentVertexManager.getVertexID();
			}
			if(currentVertexManager.getVertexTiming()<upperBoundTiming && currentVertexManager.getVertexTiming()>height && currentVertexManager.getVertexTiming()!=upperBoundTiming){
				upperBoundTiming = currentVertexManager.getVertexTiming();
				upperBoundIndex = currentVertexManager.getVertexID();
			}
			
		} 
	
		if(lastHeight>flooredHeight && flooredHeight==0){
			if(playVertex){
				foreach(VertexManager vm in timingDict[0]){
					vm.playVertex();
				}
			}
		}
		else if(lastHeight<flooredHeight){
			
			float diffOfHeights = flooredHeight - lastHeight;
			if(playVertex && flooredHeight<GridManager.getYSegments()-1){
				foreach (VertexManager vm in timingDict[flooredHeight]){
					vm.playVertex();	
				}
			}
		}
		
		if(flooredHeight!=-1){
			lastHeight = flooredHeight;
		}
		
		float difference = upperBoundTiming - lowerBoundTiming;
		float segment = 1/difference;
		float leftover = height - lowerBoundTiming;
		int numberOfDivs = Mathf.FloorToInt(leftover);
		float t = (numberOfDivs * segment) + ((leftover%1) / difference);

		if(flooredHeight==-1){
			//lead on pulse:
			return Vector3.Lerp(PulseTranslation(attachedLR.GetPosition(0)),PulseTranslation(attachedLR.GetPosition(1)),1-(height*-1));
		}else{

			return Vector3.Lerp(PulseTranslation(attachedLR.GetPosition(lowerBoundIndex)),PulseTranslation(attachedLR.GetPosition(upperBoundIndex)), t);
			
		}
	}

	#endregion
	
}
