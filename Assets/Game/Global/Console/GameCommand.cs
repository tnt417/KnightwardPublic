using System;

namespace TonyDev.Game.Global.Console
{
    public enum PermissionLevel
    {
        Default = 0,
        Cheat = 1
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class GameCommand : Attribute
    {
        public string Keyword;
        public PermissionLevel PermissionLevel = PermissionLevel.Default;
        public string SuccessMessage;
    }
}
