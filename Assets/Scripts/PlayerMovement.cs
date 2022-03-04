using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovement : CharacterMovement
    {
        
        // Start is called before the first frame update
        new public void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}
