using System;
using System.Threading.Tasks;
using _Game.Scripts.Ad;
using _Game.Scripts.Balance;
using _Game.Scripts.Model;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using GeneralTools.Model;
using GeneralTools.Tools;
using UnityEngine;

namespace _Game.Scripts.Analytics
{
    public class GoogleAnalyticsWrapper : AnalyticsWrapper
    {
        private FirebaseApp _app;
        
        public override void Init()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(t =>
            {
                _app = FirebaseApp.DefaultInstance;
                // if (!Models.Get<GameModel>().Flags.Has(GameFlag.FirstSession))
                // {
                //     AppEventsProvider.TriggerEvent(GameEvents.FirstSession);  
                //     Models.Get<GameModel>().Flags.Set(GameFlag.FirstSession);
                // }
                //
                // AppEventsProvider.TriggerEvent(GameEvents.GameStart);

                //InitializeFirebase();
            });
        }

        // private void InitializeFirebase()
        // {
        //     var defaults = new System.Collections.Generic.Dictionary<string, object>
        //         {
        //             { "AdImpressionsDelay", 30},
        //             { "ShowInterAfterLevel", true},
        //             { "ShowInterAfterMapWindow", true},
        //             { "ShowInterAfterSkinWindow", true},
        //             { "MaxLevelsWithoutAd", 5},
        //             
        //             { "Level1Config", 10},
        //             { "Level2Config", 20},
        //             { "Level3Config", 30},
        //             { "Level4Config", 40},
        //             { "Level5Config", 50},
        //             { "Level6Config", 60},
        //             { "Level7Config", 70},
        //             { "Level8Config", 80},
        //             { "Level9Config", 90},
        //             { "Level10Config", 100},
        //         };
        //
        //     FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
        //     
        //     var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        //     fetchTask.ContinueWithOnMainThread(FetchComplete);
        // }

        // private void FetchComplete(Task fetchTask) 
        // {
        //     var info = FirebaseRemoteConfig.DefaultInstance.Info;
        //     switch (info.LastFetchStatus) {
        //         case LastFetchStatus.Success:
        //             FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
        //                 .ContinueWithOnMainThread(task => 
        //                 {
        //                     var adImpressionsDelay = FirebaseRemoteConfig.DefaultInstance.GetValue("AdImpressionsDelay");
        //                     if (adImpressionsDelay.LongValue > 0) Models.Get<AdModel>().SetAdValue(adImpressionsDelay.LongValue);
        //                     
        //                     var maxLevelsWithoutAd = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("MaxLevelsWithoutAd");
        //                     if (maxLevelsWithoutAd.LongValue > 0) Models.Get<GameModel>().Connection.SetMaxLevels((int)maxLevelsWithoutAd.LongValue);
        //                     
        //                     var showInterAfterLevel = FirebaseRemoteConfig.DefaultInstance.GetValue("ShowInterAfterLevel");
        //                     Models.Get<AdModel>().SetShowInterAfterLevel(showInterAfterLevel.BooleanValue);
        //                     
        //                     var showInterAfterMapWindow = FirebaseRemoteConfig.DefaultInstance.GetValue("ShowInterAfterMapWindow");
        //                     Models.Get<AdModel>().SetShowInterAfterMapWindow(showInterAfterMapWindow.BooleanValue);
        //                     
        //                     var showInterAfterSkinWindow = FirebaseRemoteConfig.DefaultInstance.GetValue("ShowInterAfterSkinWindow");
        //                     Models.Get<AdModel>().SetShowInterAfterSkinWindow(showInterAfterSkinWindow.BooleanValue);
        //
        //                     var levelConfig = FirebaseRemoteConfig.DefaultInstance.GetValue("Level1Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(1, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = FirebaseRemoteConfig.DefaultInstance.GetValue("Level2Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(2, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = FirebaseRemoteConfig.DefaultInstance.GetValue("Level3Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(3, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Level4Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(4, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Level5Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(5, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Level6Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(6, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Level7Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(7, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Level8Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(8, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Level9Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(9, (int)levelConfig.LongValue);
        //                     
        //                     levelConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Level10Config");
        //                     if (levelConfig.LongValue > 0) SetMapConfig(10, (int)levelConfig.LongValue);
        //                 });
        //             break;
        //         
        //         case LastFetchStatus.Failure:
        //             var maxLevelsWithoutAd = FirebaseRemoteConfig.DefaultInstance.GetValue("MaxLevelsWithoutAd");
        //             if (maxLevelsWithoutAd.LongValue > 0) Models.Get<GameModel>().Connection.SetMaxLevels((int)maxLevelsWithoutAd.LongValue);
        //             break;
        //     }
        // }

