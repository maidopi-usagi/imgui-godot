using Godot;
using Hexa.NET.ImGui;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Vector2 = System.Numerics.Vector2;

namespace ImGuiGodot.Internal;

internal sealed unsafe class GodotImGuiWindow : IDisposable
{
    private readonly GCHandle _gcHandle;
    private readonly ImGuiViewportPtr _vp;
    private readonly Window _window;
    private readonly bool _isOwnedWindow = false;

    /// <summary>
    /// sub window
    /// </summary>
    public GodotImGuiWindow(ImGuiViewportPtr vp)
    {
        _gcHandle = GCHandle.Alloc(this);
        _vp = vp;
        _vp.PlatformUserData = ((IntPtr)_gcHandle).ToPointer();
        _isOwnedWindow = true;

        Rect2I winRect = new(_vp.Pos.ToVector2I(), _vp.Size.ToVector2I());

        Window mainWindow = ImGuiController.Instance.GetWindow();
        if (mainWindow.GuiEmbedSubwindows)
        {
            if ((bool)ProjectSettings.GetSetting("display/window/subwindows/embed_subwindows"))
            {
                GD.PushWarning(
                    "ImGui Viewports: 'display/window/subwindows/embed_subwindows' needs to be disabled");
            }
            mainWindow.GuiEmbedSubwindows = false;
        }

        _window = new Window()
        {
            Borderless = true,
            Position = winRect.Position,
            Size = winRect.Size,
            Transparent = true,
            TransparentBg = true,
            AlwaysOnTop = vp.Flags.HasFlag(ImGuiViewportFlags.TopMost),
            Unfocusable = vp.Flags.HasFlag(ImGuiViewportFlags.NoFocusOnClick)
        };

        _window.CloseRequested += () => _vp.PlatformRequestClose = true;
        _window.SizeChanged += () => _vp.PlatformRequestResize = true;
        _window.WindowInput += ImGuiController.WindowInputCallback;

        ImGuiController.Instance.AddChild(_window);

        // need to do this after AddChild
        _window.Transparent = true;

        // it's our window, so just draw directly to the root viewport
        var vprid = _window.GetViewportRid();
        _vp.RendererUserData = ((IntPtr)vprid.Id).ToPointer();
        _vp.PlatformHandle = ((IntPtr)_window.GetWindowId()).ToPointer();

        State.Instance.Renderer.InitViewport(vprid);
        RenderingServer.ViewportSetTransparentBackground(_window.GetViewportRid(), true);
    }

    /// <summary>
    /// main window
    /// </summary>
    public GodotImGuiWindow(ImGuiViewportPtr vp, Window gw, Rid mainSubViewport)
    {
        _gcHandle = GCHandle.Alloc(this);
        _vp = vp;
        _vp.PlatformUserData = ((IntPtr)_gcHandle).ToPointer();
        _window = gw;
        _vp.RendererUserData = ((IntPtr)mainSubViewport.Id).ToPointer();
    }

    public void Dispose()
    {
        if (_gcHandle.IsAllocated)
        {
            if (_isOwnedWindow)
            {
                State.Instance.Renderer
                    .CloseViewport(Util.ConstructRid((ulong)_vp.RendererUserData));
                _window.GetParent().RemoveChild(_window);
                _window.Free();
            }
            _gcHandle.Free();
        }
    }

    public void ShowWindow()
    {
        _window.Show();
    }

    public void SetWindowPos(Vector2I pos)
    {
        _window.Position = pos;
    }

    public Vector2I GetWindowPos()
    {
        return _window.Position;
    }

    public void SetWindowSize(Vector2I size)
    {
        _window.Size = size;
    }

    public Vector2I GetWindowSize()
    {
        return _window.Size;
    }

    public void SetWindowFocus()
    {
        _window.GrabFocus();
    }

    public bool GetWindowFocus()
    {
        return _window.HasFocus();
    }

    public bool GetWindowMinimized()
    {
        return _window.Mode.HasFlag(Window.ModeEnum.Minimized);
    }

