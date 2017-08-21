using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MaterialSkin.Properties;

namespace MaterialSkin
{
    public class MaterialSkinManager
    {
        //Singleton instance
        private static MaterialSkinManager _instance;

        //Forms to control
        private readonly List<MaterialForm> _formsToManage = new List<MaterialForm>();

        //Theme
        private Themes _theme;
        public Themes Theme
        {
            get { return _theme; }
            set
            {
                _theme = value;
                UpdateBackgrounds();
            }
        }

        private ColorScheme _colorScheme;
        public ColorScheme ColorScheme
        {
            get { return _colorScheme; }
            set
            {
                _colorScheme = value;
                UpdateBackgrounds();
            }
        }

        public enum Themes : byte
        {
            Light,
            Dark
        }

        //Constant color values
        private static readonly Color PrimaryTextBlack = Color.FromArgb(222, 0, 0, 0);
        private static readonly Brush PrimaryTextBlackBrush = new SolidBrush(PrimaryTextBlack);
        public static Color SecondaryTextBlack = Color.FromArgb(138, 0, 0, 0);
        public static Brush SecondaryTextBlackBrush = new SolidBrush(SecondaryTextBlack);
        private static readonly Color DisabledOrHintTextBlack = Color.FromArgb(66, 0, 0, 0);
        private static readonly Brush DisabledOrHintTextBlackBrush = new SolidBrush(DisabledOrHintTextBlack);
        private static readonly Color DividersBlack = Color.FromArgb(31, 0, 0, 0);
        private static readonly Brush DividersBlackBrush = new SolidBrush(DividersBlack);

        private static readonly Color PrimaryTextWhite = Color.FromArgb(255, 255, 255, 255);
        private static readonly Brush PrimaryTextWhiteBrush = new SolidBrush(PrimaryTextWhite);
        public static Color SecondaryTextWhite = Color.FromArgb(179, 255, 255, 255);
        public static Brush SecondaryTextWhiteBrush = new SolidBrush(SecondaryTextWhite);
        private static readonly Color DisabledOrHintTextWhite = Color.FromArgb(77, 255, 255, 255);
        private static readonly Brush DisabledOrHintTextWhiteBrush = new SolidBrush(DisabledOrHintTextWhite);
        private static readonly Color DividersWhite = Color.FromArgb(31, 255, 255, 255);
        private static readonly Brush DividersWhiteBrush = new SolidBrush(DividersWhite);

        // Checkbox colors
        private static readonly Color CheckboxOffLight = Color.FromArgb(138, 0, 0, 0);
        private static readonly Brush CheckboxOffLightBrush = new SolidBrush(CheckboxOffLight);
        private static readonly Color CheckboxOffDisabledLight = Color.FromArgb(66, 0, 0, 0);
        private static readonly Brush CheckboxOffDisabledLightBrush = new SolidBrush(CheckboxOffDisabledLight);

        private static readonly Color CheckboxOffDark = Color.FromArgb(179, 255, 255, 255);
        private static readonly Brush CheckboxOffDarkBrush = new SolidBrush(CheckboxOffDark);
        private static readonly Color CheckboxOffDisabledDark = Color.FromArgb(77, 255, 255, 255);
        private static readonly Brush CheckboxOffDisabledDarkBrush = new SolidBrush(CheckboxOffDisabledDark);

        //Raised button
        private static readonly Color RaisedButtonBackground = Color.FromArgb(255, 255, 255, 255);
        private static readonly Brush RaisedButtonBackgroundBrush = new SolidBrush(RaisedButtonBackground);
        private static readonly Color RaisedButtonTextLight = PrimaryTextWhite;
        private static readonly Brush RaisedButtonTextLightBrush = new SolidBrush(RaisedButtonTextLight);
        private static readonly Color RaisedButtonTextDark = PrimaryTextBlack;
        private static readonly Brush RaisedButtonTextDarkBrush = new SolidBrush(RaisedButtonTextDark);

        //Flat button
        private static readonly Color FlatButtonBackgroundHoverLight = Color.FromArgb(20.PercentageToColorComponent(), 0x999999.ToColor());
        private static readonly Brush FlatButtonBackgroundHoverLightBrush = new SolidBrush(FlatButtonBackgroundHoverLight);
        private static readonly Color FlatButtonBackgroundPressedLight = Color.FromArgb(40.PercentageToColorComponent(), 0x999999.ToColor());
        private static readonly Brush FlatButtonBackgroundPressedLightBrush = new SolidBrush(FlatButtonBackgroundPressedLight);
        private static readonly Color FlatButtonDisabledtextLight = Color.FromArgb(26.PercentageToColorComponent(), 0x000000.ToColor());
        private static readonly Brush FlatButtonDisabledtextLightBrush = new SolidBrush(FlatButtonDisabledtextLight);

