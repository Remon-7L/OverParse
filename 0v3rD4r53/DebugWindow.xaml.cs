#if DEBUG
using System.IO;
using System.Windows;

namespace Ov3rD4r53
{
    /// <summary>
    /// DebugWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow() => InitializeComponent();


        private void AddDBAttack_Click(object sender, RoutedEventArgs e) => AddAttack("13470610", 267911699, PType.HTF, 1024);

        private void AddLswAttack_Click(object sender, RoutedEventArgs e) => AddAttack("13470610", 2235773608, PType.Lsw, 1024);

        private void AddZvsAttack_Click(object sender, RoutedEventArgs e) => AddAttack("13470610", 2106601422, PType.Zvs, 1024);

        public void AddAttack(string userid, uint atkid, PType type, int dmg)
        {
            MainWindow m = (MainWindow)Application.Current.MainWindow;
            int index = -1;
            MainWindow.current.ActiveTime++;
            foreach (Player temp in MainWindow.current.players) { if (temp.ID == userid && temp.Type == PType.raw) { index = MainWindow.current.players.IndexOf(temp); } } //index処理
            if (index == -1)
            {
                MainWindow.current.players.Add(new Player(userid, "Developer", PType.raw));
                index = MainWindow.current.players.Count - 1;
            }
            Player tempattack = MainWindow.current.players[index];
            MainWindow.current.totalDamage += dmg;
            if (type == PType.Zvs) { tempattack.ZvsDamage += dmg; MainWindow.current.totalZanverse += dmg; }
            else if (type == PType.HTF) { tempattack.DBDamage += dmg; MainWindow.current.totalDBDamage += dmg; }
            else if (type == PType.Lsw) { tempattack.LswDamage += dmg; MainWindow.current.totalLswDamage += dmg; }
            tempattack.Damage += dmg;
            tempattack.Attacks.Add(new Hit(atkid, dmg, true, true, 0));
            MainWindow.IsRunning = true;
        }

