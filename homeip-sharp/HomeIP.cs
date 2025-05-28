
using System;
using Cairo;
using GLib;
using Gdk;
using Gtk;
using Glade;
using Pango;
using LibHildonDesktop;
using System.Reflection;

public class HomeIP : HomeItem
{
    private Label label = null;
    private uint timeoutId = 0;
    private bool supportsAlpha = false;
    private bool inBackground = false;
    private XML settingsWidgetTree = null;
    private Gtk.Window settingsWindow = null;

    public HomeIP() : base()
    {
        string settingsPath = "/home/" + Environment.UserName + "/.homeip-sharp";
        AppSettings.Load(settingsPath);

        SetResizeType();

        label = new Label();
        label.UseMarkup = true;
        label.ModifyFont(FontDescription.FromString(AppSettings.Font));
        Gdk.Color bgCol = new Gdk.Color();
        Gdk.Color fgCol = new Gdk.Color();
        Gdk.Color.Parse(AppSettings.BackgroundColor, ref bgCol);
        Gdk.Color.Parse(AppSettings.ForegroundColor, ref fgCol);
        label.ModifyBg(StateType.Normal, bgCol);
        label.ModifyFg(StateType.Normal, fgCol);
        label.Text = GetText();
        label.SetPadding(15, 10);
        label.SetSizeRequest(-1, -1);
        this.SetSizeRequest(-1, -1);

        ExposeEvent += new ExposeEventHandler(OnExposed);
        ScreenChanged += OnScreenChanged;
        this.Add(label);
        this.ShowAll();

        timeoutId = GLib.Timeout.Add(6000, new GLib.TimeoutHandler(Update));

        settingsMenuItem = new MenuItem("HomeIP");
        settingsMenuItem.Activated += ShowSettings;

        Background += OnBackground;
        Foreground += OnForeground;
        Unrealized += OnUnrealized;
    }

    private void SetResizeType()
    {
        if (AppSettings.Autosize)
            this.ResizeType = HildonDesktopHomeItemResizeType.Both;
        else
            this.ResizeType = HildonDesktopHomeItemResizeType.None;
    }

    private void OnUnrealized(object o, EventArgs e)
    {
        GLib.Source.Remove(timeoutId);
    }

    private void OnBackground(object o, EventArgs e)
    {
        label.Text = "refreshing";
        label.SetSizeRequest(-1, -1);
        inBackground = true;

        if (timeoutId != 0)
        {
            GLib.Source.Remove(timeoutId);
            timeoutId = 0;
        }
    }

    private void OnForeground(object o, EventArgs e)
    {
        SetResizeType();

        label.ModifyFont(FontDescription.FromString(AppSettings.Font));

        Gdk.Color bgCol = new Gdk.Color();
        Gdk.Color fgCol = new Gdk.Color();
        Gdk.Color.Parse(AppSettings.BackgroundColor, ref bgCol);
        Gdk.Color.Parse(AppSettings.ForegroundColor, ref fgCol);

        label.ModifyBg(StateType.Normal, bgCol);
        label.ModifyFg(StateType.Normal, fgCol);

        label.Text = GetText();
        label.SetSizeRequest(-1, -1);

        inBackground = false;

        if (timeoutId == 0)
            timeoutId = GLib.Timeout.Add(6000, new GLib.TimeoutHandler(Update));
    }

