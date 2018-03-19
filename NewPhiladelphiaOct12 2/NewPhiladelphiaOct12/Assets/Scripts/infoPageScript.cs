using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class infoPageScript : MonoBehaviour {

	public GUISkin mySkin;
	public Texture homeButtonTexture;

	

	void OnGUI () {
		
		//Sets GuiSkin for my Gui objects
		GUI.skin = mySkin;
		
		ARGuidebookGUI.displayTargetButton= false;


		//This sets the font size of the message text
		GUIStyle infoText =  GUI.skin.GetStyle("info");
		infoText.fontSize = Screen.width/45;
		
		GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Credits\n\nProduced by the New Philadelphia Association\n\nFunded by the National Park Service Underground Railroad Network to Freedom and the Illinois Rural Electric Cooperative\n\nDesigned and Created by Jon Amakawa, Studio Amakawa\n\nProgramming by Ben Buchwald\n\nMusic by Andy Amakawa", "info");
		
		//Restart Button
		if (GUI.Button(new Rect(20, 5, Screen.height/10, Screen.height/10),homeButtonTexture,"inventoryButton")){
            SceneManager.LoadScene(0);
		}
		
		//Return Button
		//if (GUI.Button(new Rect(Screen.width/2-Screen.height/6,Screen.height-Screen.height/3, Screen.height/3, Screen.height/3), "", "return")){
			//Application.LoadLevel(0);
		//}
		
	}
}
