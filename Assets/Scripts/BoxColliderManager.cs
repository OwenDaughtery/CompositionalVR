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

	private void addBoxColliders(){
		for (int i = 0; i < attachedLR.positionCount-1; i++){
			addBoxCollider(i);
		}
	}

	public void addBoxCollider(int index){
		
		Vector3 startPoint = attachedLR.GetPosition(index);
		Vector3 endPoint = attachedLR.GetPosition(index+1);
		GameObject newBoxCollider = objectPooler.spawnFromPool("BoxCollider", Vector3.zero, gameObject.transform);
		BoxCollider boxCollider = newBoxCollider.GetComponent<BoxCollider>();
		
		moveBoxCollder(boxCollider, startPoint, endPoint);
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

	private void moveBoxCollder(BoxCollider boxCollider, Vector3 startPoint, Vector3 endPoint){
		Vector3 midPoint = (startPoint + endPoint)/2;
		boxCollider.transform.position = midPoint; 

		float lineLength = Vector3.Distance(startPoint, endPoint); 
		boxCollider.size = new Vector3(lineLength, attachedLR.endWidth, attachedLR.endWidth); 

		float angle = Mathf.Atan2((endPoint.z - startPoint.z), (endPoint.x - startPoint.x));

		//print(Vector2.Angle(x, y));

		//float zDegree = FindDegree(startPoint.x - endPoint.x, startPoint.y - endPoint.y);
		//zDegree *=-1;

		//float xDegree = FindDegree(startPoint.z - endPoint.z, startPoint.y - endPoint.y);

		//float yDegree = FindDegree(startPoint.x - endPoint.x, startPoint.z - endPoint.z);
		//yDegree *=-1;


		//boxCollider.transform.eulerAngles = new Vector3(xDegree,yDegree,zDegree+90);

		// angle now holds our answer but it's in radians, we want degrees
		// Mathf.Rad2Deg is just a constant equal to 57.2958 that we multiply by to change radians to degrees
		angle *= Mathf.Rad2Deg;
	
		//were interested in the inverse so multiply by -1
		angle *= -1; 
		// now apply the rotation to the collider's transform, carful where you put the angle variable
		// in 3d space you don't wan't to rotate on your y axis
		//boxCollider.transform.eulerAngles = new Vector3(angle, 0, 0);
		//boxCollider.transform.Rotate(0, angle, 0);

		boxCollider.transform.LookAt(startPoint);
		Quaternion currentRot= boxCollider.transform.rotation;
		boxCollider.transform.Rotate(0,90,0);
		//boxCollider.transform.eulerAngles = new Vector3(0f,0f,0f);

	}

	private void updateBoxColliders(){
		for (int i = 0; i < attachedLR.positionCount-1; i++){
			BoxCollider boxCollider = IDToBC[i];
			moveBoxCollder(boxCollider, attachedLR.GetPosition(i), attachedLR.GetPosition(i+1));
		}
	}
}
