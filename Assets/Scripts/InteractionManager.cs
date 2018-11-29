using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour {

	#region variables
	//the fixed joint attached to the controller
	private FixedJoint fixedJoint = null;
	//the current rigid body attahced to the controller
	private Rigidbody currentRigidBody = null;
	//a list of rigid bodies that can be contacted
	private List<Rigidbody> contactRigidBodies = new List<Rigidbody>();
	//the current vertex manager of the vertex being handled (if there is one)
	private VertexManager currentVertexManager = null;
	//the current vertex being handled (if there is one)
	private GameObject currentGameObject = null;
	private GameObject controller = null;
	[SerializeField]
	private List<GameObject> hoveringBoxColliders = new List<GameObject>();
	[SerializeField]
	private AISystemManager AI;
	private GameObject tester = null;
	#endregion

	void Awake(){
		controller = gameObject;
		fixedJoint = GetComponent<FixedJoint>();
	}

	void Update(){
		//if currently holding an object, check its not out of bounds.
		if(currentGameObject){
			outOfBoundsCheck();
		}

		checkHoveringBoxColliders();
		highlightBoxCollider();
	}

	#region simple getters and setters

	//return current game object variable (the game object being held)
	public GameObject getCurrentGameObject(){
		return currentGameObject;
	}

	#endregion

	#region Box Collider Stuff

	//NOT FINISHED, but intended to make a border around the nearest line renderer
	void highlightBoxCollider(){
		GameObject bc = getClosestBoxCollider();
		if(bc){
			
		}

	}

	//add the passed box collider to the list of box colliders being hovered over.
	public void setHoveringBoxColliders(GameObject newBoxCollider){
		if(!hoveringBoxColliders.Contains(newBoxCollider)){
			hoveringBoxColliders.Add(newBoxCollider);
		}
	}	

	//check to see if each of the box colliders in the list are too far away from the controller
	private void checkHoveringBoxColliders(){
		List<GameObject> tempBoxColliderList = new List<GameObject>(hoveringBoxColliders);
		foreach (GameObject bc in tempBoxColliderList){
			float distance = Vector3.Distance(bc.GetComponent<BoxCollider>().ClosestPoint(gameObject.transform.position), gameObject.transform.position);
			if(distance>0.075){
				hoveringBoxColliders.Remove(bc);
			}
		}
	}	

	//get the closest box collider that the controller has hovered over.
	private GameObject getClosestBoxCollider(){
		float closestPoint = float.MaxValue;
		GameObject closestBoxCollider = null;
		foreach (GameObject bc in hoveringBoxColliders){
			if(bc.GetComponent<Trigger>().getStartID()!=0){
				Vector3 newPoint = bc.GetComponent<BoxCollider>().ClosestPoint(gameObject.transform.position);
				if(Vector3.Distance(newPoint, gameObject.transform.position)<closestPoint){
					closestBoxCollider = bc;
				}
			}
		}
		return closestBoxCollider;
	}

	#endregion	



	#region pick up and put down methods

	//when trigger is pulled to enter, add the passed collder to contactRigidBodies if it is a vertex
	private void OnTriggerEnter(Collider collider){
		if(!collider.gameObject.CompareTag("Vertex")){
			return;
		}else{
			contactRigidBodies.Add(collider.gameObject.GetComponent<Rigidbody>());
		}
	}

	//when trigger is pulled to exit, remove the passed collder from contactRigidBodies if it is a vertex
	private void OnTriggerExit(Collider collider){
		if(!collider.gameObject.CompareTag("Vertex")){
			return;
		}else{
			contactRigidBodies.Remove(collider.gameObject.GetComponent<Rigidbody>());
		}
	}

	//method that is called when the controller should pick up the nearest rigid body
	public void pickUp(){
		currentRigidBody = GetNearestRigidBody();
		if(!currentRigidBody){
			return;
		}else{
			//if statement checking the id of the vertex:
			if(currentRigidBody.GetComponent<VertexManager>().getBaseLineParent().GetComponent<LineManager>().getNumberOfVertices()-1==currentRigidBody.GetComponent<VertexManager>().getVertexID()){
				//if the id is the last in the line, cycle the voice of the line
				currentRigidBody.GetComponent<VertexManager>().getParentsLineManager().cycleVoices();
				resetVariables();
			}else{
				//set current-XYZ variables
				currentGameObject = currentRigidBody.gameObject;
				currentVertexManager = currentRigidBody.GetComponent<VertexManager>();
				currentVertexManager.setIsSelected(true);
				

				Vector3 oldPos = currentGameObject.transform.position;

				//set the rigidBody parent and position to the controller holding it.
				currentRigidBody.transform.parent=gameObject.transform;
				currentRigidBody.transform.position = transform.position;
				fixedJoint.connectedBody = currentRigidBody;
				currentVertexManager.onPickUp();

				//if statement to check whether the id of the vertex is editable, if it isn't, act as if a new vertex should be made:
				if(!isEditable(currentVertexManager.getVertexID(), currentVertexManager.getBaseLineParent().gameObject)){
					VertexManager newlyCreatedVertexManager = addNewVertex().GetComponent<VertexManager>();
					newlyCreatedVertexManager.moveTo(oldPos);
				}
			}
		}
	}

	//function for letting go of a vertex
	public void letGo(){
		if(!currentRigidBody){
			return;
		}else{
			if(!currentGameObject.active){
				//don't bother letting go if currentRigidBody is already been deactivated (aka removed)
				return;
			}else{
				currentGameObject.transform.parent = currentVertexManager.getBaseLineParent();
				currentVertexManager.setIsSelected(false);

				Vector3 yToSnap = snap();
				currentVertexManager.moveTo(yToSnap);
				currentVertexManager.onPutDown();
				resetVariables();
			}
		}
	}

	#endregion

	#region clamping and snapping

	//method that uses Yclamper to get the y axis and timing that the current vertex should be snapped to, and snaps it there.
	public Vector3 snap(){
		float clampedY;
		float clampedTiming;
		yClamper(out clampedY, out clampedTiming);
		Vector3 currentPos = new Vector3(currentGameObject.transform.position.x, clampedY, currentGameObject.transform.position.z);
		return currentPos;
	}

	//compares the current vertices timing and y cordinate to the vertices before and after it to see if its "out of bounds"
	private void yClamper(out float clampedY, out float clampedTiming){
		
		float siblingY;
		float siblingTiming;

		//get the vertex physically higher than the one currently being examined
		currentVertexManager.getHigherVertex(out siblingY, out siblingTiming);

		int controllersTiming = (int)currentVertexManager.getVertexTiming();
		//if statement to check whether the parent of the vertex is the controller:
		if(currentVertexManager.gameObject.transform.parent.transform==gameObject.transform){
			controllersTiming = GridManager.getClosestTiming(currentVertexManager.gameObject.transform.parent.transform.position.y);
		}
		
		clampedTiming = Mathf.Max(currentVertexManager.getVertexTiming(), siblingTiming);
		clampedY = GridManager.getYFromTiming((int)clampedTiming);//====
		
		currentVertexManager.getLowerVertex(out siblingY, out siblingTiming);

		clampedTiming = Mathf.Min(clampedTiming, siblingTiming);
		clampedY = GridManager.getYFromTiming((int)clampedTiming);
		
		if(clampedTiming>=GridManager.getYSegments()-1){
			Debug.LogWarning("Vertex is going below valid timings");
			clampedTiming = GridManager.getYSegments()-1;
			clampedY = GridManager.getYFromTiming((int)clampedTiming);
		}
	}	

	#endregion

	//remove this method???
	private Vector3 tetherToPoint(VertexManager vm, Vector3 currentPos){
		float volume = vm.getVertexVolume();
		if(!vm.getParentsLineManager().checkIfTetheredForXZ(currentGameObject)){
			return currentPos;
		}else{
			float m = (currentPos.z/currentPos.x);
			float newX = (0.815f/(Mathf.Sqrt(Mathf.Pow(m,2)+1)));
			float newZ = newX*m;
			currentPos.Set(newX, currentPos.y, newZ);
			return currentPos;
			//return currentPos.normalized*0.815f;
		}
	}



	#region adding, removing and changing the size of vertices

	//method called when user creates a new vertex while holding another
	public GameObject addNewVertex(){
		//if statement to check whether a vertex is currently held
		if(!currentRigidBody){
			//if no held vertex, try to create a new one on a line renderer.
			GameObject closestBoxCollider = getClosestBoxCollider();
			if(closestBoxCollider){
				int index = closestBoxCollider.GetComponent<Trigger>().getEndID();
				closestBoxCollider.transform.parent.GetComponent<LineManager>().addVertex(gameObject.transform.position, index, null);
				resetVariables();
			}
			return null;
		}else{
			float clampedY;
			float clampedTiming;
			yClamper(out clampedY, out clampedTiming);
			Vector3 posToSnap = new Vector3(currentGameObject.transform.position.x, clampedY, currentGameObject.transform.position.z);
			Vector3 snappedY = snap();
			return currentVertexManager.getParentsLineManager().addVertex(snappedY, currentVertexManager.getVertexID(), currentGameObject);
		}
	}

	//method to see whether a vertex is currently close enough to the controller to be considered "hovering over"
	public GameObject hoverOverVertex(){
		if(GetNearestRigidBody()){
			GameObject nearestVertex = GetNearestRigidBody().gameObject;
			Vector3 nearestVertexVector = nearestVertex.transform.position;
			if(Vector3.Distance(transform.position, nearestVertexVector)<0.1f){
				//is within range
				VertexManager nearestVertexManager = nearestVertex.GetComponent<VertexManager>();
				if(isEditable(nearestVertexManager.getVertexID(), nearestVertexManager.getBaseLineParent().gameObject)){
					//is "editable"
					return nearestVertex;
				}
				
			}
		}
		return null;
	}

	//simple method for changing a hovered over vertices timing and y cordindate
	public void moveVertexUp(){
		GameObject nearestVertex = hoverOverVertex();
		if(nearestVertex){
			VertexManager nearestVertexManager = nearestVertex.GetComponent<VertexManager>();
			float nearestVertexTiming = nearestVertexManager.getVertexTiming();

			float higherSiblingY;
			float higherSiblingTiming;
			nearestVertexManager.getHigherVertex(out higherSiblingY, out higherSiblingTiming);
			
			if(nearestVertexTiming!=0 && nearestVertexTiming>higherSiblingTiming){
				Vector3 currentPos = nearestVertex.transform.position;
				Vector3 newPos = new Vector3(currentPos.x, GridManager.getYFromTiming((int)nearestVertexTiming-1), currentPos.z);
				nearestVertexManager.moveTo(newPos);
			}
		}
	}

	//simple method for changing a hovered over vertices timing and y cordindate
	public void moveVertexDown(){
		GameObject nearestVertex = hoverOverVertex();
		if(nearestVertex){
			VertexManager nearestVertexManager = nearestVertex.GetComponent<VertexManager>();
			float nearestVertexTiming = nearestVertexManager.getVertexTiming();

			float lowerSiblingY;
			float lowerSiblingTiming;
			nearestVertexManager.getLowerVertex(out lowerSiblingY, out lowerSiblingTiming);

			if(nearestVertexTiming<=GridManager.getYSegments()-1 && nearestVertexTiming<lowerSiblingTiming){
				
				Vector3 currentPos = nearestVertex.transform.position;
				Vector3 newPos = new Vector3(currentPos.x, GridManager.getYFromTiming((int)nearestVertexTiming+1), currentPos.z);
				nearestVertexManager.moveTo(newPos);
			}
		}
	}

	//method called when user wants to remove the vertex currently being held from the game.
	public void removeVertex(){
		//if statement to check whether a vertex is currently being held:
		if(!currentRigidBody){
			//if no held vertex, try to remove one that may be being hovered over.
			GameObject nearestVertex = hoverOverVertex();
			if(nearestVertex){
				nearestVertex.transform.parent=gameObject.transform;
				VertexManager nearestVertexManager = nearestVertex.GetComponent<VertexManager>();
 				nearestVertexManager.getParentsLineManager().removeVertex(nearestVertex.transform.position, nearestVertexManager.getVertexID(), nearestVertex);
 				resetVariables();
			}
		}else if(isEditable(currentVertexManager.getVertexID(), currentVertexManager.getBaseLineParent().gameObject)){
			//if statement won't be entered if the id of the vertex being held is 1 (because that vertex is needed)
			currentVertexManager.getParentsLineManager().removeVertex(transform.position, currentVertexManager.getVertexID(), currentGameObject);
			resetVariables();
		}else{
			return;
		}
	}

	//method to check whether the vertex is considered "editable"
	private bool isEditable(int vertexID, GameObject baseLine){
		if(vertexID>1 && baseLine.GetComponent<LineManager>().getNumberOfVertices()-1!=vertexID){
			return true;
		}else{
			return false;
		}
	}

	//method used to pass control to the vertex manager
	public void decreaseVertexSize(){
		if(!currentRigidBody){
			return;
		}else{
			currentVertexManager.decreaseSize();
		}
	}

	//method used to pass control to the vertex manager
	public void increaseVertexSize(){
		if(!currentRigidBody){
			return;
		}else{
			currentVertexManager.increaseSize();
		}
	}

	#endregion

	#region utilities

	//method used to return the nearest rigid body to the called of method
	private Rigidbody GetNearestRigidBody(){
		Rigidbody neartestRigidBody = null;
		float minDistance = float.MaxValue;
		float distance = 0.0f;

		foreach (Rigidbody contactBody in contactRigidBodies){
			VertexManager vm = contactBody.gameObject.GetComponent<VertexManager>();
			//don't bother checking the position of contactBody if its ID is 0 as that vertex cannot be selected
			if(contactBody.gameObject.active && contactBody.gameObject.GetComponent<VertexManager>().getVertexID()>=1){
				distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

				if(distance<minDistance){
					minDistance=distance;
					neartestRigidBody = contactBody;
				}
			}	
		}
		return neartestRigidBody;
	}

	//method used when letting go of an object (either voluntarily or forced)
	private void resetVariables(){
		fixedJoint.connectedBody = null;
		currentRigidBody = null;
		currentVertexManager = null;
		currentGameObject = null;
		hoveringBoxColliders = new List<GameObject>();
	}

	//method used in out of bounds checking
	private float checkSiblingHeights(){
		float siblingY;
		float siblingTiming;
		currentVertexManager.getHigherVertex(out siblingY, out siblingTiming);
		
		if(transform.position.y>siblingY){
			return siblingY;
		}

		currentVertexManager.getLowerVertex(out siblingY, out siblingTiming);
		if(transform.position.y<siblingY){
			return siblingY;
		}
		
		return transform.position.y;
	}

	//method used to check if the current held vertex is going too low, too far from center, or too close to center.
	private void outOfBoundsCheck(){
		bool outOfBounds = false;
		int r =4;
		Vector3 pos = transform.position;
		float currentX = pos.x;
		float currentY = pos.y;
		float currentZ = pos.z;
		VertexManager tempVertexManager = currentVertexManager;

		
		if(currentY<=0.1){
			//pos.Set(pos.x, 0.15f, pos.z);
			//letGo();
			//currentVertexManager.moveTo(pos);
			outOfBounds = true;
		}else if(Mathf.Sqrt(Mathf.Pow(currentX,2) + Mathf.Pow(currentZ,2))>r){
			float m = (currentZ/currentX);
			float newX = ((r-0.01f)/(Mathf.Sqrt(Mathf.Pow(m,2)+1)));
			float newZ = newX*m;
			pos.Set(newX, currentY, newZ);
			//letGo();
			tempVertexManager.moveTo(pos);
			outOfBounds = true;
			//return;
		}else if(checkSiblingHeights()!=transform.position.y){
			pos.y=checkSiblingHeights();
			tempVertexManager.moveTo(pos);
			outOfBounds = true;
		}
		if(!outOfBounds){
			Vector3 tempPos= transform.position;
			tempVertexManager.moveTo(tempPos);
		}

	}

	#endregion

}
