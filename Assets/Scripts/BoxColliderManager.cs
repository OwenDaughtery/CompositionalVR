using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderManager : MonoBehaviour {

	LineManager lineManager;
	[SerializeField]
	LineRenderer attachedLR;

	private ObjectPooler objectPooler;
	[SerializeField]
	private Dictionary<int, BoxCollider> IDToBC = new Dictionary<int, BoxCollider>();
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





	public Dictionary<int, BoxCollider> getAllBoxColliders(){
		return IDToBC;
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
		newBoxCollider.GetComponent<Trigger>().setIDs(index, index+1);
		moveBoxCollder(newBoxCollider, boxCollider, startPoint, endPoint);
		BoxCollider test;
		if(IDToBC.TryGetValue(index, out test)){
			
			IDToBC.Add(IDToBC.Count, IDToBC[IDToBC.Count-1]);
			for (int i = IDToBC.Count-2; i > index; i--){
				IDToBC[i]=IDToBC[i-1];
			}
			IDToBC[index]=boxCollider;
		}else{
			IDToBC.Add(index, boxCollider);
		}
	}

	public void removeBoxCollider(int index){
		GameObject objectToReturn = IDToBC[attachedLR.positionCount-2].gameObject;
		IDToBC.Remove(attachedLR.positionCount-2);
		objectPooler.returnToPool("BoxCollider", objectToReturn);
		
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
		for (int i = 0; i < attachedLR.positionCount-1; i++){
			BoxCollider boxCollider = IDToBC[i];
			moveBoxCollder(boxCollider.gameObject, boxCollider, attachedLR.GetPosition(i), attachedLR.GetPosition(i+1));
		}
	}
}
