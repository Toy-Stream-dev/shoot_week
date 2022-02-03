namespace _Game.Scripts.View.Animations
{
	public interface IAnimationEventsSender
	{
		void AssignListener(IAnimationEventsListener listener);
		void AddEvent();
	}
}