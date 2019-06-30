using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vuforia;

public delegate void SelectedTrackableChangedHandler(TrackableBehaviour previous, TrackableBehaviour selected);

public abstract class TargetSelector : MonoBehaviour {
	private TrackableBehaviour selectedTrackable;
	
	public TrackableBehaviour SelectedTrackable {
		get { return selectedTrackable; }
		protected set {
			TrackableBehaviour prevTrackable = selectedTrackable;
			if (prevTrackable != value) {
				selectedTrackable = value; 
				if (OnSelectedTrackableChanged != null)
					OnSelectedTrackableChanged(prevTrackable, selectedTrackable);
			}
		}
	}
	public TargetAnnotation SelectedTarget {
		get { return selectedTrackable != null ? selectedTrackable.GetComponent<TargetAnnotation>() : null; }
	}

	public event SelectedTrackableChangedHandler OnSelectedTrackableChanged;
}

[RequireComponent(typeof(VuforiaBehaviour))]
public class VuforiaTargetSelector : TargetSelector {
	// Unity Variables
    public bool UseExtendedTracking = false;
	public bool PersistExtendedTracking = false;

	// Properties
	public static IEnumerable<TrackableBehaviour> Trackables {
		get { return TrackerManager.Instance.GetStateManager().GetTrackableBehaviours(); }
	}

	public static IEnumerable<TrackableBehaviour> ActiveTrackables {
		get { return TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours(); }
	}

	private ObjectTracker ImageTracker {
		get { return TrackerManager.Instance.GetTracker<ObjectTracker>(); }
	}

	// Unity Events
	protected virtual void Start() {
        // Legacy code from vuforia 7.0. Leaving to help track old functionality if this needs to be replaced with alternate method
		//if (ImageTracker != null)
		//	ImageTracker.PersistExtendedTracking(PersistExtendedTracking);

		VuforiaARController.Instance.RegisterVuforiaInitializedCallback(OnVuforiaInitialized);
        VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
	}

	// Delegates
	void OnVuforiaInitialized() {
		UpdateExtendedTracking();
	}
	
	void OnTrackablesUpdated() {
		TrackableBehaviour newTrackable = null;
		foreach (TrackableBehaviour trackable in ActiveTrackables) {
			switch (trackable.CurrentStatus) {
			case TrackableBehaviour.Status.DETECTED:
			case TrackableBehaviour.Status.EXTENDED_TRACKED:
				if (newTrackable == null)
					newTrackable = trackable;
				break;
			case TrackableBehaviour.Status.TRACKED:
				if (newTrackable == null || trackable == SelectedTrackable) // prefer previous
					newTrackable = trackable;
				break;
			}
		}
		SetTrackableDetected(newTrackable);
	}
	
	// Methods
	protected virtual void SetTrackableDetected(TrackableBehaviour trackable) {
		SelectedTrackable = trackable;
	}

    public void SetExtendedTracking(bool useExtendedTracking) {
		if (useExtendedTracking != UseExtendedTracking) {
			UseExtendedTracking = useExtendedTracking;
			UpdateExtendedTracking();
		}
	}

	public void SetPersistExtendedTracking(bool persistExtendedTracking) {
        // Legacy code from vuforia 7.0. Leaving commented for debugging if this needs to be replaced with a modern implementation
        //if (persistExtendedTracking != PersistExtendedTracking) {
        //	if (ImageTracker != null && ImageTracker.PersistExtendedTracking(persistExtendedTracking))
        //		PersistExtendedTracking = persistExtendedTracking;
        //}
    }

    protected void UpdateExtendedTracking() {
        // Legacy code from vuforia 7.0. Leaving commented for debugging if this needs to be replaced with a modern implementation
        //foreach (TrackableBehaviour trackable in Trackables) {
        //	ObjectTarget extendedTrackable = trackable.Trackable as ObjectTarget;
        //	if (extendedTrackable != null)
        //	{
        //		if (UseExtendedTracking)
        //			extendedTrackable.StartExtendedTracking();
        //		else
        //			extendedTrackable.StopExtendedTracking();
        //	}
        //}
    }
}
