using ImmersiveStats.Debug;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace ImmersiveStats.Client;

internal sealed class ImmersiveStatsDebugHud : HudElement
{
    private const string ComposerKey = "immersivestats-debug";
    private const double Width = 320;
    private const double Height = 154;

    private readonly ImmersiveStatsClientConfig _config;
    private readonly DebugStatBarState _debugState;
    private readonly Action _onDebugChanged;

    public ImmersiveStatsDebugHud(ICoreClientAPI capi, ImmersiveStatsClientConfig config, DebugStatBarState debugState, Action onDebugChanged)
        : base(capi)
    {
        _config = config;
        _debugState = debugState;
        _onDebugChanged = onDebugChanged;
        ComposeGuis();
    }

    public override double InputOrder => 0.25;

    public override bool Focusable => true;

    public override bool PrefersUngrabbedMouse => true;

    public override bool ShouldReceiveKeyboardEvents() => false;

    public void ComposeGuis()
    {
        DisposeComposer();

        double scale = RuntimeEnv.GUIScale;
        double viewportWidth = capi.Render.FrameWidth > 0 ? capi.Render.FrameWidth / scale : 1280;
        double viewportHeight = capi.Render.FrameHeight > 0 ? capi.Render.FrameHeight / scale : 720;
        HudRect barRect = _config.GetBarRect();
        double x = Math.Min(Math.Max(0, barRect.X), Math.Max(0, viewportWidth - Width));
        double y = barRect.Y + barRect.Height + 8;
        if (y + Height > viewportHeight)
        {
            y = barRect.Y - Height - 8;
        }

        y = Math.Min(Math.Max(0, y), Math.Max(0, viewportHeight - Height));

        ElementBounds dialogBounds = ElementBounds.Fixed(EnumDialogArea.LeftTop, x, y, Width, Height).WithParent(capi.Gui.WindowBounds);
        ElementBounds bgBounds = ElementBounds.Fixed(0, 0, Width, Height);
        GuiComposer composer = capi.Gui.CreateCompo(ComposerKey, dialogBounds)
            .AddShadedDialogBG(bgBounds, false, 4, 0.78f)
            .BeginChildElements(bgBounds);

        double cursorY = 14;
        AddDebugSlider(composer, "Damage", StatBarSegmentKind.Damage, ref cursorY);
        AddDebugSlider(composer, "Cold", StatBarSegmentKind.Cold, ref cursorY);
        AddDebugSlider(composer, "Heat", StatBarSegmentKind.Heat, ref cursorY);
        AddDebugSlider(composer, "Hunger", StatBarSegmentKind.Hunger, ref cursorY);

        Composers[ComposerKey] = composer.EndChildElements().Compose(false);
    }

    public override void Dispose()
    {
        DisposeComposer();
        base.Dispose();
    }

    private void AddDebugSlider(GuiComposer composer, string label, StatBarSegmentKind kind, ref double cursorY)
    {
        string key = $"debug-{kind}";
        composer
            .AddStaticText(label, CairoFont.WhiteSmallText(), ElementBounds.Fixed(16, cursorY + 2, 72, 20))
            .AddSlider(value => OnDebugValueChanged(kind, value), ElementBounds.Fixed(96, cursorY, 204, 22), key);
        composer.GetSlider(key).SetValues(_config.GetDebugValue(kind), 0, ImmersiveStatsClientConfig.MaximumDebugValue, 1);
        cursorY += 32;
    }

    private bool OnDebugValueChanged(StatBarSegmentKind kind, int value)
    {
        _config.SetDebugValue(kind, value);
        _debugState.Current = _config.ToState();
        _onDebugChanged();
        return true;
    }

    private void DisposeComposer()
    {
        if (!Composers.ContainsKey(ComposerKey))
        {
            return;
        }

        Composers[ComposerKey]?.Dispose();
        Composers.Remove(ComposerKey);
    }
}
