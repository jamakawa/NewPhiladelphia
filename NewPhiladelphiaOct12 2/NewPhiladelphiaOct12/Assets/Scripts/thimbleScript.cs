using UnityEngine;
using System.Collections;

public class thimbleScript : MonoBehaviour {
	
	public GameObject glowObject;


	void OnMouseDown() {
		ARGuidebookGUI.thimbleFound = true;

		ARGuidebookGUI.displayMessage= true;
		ARGuidebookGUI.mainMessageTimer = 20.0f;
		ARGuidebookGUI.messageText= "Well done! Archeologists found this thimble nearby.";

		//This creates the glow effect when selecting the artifact
		Instantiate(glowObject, transform.position, transform.rotation);

		//This makes the thimble head invisible after being selected
		Destroy(gameObject);
	}
}
