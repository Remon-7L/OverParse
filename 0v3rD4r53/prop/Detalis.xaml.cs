using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ov3rD4r53
{
    /// <summary>
    /// Detalis.xaml の相互作用ロジック
    /// </summary>
    public partial class Detalis : YKToolkit.Controls.Window
    {
        DispatcherTimer updatetimer = new DispatcherTimer();
        private Player Player;
        public ObservableCollection<PAList> PAS = new ObservableCollection<PAList>();

        public Detalis(Player data)
        {
            InitializeComponent();
            Player = data;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

        private void Close_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => updatetimer.Stop();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updatetimer.Interval = new TimeSpan(0, 0, 0, 2);
            updatetimer.Tick += Update;
            DatalisGrid.ItemsSource = PAS;
            Update(null, null);
            updatetimer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            if (!MainWindow.workingList.Any(x => (x.ID == Player.ID) && (x.Type == PType.raw))) { return; }
            Player = MainWindow.workingList.First(x => (x.ID == Player.ID) && (x.Type == PType.raw));
            PAS.Clear();
            List<string> attackNames = new List<string>();
            List<string> finishNames = new List<string>();
            List<Tuple<string, List<int>, List<bool>, List<bool>>> attackData = new List<Tuple<string, List<int>, List<bool>, List<bool>>>();

            if (MainWindow.SepZvs && Player.IsZanverse)
            {
                foreach (Player zvs in MainWindow.workingList) { if (0 < zvs.ZvsDamage) { attackNames.Add(zvs.ID); } }
                foreach (string s in attackNames)
                {
                    Player zvsPlayer = MainWindow.workingList.First(x => x.ID == s);
                    List<int> matchingAttacks = zvsPlayer.Attacks.Where(atk => atk.ID == 2106601422).Select(a => a.Damage).ToList();
                    List<bool> jaPercents = new List<bool> { false };
                    List<bool> criPercents = zvsPlayer.Attacks.Where(a => a.ID == 2106601422).Select(a => a.Cri).ToList();
                    attackData.Add(new Tuple<string, List<int>, List<bool>, List<bool>>(zvsPlayer.Name, matchingAttacks, jaPercents, criPercents));
                }
            }
            else if (MainWindow.SepHTF && Player.IsFinish)
            {
                foreach (Player htf in MainWindow.workingList) { if (0 < htf.HTFDamage) { finishNames.Add(htf.ID); } }
                foreach (string htf in finishNames)
                {
                    Player htfPlayer = MainWindow.workingList.First(x => x.ID == htf);
                    List<int> fmatchingAttacks = htfPlayer.Attacks.Where(a => 0 <= Sepid.HTFAtkID.BinarySearch(a.ID)).Select(a => a.Damage).ToList();
                    List<bool> jaPercents = htfPlayer.Attacks.Where(a => 0 <= Sepid.HTFAtkID.BinarySearch(a.ID)).Select(a => a.JA).ToList();
                    List<bool> criPercents = htfPlayer.Attacks.Where(a => 0 <= Sepid.HTFAtkID.BinarySearch(a.ID)).Select(a => a.Cri).ToList();
                    attackData.Add(new Tuple<string, List<int>, List<bool>, List<bool>>(htfPlayer.Name, fmatchingAttacks, jaPercents, criPercents));
                }
            }
            else
            {
                List<MainWindow.PAHit> temphits = new List<MainWindow.PAHit>();
                foreach (Hit atk in Player.Attacks)
                {
                    //PAID -> PAName
                    string temp = atk.ID.ToString();
                    if ((MainWindow.SepZvs && atk.ID == 2106601422) || (MainWindow.SepHTF && 0 <= Sepid.HTFAtkID.BinarySearch(atk.ID))) { continue; } //ザンバースの場合に何もしない
                    if (MainWindow.skillDict.ContainsKey(atk.ID)) { temp = MainWindow.skillDict[atk.ID]; } // these are getting disposed anyway, no 1 cur
                    if (!attackNames.Contains(temp)) { attackNames.Add(temp); }
                    temphits.Add(new MainWindow.PAHit(temp, atk.Damage, atk.JA, atk.Cri));
                }

                foreach (string paname in attackNames)
                {
                    //マッチングアタックからダメージを選択するだけ
                    List<int> matchingAttacks = temphits.Where(a => a.Name == paname).Select(a => a.Damage).ToList();
                    List<bool> jaPercents = temphits.Where(a => a.Name == paname).Select(a => a.JA).ToList();
                    List<bool> criPercents = temphits.Where(a => a.Name == paname).Select(a => a.Cri).ToList();
                    attackData.Add(new Tuple<string, List<int>, List<bool>, List<bool>>(paname, matchingAttacks, jaPercents, criPercents));
                }
            }

            attackData = attackData.OrderByDescending(x => x.Item2.Sum()).ToList();

            foreach (var i in attackData)
            {
                List<int> exja = i.Item3.ConvertAll(x => Convert.ToInt32(x));
                List<int> excri = i.Item4.ConvertAll(x => Convert.ToInt32(x));

                string min, max, avg, ja, cri;
                double percent = i.Item2.Sum() * 100d / Player.Damage;
                string spacer = (percent >= 9) ? "" : " ";

                string paddedPercent = percent.ToString("00.00").Substring(0, 5);
                string hits = i.Item2.Count().ToString("N0");
                string sum = i.Item2.Sum().ToString("N0");
                if (i.Item2.Any())
                {
                    min = i.Item2.Min().ToString("N0");
                    max = i.Item2.Max().ToString("N0");
                    avg = i.Item2.Average().ToString("N0");
                    ja = (exja.Average() * 100).ToString("N2");
                    cri = (excri.Average() * 100).ToString("N2");
                }
                else
                {
                    min = "0";
                    max = "0";
                    avg = "0";
                    ja = "0.00";
                    cri = "0.00";
                }

                PAList pa = new PAList(i.Item1, paddedPercent, sum, ja, cri, hits, min, avg, max);
                PAS.Add(pa);
            }
            PlayerID.Content = Player.ID;
            PlayerName.Content = Player.Name;
            Rate.Content = Player.RatioPercent + "%";
        }

    }

    public class PAList
    {
        public PAList(string name, string rate, string dmg, string ja, string cri, string hit, string min, string avg, string max)
        { PAName = name; Rate = rate; Dmg = dmg; JA = ja; Crtcl = cri; Hit = hit; Min = min; Avg = avg; Max = max; }

        public string PAName { get; set; }
        public string Rate { get; set; }
        public string Dmg { get; set; }
        public string JA { get; set; }
        public string Crtcl { get; set; }
        public string Hit { get; set; }
        public string Min { get; set; }
        public string Avg { get; set; }
        public string Max { get; set; }
    }

}
