using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

internal partial class ColorBar : UserControl {
    public const int MinSmoothness = 0;
    public const int MaxSmoothness = 7;

    public const float MinThickness = 0.1F;
    public const float MaxThickness = 0.5F;

    private const int BorderWidth = 2;
    private List<Color> lstDefault;
    private List<SolidBrush> lstBrushes;

    public enum enumOrientation {
        Horizontal,
        Vertical,
        Circular
    }

    public enum BarStyle {
        Flow,
        Expand,
        Block
    }

    private List<Color> m_ColorList;
    private BarStyle m_Style;
    private int m_Value;
    private int m_Minimum;
    private int m_Maximum;
    private int m_Smoothness;
    private enumOrientation m_Orientation;
    private bool m_Reversed = false;
    private float m_WidthThickness;
    private float m_HeightThickness;

    public float WidthThickness {
        get {
            return m_WidthThickness;
        }
        set {
            if (value != m_WidthThickness) {
                if (value < MinThickness) {
                    value = MinThickness;
                }
                if (value > MaxThickness) {
                    value = MaxThickness;
                }
                m_WidthThickness = value;
                Invalidate(false);
            }
        }
    }

    public float HeightThickness {
        get {
            return m_HeightThickness;
        }
        set {
            if (value != m_HeightThickness) {
                if (value < MinThickness) {
                    value = MinThickness;
                }
                if (value > MaxThickness) {
                    value = MaxThickness;
                }
                m_HeightThickness = value;
                Invalidate(false);
            }
        }
    }

    public enumOrientation Orientation {
        get {
            return m_Orientation;
        }
        set {
            m_Orientation = value;
            Invalidate(false);
        }
    }

    public bool Reversed {
        get {
            return m_Reversed;
        }
        set {
            if (value != m_Reversed) {
                m_Reversed = value;
                lstBrushes.Reverse();
                Invalidate(false);
            }
        }
    }

    public List<Color> ColorList {
        get {
            return m_ColorList;
        }
        set {
            m_ColorList = value;
            if (m_ColorList != null) {
                if (m_ColorList.Count < 2) {
                    BuildColorList(ref lstDefault);
                } else {
                    BuildColorList(ref m_ColorList);
                }
            } else {
                BuildColorList(ref lstDefault);
            }
            Invalidate(false);
        }
    }

    public BarStyle Style {
        get {
            return m_Style;
        }
        set {
            m_Style = value;
            Invalidate(false);
        }
    }

    public int Value {
        get {
            return m_Value;
        }
        set {
            m_Value = value;
            if (m_Value < m_Minimum) {
                m_Value = m_Minimum;
            }
            if (m_Value > m_Maximum) {
                m_Value = m_Maximum;
            }
            Invalidate(false);
        }
    }

    public int Minimum {
        get {
            return m_Minimum;
        }
        set {
            m_Minimum = value;
            if (m_Minimum > m_Maximum) {
                Swap(ref m_Minimum, ref m_Maximum);
            }
            if (m_Value < m_Minimum) {
                m_Value = m_Minimum;
            }
            Invalidate(false);
        }
    }

    public int Maximum {
        get {
            return m_Maximum;
        }
        set {
            m_Maximum = value;
            if (m_Minimum > m_Maximum) {
                Swap(ref m_Minimum, ref m_Maximum);
            }
            if (m_Value > m_Maximum) {
                m_Value = m_Maximum;
            }
            Invalidate(false);
        }
    }

    public int Smoothness {
        get {
            return m_Smoothness;
        }
        set {
            if (value < MinSmoothness) {
                value = MinSmoothness;
            }
            if (value > MaxSmoothness) {
                value = MaxSmoothness;
            }
            m_Smoothness = value;
            if (m_ColorList != null) {
                BuildColorList(ref m_ColorList);
            } else {
                BuildColorList(ref lstDefault);
            }
            Invalidate(false);
        }
    }

    private void Swap(ref int val1, ref int val2) {
        int temp = 0;
        temp = val1;
        val1 = val2;
        val2 = temp;
    }

    public ColorBar() {

        InitializeComponent();

        BorderStyle = BorderStyle.Fixed3D;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.Opaque, true);
        UpdateStyles();

        lstDefault = new List<Color> {
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.Green,
            Color.Cyan,
            Color.Blue,
            Color.Indigo,
            Color.Violet
        };

