using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISystemManager : MonoBehaviour {

	[SerializeField]
	private Dictionary<int, List<GridManager.Notes>> masterScore = new Dictionary<int, List<GridManager.Notes>>();
	private LineManager[] lineManagers;

	private static GridManager.Notes[] allNotes = new GridManager.Notes[GridManager.noteToFreq.Count];
	private static int[] majorScale = new int[8] {1,3,5,6,8,10,12,13};
	private static int[] minorScale=  new int[8] {1,3,4,6,8,9,12,13};
	
	public GridManager.Notes key = GridManager.Notes.none;
	public bool major = true;
	private GridManager.Notes lastKey = GridManager.Notes.none;
	
	void Start(){
		populateAllNotes();
	}

	void Update(){
		if(lastKey!=key){
			playScale();
		}
		lastKey=key;
	}

	private void populateAllNotes(){
		int i=0;
		foreach (KeyValuePair<GridManager.Notes, float> pair in GridManager.noteToFreq){
			allNotes[i] = pair.Key;
		}
	}

	private void playScale(){
		int offset = ((int)key)-1;
		if(major){
			for (int i = 0; i < majorScale.Length; i++){
				print("would play: " + ((GridManager.Notes)(majorScale[i]+offset)));
				VertexManager.contactSC((GridManager.Notes)(majorScale[i]+offset),0.5f,0.5f,"/playVoiceA");
			}
		}else{
			for (int i = 0; i < minorScale.Length; i++){
				print("would play: " + ((GridManager.Notes)(minorScale[i]+offset)));
			}
		}
		
	}


}
