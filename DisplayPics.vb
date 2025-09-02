Imports System.Data.Entity.ModelConfiguration.Configuration
Imports System.Data.SQLite
Imports System.IO
Imports AxWMPLib

Public Class DisplayPics
    Public Property SFileName As String
    Private pictureBox As New PictureBox()
    Private menuStrip As New MenuStrip()
    Dim WithEvents BtnRestart As New Button()
    Dim pl As New AxWindowsMediaPlayer()
    Dim TypeI As Integer
    Dim Manager As New ConnectionManager(GetConnectionString)
    Dim mfilename As String
    Private Sub DisplayPics_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.WindowState = FormWindowState.Maximized
        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height

        Dim playerPanel As New Panel With {
            .Left = 0,
            .Top = 20,
        .Width = screenWidth,
        .Height = screenHeight - 20,
        .Anchor = AnchorStyles.Top Or AnchorStyles.Left,
        .AutoScroll = True}
        Me.Controls.Add(playerPanel)


        'pl = New AxWMPLib.AxWindowsMediaPlayer()
        playerPanel.Controls.Add(pl)  ' Add it before initializing

        pl.BeginInit()
        pl.Name = "pl"
        pl.uiMode = "none"
        pl.EndInit()

        With pl
            .Width = playerPanel.Width \ 2
            .Height = CInt(0.95 * (playerPanel.Height \ 2))
            .Location = New Point((playerPanel.Width - .Width) \ 2, 50)
            .stretchToFit = True
            .Visible = True
        End With

        Dim HT As Integer = 50 + pl.Height + 50
        With BtnRestart
            .Text = "Restart"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(220, 40)
            .Location = New Point(pl.Left + pl.Width \ 2 - 110, HT)
            .Visible = True
            .AutoSize = True
        End With
        AddHandler BtnRestart.Click, AddressOf btnRestart_click


        Me.menuStrip = fmenus()
        Dim menuItemExit As New ToolStripMenuItem("Exit")
        AddHandler menuItemExit.Click, AddressOf MenuItemExit_Click
        ' Add the Exit item to the MenuStrip
        menuStrip.Items.Add(menuItemExit)
        Me.MainMenuStrip = menuStrip
        menuStrip.Items.RemoveAt(0)
        menuStrip.Items.Insert(0, menuItemExit)
        Me.Controls.Add(menuStrip)


        pictureBox.SizeMode = PictureBoxSizeMode.Zoom

        pictureBox.BorderStyle = BorderStyle.Fixed3D
        'Me.Controls.Add(pictureBox)
        Dim Ddir As String
        Ddir = GetDefaultDir()
        ' Load the image from file
        Dim filePath As String = Ddir & SFileName
        If IO.File.Exists(filePath) Then
            Dim fileName As String = (SFileName)
            Dim connection As SQLiteConnection = Manager.GetConnection()
            Dim command As New SQLiteCommand("SELECT  PPeoplelist, PMonth, PYear,Pheight, Pwidth,PType FROM Pictures WHERE Pfilename = @name", connection)
            command.Parameters.AddWithValue("@name", fileName)
            Using reader As SQLiteDataReader = command.ExecuteReader()
                While reader.Read()
                    TypeI = CInt(reader("PType"))
                    If TypeI <> 1 Then
                        mfilename = Ddir & SFileName
                        playerPanel.Controls.Add(pl)
                        Application.DoEvents()
                        playerPanel.Controls.Add(BtnRestart)


                        Playvideo(Ddir & SFileName)
                    Else
                        If SFileName.Contains(Ddir) Then
                            SFileName = SFileName.Replace(Ddir, "")
                        End If
                        pl.Visible = False
                        ' Force a true decoupling from the file stream
                        Dim imgBytes As Byte() = File.ReadAllBytes(Ddir & SFileName)
                        Using ms As New MemoryStream(imgBytes)
                            Dim originalImg = Image.FromStream(ms)
                            Dim scaledImg = ResizeImageToFitScreen(originalImg)
                            pictureBox.Image = scaledImg
                        End Using
                        pictureBox.SizeMode = PictureBoxSizeMode.AutoSize
                        pictureBox.Location = New Point((screenWidth - pictureBox.Width) \ 2, (screenHeight - pictureBox.Height) \ 2)

                        playerPanel.Controls.Add(pictureBox)
                    End If
                End While
            End Using
        End If
    End Sub
    Private Sub MenuItemExit_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub
    Public Function ResizeImageToFitScreen(img As Image) As Image
        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height

        Dim scaleX As Double = screenWidth / img.Width
        Dim scaleY As Double = screenHeight / img.Height
        Dim scale As Double = Math.Min(scaleX, scaleY) * 0.92 ' Leave some padding

        Dim newWidth As Integer = CInt(img.Width * scale)
        Dim newHeight As Integer = CInt(img.Height * scale)

        Dim resizedImg As New Bitmap(newWidth, newHeight)
        Using g As Graphics = Graphics.FromImage(resizedImg)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(img, 0, 0, newWidth, newHeight)
        End Using

        Return resizedImg
    End Function


    Private Sub CenterControl(ByRef ctrl As Control)
        ' Calculate the position to center the control
        Dim x As Integer = (Me.ClientSize.Width - ctrl.Width) \ 2
        Dim y As Integer = (Me.ClientSize.Height - ctrl.Height) \ 2

        ' Set the control's position
        ctrl.Location = New Point(x, y)
    End Sub
    Private Sub Playvideo(videopath As String)
        ' Ensure the control is created before modifying properties
        If Not pl.IsHandleCreated Then
            pl.CreateControl()
        End If
        Dim processes = Process.GetProcessesByName("wmplayer") ' For classic Windows Media Player
        For Each proc As Process In processes
            Try
                proc.Kill()
                proc.WaitForExit()
                Console.WriteLine("Windows Media Player closed.")
            Catch ex As Exception
                Console.WriteLine("Error closing Windows Media Player: " & ex.Message)
            End Try
        Next
        ' Set video path and play
        pl.URL = videopath
        pl.uiMode = "none"
        pl.Ctlcontrols.play()


    End Sub
    Private Sub btnRestart_click(sender As Object, e As EventArgs)
        If pl.Ctlcontrols.currentPosition = 0 Then
            pl.Ctlcontrols.play()
        End If
    End Sub
End Class