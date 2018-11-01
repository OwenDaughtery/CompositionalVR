using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseManager : MonoBehaviour {

	#region variables

	//how high the pulses should be
	private float height;
	//what speed the pulses should be travelling at
	[SerializeField]
	private float speed;
	//a list to hold all the line managers in the game
	private List<LineManager> lineManagers = new List<LineManager>();
	//a dictionary to hold each line manager and it's pulse (if it has one)
	private Dictionary<LineManager, GameObject> LMtoPulse = new Dictionary<LineManager, GameObject>();
	//the object pool to get pulses from
	private ObjectPooler objectPooler;

	#endregion

	void Start () {
		GameObject[] baseLines = GameObject.FindGameObjectsWithTag("BaseLine");
		/*foreach (GameObject baseLine in baseLines){
			lineManagers.Add(baseLine.GetComponent<LineManager>());
		}*/
		height=0;
		speed = 2.5f;
		objectPooler = ObjectPooler.Instance;

		foreach (LineManager LM in lineManagers){
			LMtoPulse.Add(LM, null);
		}
	}
	
	void Update () {
		//update the height
		height += Time.deltaTime * speed;
		if(height>=16){
			height=0;
		}
		updatePulses();
	}

	//method used to take each line manager, and if it has a pulse, make the position of that pulse = to a new position.
	public void updatePulses(){
		Vector3 newPulsePos;
		/*foreach (LineManager lm in lineManagers){
			newPulsePos = lm.interpole(height, (LMtoPulse[lm]!=null));
			if(LMtoPulse[lm]){
				LMtoPulse[lm].transform.position = newPulsePos;
			}
		}*/
		foreach(KeyValuePair<LineManager, GameObject> pair in LMtoPulse){
			newPulsePos = pair.Key.interpole(height, (pair.Value!=null));
			if(pair.Value){
				pair.Value.transform.position = newPulsePos;
			}
		}
	}

	#region utilities

	//"create" a pulse and add it to the dictionary.
	public void activateLineManager(LineManager newLM){
		GameObject pulse = createPulse(newLM);
		LMtoPulse[newLM] = pulse;
	}

	//take a pulse and return it to the pool, then remove it from the dictionary.
	public void deactivateLineManager(LineManager LM){
		GameObject pulse = LMtoPulse[LM];
		LMtoPulse[LM] = null;
		objectPooler.returnToPool("Pulse", pulse);
	}

	//get a pulse object from the pool.
	private GameObject createPulse(LineManager LM){
		
		GameObject pulse = objectPooler.spawnFromPool("Pulse", Vector3.zero, gameObject.transform);
		return pulse;
	}

	#endregion
}
