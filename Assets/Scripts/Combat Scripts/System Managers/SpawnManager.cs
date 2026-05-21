using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
	public static SpawnManager Instance
	{
		get; private set;
	}

	[Header( "Spawn Settings" )]
	[SerializeField] private GameObject _playerPrefab;
	[SerializeField] private Transform [] _spawnPoints;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (!IsServer)
			return;

		SpawnFromSession();
	}

	public void SpawnFromSession()
	{
		if (!IsServer)
			return;

		var players = GameSession.S_INSTANCE.players;

		for (int i = 0; i < players.Count; i++)
		{
			SpawnPlayer(players [ i ], i);
		}
	}

	private void SpawnPlayer(Player player, int index)
	{

		var spawnPoint = _spawnPoints [ index % _spawnPoints.Length ];

		GameObject obj = Instantiate(
			_playerPrefab,
			spawnPoint.position,
			spawnPoint.rotation
		);


		var netObj = obj.GetComponent<NetworkObject>();
		var character = obj.GetComponent<Character>();

		if (player.Identity.GetAI())
		{
			character.identity.SetAI(true);
			netObj.Spawn();
		}
		else
		{
			character.identity.SetAI(false);
			netObj.SpawnAsPlayerObject(player.Identity.GetClientID());
		}

		character.identity.SetCharacterID(index);
		character.identity.SetTeam(player.Identity.GetTeam());
	}
}
