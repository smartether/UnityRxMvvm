#define __Match_COLOR_DOT__
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame.Util
{
    public static class MathUtil
    {
        public static Vector2 Pos2UV(Vector2 pos, float width, float height)
        {
            float u = pos.x / width;
            float v = (height - pos.y) / height;
            
            return new Vector2(u, v);
        }

        //计算两个颜色是否匹配度够 浮点数也能用
        public static bool IsMatchColor(Color color, Color color1)
        {
#if __Match_COLOR_DOT__
            Vector3 a = new Vector3(color.r, color.g, color.b) * 2 - Vector3.one;
            Vector3 b = new Vector3(color1.r, color1.g, color1.b) * 2 - Vector3.one;

            var colorDot = Vector3.Dot(a.normalized, b.normalized);
            return colorDot > 0.9f;
#else
            Vector3 a = new Vector3(color.r, color.g, color.b);
            Vector3 b = new Vector3(color1.r, color1.g, color1.b);

            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z) < 0.1f;
#endif
        }

        //获取一个和颜色列表中最接近的颜色
        public static int MatchColorIdx(IList<Color> colors, Color color)
        {
            for (int i = 0,c = colors.Count; i < c; i++)
            {
                if (IsMatchColor(colors[i], color))
                    return i;
            }
            return -1;
        }
    }
}
