using System.Diagnostics;
using System.Text.Json;
using Microsoft.Win32;

namespace VXE;

public sealed record Tweak(string category, string name, string description, string fps, bool safe);

public sealed class MainForm : Form
{
    static readonly Color Bg = Color.FromArgb(12,12,16), PanelBg = Color.FromArgb(22,22,29), Red = Color.FromArgb(255,45,85), TextDim = Color.FromArgb(180,180,195);
    readonly List<Tweak> all;
    readonly FlowLayoutPanel cards = new() { Dock=DockStyle.Fill, AutoScroll=true, WrapContents=false, FlowDirection=FlowDirection.TopDown, Padding=new Padding(14), BackColor=Bg };
    readonly TextBox search = new() { Width=240, PlaceholderText="Search tweaks..." };
    readonly ComboBox category = new() { Width=205, DropDownStyle=ComboBoxStyle.DropDownList };
    readonly Label status = new() { AutoSize=true, ForeColor=Red, Text="READY  /  Changes are applied only when you toggle them" };
    readonly Dictionary<string, Func<bool>> readers = new();
    readonly Dictionary<string, Action<bool>> writers = new();
    string CurrentCategory = "Command Center";
    public MainForm()
    {
        Text="VXE // GAMING OPTIMIZER"; Width=1240; Height=820; MinimumSize=new Size(980,650);
        BackColor=Bg; ForeColor=Color.White; Font=new Font("Segoe UI",10);
        all=JsonSerializer.Deserialize<List<Tweak>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"tweaks.json"))) ?? new();
        ConfigureActions();
        var top=new Panel{Dock=DockStyle.Top,Height=112,Padding=new Padding(20),BackColor=PanelBg};
        top.Controls.Add(new Label{Text="VXE",Font=new Font("Segoe UI",28,FontStyle.Bold),ForeColor=Red,AutoSize=true,Location=new Point(18,4)});
        top.Controls.Add(new Label{Text="WINDOWS & FORTNITE OPTIMIZER  /  BLACK • RED EDITION",AutoSize=true,ForeColor=TextDim,Location=new Point(22,53)});
        search.Location=new Point(600,32); search.BackColor=Bg; search.ForeColor=Color.White;
        category.Location=new Point(860,32); category.BackColor=Bg; category.ForeColor=Color.White;
        category.Items.Add("All categories"); foreach(var c in all.Select(x=>x.category).Distinct()) category.Items.Add(c); category.SelectedIndex=0;
        top.Controls.Add(search); top.Controls.Add(category);
        var footer=new Panel{Dock=DockStyle.Bottom,Height=38,Padding=new Padding(14,8,10,4),BackColor=PanelBg}; footer.Controls.Add(status);
        var split=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=225,FixedPanel=FixedPanel.Panel1,BackColor=Bg};
        var nav=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(12),BackColor=PanelBg};
        nav.Controls.Add(new Label{Text="VXE CONTROL CENTER",AutoSize=true,ForeColor=Red,Font=new Font("Segoe UI",10,FontStyle.Bold),Margin=new Padding(0,4,0,20)});
        AddNav(nav,"◇  Command center","Command Center");
        AddNav(nav,"☷  Tweak library","Tweak Library");
        AddNav(nav,"✥  Game optimizer","Game Optimizer");
        AddNav(nav,"✧  Cleaner","Cleaner");
        AddNav(nav,"▤  Memory & apps","Memory & Apps");
        AddNav(nav,"⌁  Benchmark lab","Benchmark Lab");
        AddNav(nav,"↶  Restore & logs","Restore & Logs");
        split.Panel1.Controls.Add(nav); split.Panel2.Controls.Add(cards);
        Controls.Add(split); Controls.Add(footer); Controls.Add(top);
        search.TextChanged+=(s,e)=>Render(); category.SelectedIndexChanged+=(s,e)=>Render();
        Render();
    }
    void AddNav(FlowLayoutPanel nav,string label,string section)
    {
        var b=new Button{Text=label,Width=190,Height=43,FlatStyle=FlatStyle.Flat,TextAlign=ContentAlignment.MiddleLeft,BackColor=PanelBg,ForeColor=Color.White,Margin=new Padding(0,0,0,7)};
        b.FlatAppearance.BorderColor=Color.FromArgb(55,35,43);
        b.Click+=(s,e)=>{CurrentCategory=section; Render();}; nav.Controls.Add(b);
    }
    void ConfigureActions()
    {
        readers["Windows Game Mode"]=()=>ReadDword(@"Software\Microsoft\GameBar","AutoGameModeEnabled",1)==1;
        writers["Windows Game Mode"]=v=>WriteDword(@"Software\Microsoft\GameBar","AutoGameModeEnabled",v?1:0);
        readers["Game Bar overlay"]=()=>ReadDword(@"Software\Microsoft\GameBar","UseNexusForGameBarEnabled",1)==1;
        writers["Game Bar overlay"]=v=>WriteDword(@"Software\Microsoft\GameBar","UseNexusForGameBarEnabled",v?1:0);
        readers["Background game recording"]=()=>ReadDword(@"Software\Microsoft\Windows\CurrentVersion\GameDVR","AppCaptureEnabled",1)==1;
        writers["Background game recording"]=v=>WriteDword(@"Software\Microsoft\Windows\CurrentVersion\GameDVR","AppCaptureEnabled",v?1:0);
        readers["Power saver plan"]=()=>RunPowercfg("/getactivescheme").ToLowerInvariant().Contains("a1841308-3541-4fab-bc81-f71556f20b4a");
        writers["Power saver plan"]=v=>{if(v) RunPowercfg("/setactive a1841308-3541-4fab-bc81-f71556f20b4a"); else RunPowercfg("/setactive SCHEME_BALANCED");};
    }
    int ReadDword(string path,string name,int fallback)
    {
        try { using var k=Registry.CurrentUser.OpenSubKey(path); return Convert.ToInt32(k?.GetValue(name,fallback)??fallback); } catch { return fallback; }
    }
    void WriteDword(string path,string name,int value)
    {
        using var k=Registry.CurrentUser.CreateSubKey(path,true); k.SetValue(name,value,RegistryValueKind.DWord);
    }
    string RunPowercfg(string args)
    {
        using var p=Process.Start(new ProcessStartInfo("powercfg.exe",args){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true})!;
        string output=p.StandardOutput.ReadToEnd(); p.WaitForExit(4000); return output;
    }
    void Render()
    {
        cards.SuspendLayout(); cards.Controls.Clear();
        var q=search.Text.Trim();
        IEnumerable<Tweak> list=all;
        if(category.SelectedIndex>0) list=list.Where(t=>t.category==category.SelectedItem?.ToString());
        if(!string.IsNullOrWhiteSpace(q)) list=list.Where(t=>t.name.Contains(q,StringComparison.OrdinalIgnoreCase)||t.category.Contains(q,StringComparison.OrdinalIgnoreCase)||t.description.Contains(q,StringComparison.OrdinalIgnoreCase));
        var active=list.ToList();
        if(CurrentCategory=="Game Optimizer") AddSectionTitle("GAME OPTIMIZER — LIVE SWITCHES");
        else if(CurrentCategory=="Cleaner") AddActionCard("Windows Storage cleanup","Open Windows Storage settings to review temporary files before deleting them.",()=>Process.Start(new ProcessStartInfo("ms-settings:storagesense"){UseShellExecute=true}));
        else if(CurrentCategory=="Memory & Apps") { AddActionCard("Startup apps","Open Startup Apps settings to disable unwanted startup applications.",()=>Process.Start(new ProcessStartInfo("ms-settings:startupapps"){UseShellExecute=true})); AddActionCard("Background apps","Review Windows background app permissions and close apps you do not need.",()=>Process.Start(new ProcessStartInfo("ms-settings:privacy-backgroundapps"){UseShellExecute=true})); }
        else if(CurrentCategory=="Benchmark Lab") AddActionCard("Benchmark protocol","Use the same Fortnite scene and settings for three runs before and after a change. Compare average FPS, 1% lows and frametime.",()=>MessageBox.Show("Use a fixed Fortnite Creative route or replay. Record average FPS, 1% low and frametime for three runs before and after one change. VXE does not yet read Fortnite live telemetry.","Benchmark Lab"));
        else if(CurrentCategory=="Restore & Logs") { AddActionCard("Windows display settings","Open advanced display settings.",()=>Process.Start(new ProcessStartInfo("ms-settings:display-advanced"){UseShellExecute=true})); AddActionCard("Power plan","Power Saver toggle returns to Balanced when switched off. Other changes are described as guides and are not applied automatically.",()=>MessageBox.Show("VXE automatically applies only the listed supported switches. Other cards open guidance. Create a Windows restore point before advanced manual changes.","Restore & Logs")); }
        else if(CurrentCategory=="Command Center") { AddSectionTitle("QUICK CONTROLS"); AddToggleCard("Windows Game Mode","Enable or disable the current-user Game Mode preference.","Windows Game Mode"); AddToggleCard("Game Bar overlay","Toggle the Game Bar integration preference for the current user.","Game Bar overlay"); AddToggleCard("Background game recording","Toggle the Windows Game DVR capture preference.","Background game recording"); AddToggleCard("Power saver plan","ON selects Power Saver. OFF returns to Balanced.","Power saver plan"); }
        else if(CurrentCategory=="Game Optimizer") { AddToggleCard("Windows Game Mode","Enable or disable the current-user Game Mode preference.","Windows Game Mode"); AddToggleCard("Game Bar overlay","Toggle the Game Bar integration preference for the current user.","Game Bar overlay"); AddToggleCard("Background game recording","Toggle the Windows Game DVR capture preference.","Background game recording"); AddToggleCard("Power saver plan","ON selects Power Saver. OFF returns to Balanced.","Power saver plan"); }
        else if(CurrentCategory=="Tweak Library") { foreach(var t in active) AddTweakCard(t); }
        else { foreach(var t in active) AddTweakCard(t); }
        status.Text=$"READY  /  {all.Count} TWEAK GUIDES  /  4 ACTIONABLE SWITCHES";
        cards.ResumeLayout();
    }
    void AddSectionTitle(string text)=>cards.Controls.Add(new Label{Text=text,AutoSize=true,ForeColor=Red,Font=new Font("Segoe UI",13,FontStyle.Bold),Margin=new Padding(4,4,4,16)});
    void AddActionCard(string title,string description,Action action)
    {
        var p=CardPanel(120); p.Controls.Add(new Label{Text=title,AutoSize=true,ForeColor=Red,Font=new Font("Segoe UI",11,FontStyle.Bold),Location=new Point(16,12)});
        p.Controls.Add(new Label{Text=description,AutoSize=false,ForeColor=TextDim,Location=new Point(16,42),Size=new Size(600,45)});
        var b=new Button{Text="OPEN",Location=new Point(680,38),Size=new Size(90,36),BackColor=Red,ForeColor=Color.White,FlatStyle=FlatStyle.Flat}; b.Click+=(s,e)=>action(); p.Controls.Add(b); cards.Controls.Add(p);
    }
    Panel CardPanel(int height)=>new(){Width=Math.Max(640,cards.ClientSize.Width-45),Height=height,BackColor=PanelBg,Margin=new Padding(0,0,0,12),Padding=new Padding(12)};
    void AddToggleCard(string title,string description,string key)
    {
        var p=CardPanel(112); p.Controls.Add(new Label{Text=title,AutoSize=true,ForeColor=Color.White,Font=new Font("Segoe UI",11,FontStyle.Bold),Location=new Point(16,12)});
        p.Controls.Add(new Label{Text=description,AutoSize=false,ForeColor=TextDim,Location=new Point(16,42),Size=new Size(570,42)});
        var sw=new CheckBox{Appearance=Appearance.Button,TextAlign=ContentAlignment.MiddleCenter,Text="OFF",Size=new Size(92,36),Location=new Point(p.Width-125,34),FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(48,48,57),ForeColor=Color.White};
        try { sw.Checked=readers[key](); } catch { sw.Checked=false; }
        void PaintSwitch(){sw.Text=sw.Checked?"ON":"OFF";sw.BackColor=sw.Checked?Red:Color.FromArgb(48,48,57);}
        PaintSwitch();
        sw.CheckedChanged+=(s,e)=>{
            try { writers[key](sw.Checked); PaintSwitch(); status.Text=$"APPLIED  /  {title}: {(sw.Checked?"ON":"OFF")}"; }
            catch(Exception ex){sw.CheckedChanged-=null; MessageBox.Show("Windows could not apply this setting. Try again or check your permissions.\n\n"+ex.Message,"VXE",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
        };
        p.Controls.Add(sw); cards.Controls.Add(p);
    }
    void AddTweakCard(Tweak t)
    {
        var p=CardPanel(136); p.Controls.Add(new Label{Text=t.name.ToUpperInvariant(),AutoSize=true,ForeColor=Red,Font=new Font("Segoe UI",10,FontStyle.Bold),Location=new Point(16,10)});
        p.Controls.Add(new Label{Text=t.category+"  •  Estimated FPS impact: "+t.fps,AutoSize=true,ForeColor=Color.FromArgb(255,180,100),Location=new Point(16,34)});
        p.Controls.Add(new Label{Text=t.description,AutoSize=false,ForeColor=TextDim,Location=new Point(16,58),Size=new Size(700,60)});
        var b=new Button{Text="VIEW GUIDE",Location=new Point(p.Width-135,45),Size=new Size(105,34),BackColor=Red,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
        b.Click+=(s,e)=>MessageBox.Show(t.description+"\n\nThis item is a guide and is not automatically applied. FPS impact is not guaranteed.","VXE // "+t.name);
        p.Controls.Add(b); cards.Controls.Add(p);
    }
}
public static class Program { [STAThread] public static void Main(){ApplicationConfiguration.Initialize();Application.Run(new MainForm());} }
