using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISystemManager : MonoBehaviour {

	private LineManager[] lineManagers;

	[SerializeField]
	public static GridManager.Notes[] allNotes = new GridManager.Notes[GridManager.noteToFreq.Count];
	private static int[] majorScale = new int[8] {0,2,4,5,7,9,11,12};
	private static int[] minorScale=  new int[8] {0,2,3,5,7,8,11,12};
	
	public GridManager.Notes key = GridManager.Notes.none;
	public List<GridManager.Notes> availableNotes;
	public bool major = true;
	public bool playScaleButton = false;
	public bool harmonize = false;
	private GridManager.Notes lastKey = GridManager.Notes.none;
	
	public Dictionary<int, List<GridManager.Notes>> masterScore = new Dictionary<int, List<GridManager.Notes>>();
	private GridManager.Notes[] inputtedNotes = new GridManager.Notes[4]{GridManager.Notes.C4, GridManager.Notes.F4, GridManager.Notes.G4, GridManager.Notes.A4};
	void Start(){
		populateAllNotes();
		setUpMasterScore();
		addUsersInput();
	}


	private void addUsersInput(){
		masterScore[0].Add(GridManager.Notes.E4);
		masterScore[4].Add(GridManager.Notes.D4);
		masterScore[8].Add(GridManager.Notes.C4);
		masterScore[12].Add(GridManager.Notes.D4);
		masterScore[16].Add(GridManager.Notes.E4);
		masterScore[20].Add(GridManager.Notes.E4);
		masterScore[24].Add(GridManager.Notes.E4);
	}

	private void setUpMasterScore(){
		for (int i = 0; i < 64; i++){
			masterScore.Add(i, new List<GridManager.Notes>());
		}
	}

	void Update(){
		
		if(lastKey!=key && key!=GridManager.Notes.none){
			setAvailableNotes();
		}
		if(harmonize){
			Dictionary<int, List<GridManager.Notes>> harmonizedInput =  harmonizeInput();
			StartCoroutine(playDict(harmonizedInput));
			harmonize=false;
		}
		if(playScaleButton && key!=GridManager.Notes.none){
			StartCoroutine(playScale());
			playScaleButton=false;
		}
		lastKey=key;
	}


	private void setAvailableNotes(){
		int offset = (((int)key)-1)%12;
		availableNotes = new List<GridManager.Notes>();

		if(major){
			for(int octave = 0; octave<=1; octave++){
				for (int i = 0; i <= majorScale.Length-1; i++){
					availableNotes.Add(allNotes[majorScale[i%majorScale.Length]+(12*octave)+(offset)]);
				}
			}
		}else{
			for(int octave = 0; octave<=1; octave++){
				for (int i = 0; i <= minorScale.Length-1; i++){
					availableNotes.Add(allNotes[minorScale[i%minorScale.Length]+(12*octave)+(offset)]);
				}
			}	
		}
	}

	private void populateAllNotes(){
		int i=0;
		foreach (KeyValuePair<GridManager.Notes, float> pair in GridManager.noteToFreq){
			allNotes[i] = pair.Key;
			i+=1;
		}
	}

	private GridManager.Notes getNoteFromInt(int index){
		//int octaveJump = Mathf.FloorToInt(index/availableNotes.Count);
		//print("octave jump: " + octaveJump);
		int newIndex = (index)%(availableNotes.Count);
		//newIndex = newIndex+(octaveJump*12);
		return availableNotes[newIndex];
	}
	/* 

	private GridManager.Notes getIntFromNote(GridManager.Notes note){
		if((int)note>12){
			int baseOctaveIndex = note
		}
	}*/

	private Dictionary<int, List<GridManager.Notes>> harmonizeInput(){
		Dictionary<int, List<GridManager.Notes>> harmonizedInput = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> notesToAdd = new Dictionary<int, List<GridManager.Notes>>();
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in masterScore){
			harmonizedInput.Add(pair.Key, new List<GridManager.Notes>());
			notesToAdd.Add(pair.Key, new List<GridManager.Notes>());
			foreach (GridManager.Notes note in pair.Value){
				try{
					int indexInScale = availableNotes.IndexOf(note);
					print("note is: " + (availableNotes[indexInScale]));
					GridManager.Notes newHarmonizedNote = getNoteFromInt(indexInScale+2);
					print("added note is: " + (availableNotes[indexInScale]));
					notesToAdd[pair.Key].Add(note);
					notesToAdd[pair.Key].Add(newHarmonizedNote);
				}catch (System.Exception){
					Debug.Log("ERROR: Note does not index in available notes!");	
					break;
					throw;
				}
				
			}
		}

		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in notesToAdd){
			foreach (GridManager.Notes note in pair.Value){		
				harmonizedInput[pair.Key].Add(note);
			}
		}		
		return harmonizedInput;
	}

	private void playNote(GridManager.Notes note){
		//print(note);
		VertexManager.contactSC(note, 0.5f,0.5f,"VoiceA");	
	}

	IEnumerator playScale(){
		foreach (GridManager.Notes note in availableNotes){
			playNote(note);
			yield return new WaitForSeconds(0.3f);
		}
	}

	IEnumerator playDict(Dictionary<int, List<GridManager.Notes>> givenDict){
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in givenDict){
			foreach (GridManager.Notes note in pair.Value){
				playNote(note);
				
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
}
