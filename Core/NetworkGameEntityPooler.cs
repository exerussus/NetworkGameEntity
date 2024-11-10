using Exerussus._1EasyEcs.Scripts.Core;
using Exerussus._1EasyEcs.Scripts.Custom;
using Exerussus._1Extensions.SmallFeatures;
using Leopotam.EcsLite;

namespace Exerussus.NetworkGameEntity.Core
{
    public class NetworkGameEntityPooler : IGroupPooler
    {
        public virtual void Initialize(EcsWorld world)
        {
            GameEntity = new(world);
            UniqueId = new(world);
            OwnerMarker = new(world);
            _uniqueIdCounter = new Counter();
        }

        private Counter _uniqueIdCounter;
        public PoolerModule<NetworkGameEntityData.GameEntity> GameEntity { get; private set; }
        public PoolerModule<NetworkGameEntityData.UniqueId> UniqueId { get; private set; }
        public PoolerModule<NetworkGameEntityData.OwnerMarker> OwnerMarker { get; private set; }

        public int GetUniqueId()
        {
            return _uniqueIdCounter.GetNext();
        }
    }
}