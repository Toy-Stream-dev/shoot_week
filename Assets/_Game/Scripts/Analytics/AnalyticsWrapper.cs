using System;
using System.Collections.Generic;
using System.Linq;

namespace _Game.Scripts.Analytics
{
    public abstract class AnalyticsWrapper
    {
        public abstract void SendEvent(string eventType, params (string, object)[] args);

        public abstract void Init();

        protected Dictionary<string, object> ToDictionary(params (string, object)[] args)
        {
            return args.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        public virtual void CustomEventProcess(Enum eventType, params object[] parameters)
        {
        }
    }
}