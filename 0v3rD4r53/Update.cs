using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ov3rD4r53
{
    public partial class MainWindow : YKToolkit.Controls.Window
    {
        public static string lastStatus = "";
        public static byte noUpdateCount = 0;
        public static int speechcount = 1;

        //Session.current
        public async Task<bool> UpdateLog(object sender, EventArgs e)
        {
            if (logReader == null) { return false; }
            string newLines = await logReader.ReadToEndAsync();
            if (newLines == "") { return false; }

            string[] dataLine = newLines.Split('\n');
            foreach (string line in dataLine)
            {
                if (line == "") { continue; }
                string[] parts = line.Split(',');
                if (parts[0] == "0" && parts[3] == "YOU") { if (!currentPlayerID.Any(x => x == parts[2])) { currentPlayerID.Append(parts[2]); } continue; }
                if (!current.instances.Contains(int.Parse(parts[1]))) { current.instances.Add(int.Parse(parts[1])); }
                if (int.Parse(parts[7]) < 0) { continue; }
                if (parts[2] == "0" || uint.Parse(parts[6]) == 0) { continue; }
                if (0 <= Sepid.IgnoreAtkID.BinarySearch(uint.Parse(parts[6]))) { continue; }
                if (IsOnlyme && currentPlayerID.Any(x => x != parts[2])) { continue; }

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

                if (currentPlayerID.Any(x => x == sourceID)) { userattacks.Add(new Hit(attackID, hitDamage, JA, Cri, lineTimestamp)); }

                //処理スタート
                if (10000000 < int.Parse(sourceID)) //Player->Enemy
                {
                    if (!IsRunning) { current = new Session(); }

                    if (current.startTimestamp == 0) //初期が0だった場合
                    {
                        current.startTimestamp = lineTimestamp; current.nowTimestamp = lineTimestamp;
                    }

                    if (0 < (lineTimestamp - current.nowTimestamp))
                    {
                        current.diffTime++;
                        current.nowTimestamp = lineTimestamp;
                    }

                    if (IsQuestTime) { current.ActiveTime = current.diffTime; }
                    else { current.ActiveTime = lineTimestamp - current.startTimestamp; }

                    Player p;
                    if (current.players.Any(i => i.ID == sourceID && i.Type == PType.raw))
                    {
                        p = current.players.First(x => x.ID == sourceID && x.Type == PType.raw);
                    }
                    else
                    {
                        current.players.Add(new Player(sourceID, sourceName, PType.raw));
                        p = current.players[current.players.Count - 1];
                    }

                    if (0 <= Sepid.DBAtkID.BinarySearch(attackID)) { p.DBDamage += hitDamage; current.totalDBDamage += hitDamage; p.DBCount++; if (JA) { p.DBJACount++; } if (Cri) { p.DBCriCount++; } if (p.DBMaxdmg < hitDamage) { p.DBMaxdmg = hitDamage; p.DBMaxID = attackID; } }
                    else if (0 <= Sepid.LswAtkID.BinarySearch(attackID)) { p.LswDamage += hitDamage; current.totalLswDamage += hitDamage; p.LswCount++; if (JA) { p.LswJACount++; } if (Cri) { p.LswCriCount++; } if (p.LswMaxdmg < hitDamage) { p.LswMaxdmg = hitDamage; p.LswMaxID = attackID; } }
                    else if (0 <= Sepid.PwpAtkID.BinarySearch(attackID)) { p.PwpDamage += hitDamage; current.totalPwpDamage += hitDamage; p.PwpCount++; if (JA) { p.PwpJACount++; } if (Cri) { p.PwpCriCount++; } if (p.PwpMaxdmg < hitDamage) { p.PwpMaxdmg = hitDamage; p.PwpMaxID = attackID; } }
                    else if (0 <= Sepid.AISAtkID.BinarySearch(attackID)) { p.AisDamage += hitDamage; current.totalAisDamage += hitDamage; p.AisCount++; if (JA) { p.AisJACount++; } if (Cri) { p.AisCriCount++; } if (p.AisMaxdmg < hitDamage) { p.AisMaxdmg = hitDamage; p.AisMaxID = attackID; } }
                    else if (0 <= Sepid.RideAtkID.BinarySearch(attackID)) { p.RideDamage += hitDamage; current.totalRideDamage += hitDamage; p.RideCount++; if (JA) { p.RideCount++; } if (Cri) { p.RideCriCount++; } if (p.RideMaxdmg < hitDamage) { p.RideMaxdmg = hitDamage; p.RideMaxID = attackID; } }
                    else { p.AllyDamage += hitDamage; current.totalAllyDamage += hitDamage; p.AllyCount++; if (JA) { p.AllyJACount++; } if (Cri) { p.AllyCriCount++; } if (p.AllyMaxdmg < hitDamage) { p.AllyMaxdmg = hitDamage; p.AllyMaxID = attackID; } }
                    if (attackID == 2106601422) { p.ZvsDamage += hitDamage; current.totalZanverse += hitDamage; p.ZvsCount++; current.totalZvsCount++; if (Cri) { p.ZvsCriCount++; current.totalZvsCriCount++; } if (current.ZvsMaxdmg < hitDamage) { current.ZvsMaxdmg = hitDamage; } }
                    if (0 <= Sepid.HTFAtkID.BinarySearch(attackID)) { p.HTFDamage += hitDamage; current.totalFinish += hitDamage; current.totalHTFCount++; if (JA) { p.HTFJACount++; current.totalHTFJACount++; } if (Cri) { p.HTFCriCount++; current.totalHTFCriCount++; } if (current.HTFMaxdmg < hitDamage) { current.HTFMaxdmg = hitDamage; current.HTFMaxID = attackID; } }
                    current.totalDamage += hitDamage; p.Damage += hitDamage; p.AttackCount++;
                    if (JA) { p.JACount++; }
                    if (Cri) { p.CriCount++; }
                    if (p.Maxdmg < hitDamage) { p.Maxdmg = hitDamage; p.MaxHitID = attackID; }
                    p.Attacks.Add(new Hit(attackID, hitDamage, JA, Cri, current.diffTime));

                    IsRunning = true;
                }
                else if (10000000 < int.Parse(targetID)) //Enemy->Player
                {
                    if (!IsRunning) { continue; } //被ダメージからセッションが始まらないようにする

                    if (current.players.Any(p => p.ID == targetID && p.Type == PType.raw))
                    {
                        Player p = current.players.First(x => x.ID == targetID && x.Type == PType.raw);
                        p.Damaged += hitDamage;
                    }
                    else
                    {
                        Player newplayer = new Player(targetID, targetName, PType.raw);
                        newplayer.Damaged += hitDamage;
                        current.players.Add(newplayer);
                    }

                }
            }

            current.players.Sort((x, y) => y.ReadDamage.CompareTo(x.ReadDamage));
            return true;
        }


        public async void UpdateForm(object sender, EventArgs e)
        {
            if (current.players == null) { return; }
            damageTimer.Stop();
            if (Properties.Settings.Default.Clock) { Datetime.Content = DateTime.Now.ToString("HH:mm:ss.ff"); }

            bool IsLogContain = await UpdateLog(this, null);
            if (IsLogContain == false && 10 < noUpdateCount) { noUpdateCount++; StatusUpdate(); damageTimer.Start(); return; }

            noUpdateCount = 0;
            // get a copy of the right combatants
            workingList = new List<Player>(current.players);

            //Separate Part
            if (SepDB && 0 < current.totalDBDamage) { SeparateDBMethod(); }
            if (SepLsw && 0 < current.totalLswDamage) { SeparateLswMethod(); }
            if (SepPwp && 0 < current.totalPwpDamage) { SeparatePwpMethod(); }
            if (SepAIS && 0 < current.totalAisDamage) { SeparateAISMethod(); }
            if (SepRide && 0 < current.totalRideDamage) { SeparateRideMethod(); }

            //分けたものを含めて再ソート(ザンバース,HTFを最後にする為)
            if (SeparateTab.SelectedIndex == 0) { workingList.Sort((x, y) => y.ReadDamage.CompareTo(x.ReadDamage)); }

            if (SepHTF && 0 < current.totalFinish) { SeparateHTF(); }
            if (SepZvs && 0 < current.totalZanverse) { SeparateZvs(); }

            // get group damage totals
            long totalReadDamage = workingList.Sum(x => x.ReadDamage);

            long totalAVG = workingList.Any(p => p.Type == PType.raw) ? current.totalDamage / workingList.Count(p => p.Type == PType.raw) : 1;

            // dps calcs!
            foreach (Player p in workingList)
            {
                p.PercentReadDPS = p.ReadDamage / (double)totalReadDamage * 100;
                p.AllyPct = p.AllyDamage / (double)current.totalAllyDamage * 100;
                p.DBPct = p.DBDamage / (double)current.totalDBDamage * 100;
                p.LswPct = p.LswDamage / (double)current.totalLswDamage * 100;
                p.PwpPct = p.PwpDamage / (double)current.totalPwpDamage * 100;
                p.AisPct = p.AisDamage / (double)current.totalAisDamage * 100;
                p.RidePct = p.RideDamage / (double)current.totalRideDamage * 100;
                if (p.Type == PType.raw)
                {
                    p.TScore = Math.Abs(p.Damage - totalAVG) * Math.Abs(p.Damage - totalAVG);
                }
            }

            //Hensa
            if (workingList.Any(p => p.Type == PType.raw))
            {
                current.totalSD = Math.Sqrt(workingList.Where(p => p.Type == PType.raw).Average(x => x.TScore));
                foreach (Player c in workingList)
                {
                    double temp = Math.Abs(c.Damage - totalAVG) * 10 / current.totalSD;
                    if (c.ReadDamage < totalAVG) { c.TScore = 50.0 - temp; }
                    else if (totalAVG < c.ReadDamage) { c.TScore = 50.0 + temp; }
                    else { c.TScore = 50.00; }
                }
            }

            // status pane updates
            StatusUpdate();

            //damage graph stuff
            current.firstDamage = 0;
            // clear out the list
            CombatantData.Items.Clear();    //Very Heavy Method
            AllyData.Items.Clear();
            DBData.Items.Clear();
            LswData.Items.Clear();
            PwpData.Items.Clear();
            AisData.Items.Clear();
            RideData.Items.Clear();

            foreach (Player c in workingList)
            {
                if (c.IsAlly && current.firstDamage < c.ReadDamage) { current.firstDamage = c.ReadDamage; }

                bool filtered = true;
                if (SepDB || SepLsw || SepPwp || SepAIS || SepRide)
                {
                    if (c.IsAlly && c.Type == PType.raw && !HidePlayers.IsChecked) { filtered = false; }
                    if (c.IsAlly && c.Type == PType.AIS && !HideAIS.IsChecked) { filtered = false; }
                    if (c.IsAlly && c.Type == PType.HTF && !HideDB.IsChecked) { filtered = false; }
                    if (c.IsAlly && c.Type == PType.Ride && !HideRide.IsChecked) { filtered = false; }
                    if (c.IsAlly && c.Type == PType.Pwp && !HidePwp.IsChecked) { filtered = false; }
                    if (c.IsAlly && c.Type == PType.Lsw && !HideLsw.IsChecked) { filtered = false; }
                    if (c.IsZanverse) { filtered = false; }
                    if (c.IsFinish) { filtered = false; }
                }
                else
                {
                    if (c.IsAlly || c.IsZanverse || c.IsFinish) { filtered = false; }
                }

                if (!filtered && (0 < c.ReadDamage) && (SeparateTab.SelectedIndex == 0)) { CombatantData.Items.Add(c); }
                if ((0 < c.AllyDamage) && (SeparateTab.SelectedIndex == 1)) { workingList.Sort((x, y) => y.AllyDamage.CompareTo(x.AllyDamage)); AllyData.Items.Add(c); }
                if ((0 < c.DBDamage) && (SeparateTab.SelectedIndex == 2)) { workingList.Sort((x, y) => y.DBDamage.CompareTo(x.DBDamage)); DBData.Items.Add(c); }
                if ((0 < c.LswDamage) && (SeparateTab.SelectedIndex == 3)) { workingList.Sort((x, y) => y.LswDamage.CompareTo(x.LswDamage)); LswData.Items.Add(c); }
                if ((0 < c.PwpDamage) && (SeparateTab.SelectedIndex == 4)) { workingList.Sort((x, y) => y.PwpDamage.CompareTo(x.PwpDamage)); PwpData.Items.Add(c); }
                if ((0 < c.AisDamage) && (SeparateTab.SelectedIndex == 5)) { workingList.Sort((x, y) => y.AisDamage.CompareTo(x.AisDamage)); AisData.Items.Add(c); }
                if ((0 < c.RideDamage) && (SeparateTab.SelectedIndex == 6)) { workingList.Sort((x, y) => y.RideDamage.CompareTo(x.RideDamage)); RideData.Items.Add(c); }
            }

            //Overlay
            if (IsRunning && overlay != null)
            {
                Player me = workingList.Where(p => currentPlayerID.Any(x => x == p.ID) && p.Type == PType.raw).FirstOrDefault();
                if (me != null)
                {
                    int rank = workingList.IndexOf(me);
                    overlay.Rank.Content = "#" + (rank + 1).ToString() + "  " + me.RatioPercent + "%";
                    overlay.TScore.Content = me.ReadTScore;
                    overlay.Damage.Content = me.BindDamage;
                    overlay.DPS.Content = "DPS : " + me.FDPSReadout;
                    overlay.JA.Content = "JA : " + me.WJAPercent + "%";

                    if (1 < workingList.Count)
                    {
                        if (rank == 0)
                        {
                            overlay.Next.Content = "+" + (me.Damage - workingList[1].Damage).ToString("N0");
                            overlay.Next.Foreground = new SolidColorBrush(Overlay.WinBrush);
                        }
                        else
                        {
                            overlay.Next.Content = "-" + (workingList[rank - 1].Damage - me.Damage).ToString("N0");
                            overlay.Next.Foreground = new SolidColorBrush(Overlay.LoseBrush);
                        }
                    }
                    else
                    {
                        overlay.Next.Content = "";
                    }
                }
            }
            else if (overlay != null)
            {
                overlay.Rank.Content = "RDY"; overlay.TScore.Content = ""; overlay.Damage.Content = "";
                overlay.Next.Content = ""; overlay.DPS.Content = ""; overlay.JA.Content = "";
            }

            //TTS
            if (Properties.Settings.Default.Bouyomi && IsConnect && IsRunning)
            {
                Player me = workingList.Where(p => currentPlayerID.Any(x => x == p.ID) && p.Type == PType.raw).FirstOrDefault();
                int nexttime = speechcount * Properties.Settings.Default.BouyomiSpan;
                if (current.ActiveTime != 0 && me != null && nexttime < current.ActiveTime)
                {
                    speechcount = current.ActiveTime / Properties.Settings.Default.BouyomiSpan + 1;
                    string text = (nexttime / 60).ToString() + "分";
                    if (nexttime % 60 != 0) { text += (nexttime % 60).ToString() + "秒"; }
                    text += "経過　";
                    if (!Properties.Settings.Default.BouyomiFormat)
                    {
                        text += me.DPS.ToString("N0") + "DPS　";
                        text += (workingList.IndexOf(me) + 1).ToString() + "位";
                    }
                    else if (Properties.Settings.Default.BouyomiFormat)
                    {
                        text += me.DPS.ToString("N0") + "でぃーぴーえす　";
                        text += (workingList.IndexOf(me) + 1).ToString() + "い";
                    }
                    ReadUp(text);
                }

            }

            // autoend
            if (Properties.Settings.Default.AutoEndEncounters && IsRunning)
            {
                int unixTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if ((unixTimestamp - current.nowTimestamp) >= Properties.Settings.Default.EncounterTimeout) { EndEncounter_Click(null, null); }
            }
            damageTimer.Start();
        }

        private void StatusUpdate()
        {
            if (IsRunning)
            {
                EncounterIndicator.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 192, 255));
                TimeSpan timespan = TimeSpan.FromSeconds(current.ActiveTime);
                string timer = timespan.ToString(@"h\:mm\:ss");
                EncounterStatus.Content = $"{timer}";

                double totalDPS = current.totalDamage / (double)current.ActiveTime;
                if (totalDPS > 0) { EncounterStatus.Content += $" - Total : {current.totalDamage.ToString("N0")}" + $" - {totalDPS.ToString("N0")} DPS"; }
                if (!SepZvs) { EncounterStatus.Content += $" - Zanverse : {current.totalZanverse.ToString("N0")}"; }
                lastStatus = EncounterStatus.Content.ToString();
            }
            else if (!damagelogs.Exists || !damagelogs.GetFiles().Any())
            {
                EncounterIndicator.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 128, 128));
                EncounterStatus.Content = "Directory No Logs : (Re)Start PSO2, Attack Enemy  or  Failed dll plugin Install";
            }
            else if (!IsRunning)
            {
                EncounterIndicator.Fill = new SolidColorBrush(Color.FromArgb(255, 64, 192, 64));
                EncounterStatus.Content = $"Waiting - {lastStatus}";
                if (lastStatus == "") { EncounterStatus.Content = "Waiting... - " + damagelogcsv.Name + updatemsg; }
            }
        }

        #region Separate
        private void SeparateDBMethod()
        {
            List<Player> addDBPlayer = new List<Player>();
            foreach (Player p in workingList)
            {
                if (p.Type != PType.raw) { continue; }
                if (0 < p.DBDamage)
                {
                    Player separate = new Player(p.ID, "DB|" + p.Name, PType.HTF)
                    {
                        Damage = p.DBDamage,
                        JACount = p.DBJACount,
                        CriCount = p.DBCriCount,
                        AttackCount = p.DBCount,
                        MaxHitID = p.DBMaxID,
                        Maxdmg = p.DBMaxdmg
                    };
                    addDBPlayer.Add(separate);
                }
            }
            workingList.AddRange(addDBPlayer);
        }

        private void SeparateLswMethod()
        {
            List<Player> addLswPlayer = new List<Player>();
            foreach (Player p in workingList)
            {
                if (p.Type != PType.raw) { continue; }
                if (0 < p.LswDamage)
                {
                    Player separate = new Player(p.ID, "Lsw|" + p.Name, PType.Lsw)
                    {
                        Damage = p.LswDamage,
                        JACount = p.LswJACount,
                        CriCount = p.LswCriCount,
                        AttackCount = p.LswCount,
                        MaxHitID = p.LswMaxID,
                        Maxdmg = p.LswMaxdmg
                    };
                    addLswPlayer.Add(separate);
                }
            }
            workingList.AddRange(addLswPlayer);
        }

        private void SeparatePwpMethod()
        {
            List<Player> addPwpPlayer = new List<Player>();
            foreach (Player p in workingList)
            {
                if (p.Type != PType.raw) { continue; }
                if (0 < p.PwpDamage)
                {
                    Player separate = new Player(p.ID, "Pwp|" + p.Name, PType.Pwp)
                    {
                        Damage = p.PwpDamage,
                        JACount = p.PwpJACount,
                        CriCount = p.PwpCriCount,
                        AttackCount = p.PwpCount,
                        MaxHitID = p.PwpMaxID,
                        Maxdmg = p.PwpMaxdmg
                    };
                    addPwpPlayer.Add(separate);
                }
            }
            workingList.AddRange(addPwpPlayer);
        }

        private void SeparateAISMethod()
        {
            List<Player> addAISPlayer = new List<Player>();
            foreach (Player p in workingList)
            {
                if (p.Type != PType.raw) { continue; }
                if (0 < p.AisDamage)
                {
                    Player separate = new Player(p.ID, "AIS|" + p.Name, PType.AIS)
                    {
                        Damage = p.AisDamage,
                        JACount = p.AisJACount,
                        CriCount = p.AisCriCount,
                        AttackCount = p.AisCount,
                        MaxHitID = p.AisMaxID,
                        Maxdmg = p.AisMaxdmg
                    };
                    addAISPlayer.Add(separate);
                }
            }
            workingList.AddRange(addAISPlayer);
        }

        private void SeparateRideMethod()
        {
            List<Player> addRidePlayer = new List<Player>();
            foreach (Player p in workingList)
            {
                if (p.Type != PType.raw) { continue; }
                if (0 < p.RideDamage)
                {
                    Player separate = new Player(p.ID, "Ride|" + p.Name, PType.Ride)
                    {
                        Damage = p.RideDamage,
                        JACount = p.RideJACount,
                        CriCount = p.RideCriCount,
                        AttackCount = p.RideCount,
                        MaxHitID = p.RideMaxID,
                        Maxdmg = p.RideMaxdmg
                    };
                    addRidePlayer.Add(separate);
                }
            }
            workingList.AddRange(addRidePlayer);
        }

        private void SeparateHTF()
        {
            Player addHTFPlayer = new Player("99999998", "HTF Attacks", PType.HTF);
            addHTFPlayer.Damage += current.totalFinish;
            addHTFPlayer.JACount = current.totalHTFJACount;
            addHTFPlayer.CriCount = current.totalHTFCriCount;
            addHTFPlayer.AttackCount = current.totalHTFCount;
            addHTFPlayer.Maxdmg = current.HTFMaxdmg;
            addHTFPlayer.MaxHitID = current.HTFMaxID;
            workingList.Add(addHTFPlayer);
        }

        private void SeparateZvs()
        {
            Player addZvsPlayer = new Player("99999999", "Zanverse", PType.Zvs);
            addZvsPlayer.Damage += current.totalZanverse;
            addZvsPlayer.JACount = 0;
            addZvsPlayer.CriCount = current.totalZvsCriCount;
            addZvsPlayer.AttackCount = current.totalZvsCount;
            addZvsPlayer.Maxdmg = current.ZvsMaxdmg;
            addZvsPlayer.MaxHitID = 2106601422;
            workingList.Add(addZvsPlayer);
        }

        #endregion Separate

        /// <summary>
        /// Output Log
        /// </summary>
        /// <returns>filename</returns>
        public string WriteLog()
        {
            if (current.players.Count == 0) { return null; }
            if (current.ActiveTime == 0) { current.ActiveTime = 1; }
            string timer = TimeSpan.FromSeconds(current.ActiveTime).ToString(@"hh\:mm\:ss");
            StringBuilder log = new StringBuilder();
            _ = log.Append($"{DateTime.Now.ToString("F")} | {timer} | TotalDamage : { current.totalDamage.ToString("N0")} | TotalDPS : {current.totalDPS.ToString("N0")}").AppendLine().AppendLine();

            foreach (Player c in workingList)
            {
                if (Properties.Settings.Default.IsWriteTS)
                {
                    _ = log.Append($"{c.Name} | {c.RatioPercent}% | 偏差値:{c.ReadTScore} | {c.ReadDamage.ToString("N0")} dmg | {c.Writedmgd} dmgd | {c.DPS} DPS | JA: {c.WJAPercent}% | Critical: {c.WCRIPercent}% | Max: {c.WriteMaxdmg} ({c.MaxHit})").AppendLine();
                }
                else
                {
                    _ = log.Append($"{c.Name} | {c.RatioPercent}% | {c.ReadDamage.ToString("N0")} dmg | {c.Writedmgd} dmgd | {c.DPS} DPS | JA: {c.WJAPercent}% | Critical: {c.WCRIPercent}% | Max: {c.WriteMaxdmg} ({c.MaxHit})").AppendLine();
                }
            }

            _ = log.AppendLine().AppendLine();

            foreach (Player c in workingList)
            {
                if (c.IsDB || c.IsLsw || c.IsPwp || c.IsAIS || c.IsRide) { continue; }
                List<string> attackNames = new List<string>();
                List<string> finishNames = new List<string>();
                List<Tuple<string, List<int>, List<bool>, List<bool>>> attackData = new List<Tuple<string, List<int>, List<bool>, List<bool>>>();


                _ = log.AppendLine($"[ {c.Name} - {c.RatioPercent}% - {c.ReadDamage.ToString("N0")} dmg ]").AppendLine().AppendLine();

                if (SepZvs && c.IsZanverse)
                {
                    foreach (Player zvs in current.players) { if (0 < zvs.ZvsDamage) { attackNames.Add(zvs.ID); } }
                    foreach (string s in attackNames)
                    {
                        Player zvsPlayer = current.players.First(x => x.ID == s);
                        List<int> matchingAttacks = zvsPlayer.Attacks.Where(atk => atk.ID == 2106601422).Select(a => a.Damage).ToList();
                        List<bool> jaPercents = new List<bool> { false }; List<bool> criPercents = c.Attacks.Where(a => a.ID == 2106601422).Select(a => a.Cri).ToList();
                        attackData.Add(new Tuple<string, List<int>, List<bool>, List<bool>>(zvsPlayer.Name, matchingAttacks, jaPercents, criPercents));
                    }
                }
                else if (SepHTF && c.IsFinish)
                {
                    foreach (Player htf in current.players) { if (0 < htf.HTFDamage) { finishNames.Add(htf.ID); } }
                    foreach (string htf in finishNames)
                    {
                        Player htfPlayer = current.players.First(x => x.ID == htf);
                        List<int> fmatchingAttacks = htfPlayer.Attacks.Where(a => 0 <= Sepid.HTFAtkID.BinarySearch(a.ID)).Select(a => a.Damage).ToList();
                        List<bool> jaPercents = c.Attacks.Where(a => 0 <= Sepid.HTFAtkID.BinarySearch(a.ID)).Select(a => a.JA).ToList();
                        List<bool> criPercents = c.Attacks.Where(a => 0 < Sepid.HTFAtkID.BinarySearch(a.ID)).Select(a => a.Cri).ToList();
                        attackData.Add(new Tuple<string, List<int>, List<bool>, List<bool>>(htfPlayer.Name, fmatchingAttacks, jaPercents, criPercents));
                    }
                }
                else if (c.Type == PType.raw)
                {
                    List<PAHit> temphits = new List<PAHit>();
                    foreach (Hit atk in c.Attacks)
                    {
                        //PAID -> PAName
                        string temp = atk.ID.ToString();
                        if ((SepZvs && atk.ID == 2106601422) || (SepHTF && 0 <= Sepid.HTFAtkID.BinarySearch(atk.ID))) { continue; } //ザンバースの場合に何もしない
                        if (skillDict.ContainsKey(atk.ID)) { temp = skillDict[atk.ID]; } // these are getting disposed anyway, no 1 cur
                        if (!attackNames.Contains(temp)) { attackNames.Add(temp); }
                        temphits.Add(new PAHit(temp, atk.Damage, atk.JA, atk.Cri));
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
                    double percent = i.Item2.Sum() * 100d / c.Damage;
                    string spacer = (percent >= 9) ? "" : " ";

                    string paddedPercent = percent.ToString("00.00").Substring(0, 5);
                    string hits = i.Item2.Count().ToString("N0");
                    string sum = i.Item2.Sum().ToString("N0");

                    min = i.Item2.Min().ToString("N0");
                    max = i.Item2.Max().ToString("N0");
                    avg = i.Item2.Average().ToString("N0");
                    ja = exja.Any() ? (exja.Average() * 100).ToString("N2") : "NaN";
                    cri = excri.Any() ? (excri.Average() * 100).ToString("N2") : "NaN";

                    _ = log.Append($"{paddedPercent}%	| {i.Item1} - {sum} dmg - JA : {ja}% - Critical : {cri}%").AppendLine();
                    _ = log.Append($"	|   {hits} hits - {min} min, {avg} avg, {max} max").AppendLine();
                }

                _ = log.AppendLine();
            }


            _ = log.Append("Instance IDs: " + string.Join(", ", current.instances.ToArray()));

            DateTime thisDate = DateTime.Now;
            string directory = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
            Directory.CreateDirectory($"Logs/{directory}");
            string datetime = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            string filename = $"Logs/{directory}/OverParse - {datetime}.txt";
            File.WriteAllText(filename, log.ToString());

            return filename;
        }

        public string ExportWriteLog()
        {
            return "";
            /*
            if (current.players.Count == 0) { return null; }
            if (current.ActiveTime == 0) { current.ActiveTime = 1; }
            string timer = TimeSpan.FromSeconds(current.ActiveTime).ToString(@"mm\:ss");
            string log = timer + " | TotalDamage : " + current.totalDamage.ToString("N0") + " | TotalDPS : " + current.totalDPS.ToString("N0") + " | 標準偏差 : " + current.totalSD.ToString("00.00") + Environment.NewLine + Environment.NewLine;
            log += "Num/Ratio/Dmg/Dmgd/DPS/JA/Cri/Max" + Environment.NewLine;
            byte rankcount = 0;

            foreach (Player p in workingList)
            {
                rankcount++;
                log += $"#{rankcount.ToString("X")} {p.RatioPercent}% {p.ReadTScore} {p.ReadDamage.ToString("N0")} {p.BindDamaged} {p.DPS} {p.WJAPercent} {p.WCRIPercent} {p.MaxHitdmg} ({p.MaxHit})" + Environment.NewLine;
            }

            log += Environment.NewLine + Environment.NewLine;

            log += "Instance IDs: " + string.Join(", ", current.instances.ToArray());

            DateTime thisDate = DateTime.Now;
            string directory = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
            Directory.CreateDirectory($"Export/{directory}");
            string datetime = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            string filename = $"Export/{directory}/Export - {datetime}.txt";
            File.WriteAllText(filename, log);

            return filename;
            */
        }

        private void ReadUp(string text)
        {
            byte bCode = 0;
            short iVoice = 1;
            short iVolume = -1;
            short iSpeed = -1;
            short iTone = -1;
            short iCommand = 0x0001;

            byte[] bMessage = Encoding.UTF8.GetBytes(text);
            Int32 iLength = bMessage.Length;

            //棒読みちゃんのTCPサーバへ接続
            string sHost = "127.0.0.1"; //棒読みちゃんが動いているホスト
            int iPort = 50001;       //棒読みちゃんのTCPサーバのポート番号(デフォルト値)
            TcpClient tc = null;
            try
            {
                tc = new TcpClient(sHost, iPort); // TODO: Dispose this
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            try
            {
                if (tc != null)
                {
                    //メッセージ送信
                    using (NetworkStream ns = tc.GetStream())
                    {
                        using (BinaryWriter bw = new BinaryWriter(ns))
                        {
                            bw.Write(iCommand); //コマンド（ 0:メッセージ読み上げ）
                            bw.Write(iSpeed);   //速度    （-1:棒読みちゃん画面上の設定）
                            bw.Write(iTone);    //音程    （-1:棒読みちゃん画面上の設定）
                            bw.Write(iVolume);  //音量    （-1:棒読みちゃん画面上の設定）
                            bw.Write(iVoice);   //声質    （ 0:棒読みちゃん画面上の設定、1:女性1、2:女性2、3:男性1、4:男性2、5:中性、6:ロボット、7:機械1、8:機械2、10001～:SAPI5）
                            bw.Write(bCode);    //文字列のbyte配列の文字コード(0:UTF-8, 1:Unicode, 2:Shift-JIS)
                            bw.Write(iLength);  //文字列のbyte配列の長さ
                            bw.Write(bMessage); //文字列のbyte配列
                        }
                    }
                }
            }
            catch
            {

            }

        }

        public struct PAHit //Use WriteLog Only
        {
            public string Name;
            public int Damage;
            public bool JA, Cri;
            public PAHit(string paname, int dmg, bool ja, bool cri) { Name = paname; Damage = dmg; JA = ja; Cri = cri; }
        }


    }
}
