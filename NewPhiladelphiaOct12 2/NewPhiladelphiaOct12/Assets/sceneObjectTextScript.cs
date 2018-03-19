using UnityEngine;
using System.Collections;

public class sceneObjectTextScript : MonoBehaviour {

		
		public string objectText;

		
		
		void OnMouseDown() {
		    ARGuidebookGUI.displayImage= false;
			ARGuidebookGUI.displayMessage= true;
			ARGuidebookGUI.mainMessageTimer = 20.0f;
			ARGuidebookGUI.messageText= objectText;
			
		}
	}