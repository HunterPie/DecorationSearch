using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using DecorationSearch.Definitions;
using HunterPie.Core;
using HunterPie.Core.Input;
using HunterPie.Core.Native;
using HunterPie.GUI;
using HunterPie.Memory;
using System.Threading.Tasks;

namespace DecorationSearch.Controls
{
    /// <summary>
    /// Interaction logic for SearchWidget.xaml
    /// </summary>
    public partial class SearchWidget : Widget
    {

        public readonly Dictionary<string, int> Searchable = new Dictionary<string, int>();

        public override uint Flags => (uint)(WindowsHelper.SWP_WINDOWN_FLAGS.SWP_SHOWWINDOW |
                                             WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOSIZE |
                                             WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOMOVE);

        public override uint RenderFlags => (uint)(WindowsHelper.EX_WINDOW_STYLES.WS_EX_TOPMOST);
        
        public SearchWidget()
        {
            WidgetActive = true;
            InitializeComponent();
            WindowBlur.SetIsEnabled(this, true);
        }

        public override void EnterWidgetDesignMode()
        {
            return;
        }

        public override void LeaveWidgetDesignMode()
        {
            return;
        }

        private void IndexJewels()
        {
            XmlNodeList jewels = Honey.HoneyGearData.SelectNodes("//Honey/Gear/Jewels/Jewel");
            
            foreach (XmlNode je in jewels)
            {
                Searchable[GMD.GetItemNameById(int.Parse(je.Attributes["GameId"].Value))] = int.Parse(je.Attributes["ID"].Value);
            }
        }

        private void SearchForDecorations()
        {
            if (string.IsNullOrEmpty(InputBox.Text))
                return;

            sDecoration[] decorations = GetDecorationArray();

            string query = InputBox.Text;
            if (Search(query, ref decorations))
            {
                UpdateDecorationArray(decorations);
            }

            InputBox.Text = string.Empty;
        }

        private static sDecoration[] GetDecorationArray()
        {
            long address = Address.GetAddress("BASE") + Address.GetAddress("GAME_HUD_INFO_OFFSET");
            long decoArrayPtr = Kernel.ReadMultilevelPtr(address, new [] { 0x490, 0x2b20 });
            return Kernel.ReadStructure<sDecoration>(decoArrayPtr, 1000);
        }

        private static async void UpdateDecorationArray(sDecoration[] array)
        {
            long address = Address.GetAddress("BASE") + Address.GetAddress("GAME_HUD_INFO_OFFSET");
            long decoArrayPtr = Kernel.ReadMultilevelPtr(address, new [] { 0x490, 0x2b20 });

            if (Kernel.Write(decoArrayPtr, array))
            {
                await VirtualInput.PressKey('N');
                await Task.Delay(10);
                await VirtualInput.PressKey('N');
            }
        }

        private bool Search(string jewelName, ref sDecoration[] array)
        {
            Dictionary<int, string> names = new Dictionary<int, string>();
            HashSet<int> lookup = new HashSet<int>();

            var possibilities = Searchable.Keys.Where(k =>
            {
                return Regex.IsMatch(k, jewelName, RegexOptions.IgnoreCase);
            });

            foreach (var p in possibilities)
            {
                lookup.Add(Searchable[p]);
                names[Searchable[p]] = p;
            }

            if (!possibilities.Any())
                return false;

            List<sDecoration> candidates = new List<sDecoration>();
            List<sDecoration> jewels = new List<sDecoration>();

            foreach (sDecoration jwl in array)
            {
                if (lookup.Contains(jwl.id))
                {
                    candidates.Add(jwl);
                }
                else
                {
                    jewels.Add(jwl);
                }
            }

            candidates.Sort((l, r) =>
            {
                names.TryGetValue(l.id, out string v1);
                names.TryGetValue(r.id, out string v2);
                return ((v1?.Length ?? 0) < (v2?.Length ?? 0) ? 1 : -1);
            });

            foreach (var e in candidates)
                jewels.Insert(0, e);

            Array.Copy(jewels.ToArray(), array, array.Length);

            return true;
        }

        public void SetVisibility(bool newVisibility)
        {
            if (IsClosed || WidgetHasContent == newVisibility)
                return;

            InputBox.IsEnabled = newVisibility;
            WidgetHasContent = newVisibility;
            ChangeVisibility();
            
        }

        private void OnClickClear(object sender, MouseButtonEventArgs e)
        {
            InputBox.Text = string.Empty;
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            SearchForDecorations();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchForDecorations();
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            IndexJewels();
        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                return;

            //Activate();
        }

        private void OnLeftMouseButtonChange(object sender, MouseButtonEventArgs e)
        {
            Opacity = e.LeftButton == MouseButtonState.Pressed ? 0.4 : 1;
        }
    }
}
