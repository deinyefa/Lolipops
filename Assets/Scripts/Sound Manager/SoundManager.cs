using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	AudioSource audio;

	void Awake () {
		audio = GetComponent<AudioSource> ();
	}

	public void PlaySound () {
		audio.Play ();
	}
}
