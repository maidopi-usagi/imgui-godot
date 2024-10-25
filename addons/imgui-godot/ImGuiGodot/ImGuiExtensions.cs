using Godot;
using Hexa.NET.ImGui;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace ImGuiGodot;

public static class ImGuiExtensions
{
    /// <summary>
    /// Extension method to translate between <see cref="Key"/> and <see cref="ImGuiKey"/>
    /// </summary>
    public static ImGuiKey ToImGuiKey(this Key key)
    {
        return Internal.Input.ConvertKey(key);
    }

    /// <summary>
    /// Extension method to translate between <see cref="JoyButton"/> and <see cref="ImGuiKey"/>
    /// </summary>
    public static ImGuiKey ToImGuiKey(this JoyButton button)
    {
        return Internal.Input.ConvertJoyButton(button);
    }

    /// <summary>
    /// Convert <see cref="Color"/> to ImGui color RGBA
    /// </summary>
    public static Vector4 ToVector4(this Color color)
    {
        return new Vector4(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// Convert <see cref="Color"/> to ImGui color RGB
    /// </summary>
    public static Vector3 ToVector3(this Color color)
    {
        return new Vector3(color.R, color.G, color.B);
    }

    /// <summary>
    /// Convert RGB <see cref="Vector3"/> to <see cref="Color"/>
    /// </summary>
    public static Color ToColor(this Vector3 vec)
    {
        return new Color(vec.X, vec.Y, vec.Z);
    }

    /// <summary>
    /// Convert RGBA <see cref="Vector4"/> to <see cref="Color"/>
    /// </summary>
    public static Color ToColor(this Vector4 vec)
    {
        return new Color(vec.X, vec.Y, vec.Z, vec.W);
    }

    /// <summary>
    /// Set IniFilename, converting Godot path to native
    /// </summary>
    public static void SetIniFilename(this ImGuiIOPtr io, string fileName)
    {
        _ = io;
        ImGuiGD.SetIniFilename(fileName);
    }

    public static unsafe void SetFontConfigName(ImFontConfig* fontConfig, int index, byte name)
    {
        switch (index)
        {
            case 0: fontConfig->Name_0 = name; break;
            case 1: fontConfig->Name_1 = name; break;
            case 2: fontConfig->Name_2 = name; break;
            case 3: fontConfig->Name_3 = name; break;
            case 4: fontConfig->Name_4 = name; break;
            case 5: fontConfig->Name_5 = name; break;
            case 6: fontConfig->Name_6 = name; break;
            case 7: fontConfig->Name_7 = name; break;
            case 8: fontConfig->Name_8 = name; break;
            case 9: fontConfig->Name_9 = name; break;
            case 10: fontConfig->Name_10 = name; break;
            case 11: fontConfig->Name_11 = name; break;
            case 12: fontConfig->Name_12 = name; break;
            case 13: fontConfig->Name_13 = name; break;
            case 14: fontConfig->Name_14 = name; break;
            case 15: fontConfig->Name_15 = name; break;
            case 16: fontConfig->Name_16 = name; break;
            case 17: fontConfig->Name_17 = name; break;
            case 18: fontConfig->Name_18 = name; break;
            case 19: fontConfig->Name_19 = name; break;
            case 20: fontConfig->Name_20 = name; break;
            case 21: fontConfig->Name_21 = name; break;
            case 22: fontConfig->Name_22 = name; break;
            case 23: fontConfig->Name_23 = name; break;
            case 24: fontConfig->Name_24 = name; break;
            case 25: fontConfig->Name_25 = name; break;
            case 26: fontConfig->Name_26 = name; break;
            case 27: fontConfig->Name_27 = name; break;
            case 28: fontConfig->Name_28 = name; break;
            case 29: fontConfig->Name_29 = name; break;
            case 30: fontConfig->Name_30 = name; break;
            case 31: fontConfig->Name_31 = name; break;
            case 32: fontConfig->Name_32 = name; break;
            case 33: fontConfig->Name_33 = name; break;
            case 34: fontConfig->Name_34 = name; break;
            case 35: fontConfig->Name_35 = name; break;
            case 36: fontConfig->Name_36 = name; break;
            case 37: fontConfig->Name_37 = name; break;
            case 38: fontConfig->Name_38 = name; break;
            case 39: fontConfig->Name_39 = name; break;
        }
    }
}
