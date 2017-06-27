using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour {

	private Slider silderObject;
	public Image fillimg;

	public float timeLimitInSeconds = 300;
	private float timeLeft;

	void Start () {
		silderObject = GetComponent<Slider> ();

		timeLeft = timeLimitInSeconds;
		silderObject.maxValue = timeLimitInSeconds;
	}
	
	void Update () {
		timeLeft -= Time.deltaTime;
		silderObject.value = timeLeft;
	}

	public void HalfwayThroughPlusMore (float value) {
		if (value <= timeLimitInSeconds / 2) {
			fillimg.color = Color.yellow;
		}
		if (value <= (timeLimitInSeconds / 4)) {
			fillimg.color = Color.red;
		}

		if (value <= 0)
			SceneManager.LoadScene ("MainMenu");
	}
}
