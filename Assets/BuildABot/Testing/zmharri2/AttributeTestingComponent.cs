using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class AttributeTestingComponent : MonoBehaviour
    {

        [SerializeField] private FloatAttributeSelector selector2;

        [SerializeField] private AttributeSetSelector typeSelector;
        
        [SerializeField] private CharacterAttributeSet characterAttributes;

        [SerializeField] private FloatAttributeModifier modTest;

        [SerializeReference] private List<AttributeSet> sets = new List<AttributeSet>()
        {
            new CharacterAttributeSet(),
            new TestAttributeSet()
        };
    }
}
