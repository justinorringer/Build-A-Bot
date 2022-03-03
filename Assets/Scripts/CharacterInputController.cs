using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class CharacterInputController : MonoBehaviour
    {
        CharacterController controller;
        
        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
            // If A or left arrow is pressed, move left
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                controller.moveLeft();
            }
            // If D or right arrow is pressed, move right
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                controller.moveRight();
            }

            // If space is pressed, FixedUpdate will determine if they are allowed to jump on this frame
            if (Input.GetKeyDown(KeyCode.Space))
            {
                controller.startJump();
            }
        }
    }
}