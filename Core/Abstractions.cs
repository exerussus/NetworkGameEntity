using System.Collections.Generic;
using FishNet.Connection;
using Leopotam.EcsLite;
using UnityEngine;

namespace Exerussus.NetworkGameEntity.Core
{
    public interface INetworkGameEntityBootstrapper
    {
        public EcsPackedEntity EntityPack { get; }
        public GameObject GameObject { get; }
        public bool Activated { get; }
        public bool IsQuitting { get; }
        public abstract EcsWorld World { get; }
        public abstract NetworkGameEntityPooler Pooler { get; } 
        public abstract List<INetworkGameEntityComponent> Componentses { get; }
    }

    public interface INetworkGameEntityComponent
    {
        public INetworkGameEntityBootstrapper GameEntity { get; set; }
        public abstract void InvokeOnServerDeactivate(int entity);
        public abstract void InvokeOnServerActivate(int entity);
        public abstract void InvokeOnClientDeactivate(int entity);
        public abstract void InvokeOnClientActivate(int entity);
        public abstract void InitializeClientComponent(NetworkConnection connection);
    }
}