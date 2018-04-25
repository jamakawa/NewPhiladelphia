using UnityEngine;
using System.Collections;
using Vuforia;

public class InertialTracking : VuforiaTargetSelector {
	// Enums
	public enum TrackingState {
		None,
		Tracked,
		Predicted
	};

	[System.Flags]
	public enum TrackingConstraints {
		None = 0,
		Time = 1,
		Rotation = 2,
		Acceleration = 4
	};

	// Private Fields
	private float lastTrackedTime;
	private Quaternion lastTrackedRotation;
	private TrackingState state;
	private Quaternion previousAttitude;

	// Unity Variables
	public bool UseInertialTracking = false;
	public float MaxTrackingTime = 5.0f; // seconds
	public float MaxTrackingAngle = 120.0f; // degrees
	public float MaxTrackingAccel = 5.0f; // m/s^2
	[EnumFlags]
	public TrackingConstraints Constraints;

	// Unity Events
	protected override void Start() {
		base.Start();
		if (SystemInfo.supportsGyroscope)
			Input.gyro.enabled = true;
		else
			UseInertialTracking = false;
	}
	
	void Update() {
		Quaternion attitude = Input.gyro.attitude;
		Quaternion deltaAttitude = Quaternion.Inverse(previousAttitude) * attitude;
		deltaAttitude = Quaternion.Euler(-deltaAttitude.eulerAngles.x, -deltaAttitude.eulerAngles.y, deltaAttitude.eulerAngles.z);
		previousAttitude = attitude;

		if (state == TrackingState.Predicted) {
			transform.Rotate(deltaAttitude.eulerAngles);

			float acceleration = Input.gyro.userAcceleration.magnitude * 9.81f;

			if (!UseInertialTracking ||
			    (((Constraints & TrackingConstraints.Time) != 0) && Time.time - lastTrackedTime >= MaxTrackingTime) ||
			    (((Constraints & TrackingConstraints.Rotation) != 0) && Quaternion.Angle(lastTrackedRotation, transform.rotation) >= MaxTrackingAngle) ||
			    (((Constraints & TrackingConstraints.Acceleration) != 0) && acceleration >= MaxTrackingAccel)) {
				state = TrackingState.None;
				SelectedTrackable = null;
			}
		}
	}

	// Methods
	protected override void SetTrackableDetected(TrackableBehaviour trackable) {
		if (trackable != null) {
			state = TrackingState.Tracked;
			SelectedTrackable = trackable;
		} else if (!UseInertialTracking) {
			state = TrackingState.None;
			SelectedTrackable = null;
		} else if (state == TrackingState.Tracked) {
			state = TrackingState.Predicted;
			lastTrackedTime = Time.time;
			lastTrackedRotation = transform.rotation;
		}
	}
}
