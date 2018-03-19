using UnityEngine;
using System.Collections;

public class buttonScript : MonoBehaviour {

	public GameObject glowObject;

	void OnMouseDown() {
		ARGuidebookGUI.buttonFound = true;

		ARGuidebookGUI.displayMessage= true;
		ARGuidebookGUI.mainMessageTimer = 20.0f;
		ARGuidebookGUI.messageText= "Well done! Archeologists found this button nearby.";

		//This creates the glow effect when selecting the artifact
		Instantiate(glowObject, transform.position, transform.rotation);

		//This makes the button invisible after being selected
		Destroy(gameObject);
	}
}
