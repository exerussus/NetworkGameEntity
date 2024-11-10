using Exerussus.NetworkGameEntity.Core;
using FishNet.Connection;
using FishNet.Object;
using Leopotam.EcsLite;
using UnityEngine;

namespace Exerussus.GameEntity.Core
{
    [RequireComponent(typeof(INetworkGameEntityBootstrapper))]
    public abstract class NetworkEntityComponent<T> : NetworkBehaviour, INetworkGameEntityComponent where T : NetworkGameEntityStarter
    {
        public INetworkGameEntityBootstrapper GameEntity { get; set; }

        public EcsPackedEntity EntityPack => GameEntity.EntityPack;
        public bool IsActivated { get; private set; }
        public bool IsQuitting => GameEntity.IsQuitting;
        private T _core;
        public T Core
        {
            get
            {
                if (!_isStarterInitialized) _core = NetworkGameEntityStarter.GetInstance<T>();
                return _core;
            } 
        }

        private bool _isStarterInitialized;
        
        [TargetRpc]
        public void InitializeClientComponent(NetworkConnection connection)
        {
            GameEntity = GetComponent<NetworkEntity<T>>();
            InvokeOnClientActivate(GameEntity.EntityPack.Id);
        }
        
        public void InvokeOnServerActivate(int entity) 
        { 
            if (!IsActivated) return;
            OnServerActivate(GameEntity.EntityPack.Id);
            IsActivated = true;
        }

        public void InvokeOnClientActivate(int entity) 
        { 
            if (IsActivated) return;
            OnClientActivate(GameEntity.EntityPack.Id);
            IsActivated = true;
        }

        public void InvokeOnServerDeactivate(int entity) 
        {
            if (IsActivated) return;
            OnServerDeactivate(GameEntity.EntityPack.Id);
            IsActivated = false;
        }
        
        public void InvokeOnClientDeactivate(int entity)
        { 
            if (!IsActivated) return;
            OnClientDeactivate(GameEntity.EntityPack.Id);
            IsActivated = false;
        }

        public virtual void OnServerDeactivate(int entity) { }
        public virtual void OnServerActivate(int entity) { }
        public virtual void OnClientDeactivate(int entity) { }

        public virtual void OnClientActivate(int entity) { }
    }
}