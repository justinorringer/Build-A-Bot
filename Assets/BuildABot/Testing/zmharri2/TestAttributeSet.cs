using System;
using UnityEngine;

namespace BuildABot
{
    [Serializable]
    public class TestAttributeSet : AttributeSet
    {
        [SerializeField] private FloatAttributeData test;

        public FloatAttributeData Test => test;
    }
}