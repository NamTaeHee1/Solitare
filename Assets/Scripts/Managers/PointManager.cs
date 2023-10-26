using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour
{
	public static PointManager Instance
	{
		 get 
		 {
			if (_instance == null)
				_instance = FindObjectOfType<PointManager>();
			return _instance; 
		 }
	}

	private static PointManager _instance;

	public Point[] tableau;
	public Point[] foundation;
	public Point stock;
	public Point waste;
}
