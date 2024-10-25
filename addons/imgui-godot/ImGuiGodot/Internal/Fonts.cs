#nullable enable
using Godot;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ImGuiGodot.Internal;

internal sealed class Fonts
{
    private Texture2D? _fontTexture;

    private sealed class FontParams
    {
        public FontFile? Font { get; init; }
        public int FontSize { get; init; }
        public bool Merge { get; init; }
        public ushort[]? Ranges { get; init; }
    }
    private readonly List<FontParams> _fontConfiguration = [];

    public Fonts()
    {
        _fontConfiguration.Clear();
    }

    public void ResetFonts()
    {
        var io = ImGui.GetIO();
        io.Fonts.Clear();
        unsafe { io.FontDefault = null; }
        _fontConfiguration.Clear();
    }

    public void AddFont(FontFile? fontData, int fontSize, bool merge, ushort[]? ranges)
    {
        _fontConfiguration.Add(
            new FontParams
            {
                Font = fontData,
                FontSize = fontSize,
                Merge = merge,
                Ranges = ranges
            });
    }

    private static unsafe void AddFontToAtlas(FontParams fp, float scale)
    {
        var io = ImGui.GetIO();
        int fontSize = (int)(fp.FontSize * scale);
        ImFontConfig* fc = ImGui.ImFontConfig();

        if (fp.Merge)
        {
            fc->MergeMode = 1;
        }

        if (fp.Font == null)
        {
            // default font
            var fcptr = new ImFontConfigPtr(fc)
            {
                SizePixels = fontSize,
                OversampleH = 1,
                OversampleV = 1,
                PixelSnapH = true
            };
            io.Fonts.AddFontDefault(fc);
        }
        else
        {
            string name = $"{System.IO.Path.GetFileName(fp.Font.ResourcePath)}, {fontSize}px";
            for (int i = 0; i < name.Length && i < 40; ++i)
            {
                ImGuiExtensions.SetFontConfigName(fc, i, Convert.ToByte(name[i]));
            }

            int len = fp.Font.Data.Length;
            // let ImGui manage this memory
            IntPtr p = (IntPtr)ImGui.MemAlloc((uint)len);
            Marshal.Copy(fp.Font.Data, 0, p, len);
            if (fp.Ranges == null)
            {
                io.Fonts.AddFontFromMemoryTTF(
                    p.ToPointer(),
                    len,
                    fontSize,
                    fc,
                    io.Fonts.GetGlyphRangesChineseFull());
            }
            else
            {
                fixed (ushort* pranges = fp.Ranges)
                {
                    io.Fonts.AddFontFromMemoryTTF(p.ToPointer(), len, fontSize, fc,
                        (char*)((nint)pranges).ToPointer());
                }
            }
        }

        if (fp.Merge)
            io.Fonts.Build();

        ImGui.Destroy(fc);
    }

    private static unsafe void ResetStyle()
    {
        ImGuiStylePtr defaultStyle = new(ImGui.ImGuiStyle());
        ImGuiStylePtr style = ImGui.GetStyle();

        style.WindowPadding = defaultStyle.WindowPadding;
        style.WindowRounding = defaultStyle.WindowRounding;
        style.WindowMinSize = defaultStyle.WindowMinSize;
        style.ChildRounding = defaultStyle.ChildRounding;
        style.PopupRounding = defaultStyle.PopupRounding;
        style.FramePadding = defaultStyle.FramePadding;
        style.FrameRounding = defaultStyle.FrameRounding;
        style.ItemSpacing = defaultStyle.ItemSpacing;
        style.ItemInnerSpacing = defaultStyle.ItemInnerSpacing;
        style.CellPadding = defaultStyle.CellPadding;
        style.TouchExtraPadding = defaultStyle.TouchExtraPadding;
        style.IndentSpacing = defaultStyle.IndentSpacing;
        style.ColumnsMinSpacing = defaultStyle.ColumnsMinSpacing;
        style.ScrollbarSize = defaultStyle.ScrollbarSize;
        style.ScrollbarRounding = defaultStyle.ScrollbarRounding;
        style.GrabMinSize = defaultStyle.GrabMinSize;
        style.GrabRounding = defaultStyle.GrabRounding;
        style.LogSliderDeadzone = defaultStyle.LogSliderDeadzone;
        style.TabRounding = defaultStyle.TabRounding;
        style.TabMinWidthForCloseButton = defaultStyle.TabMinWidthForCloseButton;
        style.SeparatorTextPadding = defaultStyle.SeparatorTextPadding;
        style.DockingSeparatorSize = defaultStyle.DockingSeparatorSize;
        style.DisplayWindowPadding = defaultStyle.DisplayWindowPadding;
        style.DisplaySafeAreaPadding = defaultStyle.DisplaySafeAreaPadding;
        style.MouseCursorScale = defaultStyle.MouseCursorScale;

        defaultStyle.Destroy();
    }

    public unsafe void RebuildFontAtlas(float scale)
    {
        var io = ImGui.GetIO();
        int fontIndex = -1;

        // save current font index
        if (!io.FontDefault.IsNull)
        {
            for (int i = 0; i < io.Fonts.Fonts.Size; ++i)
            {
                if (io.Fonts.Fonts[i] == io.FontDefault)
                {
                    fontIndex = i;
                    break;
                }
            }
            io.FontDefault = null;
        }
        io.Fonts.Clear();

        foreach (var fontParams in _fontConfiguration)
        {
            AddFontToAtlas(fontParams, scale);
        }

        byte* pixelData = default;
        int width = 0; int height = 0;
        int bytesPerPixel = 0;
        io.Fonts.GetTexDataAsRGBA32(ref pixelData, ref width, ref height, ref bytesPerPixel);
        // io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

        byte[] pixels = new byte[width * height * bytesPerPixel];
        Marshal.Copy((IntPtr)pixelData, pixels, 0, pixels.Length);

        var img = Image.CreateFromData(width, height, false, Image.Format.Rgba8, pixels);

        _fontTexture = ImageTexture.CreateFromImage(img);
        io.Fonts.SetTexID(_fontTexture.GetRid().Id);
        io.Fonts.ClearTexData();

        // maintain selected font when rescaling
        if (fontIndex != -1 && fontIndex < io.Fonts.Fonts.Size)
        {
            io.FontDefault = io.Fonts.Fonts[fontIndex];
        }

        ResetStyle();
        ImGui.GetStyle().ScaleAllSizes(scale);
    }
}
