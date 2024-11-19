using UnityEngine;

namespace GHPCESP
{
    public class Render : MonoBehaviour
    {
        public static GUIStyle StringStyle { get; set; } = new GUIStyle(GUI.skin.label);
        public static Texture2D lineTex = new Texture2D(1, 1);

        public static void DrawString(Vector2 position, string label, Color color, bool centered = false)
        {
            // Backup the GUI color
            Color colorBackup = GUI.color;

            // Set the string style color
            GUI.color = color;

            // Draw the string
            GUIContent content = new GUIContent(label);
            Vector2 size = StringStyle.CalcSize(content);
            Vector2 upperLeft = centered ? position - (size / 2f) : position;
            GUI.Label(new Rect(upperLeft, size), content);

            // Restore the GUI color
            GUI.color = colorBackup;
        }

        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            // Backup the GUI matrix and color
            Matrix4x4 matrixBackup = GUI.matrix;
            Color colorBackup = GUI.color;

            // Set the line texture color
            GUI.color = color;

            float num = Vector3.Angle(pointB - pointA, Vector2.right);
            if (pointA.y > pointB.y)
            {
                num = -num;
            }

            // If the line has no length, do not draw it
            if ((pointB - pointA).magnitude == 0)
            {
                return;
            }

            // Draw the line
            GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
            GUIUtility.RotateAroundPivot(num, pointA);
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1f, 1f), lineTex);

            // Restore the GUI matrix and color
            GUI.matrix = matrixBackup;
            GUI.color = colorBackup;
        }

        public static void DrawBox(float x, float y, float w, float h, Color color, float thickness, string label)
        {
            float xPlusW = x + w;
            float yPlusH = y + h;

            // Draw the label
            if (label != null)
            {
                DrawString(new Vector2(x + 5, yPlusH), label, color, false);
            }

            // Draw the four sides of the box
            DrawLine(new Vector2(x, y), new Vector2(xPlusW, y), color, thickness);
            DrawLine(new Vector2(x, y), new Vector2(x, yPlusH), color, thickness);
            DrawLine(new Vector2(xPlusW, y), new Vector2(xPlusW, yPlusH), color, thickness);
            DrawLine(new Vector2(x, yPlusH), new Vector2(xPlusW, yPlusH), color, thickness);
        }
    }
}
