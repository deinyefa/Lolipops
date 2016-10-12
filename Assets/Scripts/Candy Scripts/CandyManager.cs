using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class CandyManager : MonoBehaviour {

	public Text scoreText;

	private int score;

	public CandyArray candies;

	private Vector2 BottomRight = new Vector2 (-2.37f, -4.27f);
	private Vector2 CandySize = new Vector2 (0.7f, 0.7f);

	private GameState state = GameState.None;
	private GameObject hitGo = null;

	private Vector2[] SpawnPositions;

	public GameObject[] CandyPrefabs;
	public GameObject[] ExplosionPrefabs;
	public GameObject[] BonusPrefabs;

	private IEnumerator CheckPotentialMatchesCoroutine;
	private IEnumerator AnimatePotentialMatchesCoroutine;

	IEnumerable<GameObject> potentialMatches;

	public SoundManager soundManager;

	void Start () {
		InitializeTypesOnPrefabShapesAndBonuses ();
		InitializeCandyAndSpawnPositions ();
		StartCheckForPotentialMatches ();
	}
	
	void Update () {
		if (state == GameState.None) {
			if (Input.GetMouseButtonDown (0)) {
				var hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero);
				if (hit.collider != null) {
					hitGo = hit.collider.gameObject;
					state = GameState.SelectionStarted;
				}
			}
		} else if (state == GameState.SelectionStarted) {
			if (Input.GetMouseButtonDown (0)) {
				var hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero);

				if (hit.collider != null && hitGo != hit.collider.gameObject) {
					StopCheckForPotentialMatches ();

					if (!MatchChecker.AreHorizontalOrVerticalNeigbhours (hitGo.GetComponent<Candy>(), 
						hit.collider.gameObject.GetComponent<Candy>())) {

						state = GameState.None;
					} else {
						state = GameState.Animating;
						FixSortingLayer (hitGo,hit.collider.gameObject);
						StartCoroutine (FindMatchesAndCollapse (hit));
					}
				} 
			}
		}
	}

	private void InitializeVariables () {
		score = 0; 
		ShowScore ();
	}

	private void ShowScore () {
		scoreText.text = "Score " + score.ToString ();
	}

	private void IncreaseScore (int amount) {
		score += amount;
		ShowScore ();
	}

	private void DestroyAllCandy () {
		for (int row = 0; row < GameVariables.Rows; row++) {
			for (int column = 0; column < GameVariables.Columns; column++) {
				Destroy (candies[row, column]);
			}
		}
	}

	private void InitializeTypesOnPrefabShapesAndBonuses () {
		foreach (var item in CandyPrefabs) {
			item.GetComponent<Candy> ().Type = item.name;
		}

		for (int i = 0; i < BonusPrefabs.Length; i++) {
			BonusPrefabs [i].GetComponent<Candy> ().Type = CandyPrefabs [i].name;
		}
	}

	private void InstantiateAndPlaceNewCandy (int row, int column, GameObject newCandy) {
		GameObject go = Instantiate (newCandy, 
			                BottomRight + new Vector2 (column * CandySize.x, row * CandySize.y), 
			                Quaternion.identity) as GameObject;

		go.GetComponent<Candy> ().Initialize (newCandy.GetComponent<Candy> ().Type, row, column);
		candies [row, column] = go;
	}

	private void SetupSpawnPositions () {
		for (int column = 0; column < GameVariables.Columns; column++) {
			SpawnPositions [column] = BottomRight +
			new Vector2 (column * CandySize.x, GameVariables.Rows * CandySize.y);
		}
	}

	private GameObject GetRandomCandy () {
		return CandyPrefabs [Random.Range (0, CandyPrefabs.Length)];
	}

	private GameObject GetRandomExplosion () {
		return ExplosionPrefabs [Random.Range (0, ExplosionPrefabs.Length)];
	}

	public void InitializeCandyAndSpawnPositions () {
		InitializeVariables ();

		if (candies != null)
			DestroyAllCandy ();

		candies = new CandyArray ();

		SpawnPositions = new Vector2[GameVariables.Columns];

		for (int row = 0; row < GameVariables.Rows; row++) {
			for (int column = 0; column < GameVariables.Columns; column++) {
				GameObject newCandy = GetRandomCandy ();

				//- Checking if previous 2 horizontal candies are the same
				while (column >=2 && candies[row, column - 1].GetComponent<Candy>().IsSameType (newCandy.GetComponent<Candy>())
					&& candies[row, column - 2].GetComponent<Candy>().IsSameType (newCandy.GetComponent<Candy>())) {

					newCandy = GetRandomCandy ();
				}

				//- Checking if previous 2 vertical candies are the same
				while (row >= 2 && candies [row - 1, column].GetComponent<Candy> ().IsSameType (newCandy.GetComponent<Candy> ())
				       && candies [row - 2, column].GetComponent<Candy> ().IsSameType (newCandy.GetComponent<Candy> ())) {

					newCandy = GetRandomCandy ();
				}
				InstantiateAndPlaceNewCandy (row, column, newCandy);
			}
		}
		SetupSpawnPositions ();
	}

	private void StartCheckForPotentialMatches () {
		StopCheckForPotentialMatches ();

		CheckPotentialMatchesCoroutine = CheckPotentialMatches ();
		StartCoroutine (CheckPotentialMatchesCoroutine);
	}

	private void StopCheckForPotentialMatches () {
		if (AnimatePotentialMatchesCoroutine != null)
			StopCoroutine (AnimatePotentialMatchesCoroutine);

		if (CheckPotentialMatchesCoroutine != null)
			StopCoroutine (CheckPotentialMatchesCoroutine);

		ResetOpacityOnPotentialMatches ();
	}

	private void ResetOpacityOnPotentialMatches () {
		if (potentialMatches != null) {
			foreach (var item in potentialMatches) {
				if (item == null)
					break;

				Color c = item.GetComponent<SpriteRenderer> ().color;
				c.a = 1f;
				item.GetComponent<SpriteRenderer> ().color = c;
			}
		}
	}

	private IEnumerator CheckPotentialMatches () {
		yield return new WaitForSeconds (GameVariables.WaitBeforePotentialMatchsCheck);

		potentialMatches = MatchChecker.GetPotentialMatches (candies);

		if (potentialMatches != null) {
			while (true) {
				AnimatePotentialMatchesCoroutine = MatchChecker.AnimatePotentialMatches (potentialMatches);
				StartCoroutine (AnimatePotentialMatchesCoroutine);
				yield return new WaitForSeconds (GameVariables.WaitBeforePotentialMatchsCheck);
			}
		}
	}

	private void FixSortingLayer (GameObject hitGo, GameObject hitGo2) {
		SpriteRenderer sp1 = hitGo.GetComponent<SpriteRenderer> ();
		SpriteRenderer sp2 = hitGo2.GetComponent<SpriteRenderer> ();

		if (sp1.sortingOrder <= sp2.sortingOrder) {
			sp1.sortingOrder = 1;
			sp2.sortingOrder = 0;
		}
	}

	private void RemoveFromScene (GameObject item) {
		var explosion = Instantiate (GetRandomExplosion (), item.transform.position, Quaternion.identity) as GameObject;
		Destroy (explosion, GameVariables.ExplosionAnimationDuration);
		Destroy (item); 
	}

	private GameObject GetBonusFromType (string type) {
		string color = type.Split ('_') [1].Trim ();

		string s = "green";

		foreach (var item in BonusPrefabs) {
			if (item.GetComponent<Candy>().Type.Contains (color)) {
				return item;
			}
		}
		throw new System.Exception ("you passed the wrong type");
	}

	private void CreateBonus (Candy hitGoCache) {
		GameObject Bonus = Instantiate (GetBonusFromType (hitGoCache.Type), BottomRight +
		                   new Vector2 (hitGoCache.Column * CandySize.x, hitGoCache.Row * CandySize.y),
			                   Quaternion.identity) as GameObject;

		candies [hitGoCache.Row, hitGoCache.Column] = Bonus;

		var bonusCandy = Bonus.GetComponent<Candy> ();

		bonusCandy.Initialize (hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
		bonusCandy.Bonus = BonusType.DestroyWholeRowColumn;
	}

	private AlteredCandyInfo CreateNewCandyInSpecificColumns (IEnumerable<int> columnsWithMissingCandies) {
		AlteredCandyInfo newCandyInfo = new AlteredCandyInfo ();

		foreach (int column in columnsWithMissingCandies) {
			var emptyItems = candies.GetEmptyItemsOnColumn (column);

			foreach (var item in emptyItems) {
				var go = GetRandomCandy ();

				GameObject newCandy = Instantiate (go, SpawnPositions [column], Quaternion.identity) as GameObject;
				newCandy.GetComponent<Candy> ().Initialize (go.GetComponent<Candy> ().Type, item.Row, item.Column);

				if (GameVariables.Rows - item.Row > newCandyInfo.maxDistance)
					newCandyInfo.maxDistance = GameVariables.Rows - item.Row;

				candies [item.Row, item.Column] = newCandy;
				newCandyInfo.AddCandy (newCandy);
			}
		}
		return newCandyInfo;
	}

	private void MoveAndAnimate (IEnumerable<GameObject> movedGameObjects, int distance) {
		foreach (var item in movedGameObjects) {
			item.transform.positionTo (GameVariables.MoveAnimationDuration * distance, BottomRight
			+ new Vector2 (item.GetComponent<Candy> ().Column * CandySize.x,
				item.GetComponent<Candy> ().Row * CandySize.y));
		}
	}

	private IEnumerator FindMatchesAndCollapse (RaycastHit2D hit2) {
		var hitGo2 = hit2.collider.gameObject;
		candies.Swap (hitGo, hitGo2);

		hitGo.transform.positionTo (GameVariables.AnimationDuration, hit2.transform.position);
		hitGo2.transform.positionTo (GameVariables.AnimationDuration, hitGo.transform.position);
		yield return new WaitForSeconds (GameVariables.AnimationDuration);

		var hitGoMatchesInfo = candies.GetMatches (hitGo);
		var hitGo2MatchesInfo = candies.GetMatches (hitGo2);

		var totalMatches = hitGoMatchesInfo.MatchedCandy.Union (hitGo2MatchesInfo.MatchedCandy).Distinct ();

		if (totalMatches.Count () < GameVariables.MinimumMatches) {
			hitGo.transform.positionTo (GameVariables.AnimationDuration, hitGo2.transform.position);
			hitGo2.transform.localPositionTo (GameVariables.AnimationDuration, hitGo.transform.position);
			yield return new WaitForSeconds (GameVariables.AnimationDuration);

			candies.UndoSwap ();
		}
		bool addBonus = totalMatches.Count () >= GameVariables.MinimumMatchesForBonus &&
		                !BonusTypeChecker.ContainsDestroyWholeRowColumn (hitGoMatchesInfo.BonusesContained) &&
		                !BonusTypeChecker.ContainsDestroyWholeRowColumn (hitGo2MatchesInfo.BonusesContained);

		Candy hitGoCache = null;
		if (addBonus) {
			hitGoCache = new Candy ();

			var sameTypeGo = hitGoMatchesInfo.MatchedCandy.Count () > 0 ? hitGo : hitGo2;
			var candy = sameTypeGo.GetComponent<Candy> ();

			hitGoCache.Initialize (candy.Type, candy.Row, candy.Column);
		}
		int timesRun = 1;
		while (totalMatches.Count() >= GameVariables.MinimumMatches) {
			IncreaseScore (totalMatches.Count () - 2 * GameVariables.Match3Score);

			if (timesRun >= 2)
				IncreaseScore (GameVariables.SubsequelMatchScore);

			soundManager.PlaySound ();

			foreach (var item in totalMatches) {
				candies.Remove (item);
				RemoveFromScene (item);
			}

			if (addBonus) {
				CreateBonus (hitGoCache);
			}
			addBonus = false;

			var columns = totalMatches.Select (go => go.GetComponent<Candy> ().Column).Distinct ();
			var collapseCandyInfo = candies.Collapse (columns);
			var newCandyInfo = CreateNewCandyInSpecificColumns (columns);
			int maxDistance = Mathf.Max (collapseCandyInfo.maxDistance, newCandyInfo.maxDistance);

			MoveAndAnimate (newCandyInfo.AlteredCandy, maxDistance);
			MoveAndAnimate (collapseCandyInfo.AlteredCandy, maxDistance);

			yield return new WaitForSeconds (GameVariables.AnimationDuration * maxDistance);

			totalMatches = candies.GetMatches (collapseCandyInfo.AlteredCandy)
				.Union (candies.GetMatches (newCandyInfo.AlteredCandy)).Distinct ();

			timesRun++;
		}
		state = GameState.None;
		StartCheckForPotentialMatches ();
	}

} // CandyManager
