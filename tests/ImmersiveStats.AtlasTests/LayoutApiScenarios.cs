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

        Assembly assembly = GetImmersiveStatsAssembly();

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

    [AtlasScenario(TimeoutMs = 120000)]
    public async Task ServerCommandRegistration_Should_LoadWithoutCrash()
    {
        await World.Ticks(2);

        Assembly assembly = GetImmersiveStatsAssembly();

        Assert.NotNull(assembly.GetType("ImmersiveStats.Network.ImmersiveStatsEditModePacket"));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Network.ImmersiveStatsVitalsPacket"));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Commands.ImmersiveStatsCommandParser", throwOnError: false));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Stats.ImmersiveStatsVitalsSnapshot", throwOnError: false));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Stats.ImmersiveStatsVitalsMapper", throwOnError: false));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Server.ImmersiveStatsServerVitalsTracker", throwOnError: false));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Server.ImmersiveStatsDamageTrackerBehavior", throwOnError: false));

        Type segmentKindType = assembly.GetType("ImmersiveStats.StatBarSegmentKind")
            ?? throw new InvalidOperationException("StatBarSegmentKind type was not found.");
        Assert.Contains("Poison", Enum.GetNames(segmentKindType));
        Assert.Contains("Fall", Enum.GetNames(segmentKindType));
        Assert.Contains("Suffocation", Enum.GetNames(segmentKindType));
        Assert.Contains("Crushing", Enum.GetNames(segmentKindType));
        Assert.Contains("Electricity", Enum.GetNames(segmentKindType));
        Assert.Contains("Acid", Enum.GetNames(segmentKindType));

        Type packetType = assembly.GetType("ImmersiveStats.Network.ImmersiveStatsVitalsPacket")
            ?? throw new InvalidOperationException("ImmersiveStatsVitalsPacket type was not found.");
        Assert.NotNull(packetType.GetProperty("PoisonReducer"));
        Assert.NotNull(packetType.GetProperty("FallReducer"));
        Assert.NotNull(packetType.GetProperty("SuffocationReducer"));
        Assert.NotNull(packetType.GetProperty("CrushingReducer"));
        Assert.NotNull(packetType.GetProperty("ElectricityReducer"));
        Assert.NotNull(packetType.GetProperty("AcidReducer"));
    }

    private static Assembly GetImmersiveStatsAssembly()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(candidate => candidate.GetName().Name == "immersivestats")
            ?? throw new InvalidOperationException("The immersivestats assembly was not loaded by Atlas.");
    }

    private static string GetKindName(object segment)
    {
        object kind = segment.GetType().GetProperty("Kind")?.GetValue(segment)
            ?? throw new InvalidOperationException("Segment kind was not available.");
        return kind.ToString() ?? string.Empty;
    }
}
