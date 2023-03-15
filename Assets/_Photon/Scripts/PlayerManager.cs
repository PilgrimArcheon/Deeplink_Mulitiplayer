using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
	PhotonView PV;

	public bool HasHitFinish = false;

	GameObject controller;

	void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	void Start()
	{
		if(PV.IsMine)
		{
			CreateController();
		}
	}

	void CreateController()
	{
		Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
		controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
		Destroy(spawnpoint.gameObject);
	}

	public void Die()
	{
		PhotonNetwork.Destroy(controller);
		CreateController();
	}

	public void CheckFinish()
	{
		if(PV.IsMine)
		{
			if(!HasHitFinish)
			{
				//Show Eliminated
				GameObject showEl = GameObject.FindGameObjectWithTag("Eliminated");
				showEl.transform.GetChild(0).gameObject.SetActive(true);
				controller.SetActive(false);
				RoomManager.Instance.ResetAll();
				Invoke("LeaveRoom", 1f);
			}
		}
	}

	void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene(1);
	}

	public void Finished()
	{
		if(PV.IsMine)
		{
			//Show Qualified
			if(HasHitFinish)
			{
				GameObject showEl = GameObject.FindGameObjectWithTag("Qualified");
				showEl.transform.GetChild(0).gameObject.SetActive(true);
				controller.gameObject.SetActive(false);
			}
		}
	}
}