    [GLib.ConnectBeforeAttribute]
    private void OnExposed(object o, ExposeEventArgs args)
    {
        Gtk.Widget widget = (Gtk.Widget)o;
        using (Cairo.Context cr = Gdk.CairoHelper.Create(widget.GdkWindow))
        {
            if (supportsAlpha)
                cr.SetSourceRGBA(1.0, 1.0, 1.0, 0.0); // Transparent
            else
                cr.SetSourceRGB(1.0, 1.0, 1.0); // Opaque white

            cr.Operator = Cairo.Operator.Source;
            cr.Paint();

            int width = this.Allocation.Width;
            int height = this.Allocation.Height;

            double x0 = 0, y0 = 0;
            double x1 = x0 + width;
            double y1 = y0 + height;

            double radius = Math.Min(15, Math.Min(width / 2.0, height / 2.0));

            // Rounded rectangle path
            cr.MoveTo(x0, y0 + radius);
            cr.Arc(x0 + radius, y0 + radius, radius, Math.PI, 1.5 * Math.PI);
            cr.LineTo(x1 - radius, y0);
            cr.Arc(x1 - radius, y0 + radius, radius, 1.5 * Math.PI, 0);
            cr.LineTo(x1, y1 - radius);
            cr.Arc(x1 - radius, y1 - radius, radius, 0, 0.5 * Math.PI);
            cr.LineTo(x0 + radius, y1);
            cr.Arc(x0 + radius, y1 - radius, radius, 0.5 * Math.PI, Math.PI);
            cr.ClosePath();

            Gdk.Color bgColor = new Gdk.Color();
            Gdk.Color.Parse(AppSettings.BackgroundColor, ref bgColor);
            double alpha = (Double)AppSettings.BackgroundAlpha / 65535.0;

            cr.SetSourceRGBA(bgColor.Red / 65535.0, bgColor.Green / 65535.0, bgColor.Blue / 65535.0, alpha);
            cr.FillPreserve();
        }
    }

    private void OnScreenChanged(object o, ScreenChangedArgs args)
    {
        Gtk.Widget widget = (Gtk.Widget)o;
        Gdk.Screen screen = widget.Screen;
        Gdk.Colormap colorMap = screen.RgbaColormap;

        if (colorMap == null)
        {
            colorMap = screen.RgbColormap;
            supportsAlpha = false;
        }
        else
        {
            supportsAlpha = true;
        }

        widget.Colormap = colorMap;

        args.RetVal = false;
    }

    private bool Update()
    {
        label.Text = GetText();
        label.SetSizeRequest(-1, -1);
        this.SetSizeRequest(-1, -1);

        return !inBackground;
    }

    private string GetText()
    {
        string text = "No IP";
        string devs = GetCommandOutput("/sbin/ifconfig wlan0");

        string[] sdevs = devs.Split('\n');
        foreach (string stdev in sdevs)
        {
            if (stdev.Contains("inet addr:"))
            {
                string[] parts = stdev.Split(':');
                if (parts.Length > 1)
                {
                    string[] sdev2 = parts[1].Trim().Split(' ');
                    if (sdev2.Length > 0)
                    {
                        text = "IP: " + sdev2[0];
                        break;
                    }
                }
            }
        }

        if (text == "No IP")
        {
            devs = GetCommandOutput("/sbin/ifconfig ppp0");
            sdevs = devs.Split('\n');
            foreach (string stdev in sdevs)
            {
                if (stdev.Contains("inet addr:"))
                {
                    string[] parts = stdev.Split(':');
                    if (parts.Length > 1)
                    {
                        string[] sdev2 = parts[1].Trim().Split(' ');
                        if (sdev2.Length > 0)
                        {
                            text = "IP: " + sdev2[0];
                            break;
                        }
                    }
                }
            }
        }

        if (text == "No IP")
        {
            string interfaceName = GetCommandOutput("sh -c \"ifconfig -a | grep -Eo '^[a-zA-Z0-9]+' | grep -E '^(eth|en)' | head -n1\"").Trim();
            if (interfaceName != "")
            {
                devs = GetCommandOutput("/sbin/ifconfig " + interfaceName);
                sdevs = devs.Split('\n');
                foreach (string stdev in sdevs)
                {
                    if (stdev.Contains("inet addr:"))
                    {
                        string[] parts = stdev.Split(':');
                        if (parts.Length > 1)
                        {
                            string[] sdev2 = parts[1].Trim().Split(' ');        
                            if (sdev2.Length > 0)
                            {
                                text = "IP: " + sdev2[0];
                                break;
                            }
                        }
                    }
                }
            }
        }

        return text;
    }

