using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vuforia;

public class DepthMask : MonoBehaviour {
	// Unity Variables
	public Material MaskMaterial;

	// Fields
	private Material[] origMaterials;

	// Unity Events
	void OnEnable() {
		origMaterials = GetComponent<Renderer>().sharedMaterials;

		if (!VuforiaRuntimeUtilities.IsVuforiaEnabled())
			return;

		int numMaterials = GetComponent<Renderer>().materials.Length;
		if (numMaterials == 1) {
            GetComponent<Renderer>().sharedMaterial = MaskMaterial;
		} else {
			Material[] maskMaterials = new Material[numMaterials];
			for (int i = 0; i < numMaterials; i++)
				maskMaterials[i] = MaskMaterial;

            GetComponent<Renderer>().sharedMaterials = maskMaterials;
		}
	}

	void OnDisable() {
		GetComponent<Renderer>().sharedMaterials = origMaterials;
	}
}
