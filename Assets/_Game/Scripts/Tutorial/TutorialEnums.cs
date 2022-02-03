namespace _Game.Scripts.Tutorial
{
	public enum TutorialElementType
	{
		None,
		Soft,
	}

	public enum TutorialTipPos
	{
		Regular
	}

	public enum TutorialBackState
	{
		Inactive,
		Active,
		Transparent
	}

	public enum TutorialNextStepCondition
	{
		None,
		Tap,
		PressedButton,
		FinishedTask,
		Action,
		UIOpened,
		UIClosed,
		GoToNextStep
	}

	public enum TutorialCustomAction
	{
		None,
		FocusBuilding
	}
}