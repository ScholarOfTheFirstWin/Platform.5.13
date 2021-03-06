﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public Rigidbody playerRB; 
    private Rigidbody cubeRB;
	private Vector3 gravDown;
	private Transform cam;

	public GameObject cubePrefab;

    public MeshRenderer playerRenderer; //needs to be accessed by Halfpipe script

    private float offsetX;
	private float offsetZ;
    public float topSpeed;
	public float speed;
    
	public float jumpSpeed;
	public bool inAir;
	private bool PowerUpDoubleJump;

	void Start(){
		cam = GameObject.FindGameObjectWithTag ("MainCamera").transform; 
        topSpeed = speed * 1.2f;
		playerRB = GetComponent<Rigidbody> ();
		cubeRB = cubePrefab.GetComponent<Rigidbody>();

		gravDown = new Vector3(0, -25, 0);

		Physics.gravity = gravDown; //Set the world gravity to 25 - felt realistic
	}

	void FixedUpdate(){
        /* Adding both torque and force at the same time
		 gives you good control at lower speed but still
		 leaves the ability to accelerate */

        float moveHor = Input.GetAxis ("Horizontal");  
 		float moveVer = Input.GetAxis ("Vertical");

		offsetX = (transform.position.x - cam.position.x)/4;
		offsetZ = (transform.position.z - cam.position.z)/4;

		// Getting the offset from the X and Z allows us to always move the ball in relation to the cam
		
		Vector3 forceMovement = new Vector3 (moveHor*offsetZ + moveVer*offsetX, 0.0f, moveVer*offsetZ - moveHor*offsetX);
		// Because torque is weird the axes are flipped for the torque movement vector		
		Vector3 torqueMovement = new Vector3 (moveVer*offsetZ - moveHor*offsetX, 0.0f, -moveHor*offsetZ - moveVer*offsetX);
		
		Vector3 jump = new Vector3 (0.0f, jumpSpeed, 0.0f);

		playerRB.AddTorque(torqueMovement * (speed * 2));

        // I felt like giving a higher precedence to torque but I actually dont know if this does anything
        // The following if statement ensures that the ball does not continue to accelerate indefinitely
        // The following insanity is necessary for the proper functionality of the control inversion gate.
        
		if (-topSpeed < playerRB.velocity.magnitude && playerRB.velocity.magnitude < topSpeed ) {
            playerRB.AddForce(forceMovement * (speed * 2 / 3));
        }

		if (Input.GetKey (KeyCode.Space)) {
			if (!inAir)
			{
				inAir = true;
				playerRB.AddForce(jump); //This code is useful if you want to implement double jump so I'll just leave it
			}
		}

        //&& !HalfpipeManager.inHalfpipe
        if (Input.GetKeyDown(KeyCode.LeftShift) && !HalfpipeManager.inHalfpipe)
        {
            if (!cubePrefab.activeSelf)
            {
                cubePrefab.SetActive(true);
                playerRenderer.enabled = false; //disable renderer dynamically in case the circle collider bleeds through the cube
                cubeRB.velocity = Vector3.zero;
                cubeRB.angularVelocity = Vector3.zero;
                cubeRB.AddForce(new Vector3(0, -20f, 0)); //in case the cube has become inactive in the middle of movement
            }
            else if (cubePrefab.activeSelf)
            {
                cubePrefab.SetActive(false);
                playerRenderer.enabled = true;
            }
        }
    }

	void OnTriggerEnter(Collider Other) {

        if (Other.gameObject.CompareTag("Jumpable"))
        {
            // This was the best way I found for specifying where you can jump
            // In order to implement this you need to have a mesh collider that
            // covers the surface of the area you want to let the player jump on
            // The tag can also just be placed on all ground objects

            inAir = false;
        }
	}

	void OnTriggerStay(Collider Other) {

        if (Other.gameObject.CompareTag("Jumpable"))
        {
			inAir = false;
        }
    }

	void OnTriggerExit(Collider Other) {
        inAir = true; // Whenever you leave a trigger its probably because you're in the air.
    }
}
