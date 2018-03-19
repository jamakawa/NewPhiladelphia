using UnityEngine;
using System.Collections;
using Vuforia;

public enum DepthMaskMode {
	Hide,
	Mask,
	Show
}

public class HideUntracked : MonoBehaviour {
	// Unity Variables
	public TargetSelector Selector;

	public bool HideTargets = true;
	public bool HideDebug = true;
	public DepthMaskMode DepthMaskMode = DepthMaskMode.Mask;
	public bool SelectedPOIsOnly = false;

	// Fields

	// Unity Events
	void Start() {
		UpdateShowing();
        
		if (Selector != null) {
			Selector.OnSelectedTrackableChanged += SelectedTrackableChanged;
            
		}
	}

	// Delegates
	void SelectedTrackableChanged(TrackableBehaviour previous, TrackableBehaviour selected) {
        UpdateShowing(Selector.SelectedTarget);
	}

	// Methods
	public void UpdateShowing() {

        if (Selector != null)
			UpdateShowing(Selector.SelectedTarget);
		else
			UpdateShowing(null);
	}

	public void UpdateShowing(TargetAnnotation selected) {
		if (!VuforiaRuntimeUtilities.IsVuforiaEnabled())
			return;

		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		Collider[] colliders = GetComponentsInChildren<Collider>();

		foreach (Renderer renderer in renderers) {
			renderer.enabled = !ShouldHide(selected, renderer.gameObject);
			if (renderer.enabled) {
				DepthMask depthMask = renderer.GetComponent<DepthMask>();
				if (depthMask != null)
					depthMask.enabled = DepthMaskMode == DepthMaskMode.Mask;
			}
		}

		foreach (Collider collider in colliders) {
			collider.enabled = !ShouldHide(selected, collider.gameObject);
		}
	}

	private bool ShouldHide(TargetAnnotation selected, GameObject obj) {


		if (VuforiaRuntimeUtilities.IsVuforiaEnabled() && selected == null) {
			return true;
		} else if (HideTargets && obj.GetComponent<TrackableBehaviour>() != null) {
			return true;
		} else if (HideDebug && obj.tag == "Debug") {
			return true;
		} else if (DepthMaskMode == DepthMaskMode.Hide && obj.GetComponent<DepthMask>() != null) {
			return true;
		} else if (SelectedPOIsOnly) {
			PointOfInterest poi = obj.GetComponentInParent<PointOfInterest>();
			if (poi != null && !selected.ContainsPOI(poi))
				return true;
		}
		return false;
	}
}
