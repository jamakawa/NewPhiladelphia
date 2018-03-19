using UnityEngine;
using System.Collections;

public class slateScript : MonoBehaviour {

	public GameObject glowObject;
	public GameObject pencilObject;

	void OnMouseDown() {
		ARGuidebookGUI.slateFound = true;

		ARGuidebookGUI.displayMessage= true;
		ARGuidebookGUI.mainMessageTimer = 20.0f;
		ARGuidebookGUI.messageText= "Well done! Archeologists found fragments of slate and pencil nearby that may have been used at the schoolhouse.";

		//This creates the glow effect when selecting the artifact
		Instantiate(glowObject, transform.position, transform.rotation);

		//This makes the slate invisible after being selected
		Destroy(gameObject);
		Destroy(pencilObject);
	}
}
