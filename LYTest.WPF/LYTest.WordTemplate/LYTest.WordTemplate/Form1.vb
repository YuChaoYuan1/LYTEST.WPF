Imports System.Globalization
Imports System.IO

Public Class Form1
    Public 检定参数 As Dictionary(Of String, List(Of String)) = New Dictionary(Of String, List(Of String))
    Public 参数编号 As Dictionary(Of String, List(Of String)) = New Dictionary(Of String, List(Of String))
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            载入模板列表()
            载入台体信息()
            载入表位信息()
            载入检定方案()
            载入检定参数数据源()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub 载入模板列表()
        Dim fileNames() As String = Directory.GetFiles(String.Format("{0}\Res\Word", Directory.GetCurrentDirectory()))
        Cmb_模板列表.Items.Clear()
        For index = 0 To fileNames.Length - 1
            Dim arrayName() As String = fileNames(index).Split("\")
            Dim nameTemp As String = arrayName(arrayName.Length - 1)
            If nameTemp.EndsWith(".doc") Or nameTemp.EndsWith(".docx") Then
                Cmb_模板列表.Items.Add(nameTemp)
            End If
        Next
        If Cmb_模板列表.Items.Count > 0 Then
            Cmb_模板列表.SelectedIndex = 0
        End If
    End Sub
    Private Sub 载入台体信息()
        Dim Path As String = Directory.GetCurrentDirectory() & "\Ini\DeviceData.ini"
        Dim NameS() As String = GetINI("Data", "名称", "", Path).Split("|")
        Dim NameSCount As Integer = NameS.Length
        Dim AddNameS() As String = GetINI("Regulation", "名称", "", Path).Split("|")
        Dim RegNameS() As String = {"单-本地依据", "单-远程依据", "三-本地依据", "三-远程依据"}
        Array.Resize(NameS, NameSCount + AddNameS.Length + RegNameS.Length)
        For n = 0 To RegNameS.Length - 1
            If Not String.IsNullOrWhiteSpace(RegNameS(n)) Then
                NameS(NameSCount + n) = RegNameS(n)
            End If
        Next
        For n = 0 To AddNameS.Length - 1
            If Not String.IsNullOrWhiteSpace(AddNameS(n)) And Not RegNameS.Contains(AddNameS(n)) Then
                NameS(NameSCount + RegNameS.Length + n) = AddNameS(n)
            End If
        Next

        P_台体信息.Controls.Clear()
        For index = 0 To NameS.Length - 1
            If Not String.IsNullOrWhiteSpace(NameS(index)) Then
                Dim btn As Label = New Label()
                btn.BorderStyle = BorderStyle.Fixed3D
                btn.TextAlign = ContentAlignment.MiddleCenter
                btn.Text = NameS(index)
                btn.BackColor = Control.DefaultBackColor
                AddHandler btn.MouseEnter, AddressOf 选中修改背景
                AddHandler btn.MouseLeave, AddressOf 离开修改背景
                AddHandler btn.MouseDown， AddressOf 按下修改背景
                AddHandler btn.MouseUp， AddressOf 弹起修改背景
                AddHandler btn.DoubleClick, AddressOf 插入标签_台体数据
                P_台体信息.Controls.Add(btn)
            End If
        Next
    End Sub
    Private Sub 载入表位信息()
        Dim Db As DataTable = AccessHelper.GetDataTable("select * from T_VIEW_CONFIG where AVR_VIEW_ID='900'")
        P_表位信息.Controls.Clear()
        If IsNothing(Db) Then
            Return
        End If
        Dim name As String = Db.Rows(0)("AVR_COL_SHOW_NAME").ToString()
        'Dim value As String = Db.Rows(0)("AVR_COL_NAME").ToString()
        Dim tem() As String = name.Split(",")
        For i = 0 To tem.Length - 1
            Dim v() As String = tem(i).Split("|")
            Dim btn As Label = New Label()
            btn.BorderStyle = BorderStyle.Fixed3D
            btn.TextAlign = ContentAlignment.MiddleCenter
            btn.Text = v(2)
            btn.BackColor = Control.DefaultBackColor
            btn.Tag = v(1)
            'btn.Width = 100
            AddHandler btn.DoubleClick, AddressOf 插入标签_表位数据
            AddHandler btn.MouseEnter, AddressOf 选中修改背景
            AddHandler btn.MouseLeave, AddressOf 离开修改背景
            AddHandler btn.MouseDown， AddressOf 按下修改背景
            AddHandler btn.MouseUp， AddressOf 弹起修改背景
            P_表位信息.Controls.Add(btn)
        Next

    End Sub
    Private Sub 载入检定方案()
        TreeView1.Nodes.Clear()
        Dim Db As DataTable = AccessHelper.GetDataTable("select * from T_CODE_TREE where CODE_PARENT='SchemaCategory'")
        For i = 0 To Db.Rows.Count - 1
            Dim TreeNode As TreeNode = New TreeNode()

            Dim nameCN As String = Db.Rows(i)("CODE_CN_NAME").ToString()
            Dim nameEN As String = Db.Rows(i)("CODE_EN_NAME").ToString()
            Dim ID As String = Db.Rows(i)("CODE_VALUE").ToString()
            TreeNode.Tag = ID
            TreeNode.Text = nameCN
            TreeNode.SelectedImageIndex = 0
            '这里需要递归调用
            获取节点数据(nameEN, TreeNode)
            TreeView1.Nodes.Add(TreeNode)
        Next
    End Sub
    Private Sub 获取节点数据(name As String, node As TreeNode)
        Try
            Dim Db As DataTable = AccessHelper.GetDataTable($"select * from T_CODE_TREE where CODE_PARENT='{name}'")
            For i = 0 To Db.Rows.Count
                Dim TreeNode As TreeNode = New TreeNode()
                Dim nameCN As String = Db.Rows(i)("CODE_CN_NAME").ToString()
                Dim nameEN As String = Db.Rows(i)("CODE_EN_NAME").ToString()
                Dim ID As String = Db.Rows(i)("CODE_VALUE").ToString()
                TreeNode.Tag = node.Tag.ToString() & ID.PadLeft(3, "0")
                TreeNode.Text = nameCN
                TreeNode.SelectedImageIndex = 0
                获取节点数据(nameEN, TreeNode)
                node.Nodes.Add(TreeNode)
                '这里需要递归调用
            Next
        Catch ex As Exception

        End Try


    End Sub
    Private Sub 载入检定参数数据源()
        检定参数.Clear()
        参数编号.Clear()
        Dim Db As DataTable = AccessHelper.GetDataTable("select * from T_CODE_TREE where CODE_PARENT='CheckParamSource'")
        For i = 0 To Db.Rows.Count - 1
            Dim list As List(Of String) = New List(Of String)
            Dim list2 As List(Of String) = New List(Of String)

            Dim nameEN As String = Db.Rows(i)("CODE_EN_NAME").ToString()
            Dim Db2 As DataTable = AccessHelper.GetDataTable($"select * from T_CODE_TREE where CODE_PARENT='{nameEN}'")
            For j = 0 To Db2.Rows.Count - 1
                Dim nameCN As String = Db2.Rows(j)("CODE_CN_NAME").ToString()
                Dim num As String = Db2.Rows(j)("CODE_VALUE").ToString()
                list.Add(nameCN)
                list2.Add(num)
            Next
            检定参数.Add(nameEN, list)
            参数编号.Add(nameEN, list2)
        Next

    End Sub


    ''' <summary>
    ''' 选中节点
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect
        Dim ID = TreeView1.SelectedNode.Tag.ToString '当前选中节点的编号
        p_检定参数.Controls.Clear()
        list_结论.Items.Clear()
        Dim Db As DataTable = AccessHelper.GetDataTable($"select * from T_SCHEMA_PARA_FORMAT where PARA_NO='{ID}'")
        If Db.Rows.Count <= 0 Then
            Return
        End If
        'chk_是否代码解析.Checked = False
        cmb_处理方式.Text = ""
        Dim IsVisible() As String = Db.Rows(0)("PARA_KEY_RULE").ToString().Split("|")
        Dim Name() As String = Db.Rows(0)("PARA_VIEW").ToString().Split("|")
        Dim Value() As String = Db.Rows(0)("PARA_P_CODE").ToString().Split("|")
        Dim DefaultValue() As String = Db.Rows(0)("DEFAULT_VALUE").ToString().Split("|") '默认值
        Dim width As Integer = p_检定参数.Width - 10

        For i = 0 To IsVisible.Length - 1
            If IsVisible(i).ToLower = "true" Then
                Dim leb As Label = New Label()
                leb.Text = Name(i)
                leb.Width = width
                leb.TextAlign = ContentAlignment.BottomLeft
                p_检定参数.Controls.Add(leb)
                Dim cmb As ComboBox = New ComboBox()
                cmb.Width = width
                If 检定参数.ContainsKey(Value(i)) Then
                    Dim list As List(Of String) = 检定参数(Value(i))
                    cmb.Items.AddRange(list.ToArray())
                    If cmb.Items.Count > 0 Then
                        cmb.SelectedItem = DefaultValue(i)
                    End If
                End If
                p_检定参数.Controls.Add(cmb)
            End If
        Next

        Dim ViewID As String = Db.Rows(0)("RESULT_VIEW_ID").ToString()
        Dim Db2 As DataTable = AccessHelper.GetDataTable($"select * from T_VIEW_CONFIG where AVR_VIEW_ID='{ViewID}'")
        If Db2.Rows.Count <= 0 Then
            Return
        End If
        Dim ResultName() As String = Db2.Rows(0)("AVR_COL_SHOW_NAME").ToString().Split(",")
        Dim R As List(Of String) = New List(Of String)
        For i = 0 To ResultName.Length - 1
            R.AddRange(ResultName(i).Split("|"))
        Next
        list_结论.Items.AddRange(R.ToArray())
        list_结论.Tag = Value
    End Sub

    Private Sub 插入标签_表位数据(sender As Object, e As EventArgs)
        Dim lab As Label = sender
        Dim 是否插入代码解析 As Boolean = False
        Dim 是否是计算公式 As Boolean = False
        Dim 解析代码 As String = ""
        If cmb_处理方式2.Text <> "" And txt解析代码2.Text.Trim <> "" Then
            Select Case cmb_处理方式2.Text
                Case "日期"
                    解析代码 = "$T"
                    解析代码 = 解析代码 & txt解析代码2.Text.Trim
                Case "分割"
                    解析代码 = "$S"
                    解析代码 = 解析代码 & txt解析代码2.Text.Trim
                    If txt解析代码2.Text.IndexOf("|") = -1 Then
                        MessageBox.Show("分割必须带有分界福符号【|】")
                        Return
                    End If
                Case "求和"
                    解析代码 = $"=SUM({txt解析代码2.Text.Trim()})"
                    是否是计算公式 = True
                Case "求差"
                    解析代码 = $"=MINUS({txt解析代码2.Text.Trim()})"
                    是否是计算公式 = True
                Case "求平均值"
                    是否是计算公式 = True
                    解析代码 = $"=AVERAGE({txt解析代码2.Text.Trim()})"
            End Select

            是否插入代码解析 = True
        End If
        If 是否插入代码解析 Then
            If 是否是计算公式 Then
                插入书签(解析代码)
            Else
                插入书签("M_" & lab.Tag.ToString() & "_" & 解析代码)
            End If
        Else
            插入书签("M_" & lab.Tag.ToString())
        End If

    End Sub
    Private Sub 插入标签_台体数据(sender As Object, e As EventArgs)
        Dim lab As Label = sender
        Dim 是否插入代码解析 As Boolean = False
        Dim 是否是计算公式 As Boolean = False
        Dim 解析代码 As String = ""
        If cmb_处理方式2.Text <> "" And txt解析代码2.Text.Trim <> "" Then
            Select Case cmb_处理方式2.Text
                Case "日期"
                    解析代码 = "$T"
                    解析代码 = 解析代码 & txt解析代码2.Text.Trim
                Case "分割"
                    解析代码 = "$S"
                    解析代码 = 解析代码 & txt解析代码2.Text.Trim
                    If txt解析代码2.Text.IndexOf("|") = -1 Then
                        MessageBox.Show("分割必须带有分界福符号【|】")
                        Return
                    End If
                Case "求和"
                    解析代码 = $"=SUM({txt解析代码2.Text.Trim()})"
                    是否是计算公式 = True
                Case "求差"
                    解析代码 = $"=MINUS({txt解析代码2.Text.Trim()})"
                    是否是计算公式 = True
                Case "求平均值"
                    是否是计算公式 = True
                    解析代码 = $"=AVERAGE({txt解析代码2.Text.Trim()})"
            End Select

            是否插入代码解析 = True
        End If
        If 是否插入代码解析 Then
            If 是否是计算公式 Then
                插入书签(解析代码)
            Else
                插入书签("D_" & lab.Text.ToString() & "_" & 解析代码)
            End If
        Else
            插入书签("D_" & lab.Text.ToString())
        End If

    End Sub
    Private Sub 插入标签_结论数据(sender As Object, e As EventArgs) Handles list_结论.DoubleClick
        If list_结论.SelectedIndex < 0 Then
            Return
        End If
        '根据combobox的选中编号,组合获得子项编号加上结论名字(后期可以考虑结论的数据库编号)
        Dim ID = TreeView1.SelectedNode.Tag.ToString '当前选中节点的编号
        Dim Num As String = ""
        Dim str() As String = list_结论.Tag
        'For i = 0 To str.Length - 1
        '    Num += 检定参数(str)
        'Next
        Dim index As Integer = 0
        For i = 0 To p_检定参数.Controls.Count - 1
            If TypeOf p_检定参数.Controls(i) Is ComboBox Then
                Dim cmb As ComboBox = p_检定参数.Controls(i)
                Num += 参数编号(str(index))(cmb.SelectedIndex)
                index = index + 1
            End If
        Next

        'For Each control In p_检定参数.Controls
        '    If TypeOf control Is ComboBox Then
        '        Dim cmb As ComboBox = control
        '        Num += cmb.SelectedIndex.ToString()
        '    End If
        'Next
        Num = ID & "_" & Num
        Num = Num.TrimEnd("_")
        'If ID = "12001" Then '基本误差的编号需要调整一下，因为之前程序写的bug'
        '    Num = 处理基本误差字符串(Num)
        'End If
        Dim 是否插入代码解析 As Boolean = False
        Dim 是否是计算公式 As Boolean = False
        Dim 解析代码 As String = ""
        If cmb_处理方式.Text <> "" And txt解析代码.Text.Trim <> "" Then
            Select Case cmb_处理方式.Text
                Case "日期"
                    解析代码 = "$T"
                    解析代码 = 解析代码 & txt解析代码.Text.Trim
                    'Dim dt As DateTime
                    'Dim ifp As IFormatProvider = New CultureInfo("zh-CN", True)


                    'If DateTime.TryParseExact(DateTime.Now, txt解析代码.Text, ifp, DateTimeStyles.None, dt) <> True Then
                    '    MessageBox.Show("必须为日期格式")
                    '    Return
                    'End If

                Case "分割"
                    解析代码 = "$S"
                    解析代码 = 解析代码 & txt解析代码.Text.Trim
                    If txt解析代码.Text.IndexOf("|") = -1 Then
                        MessageBox.Show("分割必须带有分界福符号【|】")
                        Return
                    End If
                Case "求和"
                    解析代码 = $"=SUM({txt解析代码.Text.Trim()})"
                    是否是计算公式 = True
                Case "求差"
                    解析代码 = $"=MINUS({txt解析代码.Text.Trim()})"
                    是否是计算公式 = True
                Case "求平均值"
                    是否是计算公式 = True
                    解析代码 = $"=AVERAGE({txt解析代码.Text.Trim()})"
            End Select

            是否插入代码解析 = True
        End If
        If 是否插入代码解析 Then
            If 是否是计算公式 Then
                插入书签(解析代码)
            Else
                插入书签("R_" & Num & "_" & list_结论.Items(list_结论.SelectedIndex) & "_" & 解析代码)
            End If
        Else
            插入书签("R_" & Num & "_" & list_结论.Items(list_结论.SelectedIndex))
        End If
    End Sub
    Private Function 处理基本误差字符串(str As String) As String
        If str = "" Then
            Return ""
        End If
        Dim arrayTemp() As String = str.Split("_")
        If arrayTemp.Length = 3 Then
            Dim strPara As String = arrayTemp(2)
            Dim currentString As String = strPara.Substring(4, 2)
            strPara = strPara.Remove(4, 2)
            strPara = strPara.Insert(3, currentString)
            Return arrayTemp(0) + "_" + arrayTemp(1) + "_" + strPara
        ElseIf arrayTemp.Length = 2 Then
            Dim strPara As String = arrayTemp(1)
            Dim currentString As String = strPara.Substring(4, 2)
            strPara = strPara.Remove(4, 2)
            strPara = strPara.Insert(3, currentString)
            Return arrayTemp(0) + "_" + strPara
        End If
        Return str

    End Function

    Private Sub Btn_打开模板_Click(sender As Object, e As EventArgs) Handles Btn_打开模板.Click
        If Cmb_模板列表.Text.Trim() <> "" Then
            Dim str As String = Directory.GetCurrentDirectory() & "\Res\Word\" & Cmb_模板列表.Text.Trim()
            AxOA1.Close()
            AxOA1.Open(str)
        End If
    End Sub

    Private Sub Btn_保存_Click(sender As Object, e As EventArgs) Handles Btn_保存.Click
        Try
            Dim str As String = Directory.GetCurrentDirectory() & "\Res\Word\" & Cmb_模板列表.Text.Trim()
            AxOA1.Save(str)
            MessageBox.Show("保存成功！")
        Catch ex As Exception
            MessageBox.Show("保存失败：" & ex.Message)
        End Try

    End Sub

    Private Sub 介绍ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 介绍ToolStripMenuItem.Click
        Dim 说明 As 使用说明 = New 使用说明
        说明.Show()
    End Sub

    Private Sub list_结论_SelectedIndexChanged(sender As Object, e As EventArgs) Handles list_结论.SelectedIndexChanged

    End Sub

    ''' <summary>
    ''' 插入项目总结论
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button1_Click(sender As Object, e As EventArgs)

        '根据combobox的选中编号,组合获得子项编号加上结论名字(后期可以考虑结论的数据库编号)



    End Sub

    Private Sub Label2_DoubleClick(sender As Object, e As EventArgs) Handles Label2.DoubleClick
        Dim ID = TreeView1.SelectedNode.Tag.ToString '当前选中节点的编号

        插入书签("R_" & ID & "_总结论")
    End Sub
End Class
