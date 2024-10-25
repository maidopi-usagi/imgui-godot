using Godot;
using Hexa.NET.ImGui;
using System.Runtime.InteropServices;
using System;

namespace ImGuiGodot;

public partial class ImGuiSync : GodotObject
{
    public static readonly StringName GetImGuiPtrs = "GetImGuiPtrs";

    public static unsafe void SyncPtrs()
    {
        GodotObject gd = Engine.GetSingleton("ImGuiGD");
        long[] ptrs = (long[])gd.Call(GetImGuiPtrs,
            (IntPtr)ImGui.GetVersion(),
            Marshal.SizeOf<ImGuiIO>(),
            Marshal.SizeOf<ImDrawVert>(),
            sizeof(ushort),
            sizeof(ushort)
            );

        if (ptrs.Length != 3)
        {
            throw new NotSupportedException("ImGui version mismatch");
        }

        checked
        {
            var allocFunc = Marshal.GetDelegateForFunctionPointer<ImGuiMemAllocFunc>((nint)ptrs[1]);
            var freeFunc = Marshal.GetDelegateForFunctionPointer<ImGuiMemFreeFunc>((nint)ptrs[2]);
            ImGui.SetCurrentContext(new ImGuiContextPtr{Handle = (ImGuiContext*)ptrs[0]});
            ImGui.SetAllocatorFunctions(allocFunc, freeFunc);
        }
    }
}
