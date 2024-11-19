using BepInEx;
using GHPC;
using GHPC.Camera;
using GHPC.Player;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GHPCESP
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class GHPCESP : BaseUnityPlugin
    {
        // Basic plugin information
        public const string pluginGuid = "ghpcesp.ghpc.mod";
        public const string pluginName = "GHPCESP";
        public const string pluginVersion = "1.0.0.0";

        // Singleton instance
        public static GHPCESP Instance;

        // Lists to track objects in each round of the game
        public List<Unit> Units = new List<Unit>();

        // ESP toggles
        private bool EnableUnitsESP { get; set; } = false;

        public void Awake()
        {
            if (Instance != null)
            {
                Logger.LogError($"Another instance of {pluginName} is already loaded");
                Destroy(this);
                return;
            }
            Instance = this;
            Logger.LogInfo($"{pluginName} {pluginVersion} loaded");

            // Enable hooks for tracking game object spawning
            Harmony harmony = new Harmony(Guid.NewGuid().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnGUI()
        {
            // remove the destroyed objects
            Units = Units.Where(obj => obj != null).ToList();

            if (EnableUnitsESP)
            {
                Render.DrawString(new Vector2(5, 2), "Unit ESP Enabled", Color.red, false);
                DrawUnitESP();
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                EnableUnitsESP = !EnableUnitsESP;
            }
        }

        private void DrawUnitESP()
        {
            if (PlayerInput.Instance == null)
            {
                return;
            }

            foreach (Unit unit in Units)
            {
                // Skip the current player unit
                if (unit == PlayerInput.Instance.CurrentPlayerUnit)
                {
                    continue;
                }

                (Vector3 componentTopPos, Vector3 componentBottomPos, float componentWidth) = GetComponentDimensions(unit, false, CameraManager.MainCam);

                if (componentTopPos.z > 0f && componentBottomPos.z > 0f)
                {
                    Color highlightColor;
                    switch (unit.Allegiance)
                    {
                        case GHPC.Faction.Blue:
                            highlightColor = Color.blue;
                            break;
                        case GHPC.Faction.Red:
                            highlightColor = Color.red;
                            break;
                        case GHPC.Faction.Green:
                            highlightColor = Color.green;
                            break;
                        case GHPC.Faction.Neutral:
                            highlightColor = Color.yellow;
                            break;
                        default:
                            highlightColor = Color.white;
                            break;
                    }

                    if (unit.Neutralized)
                    {
                        highlightColor = Color.gray;
                    }

                    DrawBoxESP(componentBottomPos, componentTopPos, highlightColor, componentWidth, true, unit.FriendlyName);
                }
            }
        }

        private (Vector3, Vector3, float) GetComponentDimensions(Component component, bool pivotPoint, Camera camera)
        {
            // Get the initial position of the component.
            Vector3 componentPos = component.transform.position;

            // Find all Colliders in children to calculate bounds.
            Collider[] colliders = component.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
            {
                Logger.LogWarning($"{component.name} HAS NO COLLIDERS");

                // Return the position and 0 width if no colliders are found.
                return (componentPos, componentPos, 0f);
            }

            // Combine all collider bounds into one
            Bounds combinedBounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                combinedBounds.Encapsulate(colliders[i].bounds);
            }

            // Compute top and bottom positions based on the combined bounds
            Vector3 componentTopPos = componentPos;
            Vector3 componentBottomPos = componentPos;

            if (pivotPoint)
            {
                // Adjust based on the pivot being at the center of the component.
                float pivotOffsetY = componentPos.y - combinedBounds.center.y;
                componentTopPos.y = combinedBounds.max.y + pivotOffsetY;
                componentBottomPos.y = combinedBounds.min.y + pivotOffsetY;
            }
            else
            {
                // Use the exact bounds without any pivot adjustments.
                componentTopPos.y = combinedBounds.max.y;
                componentBottomPos.y = combinedBounds.min.y;
            }

            // Get the corners of the combined bounds
            Vector3[] corners = new Vector3[8];
            Vector3 center = combinedBounds.center;
            Vector3 extents = combinedBounds.extents;

            corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            corners[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
            corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
            corners[3] = center + new Vector3(-extents.x, extents.y, extents.z);
            corners[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
            corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
            corners[6] = center + new Vector3(extents.x, extents.y, -extents.z);
            corners[7] = center + new Vector3(extents.x, extents.y, extents.z);

            // Convert the corners to screen space
            Vector3[] screenCorners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                screenCorners[i] = camera.WorldToScreenPoint(corners[i]);
            }

            // Find the min and max screen X positions
            float minScreenX = float.MaxValue;
            float maxScreenX = float.MinValue;
            foreach (Vector3 screenCorner in screenCorners)
            {
                minScreenX = Mathf.Min(minScreenX, screenCorner.x);
                maxScreenX = Mathf.Max(maxScreenX, screenCorner.x);
            }

            // Calculate the screen-space width
            float widthScreen = maxScreenX - minScreenX;

            return (camera.WorldToScreenPoint(componentTopPos), camera.WorldToScreenPoint(componentBottomPos), widthScreen);
        }

        private void DrawBoxESP(Vector3 bottomPos, Vector3 topPos, Color color, float width, bool snapLine, string label)
        {
            // Scale the pixel's position on screen
            bottomPos = ScaleVector(bottomPos);
            topPos = ScaleVector(topPos);

            // Check if the object is within the screen
            if (bottomPos.x < 0 || bottomPos.x > Screen.width || bottomPos.y < 0 || bottomPos.y > Screen.height)
            {
                return;
            }

            // Flip the y-coordinates after scaling
            topPos.y = Screen.height - topPos.y;
            bottomPos.y = Screen.height - bottomPos.y;

            // Calculate the width of the box
            float height = topPos.y - bottomPos.y;
            if (width < Math.Abs(height))
            {
                width = Math.Abs(height);
            }

            // Draw the ESP box
            Render.DrawBox(bottomPos.x - (width / 2), bottomPos.y, width, height, color, 2f, label);

            // Snapline
            if (snapLine)
            {
                Render.DrawLine(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(bottomPos.x, bottomPos.y), color, 2f);
            }
        }

        // Scale the pixel's position on screen according to resolution vs default resolution (860x540)
        public static Vector3 ScaleVector(Vector3 position)
        {
            return new Vector3(
                position.x / CameraManager.MainCam.pixelWidth * Screen.width,
                position.y / CameraManager.MainCam.pixelHeight * Screen.height,
                position.z
            );
        }
    }
}
