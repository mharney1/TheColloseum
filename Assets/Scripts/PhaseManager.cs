using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PhaseManager : NetworkBehaviour
{
	public static PhaseManager Instance
	{
		get; private set;
	}

	private float _startTime = 15f;
	private float _shortenedTime = 5f;
	[SerializeField] private TextMeshProUGUI _prepareTimerText;
	[SerializeField] private GameObject _combatManagerPrefab;
	private readonly List<Character> _undecidedCharacters = new();
	private bool _winCondition = false;
	private NetworkVariable<float> _timer = new NetworkVariable<float>( 0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	public NetworkVariable<Phases> CurrentPhase = new( Phases.Load, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );

	public void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy( gameObject );
			return;
		}
	}
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		Instance = this;
		_timer.OnValueChanged += OnTimerChanged;

		if (IsServer)
		{
			StartCoroutine( GameLoop() );
		}
	}

	public override void OnNetworkDespawn()
	{
		_timer.OnValueChanged -= OnTimerChanged;
	}

	private IEnumerator GameLoop()
	{
		yield return LoadPhase();
		while (!_winCondition)
		{
			yield return PreparePhase();
			yield return ActionPhase();
			yield return ResolvePhase();
		}
		yield return EndPhase();
	}

	private IEnumerator LoadPhase()
	{
		CurrentPhase.Value = Phases.Load;
		Instantiate( _combatManagerPrefab );
		CombatResolver.Initialize();
		yield return new WaitUntil( () => PlayerManager.Instance != null );
		yield return new WaitUntil( () => PairManager.Instance != null );
		yield return new WaitUntil( () => PlayerManager.Instance.AllPlayersRegistered( ) );
		yield return StartCoroutine( PairManager.Instance.CreatePossiblePairs() );
		yield return new WaitUntil( () => CombatManager.Instance != null );
	}
	private IEnumerator PreparePhase()
	{
		WinManager.Instance.GetLastAlive();
		_undecidedCharacters.Clear();
		PairManager.Instance.GetCombatants();
		CurrentPhase.Value = Phases.Prepare;
		float serverTime = _startTime;
		_timer.Value = _startTime;
		SetPrepareTimerActiveClientRpc( true );


		while (_timer.Value > 0f)
		{
			if (_undecidedCharacters.Count == 0 && serverTime > _shortenedTime)
			{
				serverTime = _shortenedTime;
			}

			serverTime = Mathf.Clamp( serverTime - Time.deltaTime, 0f, _startTime );

			if (Mathf.Floor( _timer.Value ) != Mathf.Floor( serverTime ))
			{
				_timer.Value = Mathf.Max( 0f, Mathf.Floor( serverTime ) );
			}
			yield return null;
		}
		DefaultChoice();
		SetPrepareTimerActiveClientRpc( false );
	}
	private IEnumerator ActionPhase()
	{
		CurrentPhase.Value = Phases.Action;
		yield return StartCoroutine( CombatManager.Instance.ResolveAllPairs() );
	}
	private IEnumerator ResolvePhase()
	{
		CurrentPhase.Value = Phases.Resolve;
		yield return StartCoroutine( PairManager.Instance.CreatePossiblePairs() );
		_winCondition = WinManager.Instance.CheckWinConditions();
	}
	private IEnumerator EndPhase()
	{
		Debug.Log( "EndPhase has begun" );
		CurrentPhase.Value = Phases.End;
		WinManager.Instance.FindWinners();
		yield return null;
	}
	public Phases GetCurrentPhase()
	{
		return CurrentPhase.Value;
	}
	private void DefaultChoice()
	{
		while (_undecidedCharacters.Count > 0)
		{
			_undecidedCharacters [ 0 ].combat.SetChoiceServerRpc( Choices.Attack );
		}
	}
	public void AddUndecided(Character player)
	{
		if (!_undecidedCharacters.Contains( player ))
		{
			_undecidedCharacters.Add( player );
		}
	}
	public void RemoveUndecided(Character player)
	{
		_undecidedCharacters.Remove( player );
	}
	private void OnTimerChanged(float oldVal, float newVal)
	{
		if (_prepareTimerText != null)
		{
			_prepareTimerText.text = Mathf.CeilToInt( newVal ).ToString( "00" );
		}
	}
	[ClientRpc]
	private void SetPrepareTimerActiveClientRpc(bool isActive)
	{
		_prepareTimerText?.transform.parent.gameObject.SetActive( isActive );
	}
}
