using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	public float radius = 1;
	public Vector2 regionSize = Vector2.one;
	public int rejectionSamples = 30;
	public float displayRadius =1;
    GridGen grid;
	List<Vector2> points;
    private void Start()
    {
        grid = FindObjectOfType<GridGen>();
        regionSize.x = grid.originCorner.x;
        regionSize.y = grid.farCorner.y;
    }
    void OnValidate() {
		points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube(regionSize/2,regionSize);
		if (points != null) {
			foreach (Vector2 point in points) {
				Gizmos.DrawSphere(point, displayRadius);
			}
		}
	}
}
