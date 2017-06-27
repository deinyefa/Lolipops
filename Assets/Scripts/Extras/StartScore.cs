using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartScore : MonoBehaviour {

	private Text scoreText;

	void Start () {
		scoreText = GetComponent<Text> ();
		scoreText.text = LevelManager.instance.score.ToString ();
	}
}
