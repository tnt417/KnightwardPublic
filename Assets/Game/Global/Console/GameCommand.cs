using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TonyDev
{
    public enum PermissionLevel
    {
        Default = 0,
        Cheat = 1
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class GameCommand : Attribute
    {
        public string Keyword;
        public PermissionLevel PermissionLevel = PermissionLevel.Default;
        public string SuccessMessage;
    }
}
