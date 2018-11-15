using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderManager : MonoBehaviour {

	LineManager lineManager;
	[SerializeField]
	LineRenderer attachedLR;

	private ObjectPooler objectPooler;
	[SerializeField]

	List<GameObject> lineColliders = new List<GameObject>();

	// Use this for initialization
	void Start () {
		lineManager = this.GetComponent<LineManager>();

		objectPooler = ObjectPooler.Instance;
		addBoxColliders();
	}
	
	// Update is called once per frame
	void Update () {
		updateBoxColliders();
	}







	private void addBoxColliders(){
		for (int i = 0; i < attachedLR.positionCount-1; i++){
			addBoxCollider(i);
		}
	}

	public Vector3 rotateCollider(Vector3 pos, float rotationFloat){
		Quaternion rotation = Quaternion.Euler(0,rotationFloat,0);
		Vector3 myVector = pos;
		Vector3 rotateVector = rotation * myVector;
		return rotateVector;
	}

	public void addBoxCollider(int index){
		
		Vector3 startPoint = attachedLR.GetPosition(index);
		Vector3 endPoint = attachedLR.GetPosition(index+1);
		GameObject newBoxCollider = objectPooler.spawnFromPool("BoxCollider", Vector3.zero, gameObject.transform);
		BoxCollider boxCollider = newBoxCollider.GetComponent<BoxCollider>();

		//boxCollider.GetComponent<Trigger>().setIDs(attachedLR.positionCount-2, attachedLR.positionCount-1);
		boxCollider.GetComponent<Trigger>().setIDs(index, index+1);
		

		moveBoxCollder(newBoxCollider, boxCollider, startPoint, endPoint);


		for (int i = index; i < lineColliders.Count; i++){
			print(i);
			lineColliders[i].GetComponent<Trigger>().setIDs(i+1, i+2);
			lineColliders[i].name = (i+1 + " - " + i+2);
		}
		//BoxCollider test;
		//if box collider already exists:
		/*foreach (GameObject lineCollider in lineColliders){
			Trigger trigger = lineCollider.GetComponent<Trigger>();
			int triggerID = trigger.getStartID();
			if(triggerID>index){
				print("had ID " + triggerID + " which is greater/equal than " + index);
				trigger.setIDs(triggerID+1, triggerID+2);
			}
		}*/
		newBoxCollider.name = index + " - " + index+1;
		lineColliders.Add(newBoxCollider);
		lineColliders.Sort(sortByTriggerID);

		//foreach (KeyValuePair<int, BoxCollider> pair in IDToBC){
		//	pair.Value.gameObject.GetComponent<Trigger>().setIDs(pair.Key, pair.Key+1);
		//}
	}

	static int sortByTriggerID(GameObject v1, GameObject v2){
		return v1.GetComponent<Trigger>().getStartID() .CompareTo (v2.GetComponent<Trigger>().getStartID());
	}

	public void removeBoxCollider(int index){
		print("functionality not added yet!");
		//objectPooler.returnToPool("BoxCollider", objectToReturn);
		
		/*if(index==attachedLR.positionCount-1){
			objectPooler.returnToPool("BoxCollider", IDToBC[index].gameObject);
			IDToBC.Remove(index);
		}else{
			print(index);
			for (int i = index-1; i < attachedLR.positionCount-2; i++){
				IDToBC[i]=IDToBC[i+1];
			}
			objectPooler.returnToPool("BoxCollider", IDToBC[attachedLR.positionCount-2].gameObject);
			IDToBC.Remove(attachedLR.positionCount-2);
		}*/
	}

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

		//Vector3 pos = rotateCollider(boxCollider.transform.position, -gameObject.GetComponent<LineManager>().getLocalRotation());		
		//boxCollider.transform.position = pos;

	}

	private void updateBoxColliders(){
		int i = 0;
		foreach (GameObject lineCollider in lineColliders){
			moveBoxCollder(lineCollider, lineCollider.GetComponent<BoxCollider>(), attachedLR.GetPosition(i), attachedLR.GetPosition(i+1));
			i++;
		}
		/*
		for (int i = 0; i < attachedLR.positionCount-1; i++){
			BoxCollider boxCollider = IDToBC[i];
			moveBoxCollder(boxCollider.gameObject, boxCollider, attachedLR.GetPosition(i), attachedLR.GetPosition(i+1));
		} */
	}
}
