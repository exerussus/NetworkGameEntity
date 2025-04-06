using System.Collections.Generic;
using Exerussus._1EasyEcs.Scripts.Custom;
using Exerussus._1Extensions.SignalSystem;
using Exerussus._1Extensions.SmallFeatures;
using FishNet;
using Leopotam.EcsLite;
using UnityEngine;

namespace Exerussus.NetworkGameEntity.Core
{
    public abstract class NetworkGameEntityStarter : EcsStarter
    {
        private Signal _signal;
        private NetworkGameEntityPooler _pooler;
        protected override Signal Signal => _signal;
        public EcsWorld World => _world;
        public Signal SignalInstance => _signal;
        public NetworkGameEntityPooler Pooler => _pooler;
        
        public static NetworkGameEntityStarter Instance;
        private static bool _isInitialized;
        public static bool IsQuitting { get; protected set; }

        public static T GetInstance<T>() where T : NetworkGameEntityStarter
        {
            if (IsQuitting) return null;
            
            if (!_isInitialized || Instance == null)
            {
                _isInitialized = true;
                Instance = new GameObject {name = $"{typeof(T).Name}"}.AddComponent<T>();
                Instance._signal = Instance.SetSignal();
                Instance.PreInitialize();
                Instance._pooler = Instance.GameShare.GetSharedObject<NetworkGameEntityPooler>();
                Instance.Initialize();
                Application.quitting += () => IsQuitting = true;
            }
            return (T)Instance;
        }

        protected override EcsGroup[] GetGroups()
        {
            var groups = new List<EcsGroup> { new NetworkGameEntityGroup() };
            
            if (InstanceFinder.IsServerStarted) GetServerGroups(_world, GameShare, groups);
            if (InstanceFinder.IsClientStarted) GetClientGroups(_world, GameShare, groups);
            if (!InstanceFinder.IsServerStarted && !InstanceFinder.IsHostStarted) GetNotHostOnlyGroups(_world, GameShare, groups);

            return groups.ToArray();
        }

        protected abstract void GetNotHostOnlyGroups(EcsWorld world, GameShare gameShare, List<EcsGroup> groups);
        protected abstract void GetClientGroups(EcsWorld world, GameShare gameShare, List<EcsGroup> groups);
        protected abstract void GetServerGroups(EcsWorld world, GameShare gameShare, List<EcsGroup> groups);

        public virtual Signal SetSignal()
        {
            return new Signal();
        }

        protected virtual void OnDisable()
        {
            _isInitialized = false;
            Instance = null;
        }

        public static void ClearInstance()
        {
            _isInitialized = false;
            Instance = null;
            IsQuitting = false;
        }
    }

#if UNITY_EDITOR

    [UnityEditor.InitializeOnLoad]
    public static class StaticCleaner
    {
        static StaticCleaner()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode || state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                NetworkGameEntityStarter.ClearInstance();
            }
        }
    }
#endif
}
