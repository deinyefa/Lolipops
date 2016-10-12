using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MatchesInfo {

	private List<GameObject> matches;
	public BonusType BonusesContained { get; set;}

	public MatchesInfo () {
		matches = new List<GameObject> ();
		BonusesContained = BonusType.None;
	}

	public IEnumerable<GameObject> MatchedCandy {
		get {
			return matches.Distinct ();
		}
	}

	public void AddObject (GameObject go) {
		if (!matches.Contains (go)) {
			matches.Add (go);
		}
	}

	public void AddObjectRange (IEnumerable<GameObject> gos) {
		foreach (var item in gos) {
			AddObject (item);
		}
	}
}
