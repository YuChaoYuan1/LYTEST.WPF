<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.AxOA1 = New AxOALib.AxOA()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Btn_打开模板 = New System.Windows.Forms.Button()
        Me.Btn_保存 = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Cmb_模板列表 = New System.Windows.Forms.ComboBox()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.SplitContainer3 = New System.Windows.Forms.SplitContainer()
        Me.TreeView1 = New System.Windows.Forms.TreeView()
        Me.SplitContainer4 = New System.Windows.Forms.SplitContainer()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.p_检定参数 = New System.Windows.Forms.FlowLayoutPanel()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.list_结论 = New System.Windows.Forms.ListBox()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.cmb_处理方式 = New System.Windows.Forms.ComboBox()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.介绍ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.txt解析代码 = New System.Windows.Forms.TextBox()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.P_台体信息 = New System.Windows.Forms.FlowLayoutPanel()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.cmb_处理方式2 = New System.Windows.Forms.ComboBox()
        Me.txt解析代码2 = New System.Windows.Forms.TextBox()
        Me.P_表位信息 = New System.Windows.Forms.FlowLayoutPanel()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        CType(Me.AxOA1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        CType(Me.SplitContainer3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer3.Panel1.SuspendLayout()
        Me.SplitContainer3.Panel2.SuspendLayout()
        Me.SplitContainer3.SuspendLayout()
        CType(Me.SplitContainer4, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer4.Panel1.SuspendLayout()
        Me.SplitContainer4.Panel2.SuspendLayout()
        Me.SplitContainer4.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer2.Panel1.SuspendLayout()
        Me.SplitContainer2.Panel2.SuspendLayout()
        Me.SplitContainer2.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.Panel2)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Panel1)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.TabControl1)
        Me.SplitContainer1.Size = New System.Drawing.Size(1308, 714)
        Me.SplitContainer1.SplitterDistance = 875
        Me.SplitContainer1.TabIndex = 0
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.SystemColors.Control
        Me.Panel2.Controls.Add(Me.AxOA1)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel2.Location = New System.Drawing.Point(0, 58)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(875, 656)
        Me.Panel2.TabIndex = 1
        '
        'AxOA1
        '
        Me.AxOA1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.AxOA1.Enabled = True
        Me.AxOA1.Location = New System.Drawing.Point(0, 0)
        Me.AxOA1.Name = "AxOA1"
        Me.AxOA1.OcxState = CType(resources.GetObject("AxOA1.OcxState"), System.Windows.Forms.AxHost.State)
        Me.AxOA1.Size = New System.Drawing.Size(875, 656)
        Me.AxOA1.TabIndex = 0
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.SystemColors.Control
        Me.Panel1.Controls.Add(Me.Btn_打开模板)
        Me.Panel1.Controls.Add(Me.Btn_保存)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.Cmb_模板列表)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(875, 58)
        Me.Panel1.TabIndex = 0
        '
        'Btn_打开模板
        '
        Me.Btn_打开模板.Location = New System.Drawing.Point(480, 17)
        Me.Btn_打开模板.Name = "Btn_打开模板"
        Me.Btn_打开模板.Size = New System.Drawing.Size(93, 29)
        Me.Btn_打开模板.TabIndex = 3
        Me.Btn_打开模板.Text = "打开"
        Me.Btn_打开模板.UseVisualStyleBackColor = True
        '
        'Btn_保存
        '
        Me.Btn_保存.Location = New System.Drawing.Point(669, 17)
        Me.Btn_保存.Name = "Btn_保存"
        Me.Btn_保存.Size = New System.Drawing.Size(93, 29)
        Me.Btn_保存.TabIndex = 2
        Me.Btn_保存.Text = "保存"
        Me.Btn_保存.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("黑体", 10.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 22)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(71, 15)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "模板列表"
        '
        'Cmb_模板列表
        '
        Me.Cmb_模板列表.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.Cmb_模板列表.Font = New System.Drawing.Font("黑体", 10.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Cmb_模板列表.FormattingEnabled = True
        Me.Cmb_模板列表.Location = New System.Drawing.Point(107, 19)
        Me.Cmb_模板列表.Name = "Cmb_模板列表"
        Me.Cmb_模板列表.Size = New System.Drawing.Size(367, 22)
        Me.Cmb_模板列表.TabIndex = 0
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControl1.Location = New System.Drawing.Point(0, 0)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(429, 714)
        Me.TabControl1.TabIndex = 0
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.SplitContainer3)
        Me.TabPage2.Location = New System.Drawing.Point(4, 24)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(421, 686)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "检定信息"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'SplitContainer3
        '
        Me.SplitContainer3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer3.Location = New System.Drawing.Point(3, 3)
        Me.SplitContainer3.Name = "SplitContainer3"
        '
        'SplitContainer3.Panel1
        '
        Me.SplitContainer3.Panel1.Controls.Add(Me.TreeView1)
        '
        'SplitContainer3.Panel2
        '
        Me.SplitContainer3.Panel2.Controls.Add(Me.SplitContainer4)
        Me.SplitContainer3.Size = New System.Drawing.Size(415, 680)
        Me.SplitContainer3.SplitterDistance = 221
        Me.SplitContainer3.TabIndex = 0
        '
        'TreeView1
        '
        Me.TreeView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeView1.Font = New System.Drawing.Font("黑体", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.TreeView1.Location = New System.Drawing.Point(0, 0)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.Size = New System.Drawing.Size(221, 680)
        Me.TreeView1.TabIndex = 0
        '
        'SplitContainer4
        '
        Me.SplitContainer4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer4.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer4.Name = "SplitContainer4"
        Me.SplitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer4.Panel1
        '
        Me.SplitContainer4.Panel1.Controls.Add(Me.GroupBox3)
        '
        'SplitContainer4.Panel2
        '
        Me.SplitContainer4.Panel2.Controls.Add(Me.GroupBox4)
        Me.SplitContainer4.Size = New System.Drawing.Size(190, 680)
        Me.SplitContainer4.SplitterDistance = 437
        Me.SplitContainer4.TabIndex = 0
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.p_检定参数)
        Me.GroupBox3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBox3.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(190, 437)
        Me.GroupBox3.TabIndex = 0
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "参数"
        '
        'p_检定参数
        '
        Me.p_检定参数.Dock = System.Windows.Forms.DockStyle.Fill
        Me.p_检定参数.Location = New System.Drawing.Point(3, 20)
        Me.p_检定参数.Name = "p_检定参数"
        Me.p_检定参数.Size = New System.Drawing.Size(184, 414)
        Me.p_检定参数.TabIndex = 0
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.Label2)
        Me.GroupBox4.Controls.Add(Me.list_结论)
        Me.GroupBox4.Controls.Add(Me.Panel3)
        Me.GroupBox4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBox4.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(190, 239)
        Me.GroupBox4.TabIndex = 0
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "结论"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Lime
        Me.Label2.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Label2.Location = New System.Drawing.Point(3, 221)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(87, 15)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "项目总结论"
        '
        'list_结论
        '
        Me.list_结论.Dock = System.Windows.Forms.DockStyle.Fill
        Me.list_结论.FormattingEnabled = True
        Me.list_结论.ItemHeight = 14
        Me.list_结论.Location = New System.Drawing.Point(3, 53)
        Me.list_结论.Name = "list_结论"
        Me.list_结论.Size = New System.Drawing.Size(184, 183)
        Me.list_结论.TabIndex = 4
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.cmb_处理方式)
        Me.Panel3.Controls.Add(Me.txt解析代码)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel3.Location = New System.Drawing.Point(3, 20)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(184, 33)
        Me.Panel3.TabIndex = 3
        '
        'cmb_处理方式
        '
        Me.cmb_处理方式.ContextMenuStrip = Me.ContextMenuStrip1
        Me.cmb_处理方式.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_处理方式.FormattingEnabled = True
        Me.cmb_处理方式.Items.AddRange(New Object() {"", "日期", "分割", "求和", "求差", "求平均值"})
        Me.cmb_处理方式.Location = New System.Drawing.Point(3, 4)
        Me.cmb_处理方式.Name = "cmb_处理方式"
        Me.cmb_处理方式.Size = New System.Drawing.Size(63, 22)
        Me.cmb_处理方式.TabIndex = 2
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.介绍ToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(125, 26)
        '
        '介绍ToolStripMenuItem
        '
        Me.介绍ToolStripMenuItem.Name = "介绍ToolStripMenuItem"
        Me.介绍ToolStripMenuItem.Size = New System.Drawing.Size(124, 22)
        Me.介绍ToolStripMenuItem.Text = "使用说明"
        '
        'txt解析代码
        '
        Me.txt解析代码.ContextMenuStrip = Me.ContextMenuStrip1
        Me.txt解析代码.Location = New System.Drawing.Point(72, 3)
        Me.txt解析代码.Name = "txt解析代码"
        Me.txt解析代码.Size = New System.Drawing.Size(109, 24)
        Me.txt解析代码.TabIndex = 1
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.SplitContainer2)
        Me.TabPage1.Location = New System.Drawing.Point(4, 24)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(421, 686)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "设备信息"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'SplitContainer2
        '
        Me.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer2.Location = New System.Drawing.Point(3, 3)
        Me.SplitContainer2.Name = "SplitContainer2"
        Me.SplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.GroupBox1)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.GroupBox2)
        Me.SplitContainer2.Size = New System.Drawing.Size(415, 680)
        Me.SplitContainer2.SplitterDistance = 195
        Me.SplitContainer2.TabIndex = 0
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.P_台体信息)
        Me.GroupBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBox1.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(415, 195)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "台体信息"
        '
        'P_台体信息
        '
        Me.P_台体信息.Dock = System.Windows.Forms.DockStyle.Fill
        Me.P_台体信息.Location = New System.Drawing.Point(3, 20)
        Me.P_台体信息.Name = "P_台体信息"
        Me.P_台体信息.Size = New System.Drawing.Size(409, 172)
        Me.P_台体信息.TabIndex = 0
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.GroupBox5)
        Me.GroupBox2.Controls.Add(Me.P_表位信息)
        Me.GroupBox2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBox2.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(415, 481)
        Me.GroupBox2.TabIndex = 0
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "表位信息"
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.cmb_处理方式2)
        Me.GroupBox5.Controls.Add(Me.txt解析代码2)
        Me.GroupBox5.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.GroupBox5.Location = New System.Drawing.Point(3, 421)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(409, 57)
        Me.GroupBox5.TabIndex = 0
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "公式"
        '
        'cmb_处理方式2
        '
        Me.cmb_处理方式2.ContextMenuStrip = Me.ContextMenuStrip1
        Me.cmb_处理方式2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_处理方式2.FormattingEnabled = True
        Me.cmb_处理方式2.Items.AddRange(New Object() {"", "日期", "分割", "求和", "求差", "求平均值"})
        Me.cmb_处理方式2.Location = New System.Drawing.Point(22, 27)
        Me.cmb_处理方式2.Name = "cmb_处理方式2"
        Me.cmb_处理方式2.Size = New System.Drawing.Size(106, 22)
        Me.cmb_处理方式2.TabIndex = 6
        '
        'txt解析代码2
        '
        Me.txt解析代码2.ContextMenuStrip = Me.ContextMenuStrip1
        Me.txt解析代码2.Location = New System.Drawing.Point(144, 25)
        Me.txt解析代码2.Name = "txt解析代码2"
        Me.txt解析代码2.Size = New System.Drawing.Size(259, 24)
        Me.txt解析代码2.TabIndex = 5
        '
        'P_表位信息
        '
        Me.P_表位信息.Dock = System.Windows.Forms.DockStyle.Fill
        Me.P_表位信息.Location = New System.Drawing.Point(3, 20)
        Me.P_表位信息.Name = "P_表位信息"
        Me.P_表位信息.Size = New System.Drawing.Size(409, 458)
        Me.P_表位信息.TabIndex = 0
        '
        'Form1
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(1308, 714)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Font = New System.Drawing.Font("黑体", 10.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Name = "Form1"
        Me.Text = "Word模板编辑器1.0.0.1"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        CType(Me.AxOA1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage2.ResumeLayout(False)
        Me.SplitContainer3.Panel1.ResumeLayout(False)
        Me.SplitContainer3.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer3.ResumeLayout(False)
        Me.SplitContainer4.Panel1.ResumeLayout(False)
        Me.SplitContainer4.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer4, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer4.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.SplitContainer2.Panel1.ResumeLayout(False)
        Me.SplitContainer2.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents Panel2 As Panel
    Friend WithEvents Panel1 As Panel
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents SplitContainer2 As SplitContainer
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents AxOA1 As AxOALib.AxOA
    Friend WithEvents Btn_保存 As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents Cmb_模板列表 As ComboBox
    Friend WithEvents P_台体信息 As FlowLayoutPanel
    Friend WithEvents P_表位信息 As FlowLayoutPanel
    Friend WithEvents SplitContainer3 As SplitContainer
    Friend WithEvents TreeView1 As TreeView
    Friend WithEvents SplitContainer4 As SplitContainer
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents Btn_打开模板 As Button
    Friend WithEvents p_检定参数 As FlowLayoutPanel
    Friend WithEvents list_结论 As ListBox
    Friend WithEvents Panel3 As Panel
    Friend WithEvents txt解析代码 As TextBox
    Friend WithEvents cmb_处理方式 As ComboBox
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents 介绍ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GroupBox5 As GroupBox
    Friend WithEvents cmb_处理方式2 As ComboBox
    Friend WithEvents txt解析代码2 As TextBox
    Friend WithEvents Label2 As Label
End Class
