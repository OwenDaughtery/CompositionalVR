using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISystemManager : MonoBehaviour {

	[SerializeField]
	private Dictionary<int, List<GridManager.Notes>> masterScore = new Dictionary<int, List<GridManager.Notes>>();
	private LineManager[] lineManagers;

	// Use this for initialization

	/*
	void Start () {
		for (int i = 0; i < GridManager.getYSegments(); i++){
			masterScore.Add(i, new List<GridManager.Notes>());
		}

		GameObject[] baseLines = GameObject.FindGameObjectsWithTag("BaseLine");
		lineManagers = new LineManager[baseLines.Length];

		int index=0;
		foreach (GameObject baseLine in baseLines){
			lineManagers[index]=(baseLine.GetComponent<LineManager>());
			index++;
		}
		updateMasterScore();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void updateMasterScore(){
		foreach (LineManager lm in lineManagers){
			Dictionary<float, List<VertexManager>> timingDict = lm.getTimingDict();
			foreach(KeyValuePair<float, List<VertexManager>> pair in timingDict){
				foreach (VertexManager vm in pair.Value){
					masterScore[(int)pair.Key].Add(vm.getVertexNote());
				}
			}
		}
	} */
}
