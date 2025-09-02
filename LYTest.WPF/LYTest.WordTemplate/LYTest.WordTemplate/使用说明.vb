Public Class 使用说明
    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        Dim txt As String = ""
        If ListBox1.SelectedIndex = -1 Then
            Return
        End If
        txt = ListBox1.Items(ListBox1.SelectedIndex)
        Select Case txt
            Case "不处理的情况"
                TextBox1.Text = "下拉框为空时或处理字符串为空就为不处理，不添加字符串处理"
            Case "日期"
                TextBox1.Text = "将日期的值转为指定的日期格式--前提该值必须为时间格式,否则书签会替换失败。" & vbCrLf
                TextBox1.Text &= "日期格式举例：yyyy年MM月dd日   或   yyyy-MM-dd  只要是可处理的日期格式都可。" & vbCrLf
                TextBox1.Text &= "日期格式后可加等号公式：=AddDay1、=AddMonth1、=AddYear1。代表加1天、加1月、加1年。"
            Case "分割"
                TextBox1.Text = "对字符串根据关键字进行分割，并取指定分割位置的值" & vbCrLf
                TextBox1.Text &= "以最后一个【|】作为界限  |前面的为分割字符串的关键字，如&*(),  |后为取第几个的值，从0开始" & vbCrLf
                TextBox1.Text &= "举例：%|0  以百分号进行分割取第一个值" & vbCrLf
                TextBox1.Text &= "举例：||0  以|进行分割取第一个值" & vbCrLf
                TextBox1.Text &= "举例：123|0  以123进行分割取第一个值" & vbCrLf
                TextBox1.Text &= "出错情况：下标超出分割的界限，没有|界限符号" & vbCrLf
            Case "公式计算"
                TextBox1.Text = "输入指定的公式自动进行计算" & vbCrLf
                TextBox1.Text &= "注意：公式的单元格必须和需要计算的单元格在同一个表格中" & vbCrLf
                TextBox1.Text &= "备注:单元格格式a1,a2,b1,b2   其中字母代表单元格的列-a为第一列,数字代表单元格的行" & vbCrLf
                TextBox1.Text &= "举例:求俩个单元格的平均值 输入 a1,a1  就是求a1和a2的平均值-保留俩位小数" & vbCrLf
                TextBox1.Text &= "举例：a1:b2,c1  其中a1:b2代表从第一行第一列到第二行第二列的一个矩形区域的所有单元格" & vbCrLf
                TextBox1.Text &= "情况:单元格不是数字默认为0" & vbCrLf
        End Select

    End Sub

    Private Sub 使用说明_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ListBox1.SelectedIndex = 0
    End Sub
End Class