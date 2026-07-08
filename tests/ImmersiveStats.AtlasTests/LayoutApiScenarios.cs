using System.Reflection;
using Atlas.XUnit;
using Xunit;

namespace ImmersiveStats.AtlasTests;

public sealed class LayoutApiScenarios : AtlasScenarioBase
{
    [AtlasScenario(TimeoutMs = 120000)]
    public async Task LayoutApi_Should_BeAvailable_FromLoadedModAssembly()
    {
        await World.Ticks(2);

        Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(candidate => candidate.GetName().Name == "immersivestats")
            ?? throw new InvalidOperationException("The immersivestats assembly was not loaded by Atlas.");

        Type stateType = assembly.GetType("ImmersiveStats.StatBarState")
            ?? throw new InvalidOperationException("StatBarState type was not found.");
        Type layoutType = assembly.GetType("ImmersiveStats.StatBarLayout")
            ?? throw new InvalidOperationException("StatBarLayout type was not found.");

        object state = Activator.CreateInstance(stateType, 100f, 25f, 0f, 0f, 25f)
            ?? throw new InvalidOperationException("Could not create StatBarState.");

        MethodInfo calculate = layoutType.GetMethod("Calculate", BindingFlags.Public | BindingFlags.Static, [stateType])
            ?? throw new MissingMethodException(layoutType.FullName, "Calculate");

        object result = calculate.Invoke(null, [state])
            ?? throw new InvalidOperationException("Layout calculation returned null.");

        float energy = (float)(result.GetType().GetProperty("EnergyAmount")?.GetValue(result)
            ?? throw new InvalidOperationException("EnergyAmount was not available."));
        Assert.Equal(50f, energy);

        object segmentsValue = result.GetType().GetProperty("Segments")?.GetValue(result)
            ?? throw new InvalidOperationException("Segments was not available.");
        var segments = ((System.Collections.IEnumerable)segmentsValue).Cast<object>().ToArray();

        Assert.Equal(["Energy", "Damage", "Hunger"], segments.Select(GetKindName).ToArray());
    }

    private static string GetKindName(object segment)
    {
        object kind = segment.GetType().GetProperty("Kind")?.GetValue(segment)
            ?? throw new InvalidOperationException("Segment kind was not available.");
        return kind.ToString() ?? string.Empty;
    }
}
