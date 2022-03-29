using System;
using UnityEngine;

namespace BuildABot
{
    [Serializable]
    public class TestAttributeSet : CharacterAttributeSet
    {
        [SerializeField] private FloatAttributeData test;

        public FloatAttributeData Test => test;
    }
}