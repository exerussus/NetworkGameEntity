using Exerussus._1EasyEcs.Scripts.Core;

namespace Exerussus.NetworkGameEntity.Core
{
    public static class NetworkGameEntityData
    {
        public struct GameEntity : IEcsComponent
        {
            public INetworkGameEntityBootstrapper Value;
        }
        
        public struct UniqueId : IEcsComponent
        {
            public int Value;
        }
        
        public struct OwnerMarker : IEcsComponent
        {
            
        }
    }
}