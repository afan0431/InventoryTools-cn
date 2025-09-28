using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public class MarketboardWorldSetting : ChoiceColumnSetting<(uint,string)?>
{
    private readonly ExcelSheet<World> _worldSheet;
    public override string EmptyText => "本服务器";

    public MarketboardWorldSetting(ILogger<MarketboardWorldSetting> logger, ImGuiService imGuiService, ExcelSheet<World> worldSheet) : base(logger, imGuiService)
    {
        _worldSheet = worldSheet;
    }
    public override (uint,string)? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out uint? value);
        if (value == null)
        {
            return null;
        }

        if (value.Value == 0)
        {
            return (0, "当前服务器");
        }

        var world = _worldSheet.GetRowOrDefault(value.Value);
        if (world == null)
        {
            return null;
        }
        return (world.Value.RowId, world.Value.Name.ExtractText());
    }

    public uint SelectedWorldId(ColumnConfiguration configuration, Character character)
    {
        var settingValue = CurrentValue(configuration);
        var selectedWorld = character.WorldId;
        if (settingValue != null)
        {
            if (settingValue.Value.Item1 == 0)
            {
                selectedWorld = character.ActiveWorldId;
            }
            else
            {
                selectedWorld = settingValue.Value.Item1;
            }
        }

        return selectedWorld;
    }

    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting(Key, (uint?)null);
    }

    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, (uint,string)? newValue)
    {
        configuration.SetSetting(Key, newValue?.Item1 ?? null);
    }

    public override string Key { get; set; } = "MBWorld";
    public override string Name { get; set; } = "服务器";
    public override string HelpText { get; set; } = "此列要显示的服务器？";
    public override (uint,string)? DefaultValue { get; set; } = null;
    public override List<(uint,string)?> GetChoices(ColumnConfiguration configuration)
    {
        List<(uint RowId, string FormattedName)?> worlds = _worldSheet.Where(c => c.IsPublic).Select(c =>((uint, string)?)(c.RowId, c.Name.ExtractText())).ToList();
        worlds.Insert(0,(0,"当前服务器"));
        return worlds;
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, (uint,string)? choice)
    {
        return choice?.Item2 ?? "当前服务器";
    }
}