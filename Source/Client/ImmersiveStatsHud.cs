using ImmersiveStats.Commands;
using ImmersiveStats.Debug;
using ImmersiveStats.Network;
using ImmersiveStats.Stats;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ImmersiveStats.Client;

internal sealed class ImmersiveStatsHud : HudElement
{
    private const string ComposerKey = "immersivestats-statbar";
    private const double CornerHandleSize = 12;

    private readonly DebugStatBarState _debugState;
    private readonly ImmersiveStatsClientConfig _config;
    private readonly IImmersiveStatsVitalsSource _vitalsSource;
    private readonly VanillaStatbarSuppressor _vanillaStatbarSuppressor;
    private readonly Action _saveConfig;
    private SegmentedStatBarElement? _barElement;
    private ImmersiveStatsSettingsHud? _settingsHud;
    private ImmersiveStatsDebugHud? _debugHud;
    private bool _editMode;
    private DragMode _dragMode;
    private HudResizeHandle _resizeHandle;
    private HudRect _dragStartRect;
    private int _dragStartMouseX;
    private int _dragStartMouseY;

    public ImmersiveStatsHud(
        ICoreClientAPI capi,
        DebugStatBarState debugState,
        ImmersiveStatsClientConfig config,
        IImmersiveStatsVitalsSource vitalsSource,
        VanillaStatbarSuppressor vanillaStatbarSuppressor,
        Action saveConfig)
        : base(capi)
    {
        _debugState = debugState;
        _config = config;
        _vitalsSource = vitalsSource;
        _vanillaStatbarSuppressor = vanillaStatbarSuppressor;
        _saveConfig = saveConfig;
        _debugState.Changed += OnDebugStateChanged;
        RefreshDisplayedState();
        ComposeGuis();
        UpdateDebugHud();
    }

    public override double InputOrder => 1;

    public override bool Focusable => _editMode;

    public override bool PrefersUngrabbedMouse => _editMode;

    public override bool ShouldReceiveKeyboardEvents() => false;

    public override bool ShouldReceiveMouseEvents() => IsOpened() && _editMode;

    public override void OnOwnPlayerDataReceived()
    {
        RefreshDisplayedState();
        ComposeGuis();
        _vanillaStatbarSuppressor.SuppressHealthAndHungerBars();
    }

    public bool ApplyEditModeCommand(ImmersiveStatsEditCommand command)
    {
        SetEditMode(command.Kind == ImmersiveStatsEditCommandKind.Toggle ? !_editMode : command.Enabled);
        return _editMode;
    }

    public void ApplyEditModePacket(ImmersiveStatsEditModePacket packet)
    {
        SetEditMode(packet.HasExplicitState ? packet.Enabled : !_editMode);
    }

    public void ComposeGuis()
    {
        (double viewportWidth, double viewportHeight) = GetViewportGuiSize();
        _config.Normalize(viewportWidth, viewportHeight);

        DisposeComposer(ComposerKey);

        ElementBounds dialogBounds = ElementBounds
            .Fixed(EnumDialogArea.LeftTop, _config.BarX ?? 0, _config.BarY ?? 0, _config.BarWidth, _config.BarHeight)
            .WithParent(capi.Gui.WindowBounds);
        ElementBounds barBounds = ElementBounds.Fixed(0, 0, _config.BarWidth, _config.BarHeight);
        _barElement = new SegmentedStatBarElement(capi, barBounds, _debugState.Current, _config);
        _barElement.SetEditMode(_editMode);

        Composers[ComposerKey] = capi.Gui
            .CreateCompo(ComposerKey, dialogBounds)
            .BeginChildElements(dialogBounds)
                .AddInteractiveElement(_barElement, "bar")
            .EndChildElements()
            .Compose(false);

        _debugHud?.ComposeGuis();
    }

    public override void OnRenderGUI(float deltaTime)
    {
        RefreshDisplayedState();
        _vanillaStatbarSuppressor.SuppressHealthAndHungerBars();

        if (capi.World.Player?.WorldData.CurrentGameMode == Vintagestory.API.Common.EnumGameMode.Spectator)
        {
            return;
        }

        base.OnRenderGUI(deltaTime);
    }

    public override void OnMouseDown(MouseEvent args)
    {
        if (!_editMode || args.Handled || !IsInsideBar(args.X, args.Y))
        {
            return;
        }

        if (args.Button == EnumMouseButton.Right)
        {
            OpenSettings(args.X, args.Y);
            args.Handled = true;
            return;
        }

        if (args.Button != EnumMouseButton.Left)
        {
            return;
        }

        _resizeHandle = GetResizeHandle(args.X, args.Y);
        _dragMode = _resizeHandle == HudResizeHandle.None ? DragMode.Move : DragMode.Resize;
        _dragStartRect = _config.GetBarRect();
        _dragStartMouseX = args.X;
        _dragStartMouseY = args.Y;
        args.Handled = true;
    }

