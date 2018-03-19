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

public enum TrackableType {
	ImageTarget,
	FrameMarker
}

[RequireComponent(typeof(VuforiaBehaviour))]
public class VuforiaTargetSelector : TargetSelector {
	// Unity Variables
	public TrackableType TrackableType = TrackableType.ImageTarget;
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

	private DeviceTracker MarkerTracker {
		get { return TrackerManager.Instance.GetTracker<DeviceTracker>(); }

        
	}

 

	// Unity Events
	protected virtual void Start() {
		SetTrackableType(TrackableType);

		if (ImageTracker != null)
			ImageTracker.PersistExtendedTracking(PersistExtendedTracking);

		var vuforiaBehavior = GetComponent<VuforiaBehaviour>();
        var arControl = VuforiaARController.Instance;
		if (vuforiaBehavior != null) {
            //vuforiaBehavior.RegisterVuforiaInitializedCallback(OnVuforiaInitialized);
            //vuforiaBehavior.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
            arControl.RegisterVuforiaInitializedCallback(OnVuforiaInitialized);
            arControl.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
        }
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

	public void SetTrackableType(TrackableType trackableType) {
		TrackableType = trackableType;
		switch (TrackableType) {
		case TrackableType.ImageTarget:
			if (ImageTracker != null && !ImageTracker.IsActive)
				ImageTracker.Start();
			if (MarkerTracker != null && MarkerTracker.IsActive)
				MarkerTracker.Stop();
			break;
		case TrackableType.FrameMarker:
			if (ImageTracker != null && ImageTracker.IsActive)
				ImageTracker.Stop();
			if (MarkerTracker != null && !MarkerTracker.IsActive)
                MarkerTracker.Start();
            break;
        }
    }
    
    public void SetExtendedTracking(bool useExtendedTracking) {
		if (useExtendedTracking != UseExtendedTracking) {
			UseExtendedTracking = useExtendedTracking;
			UpdateExtendedTracking();
		}
	}

	public void SetPersistExtendedTracking(bool persistExtendedTracking) {
		if (persistExtendedTracking != PersistExtendedTracking) {
			if (ImageTracker != null && ImageTracker.PersistExtendedTracking(persistExtendedTracking))
				PersistExtendedTracking = persistExtendedTracking;
		}
	}
	
	protected void UpdateExtendedTracking() {
		foreach (TrackableBehaviour trackable in Trackables) {
            //ExtendedTrackable extendedTrackable = trackable.Trackable as ExtendedTrackable;

            SetExtendedTracking(UseExtendedTracking);
		}
	}
}
