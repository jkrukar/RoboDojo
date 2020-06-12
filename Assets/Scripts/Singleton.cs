using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	static protected T _instance = null;
	static public T instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<T>();
			}
			return _instance;
		}
		private set
		{
			_instance = value;
		}
	}
}