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

        object state = Activator.CreateInstance(stateType, 5000f, 1250f, 0f, 0f, 1250f)
            ?? throw new InvalidOperationException("Could not create StatBarState.");

        MethodInfo calculate = layoutType.GetMethod("Calculate", BindingFlags.Public | BindingFlags.Static, [stateType])
            ?? throw new MissingMethodException(layoutType.FullName, "Calculate");

        object result = calculate.Invoke(null, [state])
            ?? throw new InvalidOperationException("Layout calculation returned null.");

        float energy = (float)(result.GetType().GetProperty("EnergyAmount")?.GetValue(result)
            ?? throw new InvalidOperationException("EnergyAmount was not available."));
        Assert.Equal(2500f, energy);

        object segmentsValue = result.GetType().GetProperty("Segments")?.GetValue(result)
            ?? throw new InvalidOperationException("Segments was not available.");
        var segments = ((System.Collections.IEnumerable)segmentsValue).Cast<object>().ToArray();

        Assert.Equal(["Energy", "PenetratingTrauma", "Hunger"], segments.Select(GetKindName).ToArray());
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
        Assert.NotNull(assembly.GetType("ImmersiveStats.Stats.ImmersiveStatsTimedEnergyCondition", throwOnError: false));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Stats.ImmersiveStatsThermalExposureCondition", throwOnError: false));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Server.ImmersiveStatsServerVitalsTracker", throwOnError: false));
        Assert.NotNull(assembly.GetType("ImmersiveStats.Server.ImmersiveStatsDamageTrackerBehavior", throwOnError: false));

        Type segmentKindType = assembly.GetType("ImmersiveStats.StatBarSegmentKind")
            ?? throw new InvalidOperationException("StatBarSegmentKind type was not found.");
        Assert.Contains("PenetratingTrauma", Enum.GetNames(segmentKindType));
        Assert.Contains("BluntTrauma", Enum.GetNames(segmentKindType));
        Assert.Contains("Burn", Enum.GetNames(segmentKindType));
        Assert.Contains("CoreTemperature", Enum.GetNames(segmentKindType));
        Assert.Contains("Toxic", Enum.GetNames(segmentKindType));
        Assert.Contains("Asphyxiation", Enum.GetNames(segmentKindType));
        Assert.Contains("Hunger", Enum.GetNames(segmentKindType));

        Type packetType = assembly.GetType("ImmersiveStats.Network.ImmersiveStatsVitalsPacket")
            ?? throw new InvalidOperationException("ImmersiveStatsVitalsPacket type was not found.");
        Assert.NotNull(packetType.GetProperty("PenetratingTraumaReducer"));
        Assert.NotNull(packetType.GetProperty("BluntTraumaReducer"));
        Assert.NotNull(packetType.GetProperty("BurnReducer"));
        Assert.NotNull(packetType.GetProperty("CoreTemperatureReducer"));
        Assert.NotNull(packetType.GetProperty("ToxicReducer"));
        Assert.NotNull(packetType.GetProperty("AsphyxiationReducer"));
        Assert.NotNull(packetType.GetProperty("HungerReducer"));
        Assert.NotNull(packetType.GetProperty("PenetratingTraumaActive"));
        Assert.NotNull(packetType.GetProperty("BluntTraumaActive"));
        Assert.NotNull(packetType.GetProperty("BurnActive"));
        Assert.NotNull(packetType.GetProperty("CoreTemperatureActive"));
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