    public void SetWindowTitle(string title)
    {
        _window.Title = title;
    }
}

internal static class ViewportsExts
{
    internal static Vector2 ToImVec2(this Vector2I v)
    {
        return new Vector2(v.X, v.Y);
    }

    internal static Vector2I ToVector2I(this Vector2 v)
    {
        return new Vector2I((int)v.X, (int)v.Y);
    }
}

[SuppressMessage("Roslynator", "RCS0056:A line is too long")]
internal sealed partial class Viewports
{
    [LibraryImport("cimgui")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static unsafe partial void ImGuiPlatformIO_Set_Platform_GetWindowPos(
        ImGuiPlatformIO* platform_io,
        IntPtr funcPtr);
    [LibraryImport("cimgui")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static unsafe partial void ImGuiPlatformIO_Set_Platform_GetWindowSize(
        ImGuiPlatformIO* platform_io,
        IntPtr funcPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_CreateWindow(ImGuiViewportPtr vp);
    private static readonly Platform_CreateWindow _createWindow = Godot_CreateWindow;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_DestroyWindow(ImGuiViewportPtr vp);
    private static readonly Platform_DestroyWindow _destroyWindow = Godot_DestroyWindow;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_ShowWindow(ImGuiViewportPtr vp);
    private static readonly Platform_ShowWindow _showWindow = Godot_ShowWindow;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_SetWindowPos(ImGuiViewportPtr vp, Vector2 pos);
    private static readonly Platform_SetWindowPos _setWindowPos = Godot_SetWindowPos;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_GetWindowPos(ImGuiViewportPtr vp, out Vector2 pos);
    private static readonly Platform_GetWindowPos _getWindowPos = Godot_GetWindowPos;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_SetWindowSize(ImGuiViewportPtr vp, Vector2 pos);
    private static readonly Platform_SetWindowSize _setWindowSize = Godot_SetWindowSize;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_GetWindowSize(ImGuiViewportPtr vp, out Vector2 size);
    private static readonly Platform_GetWindowSize _getWindowSize = Godot_GetWindowSize;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_SetWindowFocus(ImGuiViewportPtr vp);
    private static readonly Platform_SetWindowFocus _setWindowFocus = Godot_SetWindowFocus;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool Platform_GetWindowFocus(ImGuiViewportPtr vp);
    private static readonly Platform_GetWindowFocus _getWindowFocus = Godot_GetWindowFocus;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool Platform_GetWindowMinimized(ImGuiViewportPtr vp);
    private static readonly Platform_GetWindowMinimized _getWindowMinimized
        = Godot_GetWindowMinimized;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Platform_SetWindowTitle(ImGuiViewportPtr vp, string title);
    private static readonly Platform_SetWindowTitle _setWindowTitle = Godot_SetWindowTitle;

    private GodotImGuiWindow _mainWindow = null!;

    private static unsafe void UpdateMonitors()
    {
        var pio = ImGui.GetPlatformIO();
        int screenCount = DisplayServer.GetScreenCount();

        int bytes = screenCount * sizeof(ImGuiPlatformMonitor);
        pio.Monitors = new ImVector<ImGuiPlatformMonitor>(screenCount, screenCount, (ImGuiPlatformMonitor*)ImGui.MemAlloc((uint)bytes));

        for (int i = 0; i < screenCount; ++i)
        {
            var monitor = pio.Monitors[i];
            monitor.MainPos = DisplayServer.ScreenGetPosition(i).ToImVec2();
            monitor.MainSize = DisplayServer.ScreenGetSize(i).ToImVec2();
            monitor.DpiScale = DisplayServer.ScreenGetScale(i);

            var r = DisplayServer.ScreenGetUsableRect(i);
            monitor.WorkPos = r.Position.ToImVec2();
            monitor.WorkSize = r.Size.ToImVec2();
        }

        // TODO: add monitor if headless
    }

    private static unsafe void InitPlatformInterface()
    {
        var pio = ImGui.GetPlatformIO();

        pio.PlatformCreateWindow = Marshal.GetFunctionPointerForDelegate(_createWindow).ToPointer();
        pio.PlatformDestroyWindow = Marshal.GetFunctionPointerForDelegate(_destroyWindow).ToPointer();
        pio.PlatformShowWindow = Marshal.GetFunctionPointerForDelegate(_showWindow).ToPointer();
        pio.PlatformSetWindowPos = Marshal.GetFunctionPointerForDelegate(_setWindowPos).ToPointer();
        //pio->Platform_GetWindowPos = Marshal.GetFunctionPointerForDelegate(_getWindowPos);
        pio.PlatformSetWindowSize = Marshal.GetFunctionPointerForDelegate(_setWindowSize).ToPointer();
        //pio->Platform_GetWindowSize = Marshal.GetFunctionPointerForDelegate(_getWindowSize);
        pio.PlatformSetWindowFocus = Marshal.GetFunctionPointerForDelegate(_setWindowFocus).ToPointer();
        pio.PlatformGetWindowFocus = Marshal.GetFunctionPointerForDelegate(_getWindowFocus).ToPointer();
        pio.PlatformGetWindowMinimized = Marshal.GetFunctionPointerForDelegate(_getWindowMinimized).ToPointer();
        pio.PlatformSetWindowTitle = Marshal.GetFunctionPointerForDelegate(_setWindowTitle).ToPointer();

        ImGuiPlatformIO_Set_Platform_GetWindowPos(pio, Marshal.GetFunctionPointerForDelegate(_getWindowPos));
        ImGuiPlatformIO_Set_Platform_GetWindowSize(pio, Marshal.GetFunctionPointerForDelegate(_getWindowSize));
    }

    public Viewports()
    {
        InitPlatformInterface();
        UpdateMonitors();
    }

    public void SetMainWindow(Window window, Rid mainSubViewport)
    {
        _mainWindow?.Dispose();
        _mainWindow = new GodotImGuiWindow(ImGui.GetMainViewport(), window, mainSubViewport);
    }

    private static void Godot_CreateWindow(ImGuiViewportPtr vp)
    {
        _ = new GodotImGuiWindow(vp);
    }

    private static unsafe void Godot_DestroyWindow(ImGuiViewportPtr vp)
    {
        if ((IntPtr)vp.PlatformUserData != IntPtr.Zero)
        {
            var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
            window.Dispose();
            vp.PlatformUserData = (IntPtr.Zero).ToPointer();
        }
    }

    private static unsafe void Godot_ShowWindow(ImGuiViewportPtr vp)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        window.ShowWindow();
    }

    private static unsafe void Godot_SetWindowPos(ImGuiViewportPtr vp, Vector2 pos)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        window.SetWindowPos(pos.ToVector2I());
    }

    private static unsafe void Godot_GetWindowPos(ImGuiViewportPtr vp, out Vector2 pos)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        pos = window.GetWindowPos().ToImVec2();
    }

    private static unsafe void Godot_SetWindowSize(ImGuiViewportPtr vp, Vector2 size)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        window.SetWindowSize(size.ToVector2I());
    }

    private static unsafe void Godot_GetWindowSize(ImGuiViewportPtr vp, out Vector2 size)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        size = window.GetWindowSize().ToImVec2();
    }

    private static unsafe void Godot_SetWindowFocus(ImGuiViewportPtr vp)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        window.SetWindowFocus();
    }

    private static unsafe bool Godot_GetWindowFocus(ImGuiViewportPtr vp)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        return window.GetWindowFocus();
    }

    private static unsafe bool Godot_GetWindowMinimized(ImGuiViewportPtr vp)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        return window.GetWindowMinimized();
    }

    private static unsafe void Godot_SetWindowTitle(ImGuiViewportPtr vp, string title)
    {
        var window = (GodotImGuiWindow)GCHandle.FromIntPtr((IntPtr)vp.PlatformUserData).Target!;
        window.SetWindowTitle(title);
    }
}
