using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nameplate : MonoBehaviour {
	public Vector3 offset = new Vector3(0, 2.0f, 0);
	public CharacterStatus status;
	TextMesh textMesh;

	// Use this for initialization
	void Start () {
		// コンポーネントのキャッシュ
		textMesh = GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		// 名前の更新
		if (textMesh.text != status.characterName){
			textMesh.text = status.characterName;
		}
		// 頭上に移動
		transform.position = status.transform.position + offset;
		// 常にカメラと同じ向きにする
		transform.rotation = Camera.main.transform.rotation;
		// 大きさ調整
		float scale = Camera.main.transform.InverseTransformPoint(transform.position).z / 30.0f;
		transform.localScale = Vector3.one * scale;
	}
}
