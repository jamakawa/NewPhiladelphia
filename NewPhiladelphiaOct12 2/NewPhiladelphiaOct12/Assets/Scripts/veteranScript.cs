using UnityEngine;
using System.Collections;

public class veteranScript : MonoBehaviour {

	public GameObject audioObject;

	
	void OnMouseDown() {

		//This stops the main audio narration if it is playing so that this audio clip can play by itself.
		audioObject.GetComponent<AudioSource>().Stop();
		//This plays the veteran's audio clip
		GetComponent<AudioSource>().Play();

	}

	
}
