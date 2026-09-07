using WCtrlDcsBiosBridge.Aircrafts;
using WCtrlDcsBiosBridge.Devices.Frontpanels;
using Xunit;

namespace WCtrlDcsBiosBridge.Tests;

public class C130JRegistryTests
{
    [Fact]
    public void DetectedByItsDcsBiosName()
    {
        // Entry/C_130J_30.lua names the aircraft "C-130J-30"; matching is on prefix.
        Assert.Same(AircraftRegistry.C130J, AircraftRegistry.FindByDcsBiosName("C-130J-30"));
    }

    [Fact]
    public void IsInTheRegistry()
    {
        Assert.Contains(AircraftRegistry.C130J, AircraftRegistry.All);
        Assert.Same(AircraftRegistry.C130J, AircraftRegistry.Find(AircraftRegistry.C130J.ModuleId));
    }

    /// <summary>
    /// DCS-BIOS carries the aircraft now, so the descriptor names its own module rather than
    /// borrowing the A-10C's. It borrowed one for as long as there was nothing else for the
    /// control locator to load, at the cost of resolving another module's controls.
    /// </summary>
    [Fact]
    public void NamesItsOwnDcsBiosModule()
    {
        Assert.Null(AircraftRegistry.C130J.DcsBiosModuleId);
        Assert.Equal(AircraftRegistry.C130J.ModuleId,
                     AircraftRegistry.C130J.EffectiveDcsBiosModuleId);
        Assert.Equal("C-130J.json", AircraftRegistry.C130J.JsonFile);
        Assert.Contains(AircraftRegistry.C130J.JsonFile, AircraftRegistry.ExpectedJsonFiles);
    }

    /// <summary>
    /// The gear and the master caution are named controls now that DCS-BIOS carries the module.
    /// The EXEC annunciator is not, and cannot be: PLT_CNI_EXEC_LED sits on an argument that
    /// never moves, so it stays something the listener works out.
    /// </summary>
    [Fact]
    public void LightsWhatItCanFromNamedControls()
    {
        var defaults = LedDefaults.For(AircraftRegistry.C130J);

        Assert.NotEmpty(defaults.Signals);
        Assert.Contains(defaults.McduLeds, l => l.Led == McduLed.Fail);
        Assert.DoesNotContain(defaults.McduLeds, l => l.Led == McduLed.Exec);
        Assert.Contains(McduLed.Exec, defaults.ComputedMcduLeds.Keys);
    }

    /// <summary>
    /// Both CNIs are carried, so the bridge has a seat to ask about when more than one CDU is
    /// connected. Without this the seat-selection screen never comes up and every CDU shows
    /// the pilot's.
    /// </summary>
    [Fact]
    public void OffersASeatToChoose()
    {
        Assert.True(AircraftRegistry.C130J.HasSeatSelection);
    }

    [Fact]
    public void DoesNotShadowAnotherAircraft()
    {
        var names = AircraftRegistry.All.SelectMany(d => d.DcsBiosNames).ToList();

        Assert.All(AircraftRegistry.C130J.DcsBiosNames, own =>
            Assert.DoesNotContain(names, other =>
                other != own && own.StartsWith(other, StringComparison.OrdinalIgnoreCase)));
    }
}
