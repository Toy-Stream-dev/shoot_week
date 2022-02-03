using System;
using System.Collections.Generic;
using _Game.Scripts.Ad;
using GeneralTools.Model;
using GeneralTools.Tools;

namespace _Game.Scripts.Analytics
{
    public struct InAppPurchaseData
    {
        public string Currency,
            Revenue,
            Quantity,
            ID,
            Transaction;
    }
    
    public class AnalyticsModel : BaseModel
    {
        private List<AnalyticsWrapper> _analyticsWrappers;
        private bool _isFirstSession;

        public override BaseModel Start()
        {
#if UNITY_EDITOR
            //return this;
#endif
            AppEventsProvider.Action += OnAction;

            _analyticsWrappers = new List<AnalyticsWrapper>()
            {
                // new AppMetricAnalyticsWrapper(),
                // new AppsFlyerAnalyticsWrapper()
                new GoogleAnalyticsWrapper(),
                new GameAnalyticsWrapper(),
            };

            foreach (var analyticsWrapper in _analyticsWrappers) analyticsWrapper.Init();

            return this;
        }
        
        private void OnAction(Enum eventType, object[] parameters)
        {
            if (eventType is GameEvents metaEvent)
            {
                switch (metaEvent)
                {
                    case GameEvents.VideoAdStarted:
                        SendVideoAdStarted((AdType)parameters[0]);
                        break;
                    
                    case GameEvents.VideoAdShowed:
                        SendVideoAdShowed((AdType)parameters[0]);
                        break;
                    
                    case GameEvents.VideoAdError:
                        SendVideoAdError((AdType)parameters[0]);
                        break;
                    
                    case GameEvents.PlayTime:
                        SendPlayTime(parameters);
                        break;
            
                    case GameEvents.Timer:
                        SendTimer(parameters);
                        break;
                    
                    case GameEvents.Timer24:
                        SendTimer24(parameters);
                        break;
                    
                    case GameEvents.Rate_us:
                        SendRateGame(parameters);
                        break;
                }
            }
            
            foreach (var wrapper in _analyticsWrappers)
            {
                wrapper.CustomEventProcess(eventType, parameters);
            }
        }

        private void SendVideoAdStarted(AdType adType)
        {
            SendEvent("video_ads_started",
                ("ad_type", "rewarded"),
                ("placement", adType.ToString()),
                ("result", true),
                ("connection", true));
        }
        
        private void SendVideoAdShowed(AdType type)
        {
            SendEvent("video_ads_watch",
                ("ad_type", "rewarded"),
                ("placement", type.ToString()),
                ("result", true),
                ("connection", true));
        }
        
        private void SendVideoAdError(AdType adType)
        {
            SendEvent("video_ads_available",
                ("ad_type", "rewarded"),
                ("placement", adType.ToString()),
                ("result", true),
                ("connection", true));
        }
        
        private void SendPlayTime(object[] parameters)
        {
            if (parameters.Length == 0 || !(parameters[0] is int minute))
            {
                return;
            }
        
            SendEvent(GetEventName(GameEvents.PlayTime),
                ("firstSession", _isFirstSession),
                ("time", minute));
        }
        
        private void SendTimer(object[] parameters)
        {
            SendEvent(GetEventName(GameEvents.Timer), 
                ("time", parameters[0].ToString()), 
                ("session", parameters[1].ToString()));
        }
        
        private void SendTimer24(object[] parameters)
        {
            SendEvent(GetEventName(GameEvents.Timer), 
                ("24h_count ", parameters[0].ToString()),
                ("24h_time", parameters[1].ToString()),
                ("24h_sessions ", parameters[2].ToString()));
        }
        
        private void SendRateGame(object[] parameters)
        {
            SendEvent("RateGame", ("Stars", parameters[0].ToString().ToLower()));
        }
        
        private string GetEventName(Enum eventType) => $"{eventType}";

        private void SendEvent(string eventName, params (string, object)[] parameters)
        {
            foreach (var analytic in _analyticsWrappers)
            {
                analytic.SendEvent(eventName, parameters);
            }
        }
    }
}