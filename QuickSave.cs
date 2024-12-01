using SFMF;
using System;
using UnityEngine;
using System.IO;

namespace QuickSave
{
    public class QuickSave : IMod
    {
        private const string SettingsPath = @".\SFMF\ModSettings\QuickSave.csv";
        private KeyCode KeyboardKey1 { get; set; }
        private KeyCode KeyboardKey2 { get; set; }

        private KeyCode ControllerButton1 { get; set; }
        private KeyCode ControllerButton2 { get; set; }


        private Vector3? savedPosition = null;
        private Quaternion? savedRotation = null;
        private float? savedSpeed = null;
        private bool hasActiveSavePoint = false;
        private int? currentSeed;

        private void Start()
        {
            var settings = File.ReadAllLines(SettingsPath);

            // First line - Create save point
            var parts1 = settings[0].Split(',');
            KeyboardKey1 = (KeyCode)Enum.Parse(typeof(KeyCode), parts1[2]);     // Defaults to 'P'
            ControllerButton1 = GetControllerButton(parts1[3]);                 // Defaults to 'RB'

            // Second line - Remove save point
            var parts2 = settings[1].Split(',');
            KeyboardKey2 = (KeyCode)Enum.Parse(typeof(KeyCode), parts2[2]);     // Defaults to 'k'
            ControllerButton2 = GetControllerButton(parts2[3]);                 // Defaults to 'R3'
        }


        public void Update()
        {
            // Variables
            var isNextWorld = currentSeed != null && (currentSeed.Value != WorldManager.currentWorld.seed);
            var isPlayerReset = LocalGameManager.Singleton.playerState == LocalGameManager.PlayerState.Flying && Input.GetButtonDown("ResetPlayer");
            // Set current seed if flying
            if (currentSeed == null && LocalGameManager.Singleton.playerState == LocalGameManager.PlayerState.Flying)
            {
                currentSeed = WorldManager.currentWorld.seed;
            }


            // If reset and active save point: restore to save point 
            if (isPlayerReset && hasActiveSavePoint)
            {
                RestoreToSavePoint();
            }

            // Check for save point creation (P key)
            if ((Input.GetKeyDown(KeyboardKey1) || Input.GetKeyDown(ControllerButton1)) && LocalGameManager.Singleton.playerState == LocalGameManager.PlayerState.Flying)
            {
                CreateSavePoint();
            }

            // Check for save point removal (K key)
            if ((Input.GetKeyDown(KeyboardKey2) || Input.GetKeyDown(ControllerButton2)) && hasActiveSavePoint)
            {
                RemoveSavePoint();
            }

            // Check if player entered a portal or endplane
            if (isNextWorld && hasActiveSavePoint)
            {
                currentSeed = null;
                if (hasActiveSavePoint) 
                {
                    RemoveSavePoint();
                }
            }
        }

        //Save position, rotation and speed
        private void CreateSavePoint()
        {
            GameObject player = LocalGameManager.Singleton.player;
            if (player != null)
            {
                savedPosition = player.transform.position;
                savedRotation = player.transform.rotation;
                
                PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    savedSpeed = playerMovement.currentSpeed;
                }

                hasActiveSavePoint = true;
                Debug.Log("Save point created!");
            }
        }

        private void RemoveSavePoint()
        {
            savedPosition = null;
            savedRotation = null;
            savedSpeed = null;
            hasActiveSavePoint = false;
            Debug.Log("Save point removed!");
        }

        private void RestoreToSavePoint()
        {
            GameObject player = LocalGameManager.Singleton.player;
            if (player != null)
            {
                // Restore position and rotation
                player.transform.position = savedPosition.Value;
                player.transform.rotation = savedRotation.Value;

                // Restore speed and player movement
                PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement != null && savedSpeed.HasValue)
                {
                    playerMovement.currentSpeed = savedSpeed.Value;
                }
            }
        }
     private KeyCode GetControllerButton(string button)
        {
            if (button == "B")
                return KeyCode.JoystickButton1;
            if (button == "X")
                return KeyCode.JoystickButton2;
            if (button == "Y")
                return KeyCode.JoystickButton3;
            if (button == "LB")
                return KeyCode.JoystickButton4;
            if (button == "RB")
                return KeyCode.JoystickButton5;
            if (button == "Select")
                return KeyCode.JoystickButton6;
            if (button == "L3")
                return KeyCode.JoystickButton8;
            if (button == "R3")
                return KeyCode.JoystickButton9;

            return KeyCode.None;
        }
    }
}