using System;

namespace GeneralTools.Tools
{
    public static class AppEventsProvider
    {
        public delegate void ActionCallback(Enum type, object[] parameters);
        public static (Enum EventType, object[] Parameters) LastEvent { get; private set; }
        public static event ActionCallback Action;

        public static void TriggerEvent(Enum action, params object[] list)
        {
            LastEvent = (action, list);
            Action?.Invoke(action, list);
        }
    }
}