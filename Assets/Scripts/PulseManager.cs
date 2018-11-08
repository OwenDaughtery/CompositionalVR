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
	private Dictionary<LineManager, List<GameObject>> LMtoPulse = new Dictionary<LineManager, List<GameObject>>();
	private Dictionary<GameObject, float> PulseToHeight = new Dictionary<GameObject, float>();
	//the object pool to get pulses from
	private ObjectPooler objectPooler;

	#endregion

	void Start () {
		
		GameObject[] baseLines = GameObject.FindGameObjectsWithTag("BaseLine");

		foreach (GameObject baseLine in baseLines){
			lineManagers.Add(baseLine.GetComponent<LineManager>());
		}
		height=0;
		speed = GridManager.getYSegments()/10;
		objectPooler = ObjectPooler.Instance;
		foreach (LineManager LM in lineManagers){
			LMtoPulse.Add(LM, new List<GameObject>());
		}
	}
	
	void Update () {
		//update the height
		
		updatePulses();
	}

	//method used to take each line manager, and if it has a pulse, make the position of that pulse = to a new position.
	public void updatePulses(){
		
		Vector3 newPulsePos;
		List<LineManager> linesToAddTo = new List<LineManager>();
		Dictionary<LineManager, GameObject> pulsesToRemove = new Dictionary<LineManager, GameObject>();
		/*foreach (LineManager lm in lineManagers){
			newPulsePos = lm.interpole(height, (LMtoPulse[lm]!=null));
			if(LMtoPulse[lm]){
				LMtoPulse[lm].transform.position = newPulsePos;
			}
		}*/
		List<LineManager> keys = new List<LineManager> (LMtoPulse.Keys);
		foreach(LineManager key in keys){
			foreach (GameObject pulse in LMtoPulse[key]){
				PulseToHeight[pulse] = PulseToHeight[pulse]+(Time.deltaTime * speed);
				if(PulseToHeight[pulse]>=GridManager.getYSegments()-1 && LMtoPulse[key].Count==1){
					linesToAddTo.Add(key);
				}
				if(PulseToHeight[pulse]>=GridManager.getYSegments()){
					pulsesToRemove.Add(key, pulse);
				}else{
					float rotation = key.getLocalRotation();
					
					newPulsePos = key.interpole(PulseToHeight[pulse], (LMtoPulse[key]!=null));
					newPulsePos = key.rotateVertex(newPulsePos, rotation);
					pulse.transform.position = newPulsePos;
				}
				
			}
		}

		foreach(LineManager lm in linesToAddTo){
			activateLineManager(lm);
		}

		foreach (KeyValuePair<LineManager, GameObject> pair in pulsesToRemove){
			LMtoPulse[pair.Key].Remove(pair.Value);
			PulseToHeight.Remove(pair.Value);
			objectPooler.returnToPool("Pulse", pair.Value);
		}
	}

	#region utilities

	//"create" a pulse and add it to the dictionary.
	public void activateLineManager(LineManager newLM){
		GameObject pulse = createPulse(newLM);
		
		PulseToHeight.Add(pulse, -1); //temp value, should be -1 not 8
		

		(LMtoPulse[newLM]).Add(pulse);
		
	}

	//take a pulse and return it to the pool, then remove it from the dictionary.
	/*
	public void deactivateLineManager(LineManager LM){
		GameObject pulse = LMtoPulse[LM];
		LMtoPulse[LM] = null;
		objectPooler.returnToPool("Pulse", pulse);
	} */

	//get a pulse object from the pool.
	private GameObject createPulse(LineManager LM){
		
		GameObject pulse = objectPooler.spawnFromPool("Pulse", Vector3.zero, gameObject.transform);
		return pulse;
	}

	#endregion
}
