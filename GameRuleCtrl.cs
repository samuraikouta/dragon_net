using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRuleCtrl : MonoBehaviour {
	// 自分のコンピュータのプレイヤーゲームオブジェクト
	public GameObject player;
	// 実体化するプレイヤーのプレハブ
	public GameObject playerPrefab;
	// 開始位置
	public Transform startPoint;
	// FollowCameraスクリプト
	public FollowCamera followCamera;

	// 残り時間
	public float timeRemaining = 30.0f * 60.0f;
	// ゲームオーバーフラグ
	public bool gameOver = false;
	// ゲームクリア
	public bool gameClear = false;
	// シーン移行時間
	public float sceneChangeTime = 3.0f;

	// Update is called once per frame
	void Update()
	{
		// プレイヤーの発生
		if (player == null && (Network.isServer || Network.isClient)){
			Vector3 shiftVector = new Vector3(Network.connections.Length * 1.5f, 0, 0);
			player = Network.Instantiate(playerPrefab, startPoint.position + shiftVector, startPoint.rotation, 0) as GameObject;
			followCamera.SetTarget(player.transform);

			// 名前を送信する
			NetworkManager networkManager = FindObjectOfType(typeof(NetworkManager)) as NetworkManager;
			player.GetComponent<NetworkView>().RPC("SetName", RPCMode.AllBuffered, networkManager.GetPlayerName());
		}

		// ゲーム終了条件成立後、シーン遷移
		if (gameOver || gameClear){
			sceneChangeTime -= Time.deltaTime;
			if (sceneChangeTime <= 0.0f){
				Application.LoadLevel("TitleScene");
			}
			return;
		}

		// ゲームが始まったらタイマーを動かす
		if (Network.isServer || Network.isClient){
			timeRemaining -= Time.deltaTime;
			// 残り時間が無くなったらゲームオーバー
			if (timeRemaining <= 0.0f){
            	GameOver();
			}	
		}
	}

	// ゲームオーバー
	public void GameOver()
	{
		if (!gameOver && Network.isServer){
			GetComponent<NetworkView>().RPC("GameOverOnNetwork", RPCMode.AllBuffered);
		}
	}

	[RPC]
	void GameOverOnNetwork()
	{
		gameOver = true;
	}

	// ゲームクリア
	public void GameClear()
	{
		if (!gameClear && Network.isServer){
			GetComponent<NetworkView>().RPC("GameClearOnNetwork", RPCMode.AllBuffered);
		}
	}

	[RPC]
	void GameClearOnNetwork(){
		gameClear = true;
	}

	// 残り時間の設定
	[RPC]
	void SetRemainTime(float time)
	{
		timeRemaining = time;
	}

	// 他のプレイヤーが接続してきたら呼び出される
	// この関数が呼び出されるのはサーバーのコンピュータのみ
	void OnPlayerConnected(NetworkPlayer player)
	{
		GetComponent<NetworkView>().RPC("SetRemainTime", player, timeRemaining);
	}
}
