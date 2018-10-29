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
	}

	#region getters and setters

	public GameObject getCurrentGameObject(){
		return currentGameObject;
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
			//set currentVertexManager
			currentGameObject = currentRigidBody.gameObject;
			currentVertexManager = currentRigidBody.GetComponent<VertexManager>();
			currentVertexManager.setIsSelected(true);
			

			Vector3 oldPos = currentGameObject.transform.position;

			//set the rigidBody parent and position to the controller holding it.
			currentRigidBody.transform.parent=gameObject.transform;
			currentRigidBody.transform.position = transform.position;
			fixedJoint.connectedBody = currentRigidBody;
			currentVertexManager.onPickUp();

		
			if(currentVertexManager.getVertexID()==1){
				VertexManager newlyCreatedVertexManager = addNewVertex().GetComponent<VertexManager>();
				newlyCreatedVertexManager.moveTo(oldPos);
			}


		

		}
	}

	public void letGo(){
		if(!currentRigidBody){
			return;
		}else{
			//don't bother letting go if currentRigidBody is already been deactivated (aka removed)
			if(!currentGameObject.active){
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
		//Vector3 currentPos = new Vector3(currentGameObject.transform.position.x, currentGameObject.transform.position.y, currentGameObject.transform.position.z);
		//Vector3 currentPos = new Vector3(controller.GetComponent<SphereCollider>().transform.position.x, clampedY, controller.GetComponent<SphereCollider>().transform.position.z);

		float maxY = 3.3f;
		float segment = 3.3f/16;
		float upper = segment*clampedTiming;
		float lower = segment *(clampedTiming-1);
		
		if(upper-currentPos.y>currentPos.y-lower){
			currentPos.Set(currentPos.x, (maxY-lower), currentPos.z);
		}else{
			currentPos.Set(currentPos.x, (maxY-upper), currentPos.z);
		}

		currentPos = tetherToPoint(currentVertexManager, currentPos);
		
		return currentPos;
	}

	//compares the current vertices timing and y cordinate to the vertices before and after it to see if its "out of bounds"
	private void yClamper(out float clampedY, out float clampedTiming){
		float siblingY;
		float siblingTiming;
		currentVertexManager.getHigherVertex(out siblingY, out siblingTiming);
		
		clampedTiming = Mathf.Max(currentVertexManager.getVertexTiming(), siblingTiming);
		clampedY = Mathf.Min(currentGameObject.transform.position.y, siblingY);
		//print("clamped higher timing = " + clampedTiming);

		currentVertexManager.getLowerVertex(out siblingY, out siblingTiming);
		clampedTiming = Mathf.Min(clampedTiming, siblingTiming);
		clampedY = Mathf.Max(clampedY, siblingY);
		//print("clamped lower timing = " + clampedTiming);
	}	

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
		}
	}

	#endregion

	#region adding, removing and changing the size of vertices

	//method called when user creates a new vertex while holding another
	public GameObject addNewVertex(){
		if(!currentRigidBody){
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

	//method called when user wants to remove the vertex currently being held from the game.
	public void removeVertex(){
		if(!currentRigidBody){
			return;
		}else if(currentVertexManager.getVertexID()!=1){
			//if statement won't be entered if the id of the vertex being held is 1 (because that vertex is needed)
			currentVertexManager.getParentsLineManager().removeVertex(transform.position, currentVertexManager.getVertexID(), currentGameObject);
			resetVariables();
		}else{
			return;
		}
	}

	public void decreaseVertexSize(){
		if(!currentRigidBody){
			return;
		}else{
			currentVertexManager.decreaseSize();
		}
	}

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
	}

	//method used to check if the current held vertex is going too low, too far from center, or too close to center.
	private void outOfBoundsCheck(){
		int r =4;
		Vector3 pos = currentGameObject.transform.position;
		float currentX = pos.x;
		float currentY = pos.y;
		float currentZ = pos.z;
		VertexManager tempVertexManager = currentVertexManager;

		
		if(currentY<=0.1){
			//pos.Set(pos.x, 0.15f, pos.z);
			//letGo();
			//currentVertexManager.moveTo(pos);
		}else if(Mathf.Sqrt(Mathf.Pow(currentX,2) + Mathf.Pow(currentZ,2))>r){
			float m = (currentZ/currentX);
			float newX = (r/(Mathf.Sqrt(Mathf.Pow(m,2)+1)));
			float newZ = newX*m;
			pos.Set(newX, currentY, newZ);
			letGo();
			tempVertexManager.moveTo(pos);
		}/*else if(currentVertexManager.getHigherVertex()<currentY){
			pos.Set(currentX, currentVertexManager.getHigherVertex(), currentZ);
			letGo();
			tempVertexManager.moveTo(pos);
			
		}else if(currentVertexManager.getLowerVertex()>currentY){
			pos.Set(currentX, currentVertexManager.getLowerVertex(), currentZ);
			letGo();
			tempVertexManager.moveTo(pos);
		}*/	
	}

	#endregion

}