        Minimum = 0;
        Maximum = 100;
        Smoothness = 0;
        Value = Minimum;
        Style = BarStyle.Expand;
        Orientation = enumOrientation.Horizontal;
        Reversed = false;
        WidthThickness = 0.2F;
        HeightThickness = 0.2F;

    }

    private Color InterpolateColors(Color color1, Color color2) {

        return Color.FromArgb(Convert.ToInt32((Convert.ToInt32(color1.R) + Convert.ToInt32(color2.R)) / 2.0), Convert.ToInt32((Convert.ToInt32(color1.G) + Convert.ToInt32(color2.G)) / 2.0), Convert.ToInt32((Convert.ToInt32(color1.B) + Convert.ToInt32(color2.B)) / 2.0));

    }

    private void BuildColorList(ref List<Color> lstAdd) {

        //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
        //		Color c = new Color();
        List<Color> lstColors = new List<Color>();

        lstBrushes = new List<SolidBrush>();

        foreach (Color c in lstAdd) {
            lstColors.Add(c);
        }

        int idx = 0; // lstColors index
        int cnt = 0; // lstColors item count
        int sdc = 0; // sub-divide count

        for (sdc = 0; sdc <= m_Smoothness; sdc++) {
            idx = 0;
            cnt = lstColors.Count - 1;
            while (idx < cnt) {
                lstColors.Insert(idx + 1, InterpolateColors(lstColors[idx], lstColors[idx + 1]));
                idx += 2;
                cnt += 1;
            }
        }

        foreach (Color c in lstColors) {
            lstBrushes.Add(new SolidBrush(c));
        }

    }

    protected override void WndProc(ref System.Windows.Forms.Message m) {
        if (m.Msg == 0x14) // ignore WM_ERASEBKGND
        {
            return;
        }
        base.WndProc(ref m);
    }

    protected override void OnResize(System.EventArgs e) {
        base.OnResize(e);
        this.Invalidate(false);
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {

        base.OnPaint(e);

        e.Graphics.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);

        float percentComplete = Convert.ToSingle((m_Value - m_Minimum) / (double)(m_Maximum - m_Minimum));

        if (percentComplete <= 0.0F) {
            return;
        }
        if (percentComplete > 1.0F) {
            percentComplete = 1.0F;
        }

        float fullWidth = 0F; // width = length

        if (m_Orientation == enumOrientation.Horizontal) {
            fullWidth = Convert.ToSingle(this.ClientRectangle.Width - BorderWidth);
        } else {
            fullWidth = Convert.ToSingle(this.ClientRectangle.Height - BorderWidth);
        }

        float totalWidth = fullWidth * percentComplete;

        float barWidth = 0F;
        if (m_Style == BarStyle.Expand) {
            barWidth = totalWidth;
        } else {
            if (m_Style == BarStyle.Flow || m_Style == BarStyle.Block) {
                barWidth = fullWidth;
            }
        }
        barWidth /= Convert.ToSingle(lstBrushes.Count);

        float height = 0F;
        float halfBorder = Convert.ToSingle(BorderWidth / 2.0);
        int idxColor = 0;
        float x = 0F;

        switch (m_Orientation) {

            case enumOrientation.Horizontal:

                height = Convert.ToSingle(this.ClientRectangle.Height - BorderWidth);

                //INSTANT C# TODO TASK: The step increment was not confirmed to be positive - confirm that the stopping condition is appropriate:
                //ORIGINAL LINE: For x = halfBorder To totalWidth Step barWidth
                for (x = halfBorder; x <= totalWidth; x += barWidth) {
                    e.Graphics.FillRectangle(lstBrushes[idxColor], x, halfBorder, barWidth, height);
                    if (barWidth > 4F && this.Style == BarStyle.Block) {
                        ControlPaint.DrawBorder(e.Graphics, new Rectangle(Convert.ToInt32(x), Convert.ToInt32(halfBorder), Convert.ToInt32(barWidth), Convert.ToInt32(height)), Color.Gray, ButtonBorderStyle.Outset);
                    }
                    if (idxColor < lstBrushes.Count) {
                        idxColor += 1;
                    }
                }

                if ((x < (this.ClientRectangle.Width - halfBorder)) && percentComplete == 1.0F) {
                    if (idxColor < lstBrushes.Count) {
                        e.Graphics.FillRectangle(lstBrushes[idxColor], x, halfBorder, ((this.ClientRectangle.Width - halfBorder) - x), height);
                    }
                }

                break;
            case enumOrientation.Vertical:

                height = Convert.ToSingle(this.ClientRectangle.Width - BorderWidth);

                //INSTANT C# TODO TASK: The step increment was not confirmed to be positive - confirm that the stopping condition is appropriate:
                //ORIGINAL LINE: For x = halfBorder To totalWidth Step barWidth
                for (x = halfBorder; x <= totalWidth; x += barWidth) {
                    e.Graphics.FillRectangle(lstBrushes[idxColor], halfBorder, this.ClientRectangle.Bottom - barWidth - x, height, barWidth);
                    if (barWidth > 4F && this.Style == BarStyle.Block) {
                        ControlPaint.DrawBorder(e.Graphics, new Rectangle(Convert.ToInt32(halfBorder), Convert.ToInt32(this.ClientRectangle.Bottom - barWidth - x), Convert.ToInt32(height), Convert.ToInt32(barWidth)), Color.Gray, ButtonBorderStyle.Outset);
                    }
                    if (idxColor < lstBrushes.Count) {
                        idxColor += 1;
                    }
                }

                if ((x < (this.ClientRectangle.Top - halfBorder)) && percentComplete == 1.0F) {
                    if (idxColor < lstBrushes.Count) {
                        e.Graphics.FillRectangle(lstBrushes[idxColor], halfBorder, x, height, x - (this.ClientRectangle.Top - halfBorder));
                    }
                }

                break;
            case enumOrientation.Circular:

                const float PI_OVER_180 = 0.0174532924F;
                float x1 = 0F;
                float y1 = 0F;
                float x2 = 0F;
                float y2 = 0F;
                float x3 = 0F;
                float y3 = 0F;
                float x4 = 0F;
                float y4 = 0F;
                float cx = Convert.ToSingle(this.ClientRectangle.Width / 2.0);
                float cy = Convert.ToSingle(this.ClientRectangle.Height / 2.0);
                float r1 = Convert.ToSingle(this.ClientRectangle.Width / 2.0);
                float r2 = Convert.ToSingle((this.ClientRectangle.Width / 2.0) - (r1 * m_WidthThickness));
                float r3 = Convert.ToSingle(this.ClientRectangle.Height / 2.0);
                float r4 = Convert.ToSingle((this.ClientRectangle.Height / 2.0) - (r3 * m_HeightThickness));
                float angle = 0.0F;
                float angleStep = 0F;
                float endAngle = 360.0F * percentComplete;
                PointF[] points = new PointF[4];

                if (m_Style == BarStyle.Expand) {
                    angleStep = (360.0F * percentComplete) / lstBrushes.Count;
                } else {
                    if (m_Style == BarStyle.Flow || m_Style == BarStyle.Block) {
                        angleStep = 360.0F / lstBrushes.Count;
                    }
                }

                x1 = Convert.ToSingle(r1 * (Math.Sin(PI_OVER_180 * angle)) + cx);
                y1 = Convert.ToSingle(r3 * (Math.Cos(PI_OVER_180 * angle)) + cy);
                x2 = Convert.ToSingle(r2 * (Math.Sin(PI_OVER_180 * angle)) + cx);
                y2 = Convert.ToSingle(r4 * (Math.Cos(PI_OVER_180 * angle)) + cy);

                do {

                    angle += angleStep;

                    x3 = Convert.ToSingle(r1 * (Math.Sin(PI_OVER_180 * angle)) + cx);
                    y3 = Convert.ToSingle(r3 * (Math.Cos(PI_OVER_180 * angle)) + cy);
                    x4 = Convert.ToSingle(r2 * (Math.Sin(PI_OVER_180 * angle)) + cx);
                    y4 = Convert.ToSingle(r4 * (Math.Cos(PI_OVER_180 * angle)) + cy);

                    points[0].X = x1;
                    points[0].Y = y1;
                    points[1].X = x3;
                    points[1].Y = y3;
                    points[2].X = x4;
                    points[2].Y = y4;
                    points[3].X = x2;
                    points[3].Y = y2;

                    x1 = x3;
                    y1 = y3;
                    x2 = x4;
                    y2 = y4;

                    if (idxColor < lstBrushes.Count) {
                        e.Graphics.FillPolygon(lstBrushes[idxColor], points);
                        idxColor += 1;
                    }

                } while (!(angle >= endAngle));

                break;
        }

    }
}

internal partial class ColorBar : UserControl {

    [System.Diagnostics.DebuggerNonUserCode()]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing && components != null) {
                components.Dispose();
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    //Required by the Windows Form Designer
    private System.ComponentModel.IContainer components;

    //NOTE: The following procedure is required by the Windows Form Designer
    //It can be modified using the Windows Form Designer.  
    //Do not modify it using the code editor.
    [System.Diagnostics.DebuggerStepThrough()]
    private void InitializeComponent() {
        this.SuspendLayout();
        //
        //ColorBar
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(6.0F, 13.0F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "ColorBar";
        this.Size = new System.Drawing.Size(200, 25);
        this.ResumeLayout(false);

    }

}