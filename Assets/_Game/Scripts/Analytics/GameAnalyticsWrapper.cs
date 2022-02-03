using System;
using GameAnalyticsSDK;

namespace _Game.Scripts.Analytics
{
    public class GameAnalyticsWrapper : AnalyticsWrapper
    {
        private BaseTenjin _instance;
        
        public override void Init()
        {
            GameAnalytics.Initialize();
            
            TenjinConnect();
        }

        public void TenjinConnect() 
        {
            _instance = Tenjin.getInstance("C1VYE96SJFZIOAGY7NTS96SFAQGXWEHY");
        
            // Sends install/open event to Tenjin
            _instance.Connect();
        }

        public override void SendEvent(string eventType, params (string, object)[] args)
        {
        }

        public override void CustomEventProcess(Enum eventType, params object[] parameters)
        {
            if (!(eventType is GameEvents metaEvents)) return;
            
            // var id = parameters[0].ToString();
            // while (id.Length < 4)
            // {
            //     id = "0" + id;
            // }
            
            switch (metaEvents)
            {
                case GameEvents.LevelStart:
                    _instance.SendEvent("level_start");
                    break;
            }
        }
        
    }
}