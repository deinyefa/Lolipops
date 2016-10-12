using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class CandyArray {
	GameObject[,] candies = new GameObject[GameVariables.Rows, GameVariables.Columns];

	private GameObject backup1;
	private GameObject backup2;

	public GameObject this [int row, int column] {
		get {
			try {
				return candies [row, column];
			} catch (Exception e) {
				throw;
			}
		}

		set {
			candies [row, column] = value;
		}
	}

	public void Swap (GameObject g1, GameObject g2) {

		backup1 = g1;
		backup2 = g2;

		var g1Candy = g1.GetComponent<Candy> ();
		var g2Candy = g2.GetComponent<Candy> ();

		int g1Row = g1Candy.Row;
		int g1Column = g1Candy.Column;
		int g2Row = g2Candy.Row;
		int g2Column = g2Candy.Column;

		var temp = candies [g1Row, g1Column];
		candies [g1Row, g1Column] = candies [g2Row, g2Column];
		candies [g2Row, g2Column] = temp;

		Candy.SwapRowColumn (g1Candy, g2Candy);
	}

	public void UndoSwap () {
		Swap (backup1, backup2);
	}

	private IEnumerable <GameObject> GetMatchesHorizontally (GameObject go) {
		List<GameObject> matches = new List<GameObject> ();
		matches.Add (go);

		var candy = go.GetComponent<Candy> ();

		if (candy.Column != 0) {
			for (int column = candy.Column - 1; column >= 0; column--) {
				if (candies[candy.Row, column].GetComponent<Candy> ().IsSameType (candy)) {
					matches.Add (candies [candy.Row, column]);
				} else {
					break;
				}
			}
		} // search left

		if (candy.Column != GameVariables.Columns - 1) {
			for (int column = candy.Column + 1; column < GameVariables.Columns; column++) {
				if (candies[candy.Row, column].GetComponent<Candy> ().IsSameType (candy)) {
					matches.Add (candies [candy.Row, column]);
				} else {
					break;
				}
			}
		} // search right

		if (matches.Count < GameVariables.MinimumMatches) {
			matches.Clear ();
		}
		return matches.Distinct ();
	}

	private IEnumerable<GameObject> GetMatchesVertically (GameObject go) {
		List<GameObject> matches = new List<GameObject> ();
		matches.Add (go);

		var candy = go.GetComponent<Candy> ();

		if (candy.Row != 0) {
			for (int row = candy.Row - 1; row >= 0; row--) {
				if (candies[row, candy.Column].GetComponent<Candy> ().IsSameType (candy)) {
					matches.Add (candies [row, candy.Column]);
				} else {
					break;
				}
			}
		} // search bottom

		if (candy.Row != GameVariables.Rows - 1) {
			for (int row = candy.Row + 1; row < GameVariables.Rows; row++) {
				if (candies[row, candy.Column].GetComponent<Candy> ().IsSameType (candy)) {
					matches.Add (candies [row, candy.Column]);
				} else {
					break;
				}
			}
		} // search top

		if (matches.Count < GameVariables.MinimumMatches) {
			matches.Clear ();
		}
		return matches.Distinct ();
	}

	private bool ContainsDestroyWholeRowColumnBonus (IEnumerable<GameObject> matches) {
		if (matches.Count () >= GameVariables.MinimumMatches) {
			foreach (var item in matches) {
				if (BonusTypeChecker.ContainsDestroyWholeRowColumn (item.GetComponent<Candy> ().Bonus)) {
					return true;
				}
			}
		}
		return false;
	}

	private IEnumerable<GameObject> GetEntireRow (GameObject go) {
		List<GameObject> matches = new List<GameObject> ();
		int row = go.GetComponent<Candy> ().Row;
		for (int column = 0; column < GameVariables.Columns; column++) {
			matches.Add (candies [row, column]);
		}
		return matches;
	}

	private IEnumerable<GameObject> GetEntireColumn (GameObject go) {
		List<GameObject> matches = new List<GameObject> ();
		int column = go.GetComponent<Candy> ().Column;
		for (int row = 0; row < GameVariables.Rows; row++) {
			matches.Add (candies [row, column]);
		}
		return matches;
	}

	public void Remove (GameObject item) {
		candies [item.GetComponent<Candy> ().Row, item.GetComponent<Candy> ().Column] = null;
	}

	public AlteredCandyInfo Collapse (IEnumerable<int> columns) {
		AlteredCandyInfo collapseInfo = new AlteredCandyInfo ();

		foreach (var column in columns) {
			for (int row = 0; row < GameVariables.Rows - 1; row++) {
				if (candies[row, column] == null) {

					for (int row2 = row + 1; row2 < GameVariables.Rows; row2++) {
						if (candies[row2, column] != null) {

							candies [row, column] = candies [row2, column];
							candies [row2, column] = null;

							if (row2 - row > collapseInfo.maxDistance)
								collapseInfo.maxDistance = row2 - row;

							candies [row, column].GetComponent<Candy> ().Row = row;
							candies [row, column].GetComponent<Candy> ().Column = column;

							collapseInfo.AddCandy (candies[row, column]);
							break;
						}
					}
				}
			}
		}
		return collapseInfo;
	}

	public IEnumerable<CandyInfo> GetEmptyItemsOnColumn(int column) {
		List<CandyInfo> emptyItems = new List<CandyInfo> ();

		for (int row = 0; row < GameVariables.Rows; row++) {
			if (candies[row, column] == null) {
				emptyItems.Add (new CandyInfo {Row = row, Column = column});
			}
		}
		return emptyItems;
	}

	public MatchesInfo GetMatches (GameObject go) {
		MatchesInfo matchesInfo = new MatchesInfo ();

		var horizontalMatches = GetMatchesHorizontally (go);
		if (ContainsDestroyWholeRowColumnBonus (horizontalMatches)) {
			horizontalMatches = GetEntireRow (go);

			if (!BonusTypeChecker.ContainsDestroyWholeRowColumn (matchesInfo.BonusesContained)) {
				matchesInfo.BonusesContained = BonusType.DestroyWholeRowColumn;
			}
		}
		matchesInfo.AddObjectRange (horizontalMatches); 

		var verticalMatches = GetMatchesVertically (go);
		if (ContainsDestroyWholeRowColumnBonus (verticalMatches)) {
			verticalMatches = GetEntireColumn (go);

			if (!BonusTypeChecker.ContainsDestroyWholeRowColumn (matchesInfo.BonusesContained)) {
				matchesInfo.BonusesContained = BonusType.DestroyWholeRowColumn;
			}
		}
		matchesInfo.AddObjectRange (verticalMatches); 

		return matchesInfo;
	}

	public IEnumerable<GameObject> GetMatches (IEnumerable<GameObject> gos) {
		List<GameObject> matches = new List<GameObject> ();

		foreach (var go in gos) {
			matches.AddRange (GetMatches (go).MatchedCandy); 
		}
		return matches.Distinct ();
	}

} // CandyArray