    private string GetCommandOutput(string command)
    {
        try
        {
            int exitStatus;
            string standardOutput, standardError;

            GLib.Process.SpawnCommandLineSync(
                command,
                out standardOutput,
                out standardError,
                out exitStatus
            );

            if (exitStatus == 0)
                return standardOutput;
        }
        catch (Exception)
        {
            // Ignore
        }
        return string.Empty;
    }

    private void ShowSettings(object o, EventArgs e)
    {
        if (settingsWindow != null)
            return;

        settingsWidgetTree = new XML("/usr/lib/homeip-sharp/prefs.glade", "prefs", null);
        settingsWindow = (Gtk.Window)settingsWidgetTree.GetWidget("prefs");

        Gdk.Color backgroundCol = new Gdk.Color();
        Gdk.Color foregroundCol = new Gdk.Color();

        Gdk.Color.Parse(AppSettings.BackgroundColor, ref backgroundCol);
        Gdk.Color.Parse(AppSettings.ForegroundColor, ref foregroundCol);

        ColorSelection backgroundColBtn = (ColorSelection)settingsWidgetTree.GetWidget("color_background");
        backgroundColBtn.CurrentColor = backgroundCol;
        backgroundColBtn.HasOpacityControl = true;
        backgroundColBtn.CurrentAlpha = AppSettings.BackgroundAlpha;

        ColorSelection foregroundColBtn = (ColorSelection)settingsWidgetTree.GetWidget("color_foreground");
        foregroundColBtn.CurrentColor = foregroundCol;
        foregroundColBtn.HasOpacityControl = true;
        foregroundColBtn.CurrentAlpha = AppSettings.ForegroundAlpha;

        FontSelection fontSelection = (FontSelection)settingsWidgetTree.GetWidget("fontselection");
        fontSelection.FontName = AppSettings.Font;

        CheckButton autosizeBtn = (CheckButton)settingsWidgetTree.GetWidget("bautosize");
        autosizeBtn.Active = AppSettings.Autosize;

        Gtk.Button cancelBtn = (Gtk.Button)settingsWidgetTree.GetWidget("bCancel1");
        cancelBtn.Clicked += OnCancelClicked;

        Gtk.Button validateBtn = (Gtk.Button)settingsWidgetTree.GetWidget("bOK1");
        validateBtn.Clicked += OnValidateClicked;

        settingsWindow.DeleteEvent += OnSettingsWindowDelete;

        settingsWindow.ShowAll();
    }

    private void OnCancelClicked(object sender, EventArgs args)
    {
        DestroySettingsWindow();
    }

    string ColorToString (Gdk.Color color)
    {
        return String.Format ("#{0:X2}{1:X2}{2:X2}", color.Red >> 8, color.Green >> 8, color.Blue >> 8);
    }

    private void OnValidateClicked(object sender, EventArgs args)
    {
        AppSettings.BackgroundColor = ColorToString(((ColorSelection)settingsWidgetTree.GetWidget("color_background")).CurrentColor);
        AppSettings.ForegroundColor = ColorToString(((ColorSelection)settingsWidgetTree.GetWidget("color_foreground")).CurrentColor);
        AppSettings.BackgroundAlpha = ((ColorSelection)settingsWidgetTree.GetWidget("color_background")).CurrentAlpha;
        AppSettings.ForegroundAlpha = ((ColorSelection)settingsWidgetTree.GetWidget("color_foreground")).CurrentAlpha;
        AppSettings.Font = ((FontSelection)settingsWidgetTree.GetWidget("fontselection")).FontName;
        AppSettings.Autosize = ((CheckButton)settingsWidgetTree.GetWidget("bautosize")).Active;

        AppSettings.Save();
        DestroySettingsWindow();
    }

    private void DestroySettingsWindow()
    {
        settingsWindow.Destroy();
        settingsWindow = null;
        settingsWidgetTree = null;
    }

    private void OnSettingsWindowDelete(object sender, DeleteEventArgs args)
    {
        settingsWindow = null;
        settingsWidgetTree = null;
    }
}

public static class HdPluginFactory
{
    public static HomeItem[] GetObjects()
    {
        try
        {
            HomeIP plugin = new HomeIP();
            return new HomeItem[] { plugin };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return new HomeItem[] { };
    }
}