        private void ShowColorBox(object sender, RoutedEventArgs e)
        {
            SelectColor selectwindow = new SelectColor(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            selectwindow.Show();
        }

        private void LoadCsvFile(object sender, RoutedEventArgs e)
        {
            MainWindow mainwindow = (MainWindow)Application.Current.MainWindow;
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "combat.csv(*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == false) { return; }

            FileStream fileStream = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.Seek(0, SeekOrigin.Begin);
            MainWindow.logReader = new StreamReader(fileStream);
            MainWindow.logReader.ReadLine();
            /*
            string[] dataLine = File.ReadAllLines(dialog.FileName);
            foreach (string line in dataLine)
            {
                if (line == "") { continue; }
                if (line == "timestamp, instanceID, sourceID, sourceName, targetID, targetName, attackID, damage, IsJA, IsCrit, IsMultiHit, IsMisc, IsMisc2") { continue; }
                string[] parts = line.Split(',');
                if (parts[0] == "0" && parts[3] == "YOU") { MainWindow.currentPlayerID = parts[2]; continue; }
                if (!MainWindow.current.instances.Contains(int.Parse(parts[1]))) { MainWindow.current.instances.Add(int.Parse(parts[1])); }
                if (int.Parse(parts[7]) < 0) { continue; }
                if (parts[2] == "0" || uint.Parse(parts[6]) == 0) { continue; }
                if (Sepid.IgnoreAtkID.Contains(uint.Parse(parts[6]))) { continue; }
                if (Properties.Settings.Default.Onlyme && parts[2] != MainWindow.currentPlayerID) { continue; }

                int lineTimestamp = int.Parse(parts[0]);
                string sourceID = parts[2];
                string sourceName = parts[3];
                string targetID = parts[4];
                string targetName = parts[5];
                uint attackID = uint.Parse(parts[6]); //WriteLog()にてID<->Nameの相互変換がある為int化が無理.......なはずだった
                int hitDamage = int.Parse(parts[7]);
                bool JA = (parts[8] == "1") ? true : false;
                bool Cri = (parts[9] == "1") ? true : false;
                if (sourceName.Contains("comma")) { sourceName = sourceName.Replace("comma", ","); }
                if (targetName.Contains("comma")) { targetName = targetName.Replace("comma", ","); }

                //処理スタート
                if (10000000 < int.Parse(sourceID)) //Player->Enemy
                {
                    if (!MainWindow.IsRunning) { MainWindow.current = new Session(); }

                    if (MainWindow.current.startTimestamp == 0) //初期が0だった場合
                    {
                        MainWindow.current.startTimestamp = lineTimestamp; MainWindow.current.nowTimestamp = lineTimestamp;
                    }

                    if (0 < (lineTimestamp - MainWindow.current.nowTimestamp))
                    {
                        MainWindow.current.diffTime++;
                        MainWindow.current.nowTimestamp = lineTimestamp;
                    }

                    if (Properties.Settings.Default.QuestTime) { MainWindow.current.ActiveTime = MainWindow.current.diffTime; } else { MainWindow.current.ActiveTime = lineTimestamp - MainWindow.current.startTimestamp; }


                    Player p;
                    if (MainWindow.current.players.Any(i => i.ID == sourceID && i.Type == PType.raw))
                    {
                        p = MainWindow.current.players.First(x => x.ID == sourceID && x.Type == PType.raw);
                    }
                    else
                    {
                        MainWindow.current.players.Add(new Player(sourceID, sourceName, PType.raw));
                        p = MainWindow.current.players[MainWindow.current.players.Count - 1];
                    }

                    if (Sepid.DBAtkID.Contains(attackID)) { p.DBDamage += hitDamage; MainWindow.current.totalDBDamage += hitDamage; p.DBCount++; if (JA) { p.DBJACount++; } if (Cri) { p.DBCriCount++; } if (p.DBMaxdmg < hitDamage) { p.DBMaxdmg = hitDamage; p.DBMaxID = attackID; } }
                    else if (Sepid.LswAtkID.Contains(attackID)) { p.LswDamage += hitDamage; MainWindow.current.totalLswDamage += hitDamage; p.LswCount++; if (JA) { p.LswJACount++; } if (Cri) { p.LswCriCount++; } if (p.LswMaxdmg < hitDamage) { p.LswMaxdmg = hitDamage; p.LswMaxID = attackID; } }
                    else if (Sepid.PwpAtkID.Contains(attackID)) { p.PwpDamage += hitDamage; MainWindow.current.totalPwpDamage += hitDamage; p.PwpCount++; if (JA) { p.PwpJACount++; } if (Cri) { p.PwpCriCount++; } if (p.PwpMaxdmg < hitDamage) { p.PwpMaxdmg = hitDamage; p.PwpMaxID = attackID; } }
                    else if (Sepid.AISAtkID.Contains(attackID)) { p.AisDamage += hitDamage; MainWindow.current.totalAisDamage += hitDamage; p.AisCount++; if (JA) { p.AisJACount++; } if (Cri) { p.AisCriCount++; } if (p.AisMaxdmg < hitDamage) { p.AisMaxdmg = hitDamage; p.AisMaxID = attackID; } }
                    else if (Sepid.RideAtkID.Contains(attackID)) { p.RideDamage += hitDamage; MainWindow.current.totalRideDamage += hitDamage; p.RideCount++; if (JA) { p.RideCount++; } if (Cri) { p.RideCriCount++; } if (p.RideMaxdmg < hitDamage) { p.RideMaxdmg = hitDamage; p.RideMaxID = attackID; } }
                    else { p.AllyDamage += hitDamage; MainWindow.current.totalAllyDamage += hitDamage; p.AllyCount++; if (JA) { p.AllyJACount++; } if (Cri) { p.AllyCriCount++; } if (p.AllyMaxdmg < hitDamage) { p.AllyMaxdmg = hitDamage; p.AllyMaxID = attackID; } }
                    if (attackID == 2106601422) { p.ZvsDamage += hitDamage; MainWindow.current.totalZanverse += hitDamage; p.ZvsCount++; if (Cri) { p.ZvsCriCount++; } if (p.ZvsMaxdmg < hitDamage) { p.ZvsMaxdmg = hitDamage; } }
                    if (Sepid.HTFAtkID.Contains(attackID)) { p.HTFDamage += hitDamage; MainWindow.current.totalFinish += hitDamage; if (JA) { p.HTFJACount++; } if (Cri) { p.HTFCriCount++; } if (p.HTFMaxdmg < hitDamage) { p.HTFMaxdmg = hitDamage; p.HTFMaxID = attackID; } }
                    MainWindow.current.totalDamage += hitDamage; p.Damage += hitDamage; p.AttackCount++;
                    if (JA) { p.JACount++; }
                    if (Cri) { p.CriCount++; }
                    if (p.Maxdmg < hitDamage) { p.Maxdmg = hitDamage; p.MaxHitID = attackID; }
                    p.Attacks.Add(new Hit(attackID, hitDamage, JA, Cri, 0));

                    MainWindow.IsRunning = true;
                }
                else if (10000000 < int.Parse(targetID)) //Enemy->Player
                {
                    if (!MainWindow.IsRunning) { continue; } //被ダメージからセッションが始まらないようにする

                    if (MainWindow.current.players.Any(p => p.ID == targetID && p.Type == PType.raw))
                    {
                        Player p = MainWindow.current.players.First(x => x.ID == targetID && x.Type == PType.raw);
                        p.Damaged += hitDamage;
                    }
                    else
                    {
                        Player newplayer = new Player(targetID, targetName, PType.raw);
                        newplayer.Damaged += hitDamage;
                        MainWindow.current.players.Add(newplayer);
                    }

                }
            }
            MainWindow.current.players.Sort((x, y) => y.ReadDamage.CompareTo(x.ReadDamage));
            */
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow m = (MainWindow)Application.Current.MainWindow;
            //MessageBox.Show(MainWindow.overlay.IsLoaded.ToString());
        }

    }
}
#endif
