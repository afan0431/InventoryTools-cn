using System.Collections.Generic;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;

namespace InventoryTools.Logic.Columns.Buttons;

public class CustomLinkButtonColumn : ButtonColumn
{
    private readonly ICommandManager _commandManager;
    private readonly StringColumnSetting _actionSetting;
    private readonly StringColumnSetting _buttonText;

    public CustomLinkButtonColumn(StringColumnSetting.Factory stringColumnFactory, ICommandManager commandManager)
    {
        _commandManager = commandManager;
        _actionSetting = stringColumnFactory.Invoke("cb_action", "链接",
            "要打开的网页链接。您可以添加***Name***输出物品名称，或***ID***输出物品ID。",
            "", "https://site.com");
        _buttonText = stringColumnFactory.Invoke("cb_label", "标签", "按钮显示的标签文本。","","按钮");
        Settings.Add(_buttonText);
        Settings.Add(_actionSetting);
    }
    public override string Name { get; set; } = "自定义链接按钮";
    public override float Width { get; set; } = 50;

    public override bool HasFilter { get; set; } = false;

    public override string HelpText { get; set; } =
        "自定义按钮，允许您打开网页，可选择将物品名称或ID嵌入到链接中";

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult,
        int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            using var id = ImRaii.PushId(rowIndex + columnIndex.ToString());
            if (ImGui.Button(_buttonText.CurrentValue(columnConfiguration)))
            {
                var webUrl = _actionSetting.CurrentValue(columnConfiguration);
                if (webUrl != null)
                {
                    if (webUrl.Contains("***ID***"))
                    {
                        webUrl = webUrl.Replace("***ID***", searchResult.ItemId.ToString());
                    }

                    if (webUrl.Contains("***Name***"))
                    {
                        webUrl = webUrl.Replace("***Name***", searchResult.Item.NameString);
                    }
                    webUrl.OpenBrowser();
                }
            }
        }

        return null;
    }
}