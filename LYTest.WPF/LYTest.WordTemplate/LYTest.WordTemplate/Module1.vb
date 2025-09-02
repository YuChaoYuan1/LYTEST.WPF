Imports System.IO

Module Module1

    Function 插入书签(内容 As String)
        If Form1.AxOA1.IsOpen = False Then
            Return False
        End If


        Dim Word_Doc As Object      'Word 文档对象
        Dim Word_App As Object      'Word 全局对象
        Dim Word_Range As Object    'Word 文档Range对象
        '    Dim Word_BookMark As Object 'Word 书签对象
        Dim Lng_MarkStart As Long

        Try
            Word_Doc = Form1.AxOA1.GetIDispatch
            Word_App = Word_Doc.Application

            Lng_MarkStart = Word_App.selection.start
            Word_App.selection.TypeText（内容）
            Word_Range = Word_Doc.Range(Lng_MarkStart, Word_App.selection.End)

            'Call Word_Doc.bookmarks.Add("B_" & Word_Doc.bookmarks.Count + 1 & Day(Now) & Hour(Now) & Minute(Now) & Second(Now), Word_Range)
            Word_Doc.bookmarks.Add("B_" & Word_Doc.bookmarks.Count + 1 & Now.Day & Hour(Now) & Minute(Now) & Second(Now), Word_Range)
            Return True
        Catch ex As Exception
            MsgBox("插入失败")
            Return False
        End Try
    End Function


#Region "文本颜色"

    Public Sub 选中修改背景(sender As Object, e As EventArgs)
        Dim L1 As Label = CType(sender, Label) '获取当前操作的控件对象，只有这样才能对该控件进行操作
        L1.BackColor = Color.FromArgb(255, 255, 192)
    End Sub
    Public Sub 离开修改背景(sender As Object, e As EventArgs)
        Dim L1 As Label = CType(sender, Label) '获取当前操作的控件对象，只有这样才能对该控件进行操作
        L1.BackColor = Control.DefaultBackColor
    End Sub
    Public Sub 按下修改背景(sender As Object, e As EventArgs)
        Dim L1 As Label = CType(sender, Label) '获取当前操作的控件对象，只有这样才能对该控件进行操作
        L1.BackColor = Color.FromArgb(128, 255, 128)
    End Sub
    Public Sub 弹起修改背景(sender As Object, e As EventArgs)
        Dim L1 As Label = CType(sender, Label) '获取当前操作的控件对象，只有这样才能对该控件进行操作
        L1.BackColor = Color.FromArgb(255, 255, 192)
    End Sub
#End Region

    '读
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Int32, ByVal lpFileName As String) As Int32


    '写
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Int32


    Public Function GetINI(ByVal Section As String, ByVal AppName As String, ByVal lpDefault As String, ByVal FileName As String) As String

        Dim Str As String = String.Empty
        Str = LSet(Str, 1024)

        GetPrivateProfileString(Section, AppName, lpDefault, Str, Len(Str), FileName)
        '                 部分，  应用名称，IP默认，          ，     ，文件名
        Return Microsoft.VisualBasic.Left(Str, InStr(Str, Chr(0)) - 1)

    End Function


    Public Function WriteINI(ByVal Section As String, ByVal AppName As String, ByVal lpDefault As String, ByVal FileName As String) As Long

        WriteINI = WritePrivateProfileString(Section, AppName, lpDefault, FileName)

        'WriteINI ("节点名","参数名","写入内容","打开路径")
    End Function
End Module
