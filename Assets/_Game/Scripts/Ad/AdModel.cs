using System;
using System.Collections.Generic;
using _Game.Scripts.Model;
using _Game.Scripts.UI.Message;
using GeneralTools.Localization;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts.Ad
{
    public enum AdType
    {
        None,
        AddSoftMaskWindow,
        LoseWindow,
        Banner,
        Interstitial,
    }
    
    public enum AdWatchType
    {
        Watched,
        Clicked,
        Canceled,
        ErrorLoaded,
        ErrorDisplay
    }
    
    public enum AdVideoType
    {
        Reward,
        Interstitial,
        Banner
    }
    
    public class AdModel : BaseModel
    {
        public event Action<bool> AdEvent;
        
        private List<AdWrapper> _adsWrappers;
        
        private AdType _adType;
        private bool _adPressed;
        private int _count;

        private GameModel _game;
        
        private const float _adLevelConst = 30f;
        private float _adLevelTimer;
        
        public override BaseModel Start()
        {
            _game = Models.Get<GameModel>();
            _adsWrappers = new List<AdWrapper>()
            {
                //new IronSourceWrapper(),
                //new ApplovinMaxWrapper()
                new ApplovinWrapper()
            };

            foreach (var adsWrapper in _adsWrappers) adsWrapper.Start();

            return this;
        }
        
        public override void Update(float deltaTime)
        {
            _adLevelTimer += deltaTime;
            
            base.Update(deltaTime);
        }

        public void ShowAd(AdType type, AdVideoType videoType = AdVideoType.Reward)
        {
            _adType = type;
            
            // if (_game.Flags.Has(GameFlag.StartOffer))
            // {
            //     if (videoType == AdVideoType.Banner) return;
            //     AdModelOnVideoShowed(AdWatchType.Watched, videoType, 0);
            //     return;
            // }
            
            if (videoType == AdVideoType.Banner)
            {
                ShowBanner();
                return;
            }
            
            // if (_adLevelTimer >= _adLevelConst)
            // {
            //     _adPressed = false;
            // }
            
            if (_adPressed && videoType == AdVideoType.Interstitial) 
            {
                //GameUI.Get<MessageContainer>().Show("ads_ended".Localized());
                return;
            }
            
            var available = videoType == AdVideoType.Reward ? RewardVideoAvailable() : InterstitialVideoAvailable();
            
            if (available)
            {
                _adPressed = true;

                switch (videoType)
                {
                    case AdVideoType.Reward:
                        ShowRewardedVideo();
                        break;
                    case AdVideoType.Interstitial:
                        ShowInterstitialVideo();
                        break;
                }
                AppEventsProvider.TriggerEvent(GameEvents.RewardVideoAvailable, videoType, _adType, "success", Application.internetReachability);
            }
            else
            {
                AppEventsProvider.TriggerEvent(GameEvents.RewardVideoAvailable, videoType, _adType, "not_available", Application.internetReachability);
#if UNITY_EDITOR
                switch (videoType)
                {
                    case AdVideoType.Reward:
                        AdModelOnVideoShowed(AdWatchType.Watched, AdVideoType.Reward, 0);
                        break;
                    case AdVideoType.Interstitial:
                        if (_adLevelTimer < _adLevelConst)
                        {
                            AdEvent?.Invoke(true);
                            AdEvent = null;
                            return;
                        }
                        AdModelOnVideoShowed(AdWatchType.Watched, AdVideoType.Interstitial, 0);
                        break;
                }
                return;
#endif
                //GameUI.Get<MessageContainer>().Show("ads_ended".Localized());
            }
            
//             if (RewardVideoAvailable())
//             {
//                 _adPressed = true;
//                 ShowRewardedVideo();
//             }
//             else
//             {
// #if UNITY_EDITOR
//                 AdModelOnRewardedVideoShowed(AdWatchType.Watched, 0);
//                 return;
// #endif
//                 GameUI.Get<MessageContainer>().Show("ads_ended".Localized());
//             }
        }
     
        private bool RewardVideoAvailable()
        {
            if (_adsWrappers.Count == 0) return true;
            var available = _adsWrappers[0].RewardVideoAvailable();
            return available;
        }
        
        private bool InterstitialVideoAvailable()
        {
            if (_adsWrappers.Count == 0) return true;
            
            var available = _adsWrappers[0].InterstitialVideoAvailable();
            return available;
        }
        
        private void ShowRewardedVideo()
        {
            _adLevelTimer = 0;
            //_adPressed = false;
            if (_adsWrappers.Count == 0)
            {
                AdModelOnVideoShowed(AdWatchType.Watched, AdVideoType.Reward, 0);
                return;
            }

            _adsWrappers[0].ShowRewardedVideo(AdModelOnVideoShowed);
            AppEventsProvider.TriggerEvent(GameEvents.RewardVideoAdStarted, _adType, 0);
        }
        
        // private void ShowRewardedVideo()
        // {
        //     _adPressed = false;
        //     if (_adsWrappers.Count == 0)
        //     {
        //         AdModelOnRewardedVideoShowed(AdWatchType.Watched, 0);
        //         return;
        //     }
        //
        //     _adsWrappers[0].ShowRewardedVideo(AdModelOnRewardedVideoShowed);
        //     AppEventsProvider.TriggerEvent(GameEvents.VideoAdStarted, _adType, 0);
        // }
        
        private void ShowInterstitialVideo()
        {
            // if (_adLevelTimer < _adLevelConst)
            // {
            //     AdEvent?.Invoke(true);
            //     AdEvent = null;
            //     return;
            // }
            
            _adLevelTimer = 0;
            _adPressed = false;
            if (_adsWrappers.Count == 0)
            {
                AdModelOnVideoShowed(AdWatchType.Watched, AdVideoType.Interstitial, 0);
                return;
            }

            _adsWrappers[0].ShowInterstitialVideo(AdModelOnVideoShowed);
            AppEventsProvider.TriggerEvent(GameEvents.InterstitialVideoAdStarted, _adType, 0);
        }
        
        private void ShowBanner()
        {
            _adsWrappers[0].LoadBannerVideo();
            _adsWrappers[0].ShowBannerVideo();
            AppEventsProvider.TriggerEvent(GameEvents.BannerStart);
        }
        
        public void HideBanner()
        {
            _adsWrappers[0].HideBannerVideo();
        }
        
        private void AdModelOnVideoShowed(AdWatchType type, AdVideoType videoType, int code)
        {
            var gameEventShowed = videoType == AdVideoType.Reward 
                ? GameEvents.RewardVideoAdShowed 
                : GameEvents.InterstitialVideoAdShowed;
            
            var gameEventError = videoType == AdVideoType.Reward
                ? GameEvents.RewardVideoAdError
                : GameEvents.InterstitialVideoAdError;
            
            switch (type)
            {
                case AdWatchType.Canceled:
                    AdEvent?.Invoke(false);
                    AdEvent = null;
                    if (videoType == AdVideoType.Reward)
                    {
                        AppEventsProvider.TriggerEvent(GameEvents.CloseReward);
                    }
                    return;
                
                case AdWatchType.ErrorLoaded:
                    AdEvent?.Invoke(false);
                    AdEvent = null;
                    AppEventsProvider.TriggerEvent(gameEventError, "Load", code);
                    return;
                
                case AdWatchType.ErrorDisplay:
                    AdEvent?.Invoke(false);
                    AdEvent = null;
                    AppEventsProvider.TriggerEvent(gameEventError, "Display", code);
                    return;
                
                case AdWatchType.Watched:
                case AdWatchType.Clicked:
                    // var rewarded = _game.CurrentData.GetParam(GameParamType.Rewarded);
                    // rewarded.Change(1);
                    // if ((int) rewarded.Value == 10)
                    // {
                    //     AppEventsProvider.TriggerEvent(GameEvents.Rewarded_10);
                    //     if (_game.Last24SessionDay > 1)
                    //     {
                    //         AppEventsProvider.TriggerEvent(GameEvents.RV10_d0);
                    //     }
                    // }
                    //
                    // if ((int) rewarded.Value == 20)
                    // {
                    //     AppEventsProvider.TriggerEvent(GameEvents.Rewarded_20);
                    //     if (_game.Last24SessionDay > 1)
                    //     {
                    //         AppEventsProvider.TriggerEvent(GameEvents.RV20_d0);
                    //     }
                    // }
                    
                    AdEvent?.Invoke(true);
                    AdEvent = null;

                    AppEventsProvider.TriggerEvent(gameEventShowed, _adType);
                    
                    _adType = AdType.None;
                    _adPressed = false;
                    
                    break;
            }
        }
        
        // private void AdModelOnRewardedVideoShowed(AdWatchType type, int code)
        // {
        //     switch (type)
        //     {
        //         case AdWatchType.Canceled:
        //             AdEvent?.Invoke(false);
        //             AdEvent = null;
        //             return;
        //         
        //         case AdWatchType.ErrorLoaded:
        //             AdEvent?.Invoke(false);
        //             AdEvent = null;
        //             AppEventsProvider.TriggerEvent(GameEvents.VideoAdError, "Load", code);
        //             return;
        //         
        //         case AdWatchType.ErrorDisplay:
        //             AdEvent?.Invoke(false);
        //             AdEvent = null;
        //             AppEventsProvider.TriggerEvent(GameEvents.VideoAdError, "Display", code);
        //             return;
        //         
        //         case AdWatchType.Watched:
        //         case AdWatchType.Clicked:
        //             AdEvent?.Invoke(true);
        //             AdEvent = null;
        //
        //             AppEventsProvider.TriggerEvent(GameEvents.VideoAdShowed, _adType);
        //             
        //             _adType = AdType.None;
        //             _adPressed = false;
        //             break;
        //     }
        // }
    }
}