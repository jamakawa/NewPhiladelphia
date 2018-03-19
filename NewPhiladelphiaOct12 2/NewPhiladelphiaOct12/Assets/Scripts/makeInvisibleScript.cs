using UnityEngine;
using System.Collections;

public class makeInvisibleScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Renderer>().enabled= false;
	}

}
