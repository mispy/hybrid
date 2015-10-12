using UnityEngine;
using System.Collections;

public static class SpaceColor {
    public static Color hostile = SpaceColor.RGB(253, 134, 3);
    public static Color neutral = SpaceColor.RGB(0, 203, 231);
    public static Color friendly = SpaceColor.RGB(0, 218, 60);

    public static Color RGB(int r, int g, int b) {
        return new Color(r/255f, g/255f, b/255f);
    }

    public static Color Hex(string hex) {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}
