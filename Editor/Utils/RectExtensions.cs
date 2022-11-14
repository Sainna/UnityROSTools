using UnityEditor;
using UnityEngine;

namespace Sainna.Utils.Extensions
{
    public static class RectExtensions
    {
        public static Rect AddYPositionLine(this Rect r, int nmbrOfLine = 1)
        {
            r.position = new Vector2(r.position.x, r.position.y + (EditorGUIUtility.singleLineHeight+ EditorGUIUtility.standardVerticalSpacing) * nmbrOfLine);
            return r;
        }
        
        public static Rect AddLinesHeight(this Rect r, int nmbrOfLine = 1)
        {
            r.height = (EditorGUIUtility.singleLineHeight * nmbrOfLine);
            return r;
        }
    }
}