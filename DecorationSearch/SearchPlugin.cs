using System;
using System.Linq;
using System.Windows;
using DecorationSearch.Controls;
using HunterPie.Plugins;
using HunterPie.Core;
using HunterPie.Core.Events;
using HunterPie.GUI;
using HunterPie.Memory;

namespace DecorationSearch
{
    public class SearchPlugin : IPlugin
    {
        public string Name { get; set; } = "Decoration Searcher";
        public string Description { get; set; } = "A plugin that lets you search for decorations when equipping them.";
        public Game Context { get; set; }

        private SearchWidget widget;
        private bool scanForDecorationGui;

        private static readonly string[] validActions = {
            "Village::TALK_NPC_POP_BTN",
            "Village::OPEN_ITEM_BOX",
            "Common::OPEN_ITEM_BOX"
        };

        public void Initialize(Game context)
        {
            Context = context;

            InitializeWidget();
            HookEvents();

            if (context.Player != null)
                scanForDecorationGui = validActions.Contains(context.Player.PlayerActionRef);
        }

        public void Unload()
        {
            if (Context is null)
                return;

            Context.Player.OnActionChange -= OnActionChange;
            Context.Player.OnPlayerScanFinished -= OnPlayerScanFinished;
            Dispatch(() =>
            {
                if (widget is null)
                    return;

                Overlay.UnregisterWidget(widget);
                widget.Close();
                widget = null;
            });
            Context = null;
        }

        private void InitializeWidget()
        {
            Dispatch(() =>
            {
                if (widget is null)
                    widget = new SearchWidget();

                Overlay.RegisterWidget(widget);
            });
            
        }

        private void HookEvents()
        {
            Context.Player.OnActionChange += OnActionChange;
            Context.Player.OnPlayerScanFinished += OnPlayerScanFinished;
        }

        private void OnPlayerScanFinished(object source, EventArgs args)
        {

            if (!scanForDecorationGui)
            {
                Dispatch(() =>
                {
                    widget.SetVisibility(false);
                });
                return;
            }

            long uGuiPtr = Kernel.ReadMultilevelPtr(
                Address.GetAddress("BASE") + Address.GetAddress("GAME_HUD_INFO_OFFSET"),
                new [] {0x490, 0x0}
            );

            long vTablePtr = Kernel.Read<long>(uGuiPtr);

            if (vTablePtr == Kernel.NULLPTR)
            {
                Dispatch(() =>
                {
                    widget.SetVisibility(false);
                });
                return;
            }

            string uGuiName = Kernel.ReadString(vTablePtr - 0x30, 18);

            if (uGuiName != "uGUIEquipSkillGem")
            {
                Dispatch(() =>
                {
                    widget.SetVisibility(false);
                });
                return;
            }

            bool isDecoSelectionPanelOpen = Kernel.Read<byte>(uGuiPtr + 0x4E4) == 1;

            Dispatch(() =>
            {
                widget.SetVisibility(isDecoSelectionPanelOpen);
            });
        }

        private void OnActionChange(object source, EventArgs args)
        {
            Dispatch(() =>
            {
                if (widget is null)
                    return;

                PlayerEventArgs e = (PlayerEventArgs)args;

                scanForDecorationGui = validActions.Contains(e.Action);
            });
        }

        private void Dispatch(Action f)
        {
            Application.Current.Dispatcher.BeginInvoke(f);
        }
    }
}
