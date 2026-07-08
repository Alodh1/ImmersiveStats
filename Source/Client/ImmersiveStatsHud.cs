using ImmersiveStats.Debug;
using Vintagestory.API.Client;

namespace ImmersiveStats.Client;

internal sealed class ImmersiveStatsHud : HudElement
{
    private const string ComposerKey = "immersivestats-statbar";
    private readonly DebugStatBarState _debugState;
    private SegmentedStatBarElement? _barElement;

    public ImmersiveStatsHud(ICoreClientAPI capi, DebugStatBarState debugState)
        : base(capi)
    {
        _debugState = debugState;
        _debugState.Changed += OnDebugStateChanged;
        ComposeGuis();
    }

    public override double InputOrder => 1;

    public override bool Focusable => false;

    public override bool ShouldReceiveKeyboardEvents() => false;

    public override void OnOwnPlayerDataReceived() => ComposeGuis();

    public void ComposeGuis()
    {
        const float width = 440;
        const float height = 58;

        ElementBounds dialogBounds = new()
        {
            Alignment = EnumDialogArea.CenterMiddle,
            BothSizing = ElementSizing.Fixed,
            fixedWidth = width,
            fixedHeight = height,
        };

        ElementBounds barBounds = ElementBounds.Fixed(0, 0, width, height);
        _barElement = new SegmentedStatBarElement(capi, barBounds, _debugState.Current);

        Composers[ComposerKey] = capi.Gui
            .CreateCompo(ComposerKey, dialogBounds)
            .BeginChildElements(dialogBounds)
                .AddInteractiveElement(_barElement, "bar")
            .EndChildElements()
            .Compose();
    }

    public override void OnRenderGUI(float deltaTime)
    {
        if (capi.World.Player?.WorldData.CurrentGameMode == Vintagestory.API.Common.EnumGameMode.Spectator)
        {
            return;
        }

        base.OnRenderGUI(deltaTime);
    }

    public override void OnMouseDown(MouseEvent args)
    {
    }

    protected override void OnFocusChanged(bool on)
    {
    }

    public override void Dispose()
    {
        _debugState.Changed -= OnDebugStateChanged;
        base.Dispose();
    }

    private void OnDebugStateChanged() => _barElement?.SetState(_debugState.Current);
}
