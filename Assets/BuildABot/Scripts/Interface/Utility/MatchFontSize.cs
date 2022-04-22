using System;
using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class MatchFontSize : MonoBehaviour
    {
        [Tooltip("The text object to be relative to.")]
        [SerializeField] private TMP_Text relativeReference;

        [Tooltip("The multiplier to apply to this objects font size relative to the reference text.")]
        [Min(0f)]
        [SerializeField] private float scale = 1.0f;

        private void Apply()
        {
            if (TryGetComponent(out TMP_Text target) && relativeReference != null)
            {
                target.fontSize = relativeReference.fontSize * scale;
                target.ForceMeshUpdate();
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            Apply();
        }
        #endif

        private void Start()
        {
            Apply();
        }
    }
}