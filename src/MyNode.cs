using Godot;
using ImGuiGodot;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;

namespace DemoProject;

public partial class MyNode : Node
{
    private Window _window = null!;
    private CheckBox _checkBox = null!;

    public override void _Ready()
    {
        _window = GetWindow();
        GetNode<Button>("%Button1").Pressed += OnButton1Pressed;
        GetNode<Button>("%Button2").Pressed += OnButton2Pressed;
        GetNode<Button>("%Button3").Pressed += OnButton3Pressed;
        _checkBox = GetNode<CheckBox>("%CheckBox");
        _checkBox.ButtonPressed = ImGui.GetIO().ConfigFlags
            .HasFlag(ImGuiConfigFlags.ViewportsEnable);
        _checkBox.Pressed += OnCheckBoxPressed;

        if (DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Disabled)
        {
            int refreshRate = (int)DisplayServer.ScreenGetRefreshRate();
            Engine.MaxFps = refreshRate > 0 ? refreshRate : 60;
        }
    }

    public override void _Process(double delta)
    {
        ImGui.ShowDemoWindow();
        ImPlot.ShowDemoWindow();
        ImGui.Begin("ImNodes");
        ImNodes.BeginNodeEditor();
        ImNodes.MiniMap();
        ImNodes.BeginNode(5);
        ImNodes.BeginNodeTitleBar();
        ImGui.TextUnformatted("Hello Node!");
        ImNodes.EndNodeTitleBar();
        ImNodes.BeginOutputAttribute(1);
        ImGui.Text("Out");
        ImNodes.EndOutputAttribute();
        ImNodes.EndNode();
        ImNodes.EndNodeEditor();
        ImGui.End();
    }

    private void OnButton1Pressed()
    {
        GetTree().ChangeSceneToFile("res://data/demo2.tscn");
    }

    private void OnButton2Pressed()
    {
        Window newWindow = new()
        {
            Position = new(
                System.Random.Shared.Next(100, 200),
                System.Random.Shared.Next(100, 200)),
            Size = new(640, 480)
        };
        newWindow.CloseRequested += newWindow.QueueFree;
        AddChild(newWindow);
        ImGuiGD.SetMainViewport(newWindow);
    }
    private void OnButton3Pressed()
    {
        GetTree().ChangeSceneToFile("res://data/gui_in_3d.tscn");
    }

    private void OnCheckBoxPressed()
    {
        var io = ImGui.GetIO();
        if (_checkBox.ButtonPressed)
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        else
            io.ConfigFlags &= ~ImGuiConfigFlags.ViewportsEnable;
    }

    private void OnContentScaleCIE()
    {
        _window.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
        _window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
    }

    private void OnContentScaleDisabled()
    {
        _window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
    }
}