    public override void OnMouseMove(MouseEvent args)
    {
        if (!_editMode || _dragMode == DragMode.None)
        {
            return;
        }

        double scale = RuntimeEnv.GUIScale;
        double deltaX = (args.X - _dragStartMouseX) / scale;
        double deltaY = (args.Y - _dragStartMouseY) / scale;
        (double viewportWidth, double viewportHeight) = GetViewportGuiSize();

        HudRect next = _dragMode == DragMode.Move
            ? HudPlacementMath.Move(_dragStartRect, deltaX, deltaY, viewportWidth, viewportHeight, ImmersiveStatsClientConfig.MinimumBarWidth, ImmersiveStatsClientConfig.MinimumBarHeight)
            : HudPlacementMath.Resize(_dragStartRect, _resizeHandle, deltaX, deltaY, viewportWidth, viewportHeight, ImmersiveStatsClientConfig.MinimumBarWidth, ImmersiveStatsClientConfig.MinimumBarHeight);

        _config.SetBarRect(next);
        ComposeGuis();
        args.Handled = true;
    }

    public override void OnMouseUp(MouseEvent args)
    {
        if (_dragMode == DragMode.None)
        {
            return;
        }

        _dragMode = DragMode.None;
        _resizeHandle = HudResizeHandle.None;
        _saveConfig();
        args.Handled = true;
    }

    protected override void OnFocusChanged(bool on)
    {
    }

    public override void Dispose()
    {
        _debugState.Changed -= OnDebugStateChanged;
        _settingsHud?.Dispose();
        _debugHud?.Dispose();
        _settingsHud = null;
        _debugHud = null;
        base.Dispose();
    }

    private void SetEditMode(bool enabled)
    {
        if (_editMode == enabled)
        {
            return;
        }

        _editMode = enabled;
        _barElement?.SetEditMode(enabled);

        if (!enabled)
        {
            _settingsHud?.TryClose();
            _dragMode = DragMode.None;
            _resizeHandle = HudResizeHandle.None;
        }
    }

    private void OpenSettings(int mouseX, int mouseY)
    {
        _settingsHud ??= new ImmersiveStatsSettingsHud(capi, _config, OnSettingsChanged);
        _settingsHud.OpenAt(mouseX, mouseY);
    }

    private void OnSettingsChanged()
    {
        (double viewportWidth, double viewportHeight) = GetViewportGuiSize();
        _config.Normalize(viewportWidth, viewportHeight);
        RefreshDisplayedState();
        _barElement?.RefreshStyle();
        UpdateDebugHud();
        _saveConfig();
    }

    private void UpdateDebugHud()
    {
        if (_config.DebugModeEnabled)
        {
            _debugHud ??= new ImmersiveStatsDebugHud(capi, _config, _debugState, () =>
            {
                _barElement?.SetState(_debugState.Current);
                _saveConfig();
            });
            _debugHud.ComposeGuis();
            _debugHud.TryOpen(false);
            return;
        }

        _debugHud?.TryClose();
    }

    private void RefreshDisplayedState()
    {
        _debugState.Current = ImmersiveStatsDisplayStateResolver.Resolve(_config, _vitalsSource, _debugState.Current);
    }

    private void OnDebugStateChanged() => _barElement?.SetState(_debugState.Current);

    private bool IsInsideBar(int screenX, int screenY)
    {
        ElementBounds? bounds = _barElement?.Bounds;
        if (bounds is null)
        {
            return false;
        }

        bounds.CalcWorldBounds();
        return bounds.PointInside(screenX, screenY);
    }

    private HudResizeHandle GetResizeHandle(int screenX, int screenY)
    {
        ElementBounds? bounds = _barElement?.Bounds;
        if (bounds is null)
        {
            return HudResizeHandle.None;
        }

        bounds.CalcWorldBounds();
        double handle = CornerHandleSize * RuntimeEnv.GUIScale;
        bool left = screenX <= bounds.absX + handle;
        bool right = screenX >= bounds.absX + bounds.OuterWidth - handle;
        bool top = screenY <= bounds.absY + handle;
        bool bottom = screenY >= bounds.absY + bounds.OuterHeight - handle;

        return (left, right, top, bottom) switch
        {
            (true, _, true, _) => HudResizeHandle.TopLeft,
            (_, true, true, _) => HudResizeHandle.TopRight,
            (true, _, _, true) => HudResizeHandle.BottomLeft,
            (_, true, _, true) => HudResizeHandle.BottomRight,
            _ => HudResizeHandle.None,
        };
    }

    private (double Width, double Height) GetViewportGuiSize()
    {
        double scale = RuntimeEnv.GUIScale;
        double width = capi.Render.FrameWidth > 0 ? capi.Render.FrameWidth / scale : 1280;
        double height = capi.Render.FrameHeight > 0 ? capi.Render.FrameHeight / scale : 720;
        return (width, height);
    }

    private void DisposeComposer(string key)
    {
        if (!Composers.ContainsKey(key))
        {
            return;
        }

        Composers[key]?.Dispose();
        Composers.Remove(key);
    }

    private enum DragMode
    {
        None,
        Move,
        Resize,
    }
}
