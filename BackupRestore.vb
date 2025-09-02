Imports System.Data.SQLite
Imports System.IO
Imports Microsoft.Win32

Public Module BackupRest
    'Private Sub BackupRestore_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    '    Dim Lbox As New Label With {
    '        .Text = "Select a directory for storing the database backsups",
    '        .Font = New Font("Arial", 24)
    '    },
    '    CenterControl(Lbox, 0)
    '    Me.Controls.Add(Lbox)
    '    Lbox.Show()
    '    GetBackupDirectroy()
    'End Sub
    Public Function GetBackupPath() As String
        ' Open the registry key
        Dim key As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\FamilyAlbum", writable:=False)

        If key IsNot Nothing Then
            Dim value As Object = key.GetValue("BackupPath", Nothing)
            key.Close()

            If value IsNot Nothing Then
                Return value.ToString()
            End If
        End If

        ' If key or value not found, fallback to prompt
        Return getpath()

    End Function


    Sub BackupSQLiteDatabase()

        Dim sourcePath As String = Path.Combine(GetDefaultDir, "FamilyAlbum.db")
        Dim sourceConn As New SQLiteConnection($"Data Source={sourcePath}; Version=3;")
        Dim BackupPath As String = GetBackupPath() & "FamAlbum" & DateTime.Now.ToString("MMddyyyy") & ".bak"

        Dim destConn As New SQLiteConnection("Data Source=" & BackupPath & "; Version=3;")

        Try
            sourceConn.Open()
            destConn.Open()

            ' Perform the backup from source to destination
            sourceConn.BackupDatabase(destConn, "main", "main", -1, Nothing, 0)

            MessageBox.Show("Backup completed successfully.")
        Catch ex As Exception
            MessageBox.Show("Backup failed: " & ex.Message)
        Finally
            sourceConn.Close()
            destConn.Close()
        End Try
    End Sub

    Sub RestoreSQLiteDatabase()
        Using ofd As New OpenFileDialog()
            ofd.Title = "Select Backup File"
            ofd.Filter = "FamilyAlbum(*.bak)|*.bak|All Files (*.*)|*.*"
            ofd.InitialDirectory = GetBackupPath()

            If ofd.ShowDialog() = DialogResult.OK Then
                Dim backupPath As String = ofd.FileName
                Dim restorePath As String = Path.Combine(GetDefaultDir, "FamilyAlbum.db")

                Dim backupConn As New SQLiteConnection($"Data Source={backupPath}; Version=3;")
                Dim restoreConn As New SQLiteConnection($"Data Source={restorePath}; Version=3;")

                Try
                    backupConn.Open()
                    restoreConn.Open()

                    backupConn.BackupDatabase(restoreConn, "main", "main", -1, Nothing, 0)

                    MessageBox.Show("Restore completed successfully to: " & restorePath)
                Catch ex As Exception
                    MessageBox.Show("Restore failed: " & ex.Message)
                Finally
                    backupConn.Close()
                    restoreConn.Close()
                End Try
            End If
        End Using
    End Sub


    Public Sub GetBackupDirectroy()
        Using fbd As New FolderBrowserDialog
            fbd.Description = "Select a folder to store database backups"
            fbd.ShowNewFolderButton = True

            If fbd.ShowDialog() = DialogResult.OK Then
                Dim selectedPath As String = fbd.SelectedPath
                SaveFilePathToRegistry(selectedPath)
            End If
        End Using

    End Sub
    Private Sub SaveFilePathToRegistry(filePath As String)
        Try
            ' Access the CurrentUser registry key and create a subkey
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey("Software\FamilyAlbum")
            key.SetValue("BackupPath", filePath)
            key.Close()
        Catch ex As Exception
            MessageBox.Show("Error saving to registry: " & ex.Message)
        End Try
    End Sub

    Public Function getpath() As String
        Using fbd As New FolderBrowserDialog
            fbd.Description = "Select a folder for Database backups"
            fbd.ShowNewFolderButton = True

            If fbd.ShowDialog() = DialogResult.OK Then
                Dim selectedPath As String = fbd.SelectedPath & "\"
                SaveFilePathToRegistry(selectedPath)
                Return selectedPath
            End If
        End Using

        Return Nothing ' If user cancels dialog
    End Function

End Module