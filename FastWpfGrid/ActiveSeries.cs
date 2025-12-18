using System.Collections.Generic;

namespace FastWpfGrid;

public class ActiveSeries {
	public HashSet<int> ScrollVisible = [];
	public HashSet<int> Selected = [];
	public HashSet<int> Frozen = [];
}
