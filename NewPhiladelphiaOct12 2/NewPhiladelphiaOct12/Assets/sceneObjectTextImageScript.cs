using UnityEngine;
using System.Collections;

public class sceneObjectTextImageScript : MonoBehaviour {

	public string objectText;
	public Texture2D objectImage;
	
	
	void OnMouseDown() {
		
		ARGuidebookGUI.displayMessage= true;
		ARGuidebookGUI.displayImage= true;
		ARGuidebookGUI.mainMessageTimer = 20.0f;
		ARGuidebookGUI.messageText= objectText;
		ARGuidebookGUI.messageImage= objectImage;
		
	}
}