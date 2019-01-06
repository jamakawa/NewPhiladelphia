using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundAnim : MonoBehaviour {

    AudioSource audio;

    private void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    public void PlaySound()
    {
        audio.PlayOneShot(audio.clip);

    }

}