        private static readonly Color FlatButtonBackgroundHoverDark = Color.FromArgb(15.PercentageToColorComponent(), 0xCCCCCC.ToColor());
        private static readonly Brush FlatButtonBackgroundHoverDarkBrush = new SolidBrush(FlatButtonBackgroundHoverDark);
        private static readonly Color FlatButtonBackgroundPressedDark = Color.FromArgb(25.PercentageToColorComponent(), 0xCCCCCC.ToColor());
        private static readonly Brush FlatButtonBackgroundPressedDarkBrush = new SolidBrush(FlatButtonBackgroundPressedDark);
        private static readonly Color FlatButtonDisabledtextDark = Color.FromArgb(30.PercentageToColorComponent(), 0xFFFFFF.ToColor());
        private static readonly Brush FlatButtonDisabledtextDarkBrush = new SolidBrush(FlatButtonDisabledtextDark);

        //ContextMenuStrip
        private static readonly Color CmsBackgroundLightHover = Color.FromArgb(255, 238, 238, 238);
        private static readonly Brush CmsBackgroundHoverLightBrush = new SolidBrush(CmsBackgroundLightHover);

        private static readonly Color CmsBackgroundDarkHover = Color.FromArgb(38, 204, 204, 204);
        private static readonly Brush CmsBackgroundHoverDarkBrush = new SolidBrush(CmsBackgroundDarkHover);

        //Application background
        private static readonly Color BackgroundLight = Color.FromArgb(255, 255, 255, 255);
        private static Brush _backgroundLightBrush = new SolidBrush(BackgroundLight);

        private static readonly Color BackgroundDark = Color.FromArgb(255, 51, 51, 51);
        private static Brush _backgroundDarkBrush = new SolidBrush(BackgroundDark);

