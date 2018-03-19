using UnityEngine;
using System.Collections;

public class trivetScript : MonoBehaviour {
	public GameObject glowObject;
	
	void OnMouseDown() {
		ARGuidebookGUI.trivetFound = true;
		
		ARGuidebookGUI.displayMessage= true;
		ARGuidebookGUI.mainMessageTimer = 20.0f;
		ARGuidebookGUI.messageText= "Well done! Archeologists found this Trivet nearby.";

		//This creates the glow effect when selecting the artifact
		Instantiate(glowObject, transform.position, transform.rotation);
		
		//This makes the doll head invisible after being selected
		Destroy(gameObject);
	}
}