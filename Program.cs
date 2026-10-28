using System.Diagnostics;
using System.Text.Json;

namespace VXE;
public record Tweak(string category, string name, string description, string fps, bool safe);

public class MainForm : Form
{
    private readonly List<Tweak> all;
    private readonly FlowLayoutPanel cards = new() { Dock=DockStyle.Fill, AutoScroll=true, WrapContents=false, FlowDirection=FlowDirection.TopDown, Padding=new Padding(12), BackColor=Color.FromArgb(13,17,31) };
    private readonly TextBox search = new() { Width=260, PlaceholderText="Zoek tweak, categorie..." };
    private readonly ComboBox category = new() { Width=220, DropDownStyle=ComboBoxStyle.DropDownList };
    private readonly Label status = new() { AutoSize=true, ForeColor=Color.FromArgb(102,245,222), Text="SYSTEM READY  /  wijzigingen vereisen bevestiging" };
    private FlowLayoutPanel? sidePanel;
    private readonly Label fpsLabel = new() { AutoSize=true, ForeColor=Color.White, Text="FPS-impact: schatting, geen garantie" };
    public MainForm()
    {
        Text="VXE // GAMING OPTIMIZER"; Width=1180; Height=820; MinimumSize=new Size(900,650);
        BackColor=Color.FromArgb(8,10,20); ForeColor=Color.White; Font=new Font("Segoe UI",10);
        all=JsonSerializer.Deserialize<List<Tweak>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"tweaks.json"))) ?? new();
        var top = new Panel { Dock=DockStyle.Top, Height=142, Padding=new Padding(20), BackColor=Color.FromArgb(18,23,42) };
        var title = new Label { Text="VXE", Font=new Font("Segoe UI",30,FontStyle.Bold), ForeColor=Color.FromArgb(102,245,222), AutoSize=true, Location=new Point(18,8) };
        var subtitle = new Label { Text="FPS LAB  •  FORTNITE / WINDOWS  •  ANIME SYSTEM CONSOLE", AutoSize=true, ForeColor=Color.FromArgb(190,199,221), Location=new Point(22,62) };
        search.Location=new Point(22,94); search.BackColor=Color.FromArgb(30,37,60); search.ForeColor=Color.White;
        category.Location=new Point(300,94); category.BackColor=Color.FromArgb(30,37,60); category.ForeColor=Color.White;
        category.Items.Add("Alle categorieën"); foreach(var c in all.Select(x=>x.category).Distinct()) category.Items.Add(c); category.SelectedIndex=0;
        search.TextChanged+=(s,e)=>Render(); category.SelectedIndexChanged+=(s,e)=>Render();
        top.Controls.AddRange(new Control[]{title,subtitle,search,category});
        var bottom=new Panel { Dock=DockStyle.Bottom, Height=42, Padding=new Padding(14,8,14,4), BackColor=Color.FromArgb(18,23,42) };
        bottom.Controls.Add(status); fpsLabel.Location=new Point(560,8); bottom.Controls.Add(fpsLabel);
        var split = new SplitContainer { Dock=DockStyle.Fill, SplitterDistance=190, BackColor=Color.FromArgb(8,10,20), FixedPanel=FixedPanel.Panel1 };
        var side=new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.TopDown, WrapContents=false, Padding=new Padding(12), BackColor=Color.FromArgb(11,15,28) };
        side.Controls.Add(new Label { Text="MISSION CONTROL", AutoSize=true, ForeColor=Color.FromArgb(102,245,222), Font=new Font("Segoe UI",11,FontStyle.Bold), Margin=new Padding(0,0,0,18) });
        AddSide("⚡  Tweak library", ()=>{category.SelectedIndex=0;search.Clear();});
        AddSide("🎯  Fortnite profile", ()=>MessageBox.Show("VXE kan Fortnite-instellingen begeleiden, maar wijzigt geen beschermde gamebestanden of anti-cheatbestanden.","Fortnite profile"));
        AddSide("🧹  PC cleaner", Cleaner);
        AddSide("📊  Live system snapshot", Snapshot);
        AddSide("🧪  Benchmark", Benchmark);
        AddSide("↩  Safety & restore", ()=>MessageBox.Show("Herstel alleen wijzigingen die je zelf hebt gemaakt. Maak vóór geavanceerde wijzigingen een herstelpunt en exporteer registerinstellingen.","Safety"));
        sidePanel=side; split.Panel1.Controls.Add(side); split.Panel2.Controls.Add(cards);
        Controls.Add(split); Controls.Add(bottom); Controls.Add(top);
        Render();
    }
    private void AddSide(string text, Action action) { var b=new Button { Text=text, Width=155, Height=42, FlatStyle=FlatStyle.Flat, ForeColor=Color.White, BackColor=Color.FromArgb(25,33,55), Margin=new Padding(0,0,0,9), TextAlign=ContentAlignment.MiddleLeft }; b.FlatAppearance.BorderColor=Color.FromArgb(47,67,94); b.Click+=(s,e)=>action(); sidePanel?.Controls.Add(b); }
    private void Render()
    {
        cards.SuspendLayout(); cards.Controls.Clear();
        var list=all.Where(t=>(category.SelectedIndex<=0||t.category==category.SelectedItem?.ToString())&&(t.name.Contains(search.Text,StringComparison.OrdinalIgnoreCase)||t.category.Contains(search.Text,StringComparison.OrdinalIgnoreCase)||t.description.Contains(search.Text,StringComparison.OrdinalIgnoreCase))).ToList();
        foreach(var t in list)
        {
            var p=new Panel { Width=Math.Max(620,cards.ClientSize.Width-42), Height=150, BackColor=Color.FromArgb(21,28,47), Margin=new Padding(0,0,0,10), Padding=new Padding(14) };
            var title=new Label { Text=t.name.ToUpperInvariant(), AutoSize=true, Font=new Font("Segoe UI",11,FontStyle.Bold), ForeColor=Color.FromArgb(102,245,222), Location=new Point(14,10) };
            var tag=new Label { Text=t.category+"  •  FPS: "+t.fps, AutoSize=true, ForeColor=Color.FromArgb(255,190,94), Location=new Point(14,36) };
            var desc=new Label { Text=t.description, AutoSize=false, Width=p.Width-185, Height=68, ForeColor=Color.FromArgb(204,211,230), Location=new Point(14,61) };
            var btn=new Button { Text=t.safe?"BEKIJK / TOEPASSEN":"OPEN HANDLEIDING", Width=145, Height=36, Location=new Point(p.Width-166,55), BackColor=Color.FromArgb(102,245,222), ForeColor=Color.FromArgb(8,10,20), FlatStyle=FlatStyle.Flat };
            btn.Click+=(s,e)=>Apply(t);
            p.Controls.AddRange(new Control[]{title,tag,desc,btn}); cards.Controls.Add(p);
        }
        status.Text=$"SYSTEM READY  /  {list.Count} TWEAKS GEVONDEN  /  GEEN WIJZIGINGEN ZONDER TOESTEMMING";
        cards.ResumeLayout();
    }
    private void Apply(Tweak t)
    {
        if(t.name=="Temporary files cleanup")
        {
            var r=MessageBox.Show("Open Windows Instellingen > Systeem > Opslag om tijdelijke bestanden te bekijken. Controleer de selectie voordat je verwijdert.","VXE Cleaner",MessageBoxButtons.OKCancel,MessageBoxIcon.Information);
            if(r==DialogResult.OK) Process.Start(new ProcessStartInfo("ms-settings:storagesense") { UseShellExecute=true });
            return;
        }
        if(t.name=="Startup app audit") { Process.Start(new ProcessStartInfo("ms-settings:startupapps"){UseShellExecute=true}); return; }
        if(t.name=="Refresh rate verification") { Process.Start(new ProcessStartInfo("ms-settings:display-advanced"){UseShellExecute=true}); return; }
        MessageBox.Show($"{t.name}\n\n{t.description}\n\nFPS-indicatie: {t.fps}\n\nVXE past deze instelling niet stilzwijgend aan. Volg de handmatige Windows-, driver- of BIOS-handleiding en test daarna opnieuw.", "VXE // Tweak details", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void Cleaner()
    {
        if(MessageBox.Show("VXE opent de ingebouwde Windows-opslagpagina. Controleer bestanden vóór verwijdering; downloads en shader caches kunnen nuttig zijn.","PC cleaner",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning)==DialogResult.OK)
            Process.Start(new ProcessStartInfo("ms-settings:storagesense"){UseShellExecute=true});
    }
    private void Snapshot()
    {
        var mem=GC.GetGCMemoryInfo();
        var msg=$"OS: {Environment.OSVersion}\nLogical CPU threads: {Environment.ProcessorCount}\nVXE process memory: {Process.GetCurrentProcess().WorkingSet64/1024/1024} MB\nSystem memory available to GC: {mem.TotalAvailableMemoryBytes/1024/1024} MB\n\nVoor echte GPU-temperaturen, ping, 1% lows en frametimes is een game- of hardware-specifieke meetbron nodig.";
        MessageBox.Show(msg,"VXE // System snapshot");
    }
    private void Benchmark()
    {
        MessageBox.Show("Gebruik een vaste Fortnite Creative-route of ingebouwde replay en meet dezelfde resolutie, scène en instellingen. Noteer gemiddelde FPS, 1% low en frametime; voer drie runs uit vóór en na één wijziging. VXE bevat nog geen betrouwbare game-hook en claimt daarom geen live Fortnite FPS-meting.","VXE // Benchmark protocol");
    }
}
public static class Program
{
    [STAThread] public static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new MainForm()); }
}
