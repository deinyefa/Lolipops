using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AlteredCandyInfo {

	private List<GameObject> newCandy;
	public int maxDistance { get; set;}

	public AlteredCandyInfo () {
		newCandy = new List<GameObject> ();
	}

	public IEnumerable<GameObject> AlteredCandy {
		get {
			return newCandy.Distinct ();
		}
	}

	public void AddCandy (GameObject go) {
		if (!newCandy.Contains (go)) {
			newCandy.Add (go);
		}
	}
}
