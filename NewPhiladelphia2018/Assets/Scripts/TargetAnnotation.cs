using UnityEngine;
using System.Collections;
using Vuforia;
using UnityEngine.Playables;

[RequireComponent(typeof(TrackableBehaviour))]
public class TargetAnnotation : MonoBehaviour {
	// Unity Variables
	public string Title;
	[Multiline]
	public string Description;

	public PointOfInterest[] TargetPointsOfInterest;
	public PointOfInterest[] AdditionalPointsOfInterest;
    public PlayableDirector tl_Director;

	// Methods
	public POIType GetPOIType(PointOfInterest poi) {
		if (System.Array.IndexOf(TargetPointsOfInterest, poi) > -1)
			return POIType.Target;
		else if (System.Array.IndexOf(AdditionalPointsOfInterest, poi) > -1)
			return POIType.Additional;
		else
			return POIType.None;
	}

	public bool ContainsPOI(PointOfInterest poi) {
		return GetPOIType(poi) != POIType.None;
	}

	/*** Put code here when this guidepost becomes active ***/
	public void Activate() {
		foreach (PointOfInterest poi in AdditionalPointsOfInterest) {
			poi.Type = POIType.Additional;
		}
		foreach (PointOfInterest poi in TargetPointsOfInterest) {
			poi.Type = POIType.Target;
		}

		//plays associated audio clip
		if (GetComponent<AudioSource>() != null && GetComponent<AudioSource>().clip != null) {
            GetComponent<AudioSource>().Play();
		}

        //play director
        if (tl_Director != null && tl_Director.state != PlayState.Playing)
            tl_Director.Play();

	}

	/*** Put code here when this guidepost becomes inactive ***/
	public void Deactivate() {
		foreach (PointOfInterest poi in TargetPointsOfInterest) {
			poi.Type = POIType.None;
		}
		foreach (PointOfInterest poi in AdditionalPointsOfInterest) {
			poi.Type = POIType.None;
		}

		//stops the audio clip
		if (GetComponent<AudioSource>() != null)
            GetComponent<AudioSource>().Stop();

        if(tl_Director != null && tl_Director.state == PlayState.Playing)
        {
            tl_Director.Stop();
        }
	}
}
