using UnityEngine.TextCore.Text;

public interface ICharacterDecision
{
	void Decide(Character character);
}
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
			Choices choice = DecideChoice();
			character.combat.SetChoiceServerRpc( choice );
		}
	}

	private Choices DecideChoice()
	{
		// placeholder logic
		return Choices.Attack;
	}
}
