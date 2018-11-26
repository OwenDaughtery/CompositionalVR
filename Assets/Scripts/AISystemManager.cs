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
	public bool playButton = false;
	public bool randomNoteProduction = false;
	public bool corpusNoteProduction = false;
	Coroutine coroutine = null;
	public bool calculateKeyButton = false;
	private System.Random random = new System.Random();
	private GridManager.Notes lastKey = GridManager.Notes.none;
	
	public Dictionary<int, List<GridManager.Notes>> masterScore = new Dictionary<int, List<GridManager.Notes>>();
	private GridManager.Notes[] inputtedNotes = new GridManager.Notes[4]{GridManager.Notes.C4, GridManager.Notes.F4, GridManager.Notes.G4, GridManager.Notes.A4};
	void Start(){
		populateAllNotes();
		setUpMasterScore();
		addUsersInput();
	}


	private void addUsersInput(){
		masterScore[0].Add(GridManager.Notes.E5);
		masterScore[4].Add(GridManager.Notes.D5);
		masterScore[8].Add(GridManager.Notes.C5);
		masterScore[12].Add(GridManager.Notes.D5);
		masterScore[16].Add(GridManager.Notes.E5);
		masterScore[20].Add(GridManager.Notes.E5);
		masterScore[24].Add(GridManager.Notes.E5);
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
		if(playButton){

			if(coroutine==null){
				//Dictionary<int, List<GridManager.Notes>> harmonizedInput =  harmonizeInput();
				coroutine = StartCoroutine(playInput());
				//coroutine = StartCoroutine(playDict(harmonizedInput));
			}else{

			}
			
			//harmonizeButton=false;
		}
		if(playScaleButton && key!=GridManager.Notes.none){
			StartCoroutine(playScale());
			playScaleButton=false;
		}
		if(calculateKeyButton){
			calculateKey();
			calculateKeyButton = false;
		}
		lastKey=key;
	}

	private void calculateKey(){
		print("functionality does not exist yet");
	}


	private void setAvailableNotes(){
		int offset = (((int)key)-1)%12;
		availableNotes = new List<GridManager.Notes>();
		int numberOfOctaves = 3;
		if(major){
			for(int octave = 0; octave<=numberOfOctaves; octave++){
				for (int i = 0; i < majorScale.Length-1; i++){
					try{
						availableNotes.Add(allNotes[majorScale[i%majorScale.Length]+(12*octave)+(offset)]);
					}catch (System.Exception){
						Debug.Log("Run out of notes for key");
						break;
						
					}
				}
				
			}
			try{
				availableNotes.Add(allNotes[majorScale[majorScale.Length-1]+(12*numberOfOctaves)+(offset)]);
			}catch (System.Exception){
				Debug.Log("Run out of notes for key");
				
			}
			
		}else{
			for(int octave = 0; octave<=numberOfOctaves; octave++){
				for (int i = 0; i <= minorScale.Length-1; i++){
					try{
						availableNotes.Add(allNotes[minorScale[i%minorScale.Length]+(12*octave)+(offset)]);
					}catch (System.Exception){
						Debug.Log("Run out of notes for key");
						break;
						
					}
				}
			}	
			try{
				availableNotes.Add(allNotes[majorScale[7%majorScale.Length-1]+(12*numberOfOctaves)+(offset)]);
			}catch (System.Exception){
				
				Debug.Log("Run out of notes for key");
				
			}
		}
	}

	private void populateAllNotes(){
		int i=0;
		foreach (KeyValuePair<GridManager.Notes, float> pair in GridManager.noteToFreq){
			if(pair.Key!=GridManager.Notes.none){
				allNotes[i] = pair.Key;
				i+=1;
			}

		}
	}

	private GridManager.Notes getNoteFromInt(int index){
		int newIndex = (index)%(availableNotes.Count);
		return availableNotes[newIndex];
	}

	/*private Dictionary<int, List<GridManager.Notes>> harmonizeInput(){
		System.Random random = new System.Random();
		int[] upperOrLower = new int[2]{2,-2};
		Dictionary<int, List<GridManager.Notes>> harmonizedInput = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> notesToAdd = new Dictionary<int, List<GridManager.Notes>>();
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in masterScore){
			harmonizedInput.Add(pair.Key, new List<GridManager.Notes>());
			notesToAdd.Add(pair.Key, new List<GridManager.Notes>());
			foreach (GridManager.Notes note in pair.Value){
				notesToAdd[pair.Key].Add(note);
				try{
					int indexInScale = availableNotes.IndexOf(note);
					int r = random.Next(upperOrLower.Length);
					GridManager.Notes newHarmonizedNote = getNoteFromInt(indexInScale+upperOrLower[r]);
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
}*/

	private Dictionary<int, List<GridManager.Notes>> harmonizeInput(Dictionary<int, List<GridManager.Notes>> input){
		int[] upperOrLower = new int[2]{2,-2};
		Dictionary<int, List<GridManager.Notes>> harmonizedInput = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> notesToAdd = new Dictionary<int, List<GridManager.Notes>>();
		
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in input){
			notesToAdd.Add(pair.Key, new List<GridManager.Notes>());
			harmonizedInput.Add(pair.Key, new List<GridManager.Notes>());
			foreach (GridManager.Notes note in pair.Value){
				
				notesToAdd[pair.Key].Add(note);
				if(pair.Key%4==0){
					try{
					
						int indexInScale = availableNotes.IndexOf(note);
						
						int r = random.Next(upperOrLower.Length);
						GridManager.Notes newHarmonizedNote = getNoteFromInt(indexInScale+upperOrLower[r]);
						
						notesToAdd[pair.Key].Add(newHarmonizedNote);
					}catch (System.Exception){
						Debug.Log("ERROR: Note does not index in available notes!");	
						break;
						throw;
					}
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

	private GridManager.Notes getRandomNoteInKey(GridManager.Notes givenKey){
		int r = random.Next(availableNotes.Count);
		return availableNotes[r];
	}

	private Dictionary<int, List<GridManager.Notes>> generateRandomNotes(Dictionary<int, List<GridManager.Notes>> randomNotes, int bar){
		int numberOfNotesAdded = 0;
		int numberOfNotesRemoved = 0;
		System.Random randomNoteChooser = new System.Random();
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in randomNotes){
			GridManager.Notes newRandomNote = GridManager.Notes.none;
			int r = randomNoteChooser.Next(0,100);
			if(pair.Value.Count!=0){
				if(r>10){
					numberOfNotesRemoved+=1;
					pair.Value.RemoveAt(0);
				}
			}
			if(r>(Mathf.Max(bar,20))){
				numberOfNotesAdded+=1;
				newRandomNote = getRandomNoteInKey(key);
				pair.Value.Add(newRandomNote);
			}
		}
		print("Number Of Notes Added: " + numberOfNotesAdded);
		print("Number Of Notes Removed: " + numberOfNotesRemoved);
		return randomNotes;
	}

	private Dictionary<int, List<GridManager.Notes>> generateCorpusNotes(Dictionary<int, List<GridManager.Notes>> input, Dictionary<int, List<GridManager.Notes>> corpusNotes){
		
		return corpusNotes;
	}


	private void playNote(GridManager.Notes note){
		//print(note);
		VertexManager.contactSC(note, 0.5f,0.5f,"VoiceA");	
	}



	IEnumerator playInput(){
		//Dictionary<int, List<GridManager.Notes>> harmonizedInput =  harmonizeInput();
		Dictionary<int, List<GridManager.Notes>> input =  masterScore;
		Dictionary<int, List<GridManager.Notes>> harmonizedNotes = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> randomNotes = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> corpusNotes = new Dictionary<int, List<GridManager.Notes>>();
		for (int i = 0; i < 64; i++){
			randomNotes.Add(i, new List<GridManager.Notes>());
			corpusNotes.Add(i, new List<GridManager.Notes>());
		}
		int count=0;
		while(playButton){
			harmonizedNotes = harmonizeInput(input);
			if(randomNoteProduction){
				randomNotes = generateRandomNotes(randomNotes, 100-(count*5));
			}
			if(corpusNoteProduction){
				corpusNotes = generateCorpusNotes(input, corpusNotes);
			}
			
			foreach (KeyValuePair<int, List<GridManager.Notes>> pair in masterScore){
				List<GridManager.Notes> notesToPlay = concat(pair.Value, harmonizedNotes[pair.Key]);
				notesToPlay = concat(notesToPlay, randomNotes[pair.Key]);
				foreach (GridManager.Notes note in notesToPlay){
					playNote(note);
					
				}
				yield return new WaitForSeconds(0.1f);
			}

			count+=1;
		}
		coroutine=null;
		
	}

	private List<GridManager.Notes> concat(List<GridManager.Notes> a, List<GridManager.Notes> b){
		List<GridManager.Notes> listToReturn = new List<GridManager.Notes>();
		foreach (GridManager.Notes note in a){
			listToReturn.Add(note);	
		}
		foreach (GridManager.Notes note in b){
			listToReturn.Add(note);	
		}
		return listToReturn;
	}

	IEnumerator playScale(){
		foreach (GridManager.Notes note in availableNotes){
			playNote(note);
			yield return new WaitForSeconds(0.15f);
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
