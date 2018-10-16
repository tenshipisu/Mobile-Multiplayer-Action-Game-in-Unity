﻿using DitzeGames.MobileJoystick;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnderdogCity
{

    public class Controller : MonoBehaviour
    {

        //Input
        protected Joystick Joystick;
        protected Button JumpButton;
        protected Button ShootButton;
        protected TouchField TouchField;
        protected Player Player;

        //Parameters
        protected const float RotationSpeed = 10;

        //Camera Controll
        public Vector3 CameraPivot;
        public float CameraDistance;
        protected float InputRotationX;
        protected float InputRotationY;

        protected Vector3 CharacterPivot;
        protected Vector3 LookDirection;

        // Use this for initialization
        void Start()
        {
            Joystick = FindObjectOfType<Joystick>();
            var buttons = new List<Button>(FindObjectsOfType<Button>());
            JumpButton = buttons.Find(b => b.gameObject.name == "btnJump");
            ShootButton = buttons.Find(b => b.gameObject.name == "btnShoot");
            TouchField = FindObjectOfType<TouchField>();
            Player = GetComponent<Player>();

            TouchField.UseFixedUpdate = true;
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            //input
            InputRotationX = InputRotationX + TouchField.TouchDist.x * RotationSpeed * Time.deltaTime % 360f;
            InputRotationY = Mathf.Clamp(InputRotationY - TouchField.TouchDist.y * RotationSpeed * Time.deltaTime, -88f, 88f);

            //left and forward
            var characterForward = Quaternion.AngleAxis(InputRotationX, Vector3.up) * Vector3.forward;
            var characterLeft = Quaternion.AngleAxis(InputRotationX + 90, Vector3.up) * Vector3.forward;

            //look and run direction
            var runDirection = characterForward * (Input.GetAxisRaw("Vertical") + Joystick.AxisNormalized.y) + characterLeft * (Input.GetAxisRaw("Horizontal") + Joystick.AxisNormalized.x);
            LookDirection = Quaternion.AngleAxis(InputRotationY, characterLeft) * characterForward;

            //set player values
            Player.Input.RunX = runDirection.x;
            Player.Input.RunZ = runDirection.z;
            Player.Input.LookX = LookDirection.x;
            Player.Input.LookZ = LookDirection.z;
            Player.Input.Jump = JumpButton.Pressed || Input.GetButton("Jump");

            if (ShootButton.Pressed)
            {
                ShootButton.Pressed = false;
                var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
                RaycastHit hitInfo;
                if(Physics.Raycast(ray, out hitInfo))
                {
                    var player = hitInfo.collider.GetComponent<Player>();
                    if (player != null)
                        player.OnHit(ray.direction);
                }
            }

            CharacterPivot = Quaternion.AngleAxis(InputRotationX, Vector3.up) * CameraPivot;
        }

        private void LateUpdate()
        {
            //set camera values
            Camera.main.transform.position = (transform.position + CharacterPivot) - LookDirection * CameraDistance;
            Camera.main.transform.rotation = Quaternion.LookRotation(LookDirection, Vector3.up);
        }
    }
}