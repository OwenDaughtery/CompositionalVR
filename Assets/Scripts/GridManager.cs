using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

	#region variables
	private ObjectPooler objectPooler;	

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

	private List<GameObject> noteBoundaries = new List<GameObject>();
	private List<GameObject> fadingInNoteBoundaries = new List<GameObject>();
	private List<GameObject> fadingOutNoteBoundaries = new List<GameObject>();
	private Dictionary<GameObject, float> fadeOutDictionary = new Dictionary<GameObject, float>();
	//The vertexManager for any specific vertex (sphere) at any given point

	/*plan is to remove these soon, they are temp debuggin */
	private List<GameObject> timingBoundaries = new List<GameObject>();
	
	private VertexManager vertexManager;
	private float fadeSpeed = 0.25f;

	//32 = 12 and 25
	//24 = 12
	private static int ySegments = 64;

	private static Dictionary<float, int> YToTiming = new Dictionary<float,int>(); 
	private static Dictionary<int, float> TimingToY = new Dictionary<int, float>();

	//The list of possible notes to play, always includes none at start.
	public enum Notes {none, C4, Cs4, D4, Ds4, E4, F4, Fs4, G4, Gs4, A4, As4, B4, C5, Cs5, D5, Ds5, E5, F5, Fs5, G5, Gs5, A5, As5, B5, C6};

	public static Dictionary<Notes, float> noteToFreq = new Dictionary<Notes, float>{
		{Notes.C4, 261.626f},
		{Notes.Cs4, 277.183f},
		{Notes.D4, 293.665f},
		{Notes.Ds4, 311.127f},
		{Notes.E4, 329.628f},
		{Notes.F4, 349.228f},
		{Notes.Fs4, 369.994f},
		{Notes.G4, 391.995f},
		{Notes.Gs4, 415.305f},
		{Notes.A4, 440.000f},
		{Notes.As4, 466.164f},
		{Notes.B4, 493.883f},
		{Notes.C5, 523.251f},
		{Notes.Cs5, 554.365f},
		{Notes.D5, 587.330f},
		{Notes.Ds5, 622.254f},
		{Notes.E5, 659.255f},
		{Notes.F5, 698.456f},
		{Notes.Fs5, 739.989f},
		{Notes.G5, 783.991f},
		{Notes.Gs5, 830.609f},
		{Notes.A5, 880.000f},
		{Notes.As5, 932.328f},
		{Notes.B5, 987.767f},
		{Notes.C6, 1046.50f}
	};

	#endregion

	public static int getYSegments(){
		return ySegments;
	}

	void Start () {
		float yIncrement = 3.3f/getYSegments();
		for (int i = 0; i <= getYSegments(); i++){
			YToTiming.Add(3.3f-(i*yIncrement),i);
			TimingToY.Add(i, 3.3f-(i*yIncrement));
		}
		objectPooler = ObjectPooler.Instance;
		xVector = new Vector3(0f, 0f, 0f);
		yVector = new Vector3(0f, 0f, 0f);
		zVector = new Vector3(0f, 0f, 0f);
		createNoteBoundaries();
		createTimingBoundaries();
	}
	
	void Update () {
		//every frame, call the method that will update the variables of all the vertices in the environment.
		getVertexStats();
		//fadeInNoteBoundaries();
		//fadeOutNoteBoundaries();
		tempFadeOut();
	}

	public void tempFadeOut(){
		List<GameObject> keys = new List<GameObject> (fadeOutDictionary.Keys);
		foreach(GameObject key in keys) {
			//print(fadeOutDictionary[key]);
			fadeOutDictionary[key] = fadeOutDictionary[key]-0.01f;
			if(fadeOutDictionary[key]<=0){
				key.SetActive(false);
				fadeOutDictionary.Remove(key);
			}
		}
		
	}

	public static float getTimingFromY(float y){
		return YToTiming[y];
	}

	public static float getYFromTiming(int timing){
		return TimingToY[timing];
	} 

	public static int getClosestTiming(float y){
		int lastValue = -1;
		foreach(KeyValuePair<float, int> pair in YToTiming){
				
		if(y>pair.Key){
			return (int)lastValue;
		}	
		lastValue = pair.Value;
		} 
		
		return (int)lastValue;
	}

	private void createNoteBoundaries(){

		//float segmentIncrement = 90;
		float segmentIncrement = 360/(Notes.GetNames(typeof(Notes)).Length-2);
		float numberOfSegments = Mathf.CeilToInt(360/segmentIncrement);
		for (int i = 0; i < numberOfSegments; i++){
			GameObject noteBoundary = objectPooler.spawnFromPool("NoteBoundary", Vector3.zero, gameObject.transform);
			noteBoundaries.Add(noteBoundary);
			//noteBoundary.transform.position.Set(noteBoundary.transform.position.x+5,noteBoundary.transform.position.y,noteBoundary.transform.position.z);
			float eularAngleValue = i*segmentIncrement;
			if(eularAngleValue<0){
				eularAngleValue+=360;
			}
			noteBoundary.transform.eulerAngles = new Vector3(0f, eularAngleValue, 90f);
			noteBoundary.transform.position = rotateNoteBoundary(noteBoundary.transform.position, i*segmentIncrement);
//			Color currentColour = noteBoundary.GetComponent<Renderer>().material.color;
//			noteBoundary.GetComponent<Renderer>().material.color= new Color(currentColour.r, currentColour.g,currentColour.b,0.0f);
			noteBoundary.SetActive(false);
		}
		
	}

	private void createTimingBoundaries(){
		float segmentIncrement = 3.3f/getYSegments();
		float numberOfSegments = getYSegments();
		for(int i = 0; i < numberOfSegments; i++){
			GameObject timingBoundary = objectPooler.spawnFromPool("NoteBoundary", Vector3.zero, gameObject.transform);
			timingBoundaries.Add(timingBoundary);
			float yHeight = 3.3f-(i*segmentIncrement);
			timingBoundary.transform.position = new Vector3(0f,yHeight,0f);
			timingBoundary.SetActive(false);//should be true, just debugging.
		}
	}

	private Vector3 rotateNoteBoundary(Vector3 pos, float theta){
		pos.x= 2.5f * Mathf.Sin(theta* Mathf.Deg2Rad);
		pos.z=2.5f * Mathf.Cos(theta* Mathf.Deg2Rad);
		return pos;
	}

	private void fadeInNoteBoundaries(){
		List<GameObject> boundariesToRemove = new List<GameObject>();
		foreach (GameObject boundary in fadingInNoteBoundaries){
			Color currentColour = boundary.GetComponent<Renderer>().material.color;
			float updatedAlpha = currentColour.a+fadeSpeed;
			boundary.GetComponent<Renderer>().material.color = new Color(currentColour.r, currentColour.g, currentColour.b, updatedAlpha);
			if(updatedAlpha>=1){
				boundariesToRemove.Add(boundary);
			}
		}
		foreach (GameObject boundary in boundariesToRemove){
				fadingInNoteBoundaries.Remove(boundary);
		}
	}

	private void fadeOutNoteBoundaries(){
		List<GameObject> boundariesToRemove = new List<GameObject>();
		foreach (GameObject boundary in fadingOutNoteBoundaries){
			Color currentColour = boundary.GetComponent<Renderer>().material.color;
			float updatedAlpha = currentColour.a-fadeSpeed;
			boundary.GetComponent<Renderer>().material.color = new Color(currentColour.r, currentColour.g, currentColour.b, updatedAlpha);
			if(updatedAlpha<=0){
				boundariesToRemove.Add(boundary);
			}
		}
		foreach (GameObject boundary in boundariesToRemove){
				fadingOutNoteBoundaries.Remove(boundary);
		}
	}

	public void showNoteBoundaries(Notes lastNote, Notes newNote){
		int lastNoteID = (int)lastNote-1;
		int newNoteID = (int)newNote-1;
		int lengthOfNotes = Notes.GetNames(typeof(Notes)).Length-2;

		if((newNoteID==0 && lastNoteID==lengthOfNotes-1) || (lastNoteID==0 && newNoteID==lengthOfNotes-1)){

			noteBoundaries[6].SetActive(true);
			if(!fadeOutDictionary.ContainsKey(noteBoundaries[6])){
				fadeOutDictionary.Add(noteBoundaries[6], 1f);
			}
			
		}else if(lastNoteID<newNoteID){
			if(!fadeOutDictionary.ContainsKey(noteBoundaries[(newNoteID+6)%lengthOfNotes])){
				noteBoundaries[(newNoteID+6)%lengthOfNotes].SetActive(true);
				fadeOutDictionary.Add(noteBoundaries[(newNoteID+6)%lengthOfNotes], 1f);
			}
		}else if(lastNoteID>=newNoteID){
			if(!fadeOutDictionary.ContainsKey(noteBoundaries[(newNoteID+7)%lengthOfNotes])){
				noteBoundaries[(newNoteID+7)%lengthOfNotes].SetActive(true);
				fadeOutDictionary.Add(noteBoundaries[(newNoteID+7)%lengthOfNotes], 1f);
			}
		}
		

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
		
			if(vertexManager!=null && vertexManager.getVertexID()!=0 && vertexManager.getVertexID()!=vertexManager.getParentsLineManager().getNumberOfVertices()-1){
				//setting vertex volume by passing x^2 + z^2 to convert volume
				vertexManager.setVertexVolume(convertVolume((xDist*xDist) + (zDist*zDist)));

				//setting vertex timing by passing y distance.
				vertexManager.setVertexTiming(convertTiming(vertex.transform.position.y));

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

	//method used to "clamp" volume to range 0-ySegments
	private float convertTiming(float yDist){
		//0.1623
		int test;
		if(YToTiming.TryGetValue(yDist, out test)){
			test = YToTiming[yDist];
		}else{
			return getClosestTiming(yDist);
		}
		//float test = Mathf.Floor((getYSegments()-((yDist - 0.1623f) / (3.3f - 0.1623f) * (getYSegments() - 0) + 0)));
		//print("converted timing: " + test);
		return test;
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
