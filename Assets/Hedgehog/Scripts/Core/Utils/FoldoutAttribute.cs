using System;
using UnityEngine;

namespace Hedgehog.Core.Utils
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public string Name;
        public FoldoutAttribute(string name)
        {
            Name = name;
        }
    }
}
