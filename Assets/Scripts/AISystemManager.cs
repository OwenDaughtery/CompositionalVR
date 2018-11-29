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
	public bool markovNoteProduction = false;
	public bool harmonizeInputNoteProduction = false;
	public bool harmonizeMarkovNoteProduction = false;
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

	private Dictionary<int, List<GridManager.Notes>> harmonizeInput(Dictionary<int, List<GridManager.Notes>> input, Dictionary<int, List<GridManager.Notes>> harmonizedInput){
		wipeDict(harmonizedInput);
		int[] upperOrLower = new int[2]{2,-2};
		Dictionary<int, List<GridManager.Notes>> notesToAdd = new Dictionary<int, List<GridManager.Notes>>();
		
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in input){
			notesToAdd.Add(pair.Key, new List<GridManager.Notes>());
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

	private Dictionary<GridManager.Notes, ChainLink> generateMarkovChain(Dictionary<int, List<GridManager.Notes>> input){
		print("eyyyoo");
		Dictionary<GridManager.Notes, ChainLink> markovChain = new Dictionary<GridManager.Notes, ChainLink>();
		List<GridManager.Notes> encounteredNotes = new List<GridManager.Notes>();
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in input){
			foreach (GridManager.Notes note in pair.Value){
				if(!markovChain.ContainsKey(note)){
					markovChain.Add(note, new ChainLink(note));
				}
				int futureKey = pair.Key+1;
				bool nextNotesNotFound = true;
				while(futureKey<input.Count && nextNotesNotFound){
					//print(input[futureKey].Count);
					if(input[futureKey].Count>0){
						foreach (GridManager.Notes futureNote in input[futureKey]){
							//print("ello?");
							markovChain[note].addToPath(futureNote);
						}
						nextNotesNotFound=false;
					}
					futureKey+=1;

					
				}
			}
		}
		return markovChain;
	}

	private Dictionary<int, List<GridManager.Notes>> generateMarkovNotes(Dictionary<int, List<GridManager.Notes>> input, Dictionary<GridManager.Notes, ChainLink> markovChain, Dictionary<int, List<GridManager.Notes>> markovNotes){
		wipeDict(markovNotes);
		
		int startingBeat = getEndOfInput(input)+4;
		List<GridManager.Notes> allNotes = getAllNotesOfInput(input);
		GridManager.Notes nextNote = allNotes[random.Next(allNotes.Count)];
		markovNotes[startingBeat].Add(nextNote);
		for (int i = startingBeat+4; i < markovNotes.Count; i+=4){
			nextNote = markovChain[nextNote].getNextNote();
			markovNotes[i].Add(nextNote);
		}
		return markovNotes;
	}

	private List<GridManager.Notes> getAllNotesOfInput(Dictionary<int, List<GridManager.Notes>> input){
		List<GridManager.Notes> allNotes = new List<GridManager.Notes>();
		foreach(KeyValuePair<int, List<GridManager.Notes>> pair in input){
			foreach(GridManager.Notes note in pair.Value){
				if(!allNotes.Contains(note)){
					allNotes.Add(note);
				}
			}
		}
		return allNotes;
	}

	private int getEndOfInput(Dictionary<int, List<GridManager.Notes>> input){
		int endOfInput = 0;
		foreach(KeyValuePair<int, List<GridManager.Notes>> pair in input){
			if(pair.Value.Count>0){
				endOfInput=pair.Key;
			}
		}
		return endOfInput;
	}


	private void playNote(GridManager.Notes note){
		//print(note);
		VertexManager.contactSC(note, 0.5f,0.5f,"VoiceA");	
	}

	IEnumerator playInput(){
		//Dictionary<int, List<GridManager.Notes>> harmonizedInput =  harmonizeInput();
		Dictionary<int, List<GridManager.Notes>> input =  masterScore;
		Dictionary<int, List<GridManager.Notes>> harmonizedInputNotes = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> harmonizedMarkovNotes = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> randomNotes = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> markovNotes = new Dictionary<int, List<GridManager.Notes>>();
		List<GridManager.Notes> notesToPlay = null;
		for (int i = 0; i < 64; i++){
			randomNotes.Add(i, new List<GridManager.Notes>());
			markovNotes.Add(i, new List<GridManager.Notes>());
			harmonizedInputNotes.Add(i, new List<GridManager.Notes>());
			harmonizedMarkovNotes.Add(i, new List<GridManager.Notes>());
		}
		int count=0;
		while(playButton){
			
			if(randomNoteProduction){
				randomNotes = generateRandomNotes(randomNotes, 100-(count*5));
			}
			if(markovNoteProduction){
				Dictionary<GridManager.Notes, ChainLink> markovChain = generateMarkovChain(input);
				markovNotes = generateMarkovNotes(input, markovChain, markovNotes);
			}
			if(harmonizeInputNoteProduction){
				harmonizedInputNotes = harmonizeInput(input, harmonizedInputNotes); //harmonise users input
			}
			if(harmonizeMarkovNoteProduction){
				harmonizedMarkovNotes = harmonizeInput(markovNotes, harmonizedMarkovNotes);
			}

			foreach (KeyValuePair<int, List<GridManager.Notes>> pair in masterScore){
				notesToPlay = pair.Value; //original tune notes
				if(randomNoteProduction){
					notesToPlay = concat(notesToPlay, randomNotes[pair.Key]); // concat random
				}
				if(markovNoteProduction){
					notesToPlay = concat(notesToPlay, markovNotes[pair.Key]);
				}
				if(harmonizeInputNoteProduction){
					notesToPlay = concat(notesToPlay, harmonizedInputNotes[pair.Key]); //concat harmonized
				}
				if(harmonizeMarkovNoteProduction){
					notesToPlay = concat(notesToPlay, harmonizedMarkovNotes[pair.Key]); //concat harmonized
				}
				
				foreach (GridManager.Notes note in notesToPlay){
					playNote(note);
					
				}
				yield return new WaitForSeconds(0.1f);
			}

			count+=1;
		}
		coroutine=null;
		
	}

	private Dictionary<int, List<GridManager.Notes>> wipeDict(Dictionary<int, List<GridManager.Notes>> dictToWipe){
		//following code wips previous notes from last loop:
		for (int i = 0; i < dictToWipe.Count; i++){
			dictToWipe[i] = new List<GridManager.Notes>();
		}

		return dictToWipe;
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


	public class ChainLink{
		private System.Random percentChooser = new System.Random();
		GridManager.Notes mainNote;
		Dictionary<GridManager.Notes, float> paths = new Dictionary<GridManager.Notes, float>();

		public ChainLink(GridManager.Notes note){
			mainNote = note;
		}

		public void addToPath(GridManager.Notes newNote){
			if(!paths.ContainsKey(newNote)){
				paths.Add(newNote, 1f);
			}else{
				paths[newNote]+=1f;
			}
		}

		public GridManager.Notes getNextNote(){
			int p = percentChooser.Next(100);
			float totalSum = sumAllValues();
			int index=0;
			float step = 100/totalSum;
			float encountered = 0;

			foreach (KeyValuePair<GridManager.Notes, float> pair in paths){
				if(p<((step*pair.Value)+encountered)){
					return pair.Key;
				}else{
					encountered+=(step*pair.Value);
				}
			}
			Debug.Log("ERROR - No next note found in markov chain");
			return GridManager.Notes.none;
		}

		private float sumAllValues(){
			float totalSum=0;
			foreach (KeyValuePair<GridManager.Notes, float> pair in paths){
				totalSum+=pair.Value;
			}
			return totalSum;
		}

		public void printContents(){
			
			foreach (KeyValuePair<GridManager.Notes, float> pair in paths){
				print(mainNote + " is followed by " + pair.Key + " " + pair.Value + " times." );
			}
		}


		public GridManager.Notes getMainNote(){
			return mainNote;
		}
		
		
	}	

	
}
