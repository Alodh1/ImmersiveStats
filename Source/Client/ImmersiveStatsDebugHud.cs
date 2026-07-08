using ImmersiveStats.Debug;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace ImmersiveStats.Client;

internal sealed class ImmersiveStatsDebugHud : HudElement
{
    private const string ComposerKey = "immersivestats-debug";
    private const double Width = 640;
    private const double Height = 184;
    private const double ColumnWidth = 310;

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

        for (int i = 0; i < StatBarSegmentCatalog.ReducerKinds.Count; i++)
        {
            StatBarSegmentKind kind = StatBarSegmentCatalog.ReducerKinds[i];
            double sliderX = 16 + (i % 2) * ColumnWidth;
            double sliderY = 14 + (i / 2) * 32;
            AddDebugSlider(composer, StatBarSegmentCatalog.DisplayName(kind), kind, sliderX, sliderY);
        }

        Composers[ComposerKey] = composer.EndChildElements().Compose(false);
    }

    public override void Dispose()
    {
        DisposeComposer();
        base.Dispose();
    }

    private void AddDebugSlider(GuiComposer composer, string label, StatBarSegmentKind kind, double x, double y)
    {
        string key = $"debug-{kind}";
        composer
            .AddStaticText(label, CairoFont.WhiteSmallText(), ElementBounds.Fixed(x, y + 2, 92, 20))
            .AddSlider(value => OnDebugValueChanged(kind, value), ElementBounds.Fixed(x + 98, y, 196, 22), key);
        composer.GetSlider(key).SetValues(_config.GetDebugValue(kind), 0, ImmersiveStatsClientConfig.MaximumDebugValue, 1);
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
