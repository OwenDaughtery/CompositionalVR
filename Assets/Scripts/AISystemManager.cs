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
	public bool harmonize = false;
	public bool lastHarmonize;
	private GridManager.Notes lastKey = GridManager.Notes.none;
	
	private GridManager.Notes[] inputtedNotes = new GridManager.Notes[4]{GridManager.Notes.C4, GridManager.Notes.F4, GridManager.Notes.G4, GridManager.Notes.A4};
	void Start(){
		populateAllNotes();
	}

	void Update(){
		
		if(lastKey!=key){
			StartCoroutine(waiter());
			//playScale();
		}
		if(harmonize&&!lastHarmonize){
			harmonizeInput();
		}
		lastHarmonize=harmonize;
		lastKey=key;
	}

	private void populateAllNotes(){
		int i=0;
		foreach (KeyValuePair<GridManager.Notes, float> pair in GridManager.noteToFreq){
			allNotes[i] = pair.Key;
		}
	}

	private void harmonizeInput(){

	}

	private void playScale(){
		int offset = ((int)key)-1;
		if(major){
			for (int i = 0; i < majorScale.Length; i++){
				print("would play: " + ((GridManager.Notes)(majorScale[i]+offset)));
			}
		}else{
			for (int i = 0; i < minorScale.Length; i++){
				print("would play: " + ((GridManager.Notes)(minorScale[i]+offset)));
			}
		}
		
		
	}

	private void playNote(GridManager.Notes note){
		VertexManager.contactSC(note, 0.5f,0.5f,"VoiceA");	
	}

	IEnumerator waiter(){
		int offset = ((int)key)-1;
		if(major){
			for (int i = 0; i < majorScale.Length; i++){
				print("Play: " + ((GridManager.Notes)(majorScale[i]+offset)));
				//Wait for 4 seconds
				playNote((GridManager.Notes)(majorScale[i]+offset));
				yield return new WaitForSeconds(0.3f);
				
			}
		}else{
			for (int i = 0; i < minorScale.Length; i++){
				print("would play: " + ((GridManager.Notes)(minorScale[i]+offset)));
				playNote((GridManager.Notes)(minorScale[i]+offset));
				yield return new WaitForSeconds(0.3f);
			}
		}

	}


}
