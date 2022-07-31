using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TonyDev
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ItemEffectAttribute : Attribute
    {
        public string ID;
    }
}
