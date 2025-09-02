Imports System.Data.OleDb
Imports System.IO

Public Class AccessHelper
    ''' <summary>
    ''' 数据库连接字符串
    ''' </summary>
    Private Shared connString As String = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\DataBase\AppData.mdb", Directory.GetCurrentDirectory())
    Private Shared com As OleDb.OleDbCommand
    Private Shared reader As OleDb.OleDbDataReader
    Private Shared adapter As OleDb.OleDbDataAdapter
    Private Shared conn As OleDb.OleDbConnection
    Public Shared ReadOnly Property NewConn() As OleDbConnection
        Get
            Dim connectionString As String
            'connectionString = System.Configuration.ConfigurationSettings.GetConfig("Supermarket")
            '连接2010数据库
            'connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=F:\GCU.accdb"
            '连接03-07数据库
            connectionString = connString

            '应该在这里先判断conn是否为Nothing
            If conn Is Nothing Then
                conn = New OleDb.OleDbConnection(connectionString)
            End If
            If conn.State <> ConnectionState.Open Then
                conn.Open()
            End If
            Return conn
        End Get
    End Property
    Public Shared Function GetDataTable(ByVal sql As String) As DataTable
        Dim dataset As DataSet
        dataset = New DataSet()
        com = New OleDb.OleDbCommand(sql, NewConn)
        adapter = New OleDbDataAdapter(com)
        adapter.Fill(dataset)
        Return dataset.Tables(0)
    End Function
End Class
