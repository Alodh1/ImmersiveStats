using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace ImmersiveStats.Client;

internal sealed class ImmersiveStatsSettingsHud : HudElement
{
    private const string ComposerKey = "immersivestats-settings";
    private const double Width = 720;
    private const double Height = 600;
    private const double ColumnWidth = 338;
    private const double RowHeight = 88;

    private readonly ImmersiveStatsClientConfig _config;
    private readonly Action _onConfigChanged;
    private int _screenX;
    private int _screenY;

    public ImmersiveStatsSettingsHud(ICoreClientAPI capi, ImmersiveStatsClientConfig config, Action onConfigChanged)
        : base(capi)
    {
        _config = config;
        _onConfigChanged = onConfigChanged;
    }

    public override double InputOrder => 0.2;

    public override bool Focusable => true;

    public override bool PrefersUngrabbedMouse => true;

    public override bool ShouldReceiveKeyboardEvents() => false;

    public void OpenAt(int screenX, int screenY)
    {
        _screenX = screenX;
        _screenY = screenY;
        ComposeGuis();
        TryOpen(false);
    }

    public void ComposeGuis()
    {
        DisposeComposer();

        double scale = RuntimeEnv.GUIScale;
        double viewportWidth = capi.Render.FrameWidth > 0 ? capi.Render.FrameWidth / scale : 1280;
        double viewportHeight = capi.Render.FrameHeight > 0 ? capi.Render.FrameHeight / scale : 720;
        double x = Math.Min(Math.Max(0, _screenX / scale), Math.Max(0, viewportWidth - Width));
        double y = Math.Min(Math.Max(0, _screenY / scale), Math.Max(0, viewportHeight - Height));

        ElementBounds dialogBounds = ElementBounds.Fixed(EnumDialogArea.LeftTop, x, y, Width, Height).WithParent(capi.Gui.WindowBounds);
        ElementBounds bgBounds = ElementBounds.Fixed(0, 0, Width, Height);
        GuiComposer composer = capi.Gui.CreateCompo(ComposerKey, dialogBounds)
            .AddShadedDialogBG(bgBounds, false, 4, 0.82f)
            .BeginChildElements(bgBounds);

        double cursorY = 14;
        composer
            .AddStaticText("Debug mode", CairoFont.WhiteSmallText(), ElementBounds.Fixed(16, cursorY + 4, 200, 22))
            .AddSwitch(OnDebugModeChanged, ElementBounds.Fixed(675, cursorY, 24, 24), "debugMode", 24, 3);
        composer.GetSwitch("debugMode").SetValue(_config.DebugModeEnabled);
        cursorY += 38;

        StatBarSegmentKind[] colorKinds = new[] { StatBarSegmentKind.Energy }.Concat(StatBarSegmentCatalog.ReducerKinds).ToArray();
        for (int i = 0; i < colorKinds.Length; i++)
        {
            double controlX = 16 + (i % 2) * ColumnWidth;
            double controlY = cursorY + (i / 2) * RowHeight;
            AddColorControls(composer, StatBarSegmentCatalog.DisplayName(colorKinds[i]), colorKinds[i], controlX, controlY);
        }

        Composers[ComposerKey] = composer.EndChildElements().Compose(false);
    }

    public override void Dispose()
    {
        DisposeComposer();
        base.Dispose();
    }

    private void AddColorControls(GuiComposer composer, string label, StatBarSegmentKind kind, double x, double y)
    {
        ImmersiveStatsRgbColor color = _config.GetColor(kind);
        composer.AddStaticText(label, CairoFont.WhiteSmallText(), ElementBounds.Fixed(x, y, 120, 18));
        AddColorSlider(composer, kind, "R", color.R, x + 8, y + 20);
        AddColorSlider(composer, kind, "G", color.G, x + 8, y + 42);
        AddColorSlider(composer, kind, "B", color.B, x + 8, y + 64);
    }

    private void AddColorSlider(GuiComposer composer, StatBarSegmentKind kind, string component, int value, double x, double y)
    {
        string key = $"color-{kind}-{component}";
        composer
            .AddStaticText(component, CairoFont.WhiteSmallText(), ElementBounds.Fixed(x, y + 2, 20, 18))
            .AddSlider(next => OnColorChanged(kind, component, next), ElementBounds.Fixed(x + 24, y, 284, 22), key);
        composer.GetSlider(key).SetValues(value, 0, 255, 1);
    }

    private void OnDebugModeChanged(bool enabled)
    {
        _config.DebugModeEnabled = enabled;
        _onConfigChanged();
    }

    private bool OnColorChanged(StatBarSegmentKind kind, string component, int value)
    {
        ImmersiveStatsRgbColor color = _config.GetColor(kind).Copy();
        switch (component)
        {
            case "R":
                color.R = value;
                break;
            case "G":
                color.G = value;
                break;
            case "B":
                color.B = value;
                break;
        }

        _config.SetColor(kind, color);
        _onConfigChanged();
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
