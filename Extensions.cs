using Exerussus.NetworkGameEntity.Core;

namespace Exerussus.NetworkGameEntity.Extensions
{
    public static class GameEntityExtensions
    {
        public static void SetEntityNameStarted(this INetworkGameEntityBootstrapper inetworkGameEntity)
        {
#if UNITY_EDITOR
            var bracketIndex = inetworkGameEntity.GameObject.name.IndexOf('(');
            if (bracketIndex != -1) inetworkGameEntity.GameObject.name = inetworkGameEntity.GameObject.name.Substring(0, bracketIndex).Trim();
            bracketIndex = inetworkGameEntity.GameObject.name.IndexOf('|');
            if (bracketIndex != -1) inetworkGameEntity.GameObject.name = inetworkGameEntity.GameObject.name.Substring(0, bracketIndex).Trim();
            inetworkGameEntity.GameObject.name = $"{inetworkGameEntity.GameObject.name} | {inetworkGameEntity.EntityPack.Id}";
#endif
        }
        
        public static void SetEntityNameDestroyed(this INetworkGameEntityBootstrapper inetworkGameEntity)
        {
#if UNITY_EDITOR
            var bracketIndex = inetworkGameEntity.GameObject.name.IndexOf('(');
            if (bracketIndex != -1) inetworkGameEntity.GameObject.name = inetworkGameEntity.GameObject.name.Substring(0, bracketIndex).Trim();
            bracketIndex = inetworkGameEntity.GameObject.name.IndexOf('|');
            if (bracketIndex != -1) inetworkGameEntity.GameObject.name = inetworkGameEntity.GameObject.name.Substring(0, bracketIndex).Trim();
            inetworkGameEntity.GameObject.name = $"{inetworkGameEntity.GameObject.name} | destroyed";
#endif
        }
    }
}