        // private void SetMapConfig(int level, int value)
        // {
        //     if (GameBalance.Instance.UnitParams.DifficultCurve.Length < level-1) return;
        //     GameBalance.Instance.UnitParams.DifficultCurve[level-1] = (float)value / 100;
        // }

        public override void CustomEventProcess(Enum eventType, params object[] parameters)
        {
            if (!(eventType is GameEvents metaEvents)) return;

            if (_app == null) return;
            switch (metaEvents)
            {
                case GameEvents.LevelStart:
                    var startId = parameters[0].ToString();
                    while (startId.Length < 4)
                    {
                        startId = "0" + startId;
                    }
                    FirebaseAnalytics.LogEvent("level_start", startId, 1);
                    break;
                
                case GameEvents.LevelComplete:
                    var endId = parameters[0].ToString();
                    while (endId.Length < 4)
                    {
                        endId = "0" + endId;
                    }
                    FirebaseAnalytics.LogEvent("level_finish", endId, 1);
                    break;
                
                case GameEvents.LevelFail:
                    var failId = parameters[0].ToString();
                    while (failId.Length < 4)
                    {
                        failId = "0" + failId;
                    }
                    FirebaseAnalytics.LogEvent("level_fail", failId, 1);
                    break;
                
                case GameEvents.FirstSession:
                    Debug.Log(0);
                    //FirebaseAnalytics.LogEvent("first_session");
                    break;
                
                case GameEvents.GameStart:
                    FirebaseAnalytics.LogEvent("game_start");
                    break;
                
                case GameEvents.InAppPurchased:
                    var purchaseData = (InAppPurchaseData)parameters[0];
                    FirebaseAnalytics.LogEvent("in_app_purchased", purchaseData.ID, 1);
                    break;
                
                case GameEvents.RewardVideoAdStarted:
                    FirebaseAnalytics.LogEvent("rv_started", parameters[0].ToString(), 1);
                    break;
                
                case GameEvents.RewardVideoAdShowed:
                    FirebaseAnalytics.LogEvent("rv_showed", parameters[0].ToString(), 1);
                    break;
                
                case GameEvents.RewardVideoAdError:
                    FirebaseAnalytics.LogEvent("rv_error", parameters[0].ToString(), 1);
                    break;
                
                case GameEvents.InterstitialVideoAdStarted:
                    FirebaseAnalytics.LogEvent("int_started", parameters[0].ToString(), 1);
                    break;
                
                case GameEvents.InterstitialVideoAdShowed:
                    FirebaseAnalytics.LogEvent("int_showed", parameters[0].ToString(), 1);
                    break;
                
                case GameEvents.InterstitialVideoAdError:
                    FirebaseAnalytics.LogEvent("int_error", parameters[0].ToString(), 1);
                    break;
                
                case GameEvents.BannerStart:
                    FirebaseAnalytics.LogEvent("banner_start");
                    break;
                case GameEvents.CloseReward:
                    FirebaseAnalytics.LogEvent("rew_closed");
                    break;
            }
        }
        
        public override void SendEvent(string eventType, params (string, object)[] args)
        {
            
        }
        
    }
}