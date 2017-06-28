using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour {

	public void LoadGameScene () {
		SceneManager.LoadScene ("Game");
	}

	public void LoadMainMenuScene () {
		SceneManager.LoadScene ("MainMenu");
	}
}
