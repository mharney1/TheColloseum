using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CharacterStats : NetworkBehaviour
{

	private int _maxHealth = 550;
	private int _minHealth;

	private NetworkVariable<int> _health = new( 0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private NetworkVariable<float> _exhaustion = new( 0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private NetworkVariable<bool> _dizzy = new( false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		_minHealth = Mathf.CeilToInt( _maxHealth * 0.05f );

		if (IsServer)
		{
			_health.Value = _maxHealth;
		}
	}
	public int GetMaxHealth()
	{
		return _maxHealth;
	}
	public int GetHealth()
	{
		return _health.Value;
	}
	public float GetExhaustion()
	{
		return _exhaustion.Value;
	}
	public bool GetDizzy()
	{
		return _dizzy.Value;
	}
	public void ModifyStats(int healthDelta, float exhaustionDelta, bool dizzy)
	{
		if (IsServer)
		{
			_health.Value = Mathf.Clamp( _health.Value + healthDelta, _minHealth, _maxHealth );
			_exhaustion.Value = Mathf.Clamp01( _exhaustion.Value + exhaustionDelta );
			_dizzy.Value = dizzy;
		}
	}
	public bool IsDefeated()
	{
		return _health.Value == _minHealth;
	}
}
