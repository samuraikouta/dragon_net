using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour {
	public enum ItemKind
	{
		Attack,
		Heal,
	};
	public ItemKind kind;

	public AudioClip itemSeClip;

	// 拾われたフラグ
	bool isPickedUp = false;

	void OnTriggerEnter(Collider other)
	{
		// Playerか判定
		if (other.tag == "Player")
		{
			// アイテム取得
			CharacterStatus aStatus = other.GetComponent<CharacterStatus>();
			aStatus.GetItem(kind);
			// オーディオ再生
			AudioSource.PlayClipAtPoint(itemSeClip, transform.position);
			// アイテム取得をオーナーへ通知する
			PlayerCtrl playerCtrl = other.GetComponent<PlayerCtrl>();
			if (playerCtrl.GetComponent<NetworkView>().isMine)
			{
				if (GetComponent<NetworkView>().isMine)
				{
					GetItemOnNetwork(playerCtrl.GetComponent<NetworkView>().viewID);
				}
				else
				{
					GetComponent<NetworkView>().RPC("GetItemOnNetwork", GetComponent<NetworkView>().owner, playerCtrl.GetComponent<NetworkView>().viewID);
				}
			}
		}
	}

	[RPC]
	// アイテム取得処理
	void GetItemOnNetwork(NetworkViewID viewId)
	{
		// 拾われたフラグ
		if(isPickedUp){
			return;
		}
		isPickedUp = true;

		// 拾ったPlayerを探す
		NetworkView player = NetworkView.Find(viewId);
		if (player == null){
			return;
		}

		// 拾ったPlayerにアイテムを与える
		if (player.isMine){
			player.SendMessage("GetItem", kind);
		} else {
			player.GetComponent<NetworkView>().RPC("GetItem", player.owner, kind);

		}

		Network.Destroy(gameObject);
		Network.RemoveRPCs(GetComponent<NetworkView>().viewID);
	}

	void OnNetworkInstantiate(NetworkMessageInfo info){
		if (!GetComponent<NetworkView>().isMine){
			Destroy(GetComponent<Rigidbody>());
		}
	}

	// Use this for initialization
	void Start () {
		Vector3 velocity = Random.insideUnitSphere * 2.0f + Vector3.up * 8.0f;
		GetComponent<Rigidbody>().velocity = velocity;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
