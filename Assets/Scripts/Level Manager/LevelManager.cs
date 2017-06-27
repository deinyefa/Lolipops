using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour {

	public static LevelManager instance;
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

	public void LoadGameScene () {
		SceneManager.LoadScene ("Game");
	}

	public void LoadMainMenuScene () {
		SceneManager.LoadScene ("MainMenu");
	}
}
