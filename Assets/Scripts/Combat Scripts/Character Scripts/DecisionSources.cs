public sealed class PlayerDecisionSource : ICharacterDecision
{
	public void Decide(Character character)
	{
		character.combat.RequestPlayerInput();
	}
}
public sealed class AIDecisionSource : ICharacterDecision
{
	public void Decide(Character character)
	{
		if(!character.stats.GetDizzy())
		{
			Choices choice = DecideChoice( character );
			character.combat.SetChoiceServerRpc( choice );
		}
	}

	private Choices DecideChoice(Character character)
	{
		// placeholder logic
		// replace with heuristics later
		return Choices.Attack;
	}
}
