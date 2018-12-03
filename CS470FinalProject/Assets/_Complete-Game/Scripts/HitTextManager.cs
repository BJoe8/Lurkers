using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitTextManager : MonoBehaviour {

	public static float health;
	private static HitTextManager instance;
	public GameObject TextPrefab;
	public RectTransform canvasTransform;
	
	public float speed;
	public float fadeTime;
	public Vector3 direction;
	
	public static HitTextManager Instance
	{
		get 
		{
			if (instance == null) {
				instance = GameObject.FindObjectOfType<HitTextManager>();
			}
			return instance;
		}
	}
	
	public void CreateText(Vector3 position, string text, Color color) 
	{
		GameObject sct = (GameObject)Instantiate(TextPrefab, position, Quaternion.identity);
		sct.transform.SetParent(canvasTransform);
		sct.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
		sct.GetComponent<HitText>().Initialize(speed, direction, fadeTime);
		sct.GetComponent<Text>().text = text;
		sct.GetComponent<Text>().color = color;
		
	}
}
