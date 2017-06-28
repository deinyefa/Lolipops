using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreHolder : MonoBehaviour {

	public static ScoreHolder instance;
	[HideInInspector]
	public int score;

	private CandyManager candyManager;

	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
		DontDestroyOnLoad (gameObject);
	}

	void Update () {
		if (SceneManager.GetActiveScene ().name == "Game") {
			candyManager = GameObject.FindObjectOfType<CandyManager> ();
			score = candyManager.score;
		}
	}
}
