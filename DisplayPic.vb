Public Class DisplayPic
    Public Property SFileName As String
    Private pictureBox As New PictureBox()
    Private menuStrip As New MenuStrip()
    Private Sub DisplayPic_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.WindowState = FormWindowState.Maximized
        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height

        ' pl.SetBounds(0, 0, 1200, 800)
        Dim playerPanel As New Panel With {
            .Location = New Point(screenWidth / 2 + 10, 100),
            .Visible = False,
            .Width = 0.95 * (screenWidth / 2),
            .Height = 0.95 * (screenHeight / 2)}
        Me.Controls.Add(playerPanel)
        playerPanel.CreateControl()

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
        Me.Controls.Add(pictureBox)
        Dim Ddir As String
        Ddir = GetDefaultDir()
        ' Load the image from file
        Dim filePath As String = Ddir & SFileName
        If IO.File.Exists(filePath) Then
            If CMovie(SFileName) = False Then
                Dim image As Image = Image.FromFile(filePath)
                pictureBox.Image = image
                pictureBox.Size = image.Size

                AdjustPictureBoxSize(pictureBox)
                CenterControl(pictureBox, 0)
                ' Set form size to be 1 inch larger on each side
                Dim dpi As Single = CreateGraphics().DpiX ' Assuming 96 DPI as standard
                Dim inchToPixels As Integer = CInt(dpi) ' Convert 1 inch to pixels

            Else
                PlayVideo(Ddir & SFileName)
            End If
        End If
    End Sub
    Private Sub MenuItemExit_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    Public Sub AdjustPictureBoxSize(Picturebox As Control)
        ' Get screen dimensions
        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height

        ' Check if PictureBox exceeds screen width or height
        If Picturebox.Width > screenWidth Then
            Picturebox.Width = screenWidth
        End If

        If Picturebox.Height > screenHeight Then
            Picturebox.Height = 0.95 * screenHeight
        End If

        ' Optional: Center the PictureBox within the screen
        Picturebox.Location = New Point((screenWidth - Picturebox.Width) \ 2, (screenHeight - Picturebox.Height) \ 2)
    End Sub
    Private Sub CenterControl(ctrl As Control, y As Integer)
        ' Calculate the position to center the control
        Dim x As Integer = (Me.ClientSize.Width - ctrl.Width) \ 2
        'Dim y As Integer = (Me.ClientSize.Height - ctrl.Height) \ 2

        ' Set the control's position
        ctrl.Location = New Point(x, y)
    End Sub
    Private Sub PlayVideo(filename As String)
        'pl = New AxWindowsMediaPlayer() With {
        '     .Location = New Point(2400 / 2 + 10, 100),
        '    .Visible = False,
        '    .Width = 0.95 * (2400 / 2),
        '    .Height = 0.95 * (1800 / 2)
        '}
        'Me.Controls.Add(pl)



        'pl.uiMode = "none"
        'pl.URL = filename
    End Sub


End Class