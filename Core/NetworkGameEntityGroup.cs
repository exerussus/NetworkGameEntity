using Exerussus._1EasyEcs.Scripts.Custom;
using Leopotam.EcsLite;

namespace Exerussus.NetworkGameEntity.Core
{
    public class NetworkGameEntityGroup : EcsGroup<NetworkGameEntityPooler>
    {
        protected override void SetFixedUpdateSystems(IEcsSystems fixedUpdateSystems)
        {
#if UNITY_EDITOR
            fixedUpdateSystems.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem());
#endif
        }
    }
}