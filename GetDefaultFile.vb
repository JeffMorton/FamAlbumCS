Imports Microsoft.Win32
Public Class GetDefaultFile

    Private Sub GetDefaultFile_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim Lbox As New Label With {
            .Text = "Select The Family Album Database",
            .Font = New Font("Arial", 24)
        }
        CenterControl(Lbox, 0)
        Me.Controls.Add(Lbox)
        Lbox.Show()
        FindDefaultDir()
    End Sub
    Private Sub FindDefaultDir()
        ' Create and configure an OpenFileDialog
        Dim openFileDialog As New OpenFileDialog With {
            .Title = "Select a File",
            .Filter = "All Files|*.*"
        }

        ' Show the dialog and check if the user selected a file
        If openFileDialog.ShowDialog() = DialogResult.OK Then
            Dim filePath As String = openFileDialog.FileName
            Dim X As Integer = filePath.IndexOf("FamilyAlbum.db")
            filePath = Mid(filePath, 1, X)

            '  Save the file path to the registry
            SaveFilePathToRegistry(filePath)

            ' Display confirmation
            Dim Strt As New Start()
            Strt.Show()
        End If
    End Sub
    Private Sub SaveFilePathToRegistry(filePath As String)
        Try
            ' Access the CurrentUser registry key and create a subkey
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey("Software\FamilyAlbum")
            key.SetValue("DefaultDir", filePath)
            key.Close()
        Catch ex As Exception
            MessageBox.Show("Error saving to registry: " & ex.Message)
        End Try
    End Sub
    Private Sub CenterControl(ctrl As Control, y As Integer)
        ' Calculate the position to center the control
        Dim x As Integer = (Me.ClientSize.Width - ctrl.Width) \ 2
        'Dim y As Integer = (Me.ClientSize.Height - ctrl.Height) \ 2

        ' Set the control's position
        ctrl.Location = New Point(x, y)
    End Sub

End Class
