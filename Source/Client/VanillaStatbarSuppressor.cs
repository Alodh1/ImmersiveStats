using System.Reflection;
using Vintagestory.API.Client;

namespace ImmersiveStats.Client;

internal sealed class VanillaStatbarSuppressor
{
    private const double ColorTolerance = 0.0001;

    private static readonly FieldInfo? InteractiveElementsField = typeof(GuiComposer).GetField("interactiveElements", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? StaticElementsField = typeof(GuiComposer).GetField("staticElements", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? InteractiveElementsInDrawOrderField = typeof(GuiComposer).GetField("interactiveElementsInDrawOrder", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? StatbarColorField = typeof(GuiElementStatbar).GetField("color", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly ICoreClientAPI _capi;
    private bool _warnedReflectionFailure;

    public VanillaStatbarSuppressor(ICoreClientAPI capi)
    {
        _capi = capi;
    }

    public void SuppressHealthAndHungerBars()
    {
        if (!HasRequiredReflectionFields())
        {
            WarnReflectionFailure();
            return;
        }

        foreach (GuiDialog dialog in _capi.Gui.LoadedGuis.ToArray())
        {
            if (dialog.GetType().Name != "HudStatbar")
            {
                continue;
            }

            foreach (GuiComposer composer in dialog.Composers.Values.ToArray())
            {
                SuppressComposerStatbars(composer);
            }
        }
    }

    private static void SuppressComposerStatbars(GuiComposer composer)
    {
        Dictionary<string, GuiElement>? interactiveElements = GetElementDictionary(InteractiveElementsField, composer);
        Dictionary<string, GuiElement>? staticElements = GetElementDictionary(StaticElementsField, composer);
        List<GuiElement>? drawOrder = GetElementList(InteractiveElementsInDrawOrderField, composer);
        if (interactiveElements is null)
        {
            return;
        }

        var removals = new List<KeyValuePair<string, GuiElement>>();
        foreach (KeyValuePair<string, GuiElement> entry in interactiveElements)
        {
            if (entry.Value is GuiElementStatbar statbar && IsSuppressedStatbar(statbar))
            {
                removals.Add(entry);
            }
        }

        if (removals.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<string, GuiElement> removal in removals)
        {
            interactiveElements.Remove(removal.Key);
            staticElements?.Remove(removal.Key);
            drawOrder?.Remove(removal.Value);
        }

        composer.ReCompose();
    }

    private static bool IsSuppressedStatbar(GuiElementStatbar statbar)
    {
        return StatbarColorField?.GetValue(statbar) is double[] color && IsSuppressedStatbarColor(color);
    }

    internal static bool IsSuppressedStatbarColor(double[] color)
    {
        return ColorMatches(color, GuiStyle.HealthBarColor) || ColorMatches(color, GuiStyle.FoodBarColor);
    }

    private static bool ColorMatches(double[] color, double[] target)
    {
        if (color.Length < 3 || target.Length < 3)
        {
            return false;
        }

        return Math.Abs(color[0] - target[0]) <= ColorTolerance
            && Math.Abs(color[1] - target[1]) <= ColorTolerance
            && Math.Abs(color[2] - target[2]) <= ColorTolerance;
    }

    private static Dictionary<string, GuiElement>? GetElementDictionary(FieldInfo? field, GuiComposer composer)
    {
        return field?.GetValue(composer) as Dictionary<string, GuiElement>;
    }

    private static List<GuiElement>? GetElementList(FieldInfo? field, GuiComposer composer)
    {
        return field?.GetValue(composer) as List<GuiElement>;
    }

    private static bool HasRequiredReflectionFields()
    {
        return InteractiveElementsField is not null
            && StaticElementsField is not null
            && InteractiveElementsInDrawOrderField is not null
            && StatbarColorField is not null;
    }

    private void WarnReflectionFailure()
    {
        if (_warnedReflectionFailure)
        {
            return;
        }

        _warnedReflectionFailure = true;
        _capi.Logger.Warning("ImmersiveStats could not inspect the vanilla HUD statbar composer; vanilla health/hunger bars may remain visible.");
    }
}