        //Application action bar
        public readonly Color ActionBarText = Color.FromArgb(255, 255, 255, 255);
        public readonly Brush ActionBarTextBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        public readonly Color ActionBarTextSecondary = Color.FromArgb(153, 255, 255, 255);
        public readonly Brush ActionBarTextSecondaryBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));

        public Color GetPrimaryTextColor()
        {
            return Theme == Themes.Light ? PrimaryTextBlack : PrimaryTextWhite;
        }

        public Brush GetPrimaryTextBrush()
        {
            return Theme == Themes.Light ? PrimaryTextBlackBrush : PrimaryTextWhiteBrush;
        }

        public Color GetSecondaryTextColor()
        {
            return Theme == Themes.Light ? SecondaryTextBlack : SecondaryTextWhite;
        }

        public Brush GetSecondaryTextBrush()
        {
            return Theme == Themes.Light ? SecondaryTextBlackBrush : SecondaryTextWhiteBrush;
        }

        public Color GetDisabledOrHintColor()
        {
            return Theme == Themes.Light ? DisabledOrHintTextBlack : DisabledOrHintTextWhite;
        }

        public Brush GetDisabledOrHintBrush()
        {
            return Theme == Themes.Light ? DisabledOrHintTextBlackBrush : DisabledOrHintTextWhiteBrush;
        }

        public Color GetDividersColor()
        {
            return Theme == Themes.Light ? DividersBlack : DividersWhite;
        }

        public Brush GetDividersBrush()
        {
            return Theme == Themes.Light ? DividersBlackBrush : DividersWhiteBrush;
        }

        public Color GetCheckboxOffColor()
        {
            return Theme == Themes.Light ? CheckboxOffLight : CheckboxOffDark;
        }

        public Brush GetCheckboxOffBrush()
        {
            return Theme == Themes.Light ? CheckboxOffLightBrush : CheckboxOffDarkBrush;
        }

        public Color GetCheckBoxOffDisabledColor()
        {
            return Theme == Themes.Light ? CheckboxOffDisabledLight : CheckboxOffDisabledDark;
        }

        public Brush GetCheckBoxOffDisabledBrush()
        {
            return Theme == Themes.Light ? CheckboxOffDisabledLightBrush : CheckboxOffDisabledDarkBrush;
        }

        public Brush GetRaisedButtonBackgroundBrush()
        {
            return RaisedButtonBackgroundBrush;
        }

        public Brush GetRaisedButtonTextBrush(bool primary)
        {
            return primary ? RaisedButtonTextLightBrush : RaisedButtonTextDarkBrush;
        }

        public Color GetFlatButtonHoverBackgroundColor()
        {
            return Theme == Themes.Light ? FlatButtonBackgroundHoverLight : FlatButtonBackgroundHoverDark;
        }

        public Brush GetFlatButtonHoverBackgroundBrush()
        {
            return Theme == Themes.Light ? FlatButtonBackgroundHoverLightBrush : FlatButtonBackgroundHoverDarkBrush;
        }

        public Color GetFlatButtonPressedBackgroundColor()
        {
            return Theme == Themes.Light ? FlatButtonBackgroundPressedLight : FlatButtonBackgroundPressedDark;
        }

        public Brush GetFlatButtonPressedBackgroundBrush()
        {
            return Theme == Themes.Light ? FlatButtonBackgroundPressedLightBrush : FlatButtonBackgroundPressedDarkBrush;
        }

        public Brush GetFlatButtonDisabledTextBrush()
        {
            return Theme == Themes.Light ? FlatButtonDisabledtextLightBrush : FlatButtonDisabledtextDarkBrush;
        }

        public Brush GetCmsSelectedItemBrush()
        {
            return Theme == Themes.Light ? CmsBackgroundHoverLightBrush : CmsBackgroundHoverDarkBrush;
        }

        public Color GetApplicationBackgroundColor()
        {
            return Theme == Themes.Light ? BackgroundLight : BackgroundDark;
        }

        //Iransans font
        public Font NazanintarRegular10 { get; set; }
        public Font NazanintarRegular11 { get; set; }
        public Font NazanintarMedium11 { get; set; }
        public Font NazanintarMedium12 { get; set; }
        public Font NazanintarMedium13 { get; set; }

        //Other constants
        public int FormPadding = 14;

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pvd, [In] ref uint pcFonts);

        private MaterialSkinManager()
        {
            NazanintarRegular10 = new Font(LoadFont(Resources.Nazanintar), 10f);
            NazanintarRegular11 = new Font(LoadFont(Resources.Nazanintar), 11f);
            NazanintarMedium11 = new Font(LoadFont(Resources.NazanintarB), 11f);
            NazanintarMedium12 = new Font(LoadFont(Resources.NazanintarB), 12f);
            NazanintarMedium13 = new Font(LoadFont(Resources.NazanintarB), 13f);
            Theme = Themes.Light;
            ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.White);
        }

        public static MaterialSkinManager Instance => _instance ?? (_instance = new MaterialSkinManager());

        public void AddFormToManage(MaterialForm materialForm)
        {
            _formsToManage.Add(materialForm);
            UpdateBackgrounds();
        }

        public void RemoveFormToManage(MaterialForm materialForm)
        {
            _formsToManage.Remove(materialForm);
        }

        private readonly PrivateFontCollection _privateFontCollection = new PrivateFontCollection();

        private FontFamily LoadFont(byte[] fontResource)
        {
            int dataLength = fontResource.Length;
            IntPtr fontPtr = Marshal.AllocCoTaskMem(dataLength);
            Marshal.Copy(fontResource, 0, fontPtr, dataLength);

            uint cFonts = 0;
            AddFontMemResourceEx(fontPtr, (uint)fontResource.Length, IntPtr.Zero, ref cFonts);
            _privateFontCollection.AddMemoryFont(fontPtr, dataLength);

            return _privateFontCollection.Families.Last();
        }

        private void UpdateBackgrounds()
        {
            var newBackColor = GetApplicationBackgroundColor();
            foreach (var materialForm in _formsToManage)
            {
                materialForm.BackColor = newBackColor;
                UpdateControl(materialForm, newBackColor);
            }
        }

        private void UpdateToolStrip(ToolStrip toolStrip, Color newBackColor)
        {
            if (toolStrip == null) return;

            toolStrip.BackColor = newBackColor;
            foreach (ToolStripItem control in toolStrip.Items)
            {
                control.BackColor = newBackColor;
                if (control is MaterialToolStripMenuItem && (control as MaterialToolStripMenuItem).HasDropDown)
                {

                    //recursive call
                    UpdateToolStrip((control as MaterialToolStripMenuItem).DropDown, newBackColor);
                }
            }
        }

        private void UpdateControl(Control controlToUpdate, Color newBackColor)
        {
            if (controlToUpdate == null) return;

            if (controlToUpdate.ContextMenuStrip != null)
            {
                UpdateToolStrip(controlToUpdate.ContextMenuStrip, newBackColor);
            }
            var tabControl = controlToUpdate as MaterialTabControl;
            if (tabControl != null)
            {
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    tabPage.BackColor = newBackColor;
                }
            }

            if (controlToUpdate is MaterialDivider)
            {
                controlToUpdate.BackColor = GetDividersColor();
            }

            if (controlToUpdate is MaterialListView)
            {
                controlToUpdate.BackColor = newBackColor;

            }

            //recursive call
            foreach (Control control in controlToUpdate.Controls)
            {
                UpdateControl(control, newBackColor);
            }

            controlToUpdate.Invalidate();
        }
    }
}
