using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
// ReSharper disable VirtualMemberCallInConstructor

namespace MaterialSkin.Controls
{
    public class MaterialForm : Form, IMaterialControl
    {
        #region Properties

        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;
        [Browsable(false)]
        public MouseState MouseState { get; set; }
        public new FormBorderStyle FormBorderStyle { get { return base.FormBorderStyle; } set { base.FormBorderStyle = value; } }
        public bool Sizable { get; set; }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] Monitorinfoex info);

        public const int WmNclbuttondown = 0xA1;
        public const int HtCaption = 0x2;
        public const int WmMousemove = 0x0200;
        public const int WmLbuttondown = 0x0201;
        public const int WmLbuttonup = 0x0202;
        public const int WmLbuttondblclk = 0x0203;
        public const int WmRbuttondown = 0x0204;
        private const int Htbottomleft = 16;
        private const int Htbottomright = 17;
        private const int Htleft = 10;
        private const int Htright = 11;
        private const int Htbottom = 15;
        private const int Httop = 12;
        private const int Httopleft = 13;
        private const int Httopright = 14;
        private const int BorderWidth = 7;
        private ResizeDirection _resizeDir;
        private ButtonState _buttonState = ButtonState.None;

        private const int WmszTop = 3;
        private const int WmszTopleft = 4;
        private const int WmszTopright = 5;
        private const int WmszLeft = 1;
        private const int WmszRight = 2;
        private const int WmszBottom = 6;
        private const int WmszBottomleft = 7;
        private const int WmszBottomright = 8;

        private readonly Dictionary<int, int> _resizingLocationsToCmd = new Dictionary<int, int>
        {
            {Httop,         WmszTop},
            {Httopleft,     WmszTopleft},
            {Httopright,    WmszTopright},
            {Htleft,        WmszLeft},
            {Htright,       WmszRight},
            {Htbottom,      WmszBottom},
            {Htbottomleft,  WmszBottomleft},
            {Htbottomright, WmszBottomright}
        };

        private const int StatusBarButtonWidth = StatusBarHeight;
        private const int StatusBarHeight = 24;
        private const int ActionBarHeight = 40;

        private const uint TpmLeftalign = 0x0000;
        private const uint TpmReturncmd = 0x0100;

        private const int WmSyscommand = 0x0112;
        private const int WsMinimizebox = 0x20000;
        private const int WsSysmenu = 0x00080000;

        private const int MonitorDefaulttonearest = 2;

        private readonly Cursor[] _resizeCursors = { Cursors.SizeNESW, Cursors.SizeWE, Cursors.SizeNWSE, Cursors.SizeWE, Cursors.SizeNS };

        private Rectangle _minButtonBounds;
        private Rectangle _maxButtonBounds;
        private Rectangle _xButtonBounds;
        private Rectangle _actionBarBounds;
        private Rectangle _statusBarBounds;

        private bool _maximized;
        private Size _previousSize;
        private Point _previousLocation;
        private bool _headerMouseDown;

        #endregion

        #region Classes

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public class Monitorinfoex
        {
            public int cbSize = Marshal.SizeOf(typeof(Monitorinfoex));
            public Rect rcMonitor = new Rect();
            public Rect rcWork = new Rect();
            public int dwFlags = 0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice = new char[32];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public int Width()
            {
                return right - left;
            }

            public int Height()
            {
                return bottom - top;
            }
        }

        private enum ResizeDirection
        {
            BottomLeft,
            Left,
            Right,
            BottomRight,
            Bottom,
            None
        }

        private enum ButtonState
        {
            XOver,
            MaxOver,
            MinOver,
            XDown,
            MaxDown,
            MinDown,
            None
        }

        #endregion

        #region Constructors

        public MaterialForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            Sizable = true;
            DoubleBuffered = true;
            RightToLeft = RightToLeft.Yes;
            AutoScaleMode = AutoScaleMode.Dpi;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            // This enables the form to trigger the MouseMove event even when mouse is over another control
            Application.AddMessageFilter(new MouseMessageFilter());
            MouseMessageFilter.MouseMove += OnGlobalMouseMove;
        }

        #endregion

        #region Methods

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (DesignMode || IsDisposed) return;

            if (m.Msg == WmLbuttondblclk)
            {
                MaximizeWindow(!_maximized);
            }
            else if (m.Msg == WmMousemove && _maximized &&
                (_statusBarBounds.Contains(PointToClient(Cursor.Position)) || _actionBarBounds.Contains(PointToClient(Cursor.Position))) &&
                !(_minButtonBounds.Contains(PointToClient(Cursor.Position)) || _maxButtonBounds.Contains(PointToClient(Cursor.Position)) || _xButtonBounds.Contains(PointToClient(Cursor.Position))))
            {
                if (_headerMouseDown)
                {
                    _maximized = false;
                    _headerMouseDown = false;

                    var mousePoint = PointToClient(Cursor.Position);
                    if (mousePoint.X < Width / 2)
                        Location = mousePoint.X < _previousSize.Width / 2 ?
                            new Point(Cursor.Position.X - mousePoint.X, Cursor.Position.Y - mousePoint.Y) :
                            new Point(Cursor.Position.X - _previousSize.Width / 2, Cursor.Position.Y - mousePoint.Y);
                    else
                        Location = Width - mousePoint.X < _previousSize.Width / 2 ?
                            new Point(Cursor.Position.X - _previousSize.Width + Width - mousePoint.X, Cursor.Position.Y - mousePoint.Y) :
                            new Point(Cursor.Position.X - _previousSize.Width / 2, Cursor.Position.Y - mousePoint.Y);

                    Size = _previousSize;
                    ReleaseCapture();
                    SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
                }
            }
            else if (m.Msg == WmLbuttondown &&
                (_statusBarBounds.Contains(PointToClient(Cursor.Position)) || _actionBarBounds.Contains(PointToClient(Cursor.Position))) &&
                !(_minButtonBounds.Contains(PointToClient(Cursor.Position)) || _maxButtonBounds.Contains(PointToClient(Cursor.Position)) || _xButtonBounds.Contains(PointToClient(Cursor.Position))))
            {
                if (!_maximized)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
                }
                else
                {
                    _headerMouseDown = true;
                }
            }
            else if (m.Msg == WmRbuttondown)
            {
                Point cursorPos = PointToClient(Cursor.Position);

                if (_statusBarBounds.Contains(cursorPos) && !_minButtonBounds.Contains(cursorPos) &&
                    !_maxButtonBounds.Contains(cursorPos) && !_xButtonBounds.Contains(cursorPos))
                {
                    // Show default system menu when right clicking titlebar
                    var id = TrackPopupMenuEx(GetSystemMenu(Handle, false), TpmLeftalign | TpmReturncmd, Cursor.Position.X, Cursor.Position.Y, Handle, IntPtr.Zero);

                    // Pass the command as a WM_SYSCOMMAND message
                    SendMessage(Handle, WmSyscommand, id, 0);
                }
            }
            else if (m.Msg == WmNclbuttondown)
            {
                // This re-enables resizing by letting the application know when the
                // user is trying to resize a side. This is disabled by default when using WS_SYSMENU.
                if (!Sizable) return;

                byte bFlag = 0;

                // Get which side to resize from
                if (_resizingLocationsToCmd.ContainsKey((int)m.WParam))
                    bFlag = (byte)_resizingLocationsToCmd[(int)m.WParam];

                if (bFlag != 0)
                    SendMessage(Handle, WmSyscommand, 0xF000 | bFlag, (int)m.LParam);
            }
            else if (m.Msg == WmLbuttonup)
            {
                _headerMouseDown = false;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var par = base.CreateParams;
                // WS_SYSMENU: Trigger the creation of the system menu
                // WS_MINIMIZEBOX: Allow minimizing from taskbar
                par.Style = par.Style | WsMinimizebox | WsSysmenu; // Turn on the WS_MINIMIZEBOX style flag
                return par;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (DesignMode) return;
            UpdateButtons(e);

            if (e.Button == MouseButtons.Left && !_maximized)
                ResizeForm(_resizeDir);
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (DesignMode) return;
            _buttonState = ButtonState.None;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (DesignMode) return;

            if (Sizable)
            {
                //True if the mouse is hovering over a child control
                var isChildUnderMouse = GetChildAtPoint(e.Location) != null;

                if (e.Location.X < BorderWidth && e.Location.Y > Height - BorderWidth && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.BottomLeft;
                    Cursor = Cursors.SizeNESW;
                }
                else if (e.Location.X < BorderWidth && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.Left;
                    Cursor = Cursors.SizeWE;
                }
                else if (e.Location.X > Width - BorderWidth && e.Location.Y > Height - BorderWidth && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.BottomRight;
                    Cursor = Cursors.SizeNWSE;
                }
                else if (e.Location.X > Width - BorderWidth && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.Right;
                    Cursor = Cursors.SizeWE;
                }
                else if (e.Location.Y > Height - BorderWidth && !isChildUnderMouse && !_maximized)
                {
                    _resizeDir = ResizeDirection.Bottom;
                    Cursor = Cursors.SizeNS;
                }
                else
                {
                    _resizeDir = ResizeDirection.None;

                    //Only reset the cursor when needed, this prevents it from flickering when a child control changes the cursor to its own needs
                    if (_resizeCursors.Contains(Cursor))
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }

            UpdateButtons(e);
        }

        protected void OnGlobalMouseMove(object sender, MouseEventArgs e)
        {
            if (IsDisposed) return;
            // Convert to client position and pass to Form.MouseMove
            var clientCursorPos = PointToClient(e.Location);
            var newE = new MouseEventArgs(MouseButtons.None, 0, clientCursorPos.X, clientCursorPos.Y, 0);
            OnMouseMove(newE);
        }

        private void UpdateButtons(MouseEventArgs e, bool up = false)
        {
            if (DesignMode) return;
            var oldState = _buttonState;
            bool showMin = MinimizeBox && ControlBox;
            bool showMax = MaximizeBox && ControlBox;

            if (e.Button == MouseButtons.Left && !up)
            {
                if (showMin && !showMax && _maxButtonBounds.Contains(e.Location))
                    _buttonState = ButtonState.MinDown;
                else if (showMin && showMax && _minButtonBounds.Contains(e.Location))
                    _buttonState = ButtonState.MinDown;
                else if (showMax && _maxButtonBounds.Contains(e.Location))
                    _buttonState = ButtonState.MaxDown;
                else if (ControlBox && _xButtonBounds.Contains(e.Location))
                    _buttonState = ButtonState.XDown;
                else
                    _buttonState = ButtonState.None;
            }
            else
            {
                if (showMin && !showMax && _maxButtonBounds.Contains(e.Location))
                {
                    _buttonState = ButtonState.MinOver;

                    if (oldState == ButtonState.MinDown && up)
                        WindowState = FormWindowState.Minimized;
                }
                else if (showMin && showMax && _minButtonBounds.Contains(e.Location))
                {
                    _buttonState = ButtonState.MinOver;

                    if (oldState == ButtonState.MinDown && up)
                        WindowState = FormWindowState.Minimized;
                }
                else if (MaximizeBox && ControlBox && _maxButtonBounds.Contains(e.Location))
                {
                    _buttonState = ButtonState.MaxOver;

                    if (oldState == ButtonState.MaxDown && up)
                        MaximizeWindow(!_maximized);

                }
                else if (ControlBox && _xButtonBounds.Contains(e.Location))
                {
                    _buttonState = ButtonState.XOver;

                    if (oldState == ButtonState.XDown && up)
                        Close();
                }
                else _buttonState = ButtonState.None;
            }

            if (oldState != _buttonState) Invalidate();
        }

        private void MaximizeWindow(bool maximize)
        {
            if (!MaximizeBox || !ControlBox) return;

            _maximized = maximize;

            if (maximize)
            {
                var monitorHandle = MonitorFromWindow(Handle, MonitorDefaulttonearest);
                var monitorInfo = new Monitorinfoex();
                GetMonitorInfo(new HandleRef(null, monitorHandle), monitorInfo);
                _previousSize = Size;
                _previousLocation = Location;
                Size = new Size(monitorInfo.rcWork.Width(), monitorInfo.rcWork.Height());
                Location = new Point(monitorInfo.rcWork.left, monitorInfo.rcWork.top);
            }
            else
            {
                Size = _previousSize;
                Location = _previousLocation;
            }

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (DesignMode) return;
            UpdateButtons(e, true);

            base.OnMouseUp(e);
            ReleaseCapture();
        }

        private void ResizeForm(ResizeDirection direction)
        {
            if (DesignMode) return;
            var dir = -1;
            switch (direction)
            {
                case ResizeDirection.BottomLeft:
                    dir = Htbottomleft;
                    break;
                case ResizeDirection.Left:
                    dir = Htleft;
                    break;
                case ResizeDirection.Right:
                    dir = Htright;
                    break;
                case ResizeDirection.BottomRight:
                    dir = Htbottomright;
                    break;
                case ResizeDirection.Bottom:
                    dir = Htbottom;
                    break;
            }

            ReleaseCapture();
            if (dir != -1)
            {
                SendMessage(Handle, WmNclbuttondown, dir, 0);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            _minButtonBounds = new Rectangle((Width - SkinManager.FormPadding / 2) - 3 * StatusBarButtonWidth, 0, StatusBarButtonWidth, StatusBarHeight);
            _maxButtonBounds = new Rectangle((Width - SkinManager.FormPadding / 2) - 2 * StatusBarButtonWidth, 0, StatusBarButtonWidth, StatusBarHeight);
            _xButtonBounds = new Rectangle((Width - SkinManager.FormPadding / 2) - StatusBarButtonWidth, 0, StatusBarButtonWidth, StatusBarHeight);
            _statusBarBounds = new Rectangle(0, 0, Width, StatusBarHeight);
            _actionBarBounds = new Rectangle(0, StatusBarHeight, Width, ActionBarHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            g.Clear(SkinManager.GetApplicationBackgroundColor());
            g.FillRectangle(SkinManager.ColorScheme.DarkPrimaryBrush, _statusBarBounds);
            g.FillRectangle(SkinManager.ColorScheme.PrimaryBrush, _actionBarBounds);

            //Draw border
            using (var borderPen = new Pen(SkinManager.GetDividersColor(), 1))
            {
                g.DrawLine(borderPen, new Point(0, _actionBarBounds.Bottom), new Point(0, Height - 2));
                g.DrawLine(borderPen, new Point(Width - 1, _actionBarBounds.Bottom), new Point(Width - 1, Height - 2));
                g.DrawLine(borderPen, new Point(0, Height - 1), new Point(Width - 1, Height - 1));
            }

            // Determine whether or not we even should be drawing the buttons.
            bool showMin = MinimizeBox && ControlBox;
            bool showMax = MaximizeBox && ControlBox;
            var hoverBrush = SkinManager.GetFlatButtonHoverBackgroundBrush();
            var downBrush = SkinManager.GetFlatButtonPressedBackgroundBrush();

            // When MaximizeButton == false, the minimize button will be painted in its place
            if (_buttonState == ButtonState.MinOver && showMin)
                g.FillRectangle(hoverBrush, showMax ? _minButtonBounds : _maxButtonBounds);

            if (_buttonState == ButtonState.MinDown && showMin)
                g.FillRectangle(downBrush, showMax ? _minButtonBounds : _maxButtonBounds);

            if (_buttonState == ButtonState.MaxOver && showMax)
                g.FillRectangle(hoverBrush, _maxButtonBounds);

            if (_buttonState == ButtonState.MaxDown && showMax)
                g.FillRectangle(downBrush, _maxButtonBounds);

            if (_buttonState == ButtonState.XOver && ControlBox)
                g.FillRectangle(hoverBrush, _xButtonBounds);

            if (_buttonState == ButtonState.XDown && ControlBox)
                g.FillRectangle(downBrush, _xButtonBounds);

            using (var formButtonsPen = new Pen(SkinManager.ActionBarTextSecondary, 2))
            {
                // Minimize button.
                if (showMin)
                {
                    int x = showMax ? _minButtonBounds.X : _maxButtonBounds.X;
                    int y = showMax ? _minButtonBounds.Y : _maxButtonBounds.Y;

                    g.DrawLine(
                        formButtonsPen,
                        x + (int)(_minButtonBounds.Width * 0.33),
                        y + (int)(_minButtonBounds.Height * 0.66),
                        x + (int)(_minButtonBounds.Width * 0.66),
                        y + (int)(_minButtonBounds.Height * 0.66)
                   );
                }

                // Maximize button
                if (showMax)
                {
                    g.DrawRectangle(
                        formButtonsPen,
                        _maxButtonBounds.X + (int)(_maxButtonBounds.Width * 0.33),
                        _maxButtonBounds.Y + (int)(_maxButtonBounds.Height * 0.36),
                        (int)(_maxButtonBounds.Width * 0.39),
                        (int)(_maxButtonBounds.Height * 0.31)
                   );
                }

                // Close button
                if (ControlBox)
                {
                    g.DrawLine(
                        formButtonsPen,
                        _xButtonBounds.X + (int)(_xButtonBounds.Width * 0.33),
                        _xButtonBounds.Y + (int)(_xButtonBounds.Height * 0.33),
                        _xButtonBounds.X + (int)(_xButtonBounds.Width * 0.66),
                        _xButtonBounds.Y + (int)(_xButtonBounds.Height * 0.66)
                   );

                    g.DrawLine(
                        formButtonsPen,
                        _xButtonBounds.X + (int)(_xButtonBounds.Width * 0.66),
                        _xButtonBounds.Y + (int)(_xButtonBounds.Height * 0.33),
                        _xButtonBounds.X + (int)(_xButtonBounds.Width * 0.33),
                        _xButtonBounds.Y + (int)(_xButtonBounds.Height * 0.66));
                }
            }

            //Form title
            g.DrawString(Text, SkinManager.NazanintarMedium13, SkinManager.ColorScheme.TextBrush, new Rectangle(SkinManager.FormPadding, StatusBarHeight, Width - 30, ActionBarHeight), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far });
        }

        #endregion
    }

    public class MouseMessageFilter : IMessageFilter
    {
        private const int WmMousemove = 0x0200;

        public static event MouseEventHandler MouseMove;

        public bool PreFilterMessage(ref Message m)
        {

            if (m.Msg == WmMousemove)
            {
                if (MouseMove != null)
                {
                    int x = Control.MousePosition.X, y = Control.MousePosition.Y;

                    MouseMove(null, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
                }
            }
            return false;
        }
    }
}