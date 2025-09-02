Imports System.Data.SQLite

Public Class ConnectionManager
    Private _connectionString As String

    Public Sub New(connectionString As String)
        _connectionString = connectionString
    End Sub
    Public Function GetConnString() As String
        Return _connectionString
    End Function
    Public Function GetConnection() As SQLiteConnection
        Dim connection As New SQLiteConnection(_connectionString)
        If connection.State <> ConnectionState.Open Then
            connection.Open()
        End If
        Return connection
    End Function
End Class



