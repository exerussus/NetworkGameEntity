using System.Collections.Generic;
using System.Linq;
using Exerussus._1Attributes;
using Exerussus._1Extensions.SmallFeatures;
using Exerussus.NetworkGameEntity.Core;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using Leopotam.EcsLite;
using Plugins.Exerussus._1EasyEcs.Scripts.Extensions;
using UnityEngine;

namespace Exerussus.GameEntity.Core
{
    public abstract class NetworkEntity<T> : NetworkBehaviour, INetworkGameEntityBootstrapper where T : NetworkGameEntityStarter
    {
        [ReadOnly] protected EcsPackedEntity _entityPack;
        [ReadOnly] protected bool _isEntityActivated;
        [ReadOnly] protected bool _isComponentsActivated;
        [SerializeField, HideInInspector] private List<INetworkGameEntityComponent> gameEntityComponents;
        [SerializeField] private bool _serverSpawned;
        private T _core;
        private bool _isStarterInitialized;
        private bool _isQuitting;
        public List<INetworkGameEntityComponent> Componentses => gameEntityComponents;
        public EcsPackedEntity EntityPack => _entityPack;
        public int UniqueId { get; private set; }
        public GameObject GameObject => gameObject;
        public bool Activated => _isEntityActivated && _isComponentsActivated;
        public bool IsQuitting { get; private set; }

        public bool ServerSpawned => _serverSpawned;

        public EcsWorld World => Core.World;
        public NetworkGameEntityPooler Pooler => Core.Pooler;

        public T Core
        {
            get
            {
                if (!_isStarterInitialized) _core = NetworkGameEntityStarter.GetInstance<T>();
                return _core;
            } 
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            if (_isEntityActivated) return;
            if (!Application.isPlaying) return;
            Application.quitting += () => IsQuitting = true;
            gameEntityComponents = GetComponents<INetworkGameEntityComponent>().ToList();
            _serverSpawned = true;
            if (gameEntityComponents is not {Count: > 0})
            {
                gameEntityComponents = new List<INetworkGameEntityComponent>(GetComponents<INetworkGameEntityComponent>());
                if (gameEntityComponents is not {Count: > 0}) InstanceFinder.ServerManager.Despawn(NetworkObject);
            }
            
            _entityPack = World.NewPackedEntity();
            
            ref var gameEntityData = ref Pooler.GameEntity.AddOrGet(_entityPack.Id);
            gameEntityData.Value = this;
            ref var uniqueEntityIdData = ref Pooler.UniqueId.AddOrGet(_entityPack.Id);
            uniqueEntityIdData.Value = Pooler.GetUniqueId();
            UniqueId = uniqueEntityIdData.Value;
            _isEntityActivated = true;
            OnServerAwakeEntity();
            
            foreach (var entityComponent in gameEntityComponents)
            {
                entityComponent.GameEntity = this;
                entityComponent.InvokeOnServerActivate(_entityPack.Id);
            }

            _isComponentsActivated = true;
            OnServerStartEntity();
        }

        public override void OnSpawnServer(NetworkConnection connection)
        {
            base.OnSpawnServer(connection);
            OnBeforeSendInitializeEntity(connection);
            InitializeEntity(connection, UniqueId);
            OnAfterSendInitializeEntity(connection);
        }

        [TargetRpc]
        private void InitializeEntity(NetworkConnection connection, int uniqueId)
        {
            gameEntityComponents = GetComponents<INetworkGameEntityComponent>().ToList();
            
            if (!InstanceFinder.IsHostStarted)
            {
                _serverSpawned = true;
                if (_isEntityActivated) return;
                if (!Application.isPlaying) return;
                Application.quitting += () => IsQuitting = true;
                _entityPack = World.PackEntity(World.NewEntity());
                ref var gameEntityData = ref Pooler.GameEntity.AddOrGet(_entityPack.Id);
                gameEntityData.Value = this;
                ref var uniqueEntityIdData = ref Pooler.UniqueId.AddOrGet(_entityPack.Id);
                uniqueEntityIdData.Value = uniqueId;
                UniqueId = uniqueEntityIdData.Value;
            }
            if (IsOwner) Pooler.OwnerMarker.AddOrGet(_entityPack.Id);
            
            OnClientAwakeEntity();
            CommandInitComponents(connection);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void CommandInitComponents(NetworkConnection connection)
        {
            OnBeforeSendInitializeComponents(connection);
            foreach (var gameEntityComponent in gameEntityComponents) gameEntityComponent.InitializeClientComponent(connection);
            OnAfterSendInitializeComponents(connection);
            OnInitializingComponentsDone(connection);
        }

        [TargetRpc]
        private void OnInitializingComponentsDone(NetworkConnection connection)
        {
            _serverSpawned = true;
            OnClientStartEntity();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            if (IsQuitting) return;
            if (_isEntityActivated && _entityPack.Unpack(World, out var entity))
            {
                if (_isEntityActivated)
                {
                    foreach (var gearComponent in gameEntityComponents)
                    {
                        gearComponent.InvokeOnServerDeactivate(entity);
                    }

                    OnBeforeServerDestroyEntity();
                    World.DelEntity(entity);
                    _isEntityActivated = false;
                    _isComponentsActivated = false;
                    OnAfterServerDestroyEntity();
                }
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            if (IsQuitting) return;
            if (_isEntityActivated && _entityPack.Unpack(World, out var entity))
            {
                if (_isEntityActivated)
                {
                    foreach (var gearComponent in gameEntityComponents)
                    {
                        gearComponent.InvokeOnClientDeactivate(entity);
                    }

                    OnBeforeClientDestroyEntity();
                    World.DelEntity(entity);
                    _isEntityActivated = false;
                    _isComponentsActivated = false;
                    OnAfterClientDestroyEntity();
                }
            }
        }
        
        public virtual void OnBeforeSendInitializeEntity(NetworkConnection connection) {}
        public virtual void OnAfterSendInitializeEntity(NetworkConnection connection) {}
        public virtual void OnBeforeSendInitializeComponents(NetworkConnection connection) {}
        public virtual void OnAfterSendInitializeComponents(NetworkConnection connection) {}
        /// <summary> Срабатывает на клиенте перед инициализацией компонентов. </summary>
        public virtual void OnClientAwakeEntity() {}
        /// <summary> Срабатывает на клиенте после инициализации компонентов. </summary>
        public virtual void OnClientStartEntity() {}
        /// <summary> Срабатывает на сервере перед инициализацией компонентов. </summary>
        public virtual void OnServerAwakeEntity() {}
        /// <summary> Срабатывает на сервере после инициализации компонентов. </summary>
        public virtual void OnServerStartEntity() {}
        public virtual void OnBeforeServerDestroyEntity() {}
        public virtual void OnAfterServerDestroyEntity() {}
        public virtual void OnBeforeClientDestroyEntity() {}
        public virtual void OnAfterClientDestroyEntity() {}
    }
}