using GeneralTools;

namespace _Game.Scripts.View.Animations
{
    public class AnimationEventsSender : BaseBehaviour, IAnimationEventsSender
    {
        private IAnimationEventsListener _listener;
        
        public void AssignListener(IAnimationEventsListener listener)
        {
            _listener = listener;
        }

        public void AddEvent()
        {
        }

        public void MeleeAttack()
        {
            _listener.ExecuteEvent(AnimationEventType.MeleeAttack);
        }

        public void Reload()
        {
            _listener.ExecuteEvent(AnimationEventType.Reload);
        }
        
        public void AnimationEnd()
        {
            _listener.ExecuteEvent(AnimationEventType.AnimationEnd);
        }
        
        public void Shoot()
        {
            _listener.ExecuteEvent(AnimationEventType.Shoot);
        }

        public void KickDoor()
        {
            _listener.ExecuteEvent(AnimationEventType.DoorKick);
        }

        public void ThrowGrenade()
        {
            _listener.ExecuteEvent(AnimationEventType.ThrowGrenade);
        }
    }
}