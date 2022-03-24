using System;
using UnityEngine;

namespace BuildABot
{
    [Serializable]
    public class TestAttributeSet2 : AttributeSet
    {
        [SerializeField] private FloatAttributeData someValue;

        public FloatAttributeData SomeValue => someValue;
    }
}