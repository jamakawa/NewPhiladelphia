using UnityEngine;
using System.Collections;

public enum POIType {
	None,
	Additional,
	Target
}

public class PointOfInterest : MonoBehaviour {
	// Unity Variables
	public GUISkin mySkin;
	public string Title;
	public string Date;
	[Multiline]
	public string Description;

	//This variable identifies another object with a potentially competing audio clip
	public GameObject audioOtherObject;

	public Texture2D OffscreenIcon;
	public float IconSize = .1f;
	public bool RotateOffscreenIcon = true;

	// Fields
	private POIType type = POIType.None;

	// Properties
	public POIType Type {
		get { return type; }
		set { type = value; }
	}

	public Vector3 ScreenPoint {
		get {
			return Camera.main.WorldToScreenPoint(transform.position);
		}
	}

	public bool IsOnScreen {
		get {
			Vector3 screenPoint = ScreenPoint;
			return (screenPoint.z > 0 &&
			    screenPoint.x >= 0 && screenPoint.x <= Screen.width &&
			    screenPoint.y >= 0 && screenPoint.y <= Screen.height);
		}
	}

	// Unity Events
	void OnGUI() {
		//Sets GuiSkin for my Gui objects
		GUI.skin = mySkin;

		//This sets the GUI Title font size and scales according to the Screen size
		GUIStyle labelTitle =  GUI.skin.GetStyle("title");
		labelTitle.fontSize = Screen.width/60;


	
		if (Type == POIType.None)
			return;

		if (IsOnScreen) {
			// Could do something here like draw a label onscreen
			//This displays the title info

			GUI.Box(new Rect(Screen.width/2-Screen.width/4, Screen.height-Screen.height/20, Screen.width/2, Screen.height/2), ""+Title +", New Philadelphia, Il "+Date,"title");
		
			//GUI.Box(new Rect(Screen.width/2-Screen.width/4, Screen.height-150, Screen.width/2, Screen.height/2), ""+Title,"title");
			//GUI.Box(new Rect(Screen.width/2-Screen.width/4, Screen.height-80, Screen.width/2, Screen.height/2), "New Philadelphia, Il "+Date,"date");
		} else {
			if (Type == POIType.Target)
				DrawOffscreenIndicator();
		}
	}

	// Methods
	private void DrawOffscreenIndicator() {
		if (OffscreenIcon == null)
			return;

		Vector3 screenPoint = ScreenPoint;
		if (screenPoint.z < 0)
			screenPoint = -screenPoint;
		
		Vector3 screenCenter = new Vector3(Screen.width/2f, Screen.height/2f, 0);
		screenPoint -= screenCenter;
		screenPoint.z = 0f;
		
		float slope = screenPoint.y/screenPoint.x;
		float angle = Mathf.Atan2(screenPoint.y,screenPoint.x) * Mathf.Rad2Deg;
		
		float iconSize = IconSize * Mathf.Min(Screen.width, Screen.height);
		Vector3 screenSize = new Vector3(Screen.width-iconSize, Screen.height-iconSize, 0)*.49f;
		
		if (screenPoint.y > 0) {
			screenPoint.x = screenSize.y/slope;
			screenPoint.y = screenSize.y;
		} else {
			screenPoint.x = -screenSize.y/slope;
			screenPoint.y = -screenSize.y;
		}
		if (screenPoint.x < -screenSize.x) {
			screenPoint.x = -screenSize.x;
			screenPoint.y = -screenSize.x*slope;
		} else if (screenPoint.x > screenSize.x) {
			screenPoint.x = screenSize.x;
			screenPoint.y = screenSize.x*slope;
		}
		
		screenPoint.y = -screenPoint.y;
		screenPoint += screenCenter;
		
		Matrix4x4 oldMatrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(-angle,new Vector2(screenPoint.x,screenPoint.y));
		
		Rect rect = new Rect(screenPoint.x - iconSize/2f, screenPoint.y - iconSize/2f, iconSize, iconSize);
		GUI.DrawTexture(rect, OffscreenIcon);
		
		GUI.matrix = oldMatrix;
	}

	/*** Put code here when this point of interest becomes active ***/
	public void Activate() {

	}

	/*** Put code here when this point of interest becomes inactive ***/
	public void Deactivate() {

	}

	/*** Put code here when this point of interest becomes focused ***/
	public void Focus() {

        //This stops the audio on the civil war vet if it is playing so that this audio clip can play by itself.
        var audio = audioOtherObject.GetComponent<AudioSource>();
        if(audio != null)
            audio.Stop();

		//plays associated audio clip
		if (audio != null && audio.clip != null) {
			audio.Play();
		
		}
	}
	
	/*** Put code here when this point of interest becomes unfocused ***/
	public void Unfocus() {
        //This stops the audio on civil war vet
        if (audioOtherObject != null)
        {
            var audio = audioOtherObject.GetComponent<AudioSource>();
        }
        else
        {
            Debug.Log(this.gameObject + " audio other object is not assigned");
        }
        //stops the audio clip
        if (GetComponent<AudioSource>() != null)
			GetComponent<AudioSource>().Stop();

	}
}
