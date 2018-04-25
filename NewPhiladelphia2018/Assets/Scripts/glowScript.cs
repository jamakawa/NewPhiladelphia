using UnityEngine;
using System.Collections;

public class glowScript : MonoBehaviour {


	public GameObject glowModel;
	private float glowTimer= 10.0f;
	private bool startTimer= false;

	void Update() {

		if (startTimer){
			glowTimer -= Time.deltaTime;

			if (glowTimer>0){
				shaderGlow gls= glowModel.GetComponent<shaderGlow>();
				gls.lightOn();
			}
			else {
				shaderGlow gls= glowModel.GetComponent<shaderGlow>();
				gls.lightOff();
				startTimer= false;
			}
		}

	}

	
	void OnMouseDown() {
		startTimer= true;
		glowTimer= 5.0f;
	}
}
