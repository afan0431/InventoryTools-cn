using System.Collections.Generic;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;

namespace InventoryTools.Logic.Columns.Buttons;

public class CustomButtonColumn : ButtonColumn
{
    private readonly ICommandManager _commandManager;
    private readonly StringColumnSetting _actionSetting;
    private readonly StringColumnSetting _buttonText;

    public CustomButtonColumn(StringColumnSetting.Factory stringColumnFactory, ICommandManager commandManager)
    {
        _commandManager = commandManager;
        _actionSetting = stringColumnFactory.Invoke("cb_action", "命令",
            "要运行的命令。系统会自动为您添加斜杠。您可以添加***Name***输出物品名称，或***ID***输出物品ID。",
            "", "gather ***Name***");
        _buttonText = stringColumnFactory.Invoke("cb_label", "标签", "按钮显示的标签文本。","","按钮");
        Settings.Add(_buttonText);
        Settings.Add(_actionSetting);
    }
    public override string Name { get; set; } = "自定义按钮";
    public override float Width { get; set; } = 50;

    public override bool HasFilter { get; set; } = false;

    public override string HelpText { get; set; } =
        "自定义按钮，允许您指定要使用物品名称或ID运行的自定义命令";

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult,
        int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            using var id = ImRaii.PushId(rowIndex + columnIndex.ToString());
            if (ImGui.Button(_buttonText.CurrentValue(columnConfiguration)))
            {
                var command = _actionSetting.CurrentValue(columnConfiguration);
                if (command != null)
                {
                    if (command.Contains("***ID***"))
                    {
                        command = command.Replace("***ID***", searchResult.ItemId.ToString());
                    }

                    if (command.Contains("***Name***"))
                    {
                        command = command.Replace("***Name***", searchResult.Item.NameString);
                    }

                    _commandManager.ProcessCommand("/" + command);
                }
            }
        }

        return null;
    }
}