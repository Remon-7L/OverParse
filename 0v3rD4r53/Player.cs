using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Ov3rD4r53
{
    /// <summary>
    /// Constant for setting player separate type
    /// </summary>
    public enum PType : byte
    {
        raw, Zvs, HTF, DB, Lsw, Pwp, AIS, Ride
    }

    /// <summary>
    /// Separate ID Lists
    /// </summary>
    public static class Sepid { public static List<uint> DBAtkID, LswAtkID, PwpAtkID, HTFAtkID, AISAtkID, RideAtkID, IgnoreAtkID; }

    public struct Hit //14byte
    {
        public uint ID;
        public int Damage, DiffTime; //Graph?
        public bool JA, Cri;
        public Hit(uint paid, int dmg, bool ja, bool cri, int time) { ID = paid; Damage = dmg; JA = ja; Cri = cri; DiffTime = time; }

        public string ReadID => ID.ToString();
        public string IDName
        {
            get
            {
                string paname = "Unknown";
                if (MainWindow.skillDict.ContainsKey(ID)) { paname = MainWindow.skillDict[ID]; }
                return paname;
            }
        }

        public string ReadDamage => Damage.ToString("N0");
        public string IsJA => JA ? "True" : "False";
        public string IsCri => Cri ? "True" : "False";
        public string UserTime => DiffTime.ToString();
    }

    /// <summary>
    /// Binding Property(MainWindow.xaml)
    /// </summary>
    public class Player //Welcome to Property Hell
    {
        public string ID, Name;
        public PType Type;
        public double PercentDPS, PercentReadDPS, AllyPct, DBPct, LswPct, PwpPct, AisPct, RidePct, TScore;
        public List<Hit> Attacks;
        //public List<Hit> Attacks, AllyAttacks, DBAttacks, LswAttacks, PwpAttacks, AisAttacks, RideAttacks;
        public long Damage, Damaged, ZvsDamage, HTFDamage, AllyDamage, DBDamage, LswDamage, PwpDamage, AisDamage, RideDamage;
        public ulong JACount, AllyJACount, DBJACount, LswJACount, PwpJACount, AisJACount, RideJACount, HTFJACount;
        public ulong CriCount, AllyCriCount, DBCriCount, LswCriCount, PwpCriCount, AisCriCount, RideCriCount, ZvsCriCount, HTFCriCount;
        public ulong AttackCount, AllyCount, DBCount, LswCount, PwpCount, AisCount, RideCount, HTFCount, ZvsCount;
        public uint MaxHitID, AllyMaxID, DBMaxID, LswMaxID, PwpMaxID, AisMaxID, RideMaxID;
        public int Maxdmg, AllyMaxdmg, DBMaxdmg, LswMaxdmg, PwpMaxdmg, AisMaxdmg, RideMaxdmg;

        public string DisplayName
        {
            get
            {
                //if (ID == "13470610") { return "Dev|" + Name; }
                //if (!Properties.Settings.Default.UserHighlight) { return "Anonymous"; }
                return Name;
            }
        }

        public string RatioPercent => $"{PercentReadDPS:00.00}";
        public string ReadTScore => Type == PType.raw ? $"{TScore:00.00}" : "--";

        //public Int64 Damage => Attacks.Sum(x => x.Damage);
        //public Int64 ZvsDamage => Attacks.Where(a => a.ID == "2106601422").Sum(x => x.Damage);
        //public Int64 HTFDamage => Attacks.Where(a => 0 <= Sepid.HTFAtkID.BinarySearch(a.ID)).Sum(x => x.Damage);
        public long ReadDamage
        {
            get
            {
                if (IsZanverse || IsFinish || IsDB || IsLsw || IsPwp || IsAIS || IsRide) { return Damage; }

                long temp = Damage;
                if (MainWindow.SepHTF) { temp -= HTFDamage; }
                if (MainWindow.SepZvs) { temp -= ZvsDamage; }
                if (MainWindow.SepDB) { temp -= DBDamage; }
                if (MainWindow.SepLsw) { temp -= LswDamage; }
                if (MainWindow.SepPwp) { temp -= PwpDamage; }
                if (MainWindow.SepAIS) { temp -= AisDamage; }
                if (MainWindow.SepRide) { temp -= RideDamage; }
                return temp;
            }
        }

        public string BindDamage
        {
            get
            {
                if (Properties.Settings.Default.DamageSI)
                {
                    return FormatDmg(ReadDamage);
                }
                else { return ReadDamage.ToString("N0"); }
            }
        }

        public string BindDamaged
        {
            get
            {
                if (Type == PType.DB) { return "--"; }
                if (Properties.Settings.Default.DamagedSI)
                {
                    return FormatDmg(Damaged);
                }
                else { return Damaged.ToString("N0"); }
            }
        }

        public double DPS => Damage / MainWindow.current.ActiveTime;
        public double ReadDPS => Math.Round(ReadDamage / (double)MainWindow.current.ActiveTime);
        public string StringDPS => ReadDPS.ToString("N0");
        public string FDPSReadout
        {
            get
            {
                if (Properties.Settings.Default.DPSSI)
                {
                    return FormatNumber(ReadDPS);
                }
                else
                {
                    return StringDPS;
                }
            }
        }

        public string JAPercent
        {
            get
            {
                try
                {
                    ulong tempJA = JACount, tempCount = AttackCount;
                    if (MainWindow.SepZvs && 0 < ZvsCount) { tempCount -= ZvsCount; }
                    if (MainWindow.SepHTF && 0 < HTFCount) { tempJA -= HTFJACount; tempCount -= HTFCount; }
                    if (Properties.Settings.Default.Nodecimal) { return ((double)tempJA / tempCount * 100).ToString("N0"); }
                    else { return ((double)tempJA / tempCount * 100).ToString("N2"); }
                }
                catch { return "Error"; }
            }
        }

        public string WJAPercent
        {
            get
            {
                try { return ((double)JACount / AttackCount * 100).ToString("N2"); } catch { return "Error"; }
            }
        }

        public string CRIPercent
        {
            get
            {
                try
                {
                    ulong tempCtl = CriCount, tempCount = AttackCount;
                    if (MainWindow.SepZvs && 0 < ZvsCount) { tempCtl -= ZvsCriCount; tempCount -= ZvsCount; }
                    if (MainWindow.SepHTF && 0 < HTFCount) { tempCtl -= HTFCriCount; tempCount -= HTFCount; }
                    if (Properties.Settings.Default.Nodecimal) { return ((double)tempCtl / tempCount * 100).ToString("N0"); }
                    else { return ((double)tempCtl / tempCount * 100).ToString("N2"); }
                }
                catch { return "Error"; }
            }
        }

        public string WCRIPercent
        {
            get
            {
                try { return ((double)CriCount / AttackCount * 100).ToString("N2"); } catch { return "Error"; }
            }
        }

        public bool IsAlly => (int.Parse(ID) >= 10000000) && !IsZanverse && !IsFinish;
        public bool IsZanverse => (Type == PType.Zvs);
        public bool IsFinish => (Type == PType.HTF);
        public bool IsDB => (Type == PType.DB);
        public bool IsLsw => (Type == PType.Lsw);
        public bool IsPwp => (Type == PType.Pwp);
        public bool IsAIS => (Type == PType.AIS);
        public bool IsRide => (Type == PType.Ride);

        public string MaxHitdmg
        {
            get
            {
                try
                {
                    if (Properties.Settings.Default.MaxSI)
                    {
                        return FormatDmg(Maxdmg);
                    }
                    else { return Maxdmg.ToString("N0"); }
                }
                catch { return "Error"; }
            }
        }

        public string MaxHit
        {
            get
            {
                string attack = "Unknown";
                if (MainWindow.skillDict.ContainsKey(MaxHitID))
                {
                    attack = MainWindow.skillDict[MaxHitID];
                }
                return attack;
            }
        }

        #region WriteProperties
        public string Writedmgd => Damaged.ToString("N0");
        public string WriteMaxdmg
        {
            get
            {
                try { return Maxdmg.ToString("N0"); } catch { return "Error"; }
            }
        }


        #endregion

        //Separate
        #region 

        //Ally Data
        public string AllyReadPct => AllyPct.ToString("N2");
        public string AllyReadDamage => AllyDamage.ToString("N0");
        public string AllyDPS => Math.Round(AllyDamage / (double)MainWindow.current.ActiveTime).ToString("N0");
        public string AllyJAPct => ((double)AllyJACount / AllyCount * 100).ToString("N2");
        public string AllyCriPct => ((double)AllyCriCount / AllyCount * 100).ToString("N2");
        public string AllyMaxHitdmg => AllyMaxdmg.ToString("N0");
        public string AllyAtkName
        {
            get
            {
                //if (AllyMaxHit == null) { return "--"; }
                string attack = "Unknown";
                if (MainWindow.skillDict.ContainsKey(AllyMaxID)) { attack = MainWindow.skillDict[AllyMaxID]; }
                return attack;
            }
        }

        //DarkBlast Data
        public string DBReadPct => DBPct.ToString("N2");
        public string DBReadDamage => DBDamage.ToString("N0");
        public string DBDPS => Math.Round(DBDamage / (double)MainWindow.current.ActiveTime).ToString("N0");
        public string DBJAPct => ((double)DBJACount / AttackCount * 100).ToString("N2");
        public string DBCriPct => ((double)DBCriCount / AttackCount * 100).ToString("N2");
        public string DBMaxHitdmg => DBMaxdmg.ToString("N0");
        public string DBAtkName
        {
            get
            {
                //if (DBMaxHit == null) { return "--"; }
                string attack = "Unknown";
                if (MainWindow.skillDict.ContainsKey(DBMaxID)) { attack = MainWindow.skillDict[DBMaxID]; }
                return attack;
            }
        }

        //Laconium sword Data
        public string LswReadPct => LswPct.ToString("N2");
        public string LswReadDamage => LswDamage.ToString("N0");
        public string LswDPS => Math.Round(LswDamage / (double)MainWindow.current.ActiveTime).ToString("N0");
        public string LswJAPct => ((double)LswJACount / AttackCount * 100).ToString("N2");
        public string LswCriPct => ((double)LswCriCount / AttackCount * 100).ToString("N2");
        public string LswMaxHitdmg => LswMaxdmg.ToString("N0");
        public string LswAtkName
        {
            get
            {
                //if (LswMaxHit == null) { return "--"; }
                string attack = "Unknown";
                if (MainWindow.skillDict.ContainsKey(LswMaxID)) { attack = MainWindow.skillDict[LswMaxID]; }
                return attack;
            }
        }

        //PhotonWeapon Data
        public string PwpReadPct => PwpPct.ToString("N2");
        public string PwpReadDamage => PwpDamage.ToString("N0");
        public string PwpDPS => Math.Round(PwpDamage / (double)MainWindow.current.ActiveTime).ToString("N0");
        public string PwpJAPct => ((double)PwpJACount / AttackCount * 100).ToString("N2");
        public string PwpCriPct => ((double)PwpCriCount / AttackCount * 100).ToString("N2");
        public string PwpMaxHitdmg => PwpMaxdmg.ToString("N0");
        public string PwpAtkName
        {
            get
            {
                //if (PwpMaxHit == null) { return "--"; }
                string attack = "Unknown";
                if (MainWindow.skillDict.ContainsKey(PwpMaxID)) { attack = MainWindow.skillDict[PwpMaxID]; }
                return attack;
            }
        }

        //AIS Data
        public string AisReadPct => AisPct.ToString("N2");
        public string AisReadDamage => AisDamage.ToString("N0");
        public string AisDPS => Math.Round(AisDamage / (double)MainWindow.current.ActiveTime).ToString("N0");
        public string AisJAPct => ((double)AisJACount / AttackCount * 100).ToString("N2");
        public string AisCriPct => ((double)AisCriCount / AttackCount * 100).ToString("N2");
        public string AisMaxHitdmg => AisMaxdmg.ToString("N0");
        public string AisAtkName
        {
            get
            {
                //if (AisMaxHit == null) { return "--"; }
                string attack = "Unknown";
                if (MainWindow.skillDict.ContainsKey(AisMaxID)) { attack = MainWindow.skillDict[AisMaxID]; }
                return attack;
            }
        }

        //Ridroid Data
        public string RideReadPct => RidePct.ToString("N2");
        public string RideReadDamage => RideDamage.ToString("N0");
        public string RideDPS => Math.Round(RideDamage / (double)MainWindow.current.ActiveTime).ToString("N0");
        public string RideJAPct => ((double)RideJACount / AttackCount * 100).ToString("N2");
        public string RideCriPct => ((double)RideCriCount / AttackCount * 100).ToString("N2");
        public string RideMaxHitdmg => RideMaxdmg.ToString("N0");
        public string RideAtkName
        {
            get
            {
                //if (RideMaxHit == null) { return "--"; }
                string attack = "Unknown";
                if (MainWindow.skillDict.ContainsKey(RideMaxID)) { attack = MainWindow.skillDict[RideMaxID]; }
                return attack;
            }
        }

        #endregion

        private string FormatDmg(long value)
        {
            if (value >= 1000000000) { return (value / 1000000000D).ToString("0.00") + "G"; } else if (value >= 100000000) { return (value / 1000000D).ToString("0.0") + "M"; } else if (value >= 1000000) { return (value / 1000000D).ToString("0.00") + "M"; } else if (value >= 100000) { return (value / 1000).ToString("#,0") + "K"; } else if (value >= 10000) { return (value / 1000D).ToString("0.0") + "K"; }
            return value.ToString("#,0");
        }

        private string FormatNumber(double value)
        {
            if (value >= 100000000) { return (value / 1000000).ToString("#,0") + "M"; }
            if (value >= 1000000) { return (value / 1000000D).ToString("0.0") + "M"; }
            if (value >= 100000) { return (value / 1000).ToString("#,0") + "K"; }
            if (value >= 1000) { return (value / 1000D).ToString("0.0") + "K"; }
            return value.ToString("#,0");
        }

        public Brush Brush
        {
            get
            {
                if (!IsZanverse && !IsFinish)
                {
                    if (MainWindow.workingList.IndexOf(this) % 2 == 0)
                    {
                        if (MainWindow.IsShowGraph)
                        {
                            return GenerateBarBrush(MainWindow.OddLeft, MainWindow.OddRgt); //奇数行OnGraph
                        }
                        else { return GenerateBarBrush(MainWindow.OddRgt, MainWindow.OddRgt); } //奇数行OffGraph
                    }
                    else
                    {
                        if (MainWindow.IsShowGraph)
                        {
                            return GenerateBarBrush(MainWindow.EveLeft, MainWindow.EveRgt); //偶数行OnGraph
                        }
                        else { return GenerateBarBrush(MainWindow.EveRgt, MainWindow.EveRgt); } //偶数行OffGraph
                    }
                }
                else
                {
                    return new SolidColorBrush(MainWindow.Other); // ザンバとかのやつ
                }
            }
        }

        LinearGradientBrush GenerateBarBrush(Color c, Color c2)
        {
            if (MainWindow.currentPlayerID.Any(x => x == ID) && MainWindow.IsHighlight) { c = MainWindow.MyColor; } //前面自分
            //if (Properties.Settings.Default.UserHighlight && MainWindow.playerid.Contains(ID)) { c = Color.FromArgb(128, 192, 128, 255); }  //前面ID

            LinearGradientBrush lgb = new LinearGradientBrush { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(1, 0) };
            lgb.GradientStops.Add(new GradientStop(c, 0));
            lgb.GradientStops.Add(new GradientStop(c, ReadDamage / MainWindow.current.firstDamage));
            lgb.GradientStops.Add(new GradientStop(c2, ReadDamage / MainWindow.current.firstDamage));
            lgb.GradientStops.Add(new GradientStop(c2, 1));
            lgb.SpreadMethod = GradientSpreadMethod.Repeat;
            return lgb;
        }

        public Player(string id, string name, PType type)
        {
            ID = id;
            Name = name;
            Type = type;
            PercentDPS = -1;
            Attacks = new List<Hit>();
            PercentReadDPS = 0;
            Damaged = 0;
        }
    }

    public class Session
    {
        public int startTimestamp, nowTimestamp, diffTime, ActiveTime, HTFMaxdmg, ZvsMaxdmg;
        public uint HTFMaxID;
        public ulong totalHTFJACount, totalHTFCriCount, totalZvsCriCount, totalHTFCount, totalZvsCount;
        public List<int> instances = new List<int>();
        public long totalDamage, totalAllyDamage, totalDBDamage, totalLswDamage, totalPwpDamage, totalAisDamage, totalRideDamage, totalZanverse, totalFinish;
        public double totalSD, firstDamage, totalDPS;
        public List<Player> players = new List<Player>();
    }


}
