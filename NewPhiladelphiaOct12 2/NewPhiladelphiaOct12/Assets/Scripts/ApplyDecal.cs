using UnityEngine;
using System.Collections;

public class ApplyDecal : MonoBehaviour {

	// Unity Variables
	public Shader DecalShader;

	// Unity Events
	void Awake() {
		if (DecalShader != null)
			this.GetComponent<Renderer>().material.shader = DecalShader;
	}
}
