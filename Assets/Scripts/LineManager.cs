using System.Collections;
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
	public enum Voices {A, B, C};
	private Voices voice;

	//the vertices of the line renderer.
	private Vector3[] vertices;
	//boolean as to whether the baseline is currently tethered to the tether point yet.
	private bool isTethered = false;
	//the object pool to get vertices from.
	private ObjectPooler objectPooler;

	public Dictionary<float, List<VertexManager>> timingDict;

	//int variable to hold the index of the last vertex that is played.
	[SerializeField]
	private int lastPlayedVertex;
	private int lastHeight;
	private float localRotation;


	//possible colours where the letter corressponds to the voice enum.
	Color colourA = new Color(0.89f, 0.12f, 0.05f, 1);
	Color colourB = new Color(0.05f, 0.09f, 0.89f, 1);
	Color colourC = new Color(0.05f, 0.89f, 0.47f, 1);


	#endregion

	void Start () {
		localRotation = gameObject.transform.eulerAngles.y;
		timingDict = new Dictionary<float, List<VertexManager>>();
		for (int i = 0; i <= 16; i++){
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
	}

	void Update(){
		setActiveLineManager();
	}

	public void updateTimingDict(float oldTiming, float newTiming, VertexManager vm){
		timingDict[oldTiming].Remove(vm);
		timingDict[newTiming].Add(vm);
	}

	#region tethering

	//method used to check if baseline is tethered. if it is and wasn't before, active this line manager.
	//if it isn't tethered and was before checking, deactive this line manager. 
	private void setActiveLineManager(){
		List<GameObject> childrenVertices = getChildrenVertices();
		foreach (GameObject vertex in childrenVertices){
			if(checkIfTethered(vertex)){
				if(isTethered){
					return;
				}else{
					isTethered=true;
					Vector3 posToReduce = vertex.transform.position;
					posToReduce.Set(posToReduce.x, posToReduce.y - 0.05f, posToReduce.z);
					vertex.GetComponent<VertexManager>().moveTo(posToReduce);
					pulseManager.activateLineManager(this);
					return;
				}
			}
		}
		if(isTethered){
			isTethered = false;
			pulseManager.deactivateLineManager(this);
		}else{
			isTethered = false;
		}
	}

	public bool checkIfTethered(GameObject vertex){
		return checkIfTetheredForY(vertex) && checkIfTetheredForXZ(vertex);
	}

	public bool checkIfTetheredForY(GameObject vertex){
		return vertex.GetComponent<VertexManager>().getVertexTiming()==16;
	}

	public bool checkIfTetheredForXZ(GameObject vertex){
		return vertex.GetComponent<VertexManager>().getVertexVolume()>0.955f;
	}

	//simple getter method of variable isTethered.

	#endregion

	#region voice and colour

	//method called once to choose a random voice, and set the attached line renderer to the corresponding colour.
	private void chooseVoice(){
		voice = (Voices)Random.Range(0, (Voices.GetNames(typeof(Voices)).Length));
		setVoiceColour();
	}

	//method used to check the current assigned voice and set the attached line renderer to the corresponding colour.
	private void setVoiceColour(){
		if(voice==Voices.A){
			attachedLR.SetColors(colourA, new Color(0.89f, colourA.g+0.41f, 0.05f, 1));
		}else if(voice==Voices.B){
			attachedLR.SetColors(colourB, new Color(colourB.g+0.18f, colourB.r, 0.89f, 1));
		}if(voice==Voices.C){
			attachedLR.SetColors(colourC,new Color(0.05f, 0.89f, colourC.b-0.28f, 1));
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
			VertexManager newVert = drawVert(vert, vertexID).GetComponent<VertexManager>();
			vertexID++;
		}	
	}

	//Method used to create a new vertex (which is a sphere) by grabbing one from pool
	private GameObject drawVert(Vector3 vert, int vertexID){
		//Get the object from pool, which will spawn it in, and set it's details to the parameters
		GameObject sphere = objectPooler.spawnFromPool("Vertex", vert, attachedLR.transform);
		sphere.transform.name = "Vertex";
		sphere.AddComponent<VertexManager>();
		sphere.AddComponent<Rigidbody>();
		sphere.GetComponent<Rigidbody>().useGravity = false;
		sphere.GetComponent<Rigidbody>().isKinematic= true;
		sphere.GetComponent<VertexManager>().setVertexID(vertexID);
		sphere.GetComponent<VertexManager>().setParentsLineManager(this);
		
		return sphere;
	}

	//Method used to move the vertex of a linerenderer to a position
	public void moveLineVertex(int index, Vector3 pos, float eugerValueRotation){
		//pos=VertexTranslation(pos);
		
		// ========temp comment ====== pos = vertexRotation(pos, eugerValueRotation+270, index);
		attachedLR.SetPosition(index, pos);
	}

	private Vector3 vertexRotation(Vector3 pos, float theta, int index){
		float distance = Vector2.Distance(Vector2.zero, new Vector2(pos.x, pos.z));

		List<GameObject> children = getAllChildrenVerticesForInterpole();
		float angle = 0;
		foreach (GameObject child in children){
			if(child.GetComponent<VertexManager>().getVertexID()==index){
				angle = child.GetComponent<VertexManager>().getVertexAngle();
			}
		}
		float angleDifference = (angle - (theta)) * Mathf.Deg2Rad;
		//print(angleDifference);
		pos.x= distance * Mathf.Sin(angleDifference);
		pos.z=distance * Mathf.Cos(angleDifference);
		//270+theta

		return pos;
	}

	private Vector3 vertexRotationTwo(Vector3 pos, float theta, int index){
		float distance = Vector2.Distance(Vector2.zero, new Vector2(pos.x, pos.z));

		List<GameObject> children = getAllChildrenVerticesForInterpole();
		float angle = 0;
		foreach (GameObject child in children){
			if(child.GetComponent<VertexManager>().getVertexID()==index){
				angle = child.GetComponent<VertexManager>().getVertexAngle();
			}
		}
		float angleDifference = (angle - theta) * Mathf.Deg2Rad;
		print(angle);
		//print(angleDifference);
		pos.x= distance * Mathf.Sin(angleDifference);
		pos.z=distance * Mathf.Cos(angleDifference);
		//270+theta

		return pos;
	}

	#endregion

	#region addVertex

	//Method used to add a new line renderer vertex and a new vertex sphere.
	public GameObject addVertex(Vector3 pos, int vertexID, GameObject selectedVertex){
		//translate the position to take into account movement of parent object
		
		//pos=vertexRotation(pos, localRotation+270, vertexID);

		//===adding a vertex to the middle of a line:===
		//Get the vertex spheres that are children of this object
		List<GameObject> children = getChildrenVertices();
		//Add the selected vertex to that list (as it won't be a child of the object if it's selected), and sort the list.
		children.Add(selectedVertex);
		children.Sort(sortByVertexID);
		
		//for loop to go through each vertex and update it's vertexID if it's PAST the selected vertex
		foreach(GameObject child in children){
			VertexManager childsVM = child.GetComponent<VertexManager>();
			if(childsVM.getVertexID()>=vertexID){
				childsVM.setVertexID(childsVM.getVertexID()+1);
			}
		}
		
		//create a new vertex sphere
		GameObject newVert = drawVert(pos, vertexID);
		VertexManager newVertsVM = newVert.GetComponent<VertexManager>();
		//make the new vertex's size and "length" field equal to the one currently selected
		newVert.transform.localScale=selectedVertex.transform.lossyScale;
		newVertsVM.setVertexLength(selectedVertex.GetComponent<VertexManager>().getVertexLength());
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
			print(localRotation);
			//tempPos = vertexRotationTwo(tempPos, localRotation+270, i);
			finalPositions[i]=tempPos;	
		}
		
		//create a new vertex on the line renderer and set it's positions to finalPositions.
		attachedLR.positionCount+=1;
		attachedLR.SetPositions(finalPositions);
		
		return newVert;
		
	}
	#endregion

	#region removeVertex

	//Method used to remove a vertex from a line and return it back to the pool
	public void removeVertex(Vector3 pos, int vertexID, GameObject selectedVertex){
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
			pos=VertexTranslation(pos);

			//create a list of final positions that the line renderer vertices will be set too
			Vector3[] finalPositions = new Vector3[children.Count];
			for (int i = 0; i < children.Count; i++){
				Vector3 tempPos = children[i].transform.position;
				tempPos = VertexTranslation(tempPos);
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

	//simple sorter method to compare 2 vertices by their vertex id's
	static int sortByVertexID(GameObject v1, GameObject v2){
		return v1.GetComponent<VertexManager>().getVertexID().CompareTo(v2.GetComponent<VertexManager>().getVertexID());
	}

	//simple method that translates a given position by the position of the attached gameobject
	private Vector3 VertexTranslation(Vector3 pos){
		Vector3 translation = attachedObject.transform.position;
		
		pos.x-=translation.x;
		pos.y-=translation.y;
		pos.z-=translation.z;

		return pos;
	}
	#endregion

	#region playing vertices
	
	//method used to play the vertices between indexA and indexB.
	/*
	private void playVertices(int indexA, int indexB){
		//first half of method is to deduce which vertices to play
		List<GameObject> childrenVertices = getChildrenVertices();
		List<VertexManager> verticesToPlay = new List<VertexManager>();
		foreach (GameObject child in childrenVertices){
			VertexManager childVM = child.GetComponent<VertexManager>();
			int vertexID = childVM.getVertexID();
			if(vertexID > indexA && vertexID <= indexB && vertexID!=0){
				verticesToPlay.Add(childVM);
			}
		}

		//once vertices to play are found, light up each one, and play it.
		foreach (VertexManager child in verticesToPlay){
			child.lightUpVertex(getColourOfVoice());
			GridManager.Notes note;
			float timing;
			float volume;
			child.formatVertex(out note, out timing, out volume);
			contactSC(note,volume);

			print(note.ToString() + " " + volume.ToString());
		}
	}

	private void contactSC(GridManager.Notes note, float volume){
			//OSC Send
			print("OSC sending");

			List<string> args = new List<string>();
			args.Add(volume.ToString());
			args.Add(((int)note-1).ToString());
			print(((int)note-1).ToString());
			OSCHandler.Instance.SendMessageToClient("SuperCollider", "/play", args);
	} */
	#endregion

	#region Pulse Methods

	//method used for tranlating a pulse specifically, different from vertex translator.
	private Vector3 PulseTranslation(Vector3 pos){
		Vector3 translation = attachedObject.transform.position;
		
		pos.x+=translation.x;
		pos.z+=translation.z;

		return pos;
	}

	//method called by pulse manager to calculate a pulse should be on this line renderer.
	public Vector3 interpole(float height, bool playVertex){
		

		//get all of the children, even ones attached to controllers.
		List<GameObject> childrenVertices = getAllChildrenVerticesForInterpole();
		childrenVertices.Sort(sortByVertexID);
		//first 1/3 of method used for deducing which 2 vertices the pulse should be between based on a given height.
		float lowerBoundTiming = float.MinValue;
		float upperBoundTiming = float.MaxValue;
		int lowerBoundIndex = 0;
		int upperBoundIndex = 0;
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

		int flooredHeight = Mathf.FloorToInt(height);

		if(lastHeight>flooredHeight){
			if(playVertex){
				foreach (VertexManager vm in timingDict[16]){
					if(vm.getVertexID()!=attachedLR.positionCount-1){
						vm.playVertex();
					}
				}
			}
		}else if(lastHeight<flooredHeight){

			float diffOfHeights = flooredHeight - lastHeight;
			if(playVertex){
				foreach (VertexManager vm in timingDict[flooredHeight]){
					vm.playVertex();	
				}
			}
		}
		lastHeight = flooredHeight;
		/*
		//second 1/3 of method used for deciding which vertices should be played, if at all based off a passed boolean variable.
		if(lowerBoundIndex>lastPlayedVertex){
			if(playVertex){
				playVertices(lastPlayedVertex, lowerBoundIndex);
			}
			
			lastPlayedVertex = lowerBoundIndex;
		}else if(lowerBoundIndex==1 && lastPlayedVertex>lowerBoundIndex){
			if(playVertex){
				playVertices(lastPlayedVertex, attachedLR.positionCount-2);
			}
			
			lastPlayedVertex = -1;
		} */


		//third 1/3 of method used for calculating which part of the line the pulse should be at.
		float difference = upperBoundTiming - lowerBoundTiming;
		float segment = 1/difference;
		float leftover = height - lowerBoundTiming;
		int numberOfDivs = Mathf.FloorToInt(leftover);
		float t = (numberOfDivs * segment) + ((leftover%1) / difference);

		return Vector3.Lerp(PulseTranslation(attachedLR.GetPosition(lowerBoundIndex)),PulseTranslation(attachedLR.GetPosition(upperBoundIndex)), t);
	}

	#endregion
	
}
