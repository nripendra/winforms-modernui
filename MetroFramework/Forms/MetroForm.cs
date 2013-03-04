/**
 * MetroFramework - Modern UI for WinForms
 * 
 * The MIT License (MIT)
 * Copyright (c) 2011 Sven Walter, http://github.com/viperneo
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in the 
 * Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using MetroFramework.Components;
using MetroFramework.Drawing;
using MetroFramework.Interfaces;
using MetroFramework.Native;

namespace MetroFramework.Forms
{

    public enum TextAlign
    {
        Left,
        Center,
        Right
    }
   
    public class MetroForm : Form, IMetroForm, IDisposable
    {
        #region Interface

        private MetroColorStyle metroStyle = MetroColorStyle.Blue;
        [Category("Metro Appearance")]
        public MetroColorStyle Style
        {
            get
            {
                if (StyleManager != null)
                    return StyleManager.Style;

                return metroStyle;
            }
            set { metroStyle = value; }
        }

        private MetroThemeStyle metroTheme = MetroThemeStyle.Light;

        [Category("Metro Appearance")]
        public MetroThemeStyle Theme
        {
            get
            {
                if (StyleManager != null)
                    return StyleManager.Theme;

                return metroTheme;
            }
            set { metroTheme = value; }
        }

        private MetroStyleManager metroStyleManager = null;
        [Browsable(false)]
        public MetroStyleManager StyleManager
        {
            get { return metroStyleManager; }
            set { metroStyleManager = value; }
        }

        #endregion

        #region Fields

        protected MetroDropShadow metroDropShadowForm = null;
        protected Dropshadow dropShadowForm = null;

        private bool isInitialized = false;

        private TextAlign textAlign = TextAlign.Left;
        [Browsable(true)]
        [Category("Metro Appearance")]
        public TextAlign TextAlign
        {
            get { return textAlign; }
            set { textAlign = value; }
        }

        [Browsable(false)]
        public override Color BackColor
        {
            get { return MetroPaint.BackColor.Form(Theme); }
        }

        [DefaultValue(FormBorderStyle.None)]
        [Browsable(false)]
        public new FormBorderStyle FormBorderStyle
        {
            get 
            {
                return FormBorderStyle.None;
            }
            set
            {
                base.FormBorderStyle = FormBorderStyle.None;
            }
        }

        private bool isMovable = true;
        [Category("Metro Appearance")]
        public bool Movable
        {
            get { return isMovable; }
            set { isMovable = value; }
        }

        public new Padding Padding
        {
            get { return base.Padding; }
            set
            {
                if (!DisplayHeader)
                {
                    if (value.Top < 30)
                    {
                        value.Top = 30;
                    }
                }
                else
                {
                    if (value.Top < 60)
                    {
                        value.Top = 60;
                    }
                }

                base.Padding = value;
            }
        }

        private bool displayHeader = true;
        [Category("Metro Appearance")]
        public bool DisplayHeader
        {
            get { return displayHeader; }
            set 
            { 
                displayHeader = value;

                if (displayHeader)
                    Padding = new Padding(20, 60, 20, 20);
                else
                    Padding = new Padding(20, 30, 20, 20);

                Invalidate();
            }
        }

        private bool isResizable = true;
        [Category("Metro Appearance")]
        public bool Resizable
        {
            get { return isResizable; }
            set { isResizable = value; }
        }

        private bool hasShadow = true;
        [Category("Metro Appearance")]
        public bool Shadow
        {
            get { return hasShadow; }
            set { hasShadow = value; }
        }


        private int borderWidth = 5;

        #endregion

        #region Constructor

        public MetroForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Name = "MetroForm";
            Padding = new Padding(20, 60, 20, 20);
            StartPosition = FormStartPosition.CenterScreen;

            RemoveCloseButton();
            FormBorderStyle = FormBorderStyle.None;
        }

        protected override void Dispose(bool disposing)
        {
            if (metroDropShadowForm != null)
            {
                if (!metroDropShadowForm.IsDisposed)
                {
                    metroDropShadowForm.Owner = null;
                    metroDropShadowForm.Dispose();
                    metroDropShadowForm = null;
                }
            }

            if (dropShadowForm != null)
            {
                if (!dropShadowForm.IsDisposed)
                {
                    dropShadowForm.Owner = null;
                    dropShadowForm.Dispose();
                    dropShadowForm = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e)
        {
            Color backColor = MetroPaint.BackColor.Form(Theme);
            Color foreColor = MetroPaint.ForeColor.Title(Theme);

            e.Graphics.Clear(backColor);

            using (SolidBrush b = MetroPaint.GetStyleBrush(Style))
            {
                Rectangle topRect = new Rectangle(0, 0, Width, borderWidth);
                e.Graphics.FillRectangle(b, topRect);
            }

            if (displayHeader)
            {
                switch (TextAlign)
                {
                    case TextAlign.Left:
                        TextRenderer.DrawText(e.Graphics, Text, MetroFonts.Title, new Point(20, 20), foreColor);
                        break;

                    case TextAlign.Center:
                        TextRenderer.DrawText(e.Graphics, Text, MetroFonts.Title, new Point(ClientRectangle.Width, 20), foreColor, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter);
                        break;

                    case TextAlign.Right:
                        Rectangle actualSize = MeasureText(e.Graphics, ClientRectangle, MetroFonts.Title, Text, TextFormatFlags.RightToLeft);
                        TextRenderer.DrawText(e.Graphics, Text, MetroFonts.Title, new Point(ClientRectangle.Width - actualSize.Width, 20), foreColor, TextFormatFlags.RightToLeft);
                        break;
                }
            }

            if (Resizable && (SizeGripStyle == SizeGripStyle.Auto || SizeGripStyle == SizeGripStyle.Show))
            {
                using (SolidBrush b = new SolidBrush(MetroPaint.ForeColor.Button.Disabled(Theme)))
                {
                    Size resizeHandleSize = new Size(2, 2);
                    e.Graphics.FillRectangles(b, new Rectangle[] {
                        new Rectangle(new Point(ClientRectangle.Width-6,ClientRectangle.Height-6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width-10,ClientRectangle.Height-10), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width-10,ClientRectangle.Height-6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width-6,ClientRectangle.Height-10), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width-14,ClientRectangle.Height-6), resizeHandleSize),
                        new Rectangle(new Point(ClientRectangle.Width-6,ClientRectangle.Height-14), resizeHandleSize)
                    });
                }
            }
        }

        #endregion

        #region Management Methods

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!(this is MetroTaskWindow))
                MetroTaskWindow.ForceClose();

            if (metroDropShadowForm != null)
            {
                metroDropShadowForm.Visible = false;
                metroDropShadowForm.Owner = null;
                metroDropShadowForm = null;
            }

            if (dropShadowForm != null)
            {
                dropShadowForm.Visible = false;
                dropShadowForm.Owner = null;
                dropShadowForm = null;
            }

            base.OnClosing(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (isInitialized)
            {
                Refresh();
            }
        }

        public bool FocusMe()
        {
            return WinApi.SetForegroundWindow(Handle);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //if (metroDropShadowForm == null && !DesignMode && hasShadow)
            //    metroDropShadowForm = new MetroDropShadow(this);

            if (dropShadowForm == null && !DesignMode && hasShadow)
            {
                dropShadowForm = new Dropshadow(this);
                dropShadowForm.Show();
            }
            
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!isInitialized)
            {
                if (ControlBox)
                {
                    AddWindowButton(WindowButtons.Close);

                    if (MaximizeBox)
                        AddWindowButton(WindowButtons.Maximize);

                    if (MinimizeBox)
                        AddWindowButton(WindowButtons.Minimize);

                    UpdateWindowButtonPosition();
                }

                if (StartPosition == FormStartPosition.CenterScreen)
                {
                    Point initialLocation = new Point();
                    initialLocation.X = (Screen.PrimaryScreen.WorkingArea.Width - (ClientRectangle.Width + 5)) / 2;
                    initialLocation.Y = (Screen.PrimaryScreen.WorkingArea.Height - (ClientRectangle.Height + 5)) / 2;
                    Location = initialLocation;
                    base.OnActivated(e);
                }

                isInitialized = true;
            }

            if (DesignMode) return;

            Refresh();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            UpdateWindowButtonPosition();
        }

        //private const int CS_DROPSHADOW = 0x20000;
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ClassStyle |= CS_DROPSHADOW;
        //        return cp;
        //    }
        //}

        protected override void WndProc(ref Message m)
        {
            if (MaximizeBox == true)
            {
                if (!WndProc_Movable(m))
                {
                    return;
                }

                base.WndProc(ref m);
            }

            if (!DesignMode)
            {
                if (MaximizeBox == false)
                {
                    if (m.Msg == (int)WinApi.Messages.WM_LBUTTONDBLCLK)
                    {
                        return;
                    }
                }
                if (!WndProc_Movable(m))
                {
                    return;
                }
                if (m.Msg == (int)WinApi.Messages.WM_NCHITTEST)
                {
                    m.Result = HitTestNCA(m.HWnd, m.WParam, m.LParam);
                }
            }

            if (MaximizeBox == false)
            {
                base.WndProc(ref m);
            }
        }

        private bool WndProc_Movable(Message m)
        {
            if (Movable) return true;

            if (m.Msg == (int)WinApi.Messages.WM_SYSCOMMAND)
            {
                int moveCommand = m.WParam.ToInt32() & 0xfff0;
                if (moveCommand == (int)WinApi.Messages.SC_MOVE)
                {
                    return false;
                }
            }

            return true;
        }

        private IntPtr HitTestNCA(IntPtr hwnd, IntPtr wparam, IntPtr lparam)
        {
            //Point vPoint = PointToClient(new Point((int)lparam & 0xFFFF, (int)lparam >> 16 & 0xFFFF));
            //Point vPoint = PointToClient(new Point((Int16)lparam, (Int16)((int)lparam >> 16)));
            Point vPoint = new Point((Int16)lparam, (Int16)((int)lparam >> 16));
            int vPadding = Math.Max(Padding.Right, Padding.Bottom);

            if (Resizable)
            {
                if (RectangleToScreen(new Rectangle(ClientRectangle.Width - vPadding, ClientRectangle.Height - vPadding, vPadding, vPadding)).Contains(vPoint))
                    return (IntPtr)WinApi.HitTest.HTBOTTOMRIGHT;
            }

            if (RectangleToScreen(new Rectangle(5, 5, ClientRectangle.Width - 10, 50)).Contains(vPoint))
                return (IntPtr)WinApi.HitTest.HTCAPTION;

            return (IntPtr)WinApi.HitTest.HTCLIENT;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && Movable)
            {
                if (WindowState == FormWindowState.Maximized) return;
                if (Width - borderWidth > e.Location.X && e.Location.X > borderWidth && e.Location.Y > borderWidth)
                {
                    MoveControl();                    
                }
            }
        }

        private void MoveControl()
        {
            WinApi.ReleaseCapture();
            WinApi.SendMessage(Handle, (int)WinApi.Messages.WM_NCLBUTTONDOWN, (int)WinApi.HitTest.HTCAPTION, 0);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        #endregion

        #region Window Buttons

        private enum WindowButtons
        {
            Minimize,
            Maximize,
            Close
        }

        private Dictionary<WindowButtons, MetroFormButton> windowButtonList;

        private void AddWindowButton(WindowButtons button)
        {
            if (windowButtonList == null)
                windowButtonList = new Dictionary<WindowButtons, MetroFormButton>();

            if (windowButtonList.ContainsKey(button))
                return;

            MetroFormButton newButton = new MetroFormButton();

            if (button == WindowButtons.Close)
            {
                newButton.Text = "r";
            }
            else if (button == WindowButtons.Minimize)
            {
                newButton.Text = "0";
            }
            else if (button == WindowButtons.Maximize)
            {
                if (WindowState == FormWindowState.Normal)
                    newButton.Text = "1";
                else
                    newButton.Text = "2";
            }

            newButton.Tag = button;
            newButton.Size = new Size(25, 20);
            newButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newButton.Click += new EventHandler(WindowButton_Click);
            Controls.Add(newButton);

            windowButtonList.Add(button, newButton);
        }

        private void WindowButton_Click(object sender, EventArgs e)
        {
            var btn = sender as MetroFormButton;
            if (btn != null)
            {
                var btnFlag = (WindowButtons)btn.Tag;
                if (btnFlag == WindowButtons.Close)
                {
                    Close();
                }
                else if (btnFlag == WindowButtons.Minimize)
                {
                    WindowState = FormWindowState.Minimized;
                }
                else if (btnFlag == WindowButtons.Maximize)
                {
                    if (WindowState == FormWindowState.Normal)
                    {
                        WindowState = FormWindowState.Maximized;
                        btn.Text = "2";
                    }
                    else
                    {
                        WindowState = FormWindowState.Normal;
                        btn.Text = "1";
                    }
                }
            }
        }

        private void UpdateWindowButtonPosition()
        {
            if (!ControlBox) return;

            Dictionary<int, WindowButtons> priorityOrder = new Dictionary<int, WindowButtons>(3) { {0, WindowButtons.Close}, {1, WindowButtons.Maximize}, {2, WindowButtons.Minimize} };

            Point firstButtonLocation = new Point(ClientRectangle.Width - 40, borderWidth);
            int lastDrawedButtonPosition = firstButtonLocation.X - 25;

            MetroFormButton firstButton = null;

            if (windowButtonList.Count == 1)
            {
                foreach (KeyValuePair<WindowButtons, MetroFormButton> button in windowButtonList)
                {
                    button.Value.Location = firstButtonLocation;
                }
            }
            else
            {
                foreach (KeyValuePair<int, WindowButtons> button in priorityOrder)
                {
                    bool buttonExists = windowButtonList.ContainsKey(button.Value);

                    if (firstButton == null && buttonExists)
                    {
                        firstButton = windowButtonList[button.Value];
                        firstButton.Location = firstButtonLocation;
                        continue;
                    }

                    if (firstButton == null || !buttonExists) continue;

                    windowButtonList[button.Value].Location = new Point(lastDrawedButtonPosition, borderWidth);
                    lastDrawedButtonPosition = lastDrawedButtonPosition - 25;
                }
            }

            Refresh();
        }

        private class MetroFormButton : Button, IMetroControl
        {
            #region Interface

            private MetroColorStyle metroStyle = MetroColorStyle.Blue;
            [Category("Metro Appearance")]
            public MetroColorStyle Style
            {
                get
                {
                    if (StyleManager != null)
                        return StyleManager.Style;

                    return metroStyle;
                }
                set { metroStyle = value; }
            }

            private MetroThemeStyle metroTheme = MetroThemeStyle.Light;
            [Category("Metro Appearance")]
            public MetroThemeStyle Theme
            {
                get
                {
                    if (StyleManager != null)
                        return StyleManager.Theme;

                    return metroTheme;
                }
                set { metroTheme = value; }
            }

            private MetroStyleManager metroStyleManager = null;
            [Browsable(false)]
            public MetroStyleManager StyleManager
            {
                get { return metroStyleManager; }
                set { metroStyleManager = value; }
            }

            #endregion

            #region Fields

            private bool isHovered = false;
            private bool isPressed = false;

            #endregion

            #region Constructor

            public MetroFormButton()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);
            }

            #endregion

            #region Paint Methods

            protected override void OnPaint(PaintEventArgs e)
            {
                Color backColor, foreColor;

                if (Parent != null)
                {
                    if (Parent is IMetroForm)
                    {
                        backColor = MetroPaint.BackColor.Form(Theme);
                    }
                    else if (Parent is IMetroControl)
                    {
                        backColor = MetroPaint.GetStyleColor(Style);
                    }
                    else
                    {
                        backColor = Parent.BackColor;
                    }
                }
                else
                {
                    backColor = MetroPaint.BackColor.Form(Theme);
                }

                if (isHovered && !isPressed && Enabled)
                {
                    foreColor = MetroPaint.ForeColor.Button.Normal(Theme);
                    backColor = MetroPaint.BackColor.Button.Normal(Theme);
                }
                else if (isHovered && isPressed && Enabled)
                {
                    foreColor = MetroPaint.ForeColor.Button.Press(Theme);
                    backColor = MetroPaint.GetStyleColor(Style);
                }
                else if (!Enabled)
                {
                    foreColor = MetroPaint.ForeColor.Button.Disabled(Theme);
                    backColor = MetroPaint.BackColor.Button.Disabled(Theme);
                }
                else
                {
                    foreColor = MetroPaint.ForeColor.Button.Normal(Theme);
                }

                e.Graphics.Clear(backColor);
                Font buttonFont = new Font("Webdings", 9.25f);
                TextRenderer.DrawText(e.Graphics, Text, buttonFont, ClientRectangle, foreColor, backColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            #endregion

            #region Mouse Methods

            protected override void OnMouseEnter(EventArgs e)
            {
                isHovered = true;
                Invalidate();

                base.OnMouseEnter(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    isPressed = true;
                    Invalidate();
                }

                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                isPressed = false;
                Invalidate();

                base.OnMouseUp(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                isHovered = false;
                Invalidate();

                base.OnMouseLeave(e);
            }

            #endregion
        }

        #endregion

        #region DropShadow Form

        protected class MetroDropShadow : Form
        {
            private Form shadowTargetForm;

            public MetroDropShadow(Form targetForm)
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);

                shadowTargetForm = targetForm;
                shadowTargetForm.Activated += new EventHandler(shadowTargetForm_Activated);
                shadowTargetForm.ResizeBegin += new EventHandler(shadowTargetForm_ResizeBegin);
                shadowTargetForm.ResizeEnd += new EventHandler(shadowTargetForm_ResizeEnd);
                shadowTargetForm.VisibleChanged += new EventHandler(shadowTargetForm_VisibleChanged);

                Opacity = 0.2;
                ShowInTaskbar = false;
                ShowIcon = false;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = shadowTargetForm.StartPosition;

                if (shadowTargetForm.Owner != null)
                    Owner = shadowTargetForm.Owner;
                
                shadowTargetForm.Owner = this;
            }

            private void shadowTargetForm_VisibleChanged(object sender, EventArgs e)
            {
                Visible = shadowTargetForm.Visible;
            }

            private void shadowTargetForm_Activated(object sender, EventArgs e)
            {
                Bounds = new Rectangle(shadowTargetForm.Location.X - 5, shadowTargetForm.Location.Y - 5, shadowTargetForm.Width + 10, shadowTargetForm.Height + 10);

                Visible = (shadowTargetForm.WindowState == FormWindowState.Normal);
                if (Visible) Show();
            }

            private void shadowTargetForm_ResizeBegin(object sender, EventArgs e)
            {
                Visible = false;
                Hide();
            }

            private void shadowTargetForm_ResizeEnd(object sender, EventArgs e)
            {
                Bounds = new Rectangle(shadowTargetForm.Location.X - 5, shadowTargetForm.Location.Y - 5, shadowTargetForm.Width + 10, shadowTargetForm.Height + 10);

                Visible = (shadowTargetForm.WindowState == FormWindowState.Normal);
                if (Visible) Show();
            }

            private const int WS_EX_TRANSPARENT = 0x20;
            private const int WS_EX_NOACTIVATE = 0x8000000;

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle = cp.ExStyle | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(Color.Gray);

                using (Brush b = new SolidBrush(Color.Black))
                {
                    e.Graphics.FillRectangle(b, new Rectangle(4, 4, ClientRectangle.Width - 8, ClientRectangle.Height - 8));
                }
            }
        }

        //http://stackoverflow.com/questions/8793445/windows-7-style-dropshadow-in-borderless-form
        /// <author>Corylulu</author>
        protected class Dropshadow : Form
        {
            Form shadowTargetForm;
            Point Offset = new Point(-15, -15);
            bool isBringingToFront;
            Bitmap getShadow;
            Timer timer = new Timer();

            public Dropshadow(Form parentForm)
            {
                shadowTargetForm = parentForm;

                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;

                /*This bit of code makes the form click-through. 
                  So you can click forms that are below it in z-space */
                uint wl = Win32.GetWindowLong(this.Handle, -20);
                wl = wl | 0x80000 | 0x20;
                Win32.SetWindowLong(this.Handle, -20, wl);

                //Makes the start location the same as parent.
                this.StartPosition = parentForm.StartPosition;

                parentForm.Activated += ParentForm_Activated; //Fires on parent activation to do a this.BringToFront() 
                this.Deactivate += This_Deactivated; //Toggles a boolean that ensures that ParentForm_Activated does fire when clicking through (this)
                parentForm.Move += ParentForm_Move; //Follows movement of parent form
                parentForm.Resize += ParentForm_Resize;
                parentForm.ResizeEnd += ParentForm_ResizeEnd;

                parentForm.Owner = this;

                Bounds = GetBounds();

                this.Load += Dropshadow_Load;
                
            }

            private void Dropshadow_Load(object sendr, EventArgs e)
            {
                //@nrip - Avoid showing shadow while form is loading.
                timer.Interval = 50;
                timer.Tick += timer_Tick;
                timer.Start();
            }

            private void timer_Tick(object sendr, EventArgs e)
            {
                timer.Tick -= timer_Tick;
                timer.Stop();
                getShadow = DrawBlurBorder();
                SetBitmap(getShadow, 255); //Sets background as 32-bit image with full alpha.
            }

            private Rectangle GetBounds()
            {
                return new Rectangle((shadowTargetForm).Location.X + Offset.X, (shadowTargetForm).Location.Y + Offset.Y, shadowTargetForm.ClientRectangle.Width + Math.Abs(Offset.X * 2), shadowTargetForm.ClientRectangle.Height + Math.Abs(Offset.Y * 2));
            }

            private void ParentForm_Activated(object o, EventArgs e)
            {
                if (isBringingToFront)
                {
                    isBringingToFront = false;
                    return;
                }

                this.BringToFront();
            }

            private void This_Deactivated(object o, EventArgs e)
            {
                /* Prevents recusion. */
                isBringingToFront = true;
            }

            /* Adjust position when parent moves. */
            private void ParentForm_Move(object o, EventArgs e)
            {
                if (o is Form)
                    this.Bounds = GetBounds();
            }

            long lastResizedOn = 0;
            private void ParentForm_Resize(object o, EventArgs e)
            {
                if (o is Form)
                    this.Bounds = GetBounds();
                //@nrip - Quick and dirty throttling.
                long delta = DateTime.Now.Ticks - lastResizedOn;
                if (delta > 100000)
                {
                    lastResizedOn = DateTime.Now.Ticks;
                    Invalidate();
                }
            }

            private void ParentForm_ResizeEnd(object o, EventArgs e)
            {
                lastResizedOn = 0;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                getShadow = DrawBlurBorder();
                SetBitmap(getShadow, 255); //Sets background as 32-bit image with full alpha.
            }

            //http://www.codeproject.com/Articles/1822/Per-Pixel-Alpha-Blend-in-C
            /// <author><name>Rui Godinho Lopes</name><email>rui@ruilopes.com</email></author>
            private void SetBitmap(Bitmap bitmap, byte opacity = 255)
            {
                if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                    throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

                // The idea of this is very simple,
                // 1. Create a compatible DC with screen;
                // 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC;
                // 3. Call the UpdateLayeredWindow.

                IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
                IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
                IntPtr hBitmap = IntPtr.Zero;
                IntPtr oldBitmap = IntPtr.Zero;

                try
                {
                    hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));  // grab a GDI handle from this GDI+ bitmap
                    oldBitmap = Win32.SelectObject(memDc, hBitmap);

                    Win32.Size size = new Win32.Size(bitmap.Width, bitmap.Height);
                    Win32.Point pointSource = new Win32.Point(0, 0);
                    Win32.Point topPos = new Win32.Point(Left, Top);
                    Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION();
                    blend.BlendOp = Win32.AC_SRC_OVER;
                    blend.BlendFlags = 0;
                    blend.SourceConstantAlpha = opacity;
                    blend.AlphaFormat = Win32.AC_SRC_ALPHA;

                    Win32.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);
                }
                finally
                {
                    Win32.ReleaseDC(IntPtr.Zero, screenDc);
                    if (hBitmap != IntPtr.Zero)
                    {
                        Win32.SelectObject(memDc, oldBitmap);
                        //Windows.DeleteObject(hBitmap); // The documentation says that we have to use the Windows.DeleteObject... but since there is no such method I use the normal DeleteObject from Win32 GDI and it's working fine without any resource leak.
                        Win32.DeleteObject(hBitmap);
                    }
                    Win32.DeleteDC(memDc);
                }
            }

            private Bitmap DrawBlurBorder()
            {
                return (Bitmap)DrawOutsetShadow(0, 0, 40, 1, Color.Black, new Rectangle(1, 1, ClientRectangle.Width, ClientRectangle.Height));
            }

            //http://stackoverflow.com/questions/12924296/css3-like-box-shadow-implementation-algorithm
            /// <author>Marino Šimić</author>
            private Image DrawOutsetShadow(int hShadow, int vShadow, int blur, int spread, Color color, Rectangle shadowCanvasArea)
            {
                var rOuter = shadowCanvasArea;
                var rInner = shadowCanvasArea;
                rInner.Offset(hShadow, vShadow);
                rInner.Inflate(-blur, -blur);
                rOuter.Inflate(spread, spread);
                rOuter.Offset(hShadow, vShadow);
                var originalOuter = rOuter;

                var img = new Bitmap(originalOuter.Width, originalOuter.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var g = Graphics.FromImage(img);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                var currentBlur = 0;
                do
                {
                    var transparency = (rOuter.Height - rInner.Height) / (double)(blur * 2 + spread * 2);
                    var shadowColor = Color.FromArgb(((int)(255 * (transparency * transparency))), color);
                    var rOutput = rInner;
                    rOutput.Offset(-originalOuter.Left, -originalOuter.Top);

                    DrawRoundedRectangle(g, rOutput, currentBlur, Pens.Transparent, shadowColor);
                    rInner.Inflate(1, 1);
                    currentBlur = (int)((double)blur * (1 - (transparency * transparency)));
                } while (rOuter.Contains(rInner));

                g.Flush();
                g.Dispose();

                return img;
            }

            private void DrawRoundedRectangle(Graphics gfx, Rectangle bounds, int cornerRadius, Pen drawPen, Color fillColor)
            {
                int strokeOffset = Convert.ToInt32(Math.Ceiling(drawPen.Width));
                bounds = Rectangle.Inflate(bounds, -strokeOffset, -strokeOffset);

                var gfxPath = new System.Drawing.Drawing2D.GraphicsPath();
                if (cornerRadius > 0)
                {
                    gfxPath.AddArc(bounds.X, bounds.Y, cornerRadius, cornerRadius, 180, 90);
                    gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y, cornerRadius, cornerRadius, 270, 90);
                    gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y + bounds.Height - cornerRadius, cornerRadius,
                                   cornerRadius, 0, 90);
                    gfxPath.AddArc(bounds.X, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                }
                else
                {
                    gfxPath.AddRectangle(bounds);
                }
                gfxPath.CloseAllFigures();

                if (cornerRadius > 5)
                {
                    gfx.FillPath(new SolidBrush(fillColor), gfxPath);
                }
                if (drawPen != Pens.Transparent)
                {
                    var pen = new Pen(drawPen.Color);
                    pen.EndCap = pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    gfx.DrawPath(pen, gfxPath);
                }
            }

            #region native win32 helpers, probably should be merged into WinApi class
            class Win32
            {
                public enum Bool
                {
                    False = 0,
                    True
                };


                [StructLayout(LayoutKind.Sequential)]
                public struct Point
                {
                    public Int32 x;
                    public Int32 y;

                    public Point(Int32 x, Int32 y) { this.x = x; this.y = y; }
                }


                [StructLayout(LayoutKind.Sequential)]
                public struct Size
                {
                    public Int32 cx;
                    public Int32 cy;

                    public Size(Int32 cx, Int32 cy) { this.cx = cx; this.cy = cy; }
                }


                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                struct ARGB
                {
                    public byte Blue;
                    public byte Green;
                    public byte Red;
                    public byte Alpha;
                }


                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                public struct BLENDFUNCTION
                {
                    public byte BlendOp;
                    public byte BlendFlags;
                    public byte SourceConstantAlpha;
                    public byte AlphaFormat;
                }


                public const Int32 ULW_COLORKEY = 0x00000001;
                public const Int32 ULW_ALPHA = 0x00000002;
                public const Int32 ULW_OPAQUE = 0x00000004;

                public const byte AC_SRC_OVER = 0x00;
                public const byte AC_SRC_ALPHA = 0x01;


                [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                public static extern Bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

                [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                public static extern IntPtr GetDC(IntPtr hWnd);

                [DllImport("user32.dll", ExactSpelling = true)]
                public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

                [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                public static extern Bool DeleteDC(IntPtr hdc);

                [DllImport("gdi32.dll", ExactSpelling = true)]
                public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

                [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                public static extern Bool DeleteObject(IntPtr hObject);

                [DllImport("user32.dll", SetLastError = true)]
                public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

                [DllImport("user32.dll")]
                public static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);
            }
            #endregion
        }
        #endregion

        #region Helper methods

        public void RemoveCloseButton()
        {
            IntPtr hMenu = WinApi.GetSystemMenu(Handle, false);
            if (hMenu == IntPtr.Zero) return;

            int n = WinApi.GetMenuItemCount(hMenu);
            if (n <= 0) return;

            WinApi.RemoveMenu(hMenu, (uint)(n - 1), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.RemoveMenu(hMenu, (uint)(n - 2), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.DrawMenuBar(Handle);
        }

        private Rectangle MeasureText(Graphics g, Rectangle clientRectangle, Font font, string text, TextFormatFlags flags)
        {
            var proposedSize = new Size(int.MaxValue, int.MinValue);
            var actualSize = TextRenderer.MeasureText(g, text, font, proposedSize, flags);
            return new Rectangle(clientRectangle.X, clientRectangle.Y, actualSize.Width, actualSize.Height);
        }

        #endregion
    }
}