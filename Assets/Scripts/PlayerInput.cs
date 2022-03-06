using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class PlayerInput : MonoBehaviour
    {
        PlayerMovement controller;
        
        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<PlayerMovement>();
        }

        // Update is called once per frame
        void Update()
        {
            // If A or left arrow is pressed, move left
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                controller.MoveLeft();
            }
            // If D or right arrow is pressed, move right
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                controller.MoveRight();
            }

            // If space is pressed, FixedUpdate will determine if they are allowed to jump on this frame
            if (Input.GetKeyDown(KeyCode.Space))
            {
                controller.StartJump();
            }
        }
    }
}