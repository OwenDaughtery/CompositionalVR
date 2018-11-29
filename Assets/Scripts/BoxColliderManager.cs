using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderManager : MonoBehaviour {

	#region variables
	//the line manager of the baseline that this box collider is attached too
	LineManager lineManager;
	[SerializeField]
	//the line renderer that this box collider associates too
	LineRenderer attachedLR;
	private ObjectPooler objectPooler;
	[SerializeField]
	//a list of box colliders for the attached baseline.
	List<GameObject> lineColliders = new List<GameObject>();
	#endregion

	void Start () {
		lineManager = this.GetComponent<LineManager>();
		objectPooler = ObjectPooler.Instance;
		addBoxColliders();
	}
	
	void Update () {
		updateBoxColliders();
	}

	//==MAY REMOVE THIS METHOD==
	public Vector3 rotateCollider(Vector3 pos, float rotationFloat){
		Quaternion rotation = Quaternion.Euler(0,rotationFloat,0);
		Vector3 myVector = pos;
		Vector3 rotateVector = rotation * myVector;
		return rotateVector;
	}

	#region collider methods
	//simple method used to call box collider for every line in the attached line renderer.
	private void addBoxColliders(){
		for (int i = 0; i < attachedLR.positionCount-1; i++){
			addBoxCollider(i);
		}
	}

	//method that creates a box collider, sets its variables, and sets it dimensions appropriately, and change the indexes of all other colliders appropriately.
	public void addBoxCollider(int index){
		Vector3 startPoint = attachedLR.GetPosition(index);
		Vector3 endPoint = attachedLR.GetPosition(index+1);
		GameObject newBoxCollider = objectPooler.spawnFromPool("BoxCollider", Vector3.zero, gameObject.transform);
		BoxCollider boxCollider = newBoxCollider.GetComponent<BoxCollider>();
		boxCollider.GetComponent<Trigger>().setIDs(index, index+1);
		moveBoxCollder(newBoxCollider, boxCollider, startPoint, endPoint);
		for (int i = index; i < lineColliders.Count; i++){
			lineColliders[i].GetComponent<Trigger>().setIDs(i+1, i+2);
			lineColliders[i].name = (i+1 + " - " + i+2);
		}
		newBoxCollider.name = (index) + " - " + (index+1);
		lineColliders.Add(newBoxCollider);
		lineColliders.Sort(sortByTriggerID);
	}

	//given an index, remove the box collider that starts at that index, and change the indexes of all other colliders appropriately.
	public void removeBoxCollider(int index){
		GameObject colliderToRemove = null;
		foreach (GameObject lc in lineColliders){
			Trigger trigger = lc.GetComponent<Trigger>();
			if(trigger.getStartID()==index){
				colliderToRemove=lc;
			}else if(trigger.getStartID()>=index){
	
				trigger.setIDs(trigger.getStartID()-1, trigger.getEndID()-1);
			}
		}
		lineColliders.Remove(colliderToRemove);
		objectPooler.returnToPool("BoxCollider", colliderToRemove);
	}

	//move a box collider to the given start point and end point
	private void moveBoxCollder(GameObject objectBoxCollider, BoxCollider boxCollider, Vector3 startPoint, Vector3 endPoint){
		startPoint = lineManager.rotateVertex(startPoint, lineManager.getLocalRotation());
		endPoint = lineManager.rotateVertex(endPoint, lineManager.getLocalRotation());

		Vector3 midPoint = (startPoint + endPoint)/2;
		boxCollider.transform.position = midPoint; 

		float lineLength = Vector3.Distance(startPoint, endPoint); 
		boxCollider.size = new Vector3(lineLength, attachedLR.endWidth+0.1f, attachedLR.endWidth+0.1f); 

		float angle = Mathf.Atan2((endPoint.z - startPoint.z), (endPoint.x - startPoint.x));
		angle *= Mathf.Rad2Deg;
		angle *= -1; 
		boxCollider.transform.LookAt(startPoint);
		Quaternion currentRot= boxCollider.transform.rotation;
		boxCollider.transform.Rotate(0,90,0);

	}

	//move all of the box colliders that this script manages.
	private void updateBoxColliders(){
		int i = 0;
		foreach (GameObject lineCollider in lineColliders){
			moveBoxCollder(lineCollider, lineCollider.GetComponent<BoxCollider>(), attachedLR.GetPosition(i), attachedLR.GetPosition(i+1));
			i++;
		}
	}
	#endregion

	#region utilities
	//simple method that is passed as a parameter when sorting by a variable.
	static int sortByTriggerID(GameObject v1, GameObject v2){
		return v1.GetComponent<Trigger>().getStartID() .CompareTo (v2.GetComponent<Trigger>().getStartID());
	}
	#endregion
}
