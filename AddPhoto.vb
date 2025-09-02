Imports System.Data.Entity.ModelConfiguration.Configuration
Imports System.Data.SqlClient
Imports System.Data.SQLite
Imports System.IO
Imports System.Windows.Forms.VisualStyles
Imports AxWMPLib
Imports System.Drawing.ContentAlignment

Public Class AddPhoto
    Dim lvNames As New ListView
    Public Property Label1 As New Label()
    Private New1 As NewName
    Public Property SFileName As String
    Dim picBox As New PictureBox
    Private menuStrip As New MenuStrip()
    Dim Tposition As New TextBox()
    Dim NameCount As Integer
    Dim NameArray As New List(Of String)
    Dim WithEvents btnAdd As New System.Windows.Forms.Button()
    Dim WithEvents btnNewPerson As New System.Windows.Forms.Button()
    Dim WithEvents btnNext As New Button()
    Dim WithEvents btnPrior As New Button
    Dim WithEvents btnDelete As New Button
    Dim WithEvents btnSave As New Button
    Dim WithEvents btnCopy As New Button

    Dim WithEvents btnSavePhoto As New Button
    Dim WithEvents btnFixOrient As New Button() With {
                    .Text = "Fixo",
                    .BackColor = Color.LightBlue,
                    .ForeColor = Color.DarkBlue,
                    .Font = New Font("Arial", 12, FontStyle.Bold),
                    .Size = New Size(220, 40),
                    .Location = New Point(410, 1540),
                    .Visible = True,
                    .AutoSize = True
                    }

    Dim WithEvents BtnRestart As New Button()
    Dim cbNamesOnFile As New System.Windows.Forms.ComboBox()
    Dim dt As New DataTable()
    Dim WithEvents txtRelation As New TextBox() With {
            .Location = New Point(20, 1120),
            .Size = New Size(800, 400),
            .Font = New Font("Arial", 12, FontStyle.Regular),
            .Multiline = True,
            .BorderStyle = BorderStyle.None,
            .Visible = False,
                    .AutoSize = True
}
    Dim lblPosition As New Label()
    Dim Manager As New ConnectionManager(GetConnectionString)
    Dim connection As New SQLiteConnection
    Dim cnt As Integer = 1
    Dim mfilename As String
    Dim MemberID As String
    Dim TypeI As Integer
    Dim state As Integer
    Dim Dfilename As String
    Dim FileDirectory As String
    Dim lbldimen As New Label()
    Dim Mheight As Integer
    Dim Mwidth As Integer
    Dim Playtime As Integer
    Dim screenheight As Integer
    Dim screenwidth As Integer
    Dim thumb As Byte()
    Dim txtFilename As New TextBox
    Public Property Etype As String
    Public Property EventID As Integer
    Dim itype As String
    Dim Formchanged As Boolean = False

    Dim lblDimension As New Label With {
                            .Font = New Font(.Font.FontFamily, 12),
                            .Location = New Point(50, 150),
                            .AutoSize = True
                            }


    Dim lblRelate As New Label With {
                        .Font = New Font(.Font.FontFamily, 12),
                        .Text = "Relation to Morton Family ",
                        .Location = New Point(20, 1090),
                        .Visible = False,
                        .AutoSize = True
                      }
    Dim WithEvents TxtFullName As New TextBox() With {
            .Location = New Point(20, 1050),
            .Size = New Size(800, 30),
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .Multiline = False,
            .Visible = False,
                    .AutoSize = True}
    Dim lblFileName As New Label
    Dim TxtMonth As New TextBox()
    Dim txtYear As New TextBox()
    Dim lblDescription As New Label
    Dim txtDescription As New TextBox()
    Dim txtEvent As New TextBox()
    Dim txtEventDetails As New TextBox()
    Dim lblEvent As New Label()
    Dim ImageData As Image()
    Dim DDir As String
    Dim pl As New AxWindowsMediaPlayer()
    Dim lhp As New Panel()
    Dim rhp As New Panel()
    Dim lpw As Integer
    Dim lph As Integer
    Dim offset As Integer = 0 ' Start at the first record
    Dim TotalCount As Integer = 0

    Sub AddPhoto_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.WindowState = FormWindowState.Maximized
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.IsMdiContainer = True
        Dim screenWidth As Integer = CInt(Screen.PrimaryScreen.Bounds.Width)
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        Dim New1 As New NewName(SFileName, "AddPhoto")
        AddHandler New1.FormClosed, AddressOf SubformClosed

        Me.menuStrip = fmenus()
        Dim menuItemExit As New ToolStripMenuItem("Exit")
        AddHandler menuItemExit.Click, AddressOf MenuItemExit_Click
        ' Add the Exit item to the MenuStrip
        menuStrip.Items.Add(menuItemExit)
        Me.MainMenuStrip = menuStrip
        menuStrip.Items.RemoveAt(0)
        menuStrip.Items.Insert(0, menuItemExit)
        Me.Controls.Add(menuStrip)

        connection = Manager.GetConnection()
        Using connection = Manager.GetConnection()
            Using command As New SQLiteCommand("SELECT COUNT(*) FROM Unindexedfiles", connection)
                TotalCount = Convert.ToInt32(command.ExecuteScalar())
            End Using
        End Using

        With lhp
            .Left = 0
            .Top = 80
            .Width = screenWidth \ 2
            .Height = screenHeight - 80
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
            .AutoScroll = True
        End With
        Me.Controls.Add(lhp)

        With rhp
            .Left = lhp.Right ' ensures it starts right after lhp
            .Top = 80
            .Width = screenWidth - lhp.Width
            .Height = screenHeight - 80
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
            .AutoScroll = True
        End With
        Me.Controls.Add(rhp)

        lpw = lhp.Width
        lph = lhp.Height
        Me.Controls.Add(rhp)

        DDir = GetDefaultDir()

        Dim title As Label = New Label() With {
            .Text = "Family Album",
            .Font = New Font("segoe", 40),
            .Size = New Size(screenWidth, 60),
            .Location = New Point(0, 20),
            .TextAlign = CType(ContentAlignment.Center, Drawing.ContentAlignment),
            .AutoSize = False
        }


        Dim Rpont As Integer = CInt(lpw * 0.55 + 50)
        With btnAdd
            .Text = "Add Names"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(200, 40)
            .Location = New Point(CInt(Rpont), 300)
        End With
        lhp.Controls.Add(btnAdd)

        With btnNewPerson
            .Text = "Add Name not in List"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(200, 40)
            .Location = New Point(Rpont, 340)
        End With
        lhp.Controls.Add(btnNewPerson)

        With btnFixOrient
            .Text = "Fix Orientation"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(200, 40)
            .Location = New Point(Rpont, 380)

            .Visible = True
        End With


        With btnNext
            .Text = "Next Image"
            .Location = New Point(Rpont, 420)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(200, 40)
        End With

        With btnPrior
            .Text = "Prior Image"
            .Location = New Point(Rpont, 460)
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(200, 40)
            .Enabled = False
        End With

        With btnSavePhoto
            .Text = "Save"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(200, 40)
            .Location = New Point(Rpont, 500)
            .Visible = True
        End With
        With btnDelete
            .Text = "Delete"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(200, 40)
            .Location = New Point(Rpont, 620)
            .Visible = True
        End With
        mfilename = ""


        Dim lblPeoplelist As New Label With {
                        .Font = New Font(.Font.FontFamily, 12),
                        .Text = "People In Picture",
                        .Location = New Point(50, 200),
                        .Width = 100,
                        .Height = 30,
                        .AutoSize = True
                }

        ' Initialize the lvNames control
        With lvNames
            .Size = New Drawing.Size(CInt(lpw * 0.55), 450)
            .Location = New Drawing.Point(30, 235)
            .View = View.Details
            .FullRowSelect = True
            .Font = New Font("Arial", 12, FontStyle.Regular)
        End With

        lvNames.Columns.Add("FullName", CInt(lvNames.Width / 2 - 3), HorizontalAlignment.Left)
        lvNames.Columns.Add("Relation", CInt(lvNames.Width / 2 - 3), HorizontalAlignment.Left)

        cbNamesOnFile.Location = New Point(30, 690)
        cbNamesOnFile.Size = New Size(CInt(lpw * 0.55), 690)
        cbNamesOnFile.Font = New Font(cbNamesOnFile.Font.FontFamily, 12)
        dt.Columns.Add("Names", GetType(String))
        dt.Columns.Add("Relation", GetType(String))
        With Tposition
            .Location = New Point(CInt(lpw * 0.55) + 310, 690)
            .Size = New Size(50, 40)
            .Visible = False
            .TextAlign = HorizontalAlignment.Left
            .Text = "1"
            .Font = New Font("Segoe UI", 12, FontStyle.Regular)
        End With
        With lblPosition
            .Location = New Point(CInt(lpw * 0.55) + 100, 690)
            .Size = New Size(200, 40)
            .Text = "Position of new Person"
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Visible = False
        End With

        Dim lblMonth As New Label With {
                            .Font = New Font(.Font.FontFamily, 12),
                            .Text = "Month of Picture: ",
                            .Location = New Point(50, 40),
                            .Width = 100,
                            .Height = 30,
                            .AutoSize = True
                            }
        With TxtMonth
            .Font = New Font(.Font.FontFamily, 12)
            ' .Text = IIf(reader.IsDBNull(reader.GetOrdinal("PMonth")), "0", reader("PMonth").ToString())
            .Location = New Point(260, 40)
            .Width = 100
            .Height = 30
            .AutoSize = True
        End With

        Dim lblYear As New Label With {
                .Font = New Font(.Font.FontFamily, 12),
                .Text = "Year of Picture:",
                .Location = New Point(50, 80),
                .Width = 200
                          }

        With txtYear
            .Font = New Font(.Font.FontFamily, 12)
            '               .Text = IIf(reader.IsDBNull(reader.GetOrdinal("Pyear")), "0", reader("Pyear").ToString())
            .Location = New Point(260, 80)
            .Size = New Size(100, 30)
        End With

        Dim lblFilename As New Label With {
                .Font = New Font(.Font.FontFamily, 12),
                .Text = "File name & Location",
                .Location = New Point(50, 120),
                .Width = 200
                          }



        With txtFilename
            .Font = New Font(.Font.FontFamily, 12)
            '               .Text = Sfilename
            .Location = New Point(260, 120)
            .Size = New Size(600, 40)
        End With


        Dim LABEL1 As New Label()


        AddHandler lvNames.MouseUp, AddressOf DeletePerson

        Me.Controls.Add(title)
        lhp.Controls.Add(lblPeoplelist)
        lhp.Controls.Add(lvNames)
        lhp.Controls.Add(lblMonth)
        lhp.Controls.Add(lblYear)
        lhp.Controls.Add(lblDimension)
        lhp.Controls.Add(txtRelation)
        lhp.Controls.Add(lblRelate)
        lhp.Controls.Add(txtFilename)
        lhp.Controls.Add(lblFilename)

        lhp.Controls.Add(lblPosition)
        lhp.Controls.Add(Tposition)
        rhp.Controls.Add(txtEvent)
        rhp.Controls.Add(lblEvent)
        rhp.Controls.Add(txtEventDetails)
        lhp.Controls.Add(TxtMonth)
        lhp.Controls.Add(txtYear)
        lhp.Controls.Add(btnNext)
        lhp.Controls.Add(btnFixOrient)
        lhp.Controls.Add(btnSavePhoto)
        lhp.Controls.Add(btnDelete)
        lhp.Controls.Add(btnPrior)
        rhp.Controls.Add(lblDescription)
        rhp.Controls.Add(txtDescription)
        If itype = "1" Then
            rhp.Controls.Add(picBox)
        Else
            rhp.Controls.Add(BtnRestart)
        End If
        If Not mfilename = "" Then
            '    PlayVideo(mfilename)
        End If
        AddHandler BtnRestart.Click, AddressOf btnRestart_click
        AddHandler btnFixOrient.Click, AddressOf btnFixOrient_click
        AddHandler btnNext.Click, AddressOf btnNext_click
        AddHandler btnSavePhoto.Click, AddressOf btnSavePhoto_click
        AddHandler btnDelete.Click, AddressOf btnDelete_click
        AddHandler btnPrior.Click, AddressOf btnPrior_click
        AddHandler btnAdd.Click, AddressOf btnAdd_click
        Dim EventName As String = ""
        Dim EventDetail As String = ""
        connection = Manager.GetConnection()
        Using connection
            Dim command1 As New SQLiteCommand("select neName,neRelation from NameEvent where ID = @eventid", connection)
            command1.Parameters.AddWithValue("@eventid", EventID)

            Using reader1 As SQLiteDataReader = command1.ExecuteReader()
                While reader1.Read()
                    EventName = reader1("neName").ToString
                    txtEvent.Visible = True
                    If Trim(reader1("neRelation").ToString) = "NULL" Then
                        txtEventDetails.Visible = False
                    Else
                        txtEventDetails.Text = reader1("neRelation").ToString
                        txtEventDetails.Visible = True
                    End If
                End While
            End Using
        End Using
        With lblDescription
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(CInt(lpw / 2 - 400), CInt(lph / 2) + 50)
            .AutoSize = True
            .Visible = True
            .Size = New Size(40, 80)
            .Text = "Description:"
        End With
        With txtDescription
            .Location = New Point()
            .Visible = True
            .Width = 750
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 60
            .Location = New Point(CInt(lpw / 2 - 300), CInt(lph / 2) + 20)

            .Multiline = True
            .AutoSize = True
            .BorderStyle = BorderStyle.FixedSingle
            .Text = ""
        End With


        With lblEvent
            .Height = 40
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Location = New Point(CInt(lpw / 2 - 400), CInt(lph / 2) + 100)
            .AutoSize = True
            .Visible = False
            .Size = New Size(40, 80)
            .Text = "Event:"
        End With
        With txtEvent
            .Location = New Point()
            .Visible = False
            .Width = 750
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 40
            .Location = New Point(CInt(lpw / 2 - 300), CInt(lph / 2) + 100)

            .ReadOnly = True
            .AutoSize = True
            .BorderStyle = BorderStyle.None
            .Text = EventName
        End With
        With txtEventDetails
            .Location = New Point(CInt(lpw / 2 - 300), CInt(lph / 2 + 150))
            .Width = 750
            .Font = New Font("Arial", 12, FontStyle.Regular)
            .Height = 100
            '.AutoSize = True
            .BorderStyle = BorderStyle.None
            .ReadOnly = True
            .Multiline = True
            .Visible = False
        End With


        ProcessNextImage()


    End Sub

    Private Sub ProcessNextImage()
        DDir = GetDefaultDir()

        Dim qryPhotos As String = $"SELECT * FROM Unindexedfiles WHERE uiStatus = 'N' ORDER BY uiFilename LIMIT 1 OFFSET {offset}"
        connection = Manager.GetConnection()
        Using command As New SQLiteCommand(qryPhotos, connection)
            Using reader As SQLiteDataReader = command.ExecuteReader()
                If reader.Read() Then
                    itype = reader("uiType").ToString
                    Dfilename = reader("uiFilename").ToString
                    Dim imageData As Byte() = TryCast(reader("uiThumb"), Byte())
                    If imageData IsNot Nothing AndAlso imageData.Length > 0 Then
                        Using ms As New MemoryStream(imageData)
                            'yourImage.Save(ms, Imaging.ImageFormat.Png) ' Or use .Jpeg, .Bmp, etc.
                            thumb = ms.ToArray()
                        End Using
                    End If

                    FileDirectory = reader("uiDirectory").ToString
                    SFileName = Dfilename.Remove(0, DDir.Length)
                    txtFilename.Text = SFileName
                    Playtime = CInt(reader("uiVtime"))
                    Mwidth = CInt(reader("uiWidth"))
                    Mheight = CInt(reader("uiHeight"))
                Else
                    MessageBox.Show("All Images Processed")
                End If
                reader.Close()
                reader.Dispose()
                txtDescription.Text = ""
            End Using
            If itype = "1" Then
                pl.Visible = False
                BtnRestart.Visible = False
                picBox.Visible = True
                With picBox
                    .SizeMode = PictureBoxSizeMode.Zoom
                    .Width = lpw
                    .Location = New Point(lhp.Width - picBox.Width, 50)
                    .Height = CInt(0.95 * lph / 2)
                    .Visible = True
                    .BorderStyle = BorderStyle.FixedSingle
                End With
                rhp.Controls.Add(picBox)
                Dim img As Image
                Try
                    Using fs As New FileStream(Dfilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        img = Image.FromStream(fs)
                    End Using
                    picBox.Image = img
                    lblDimension.Text = $"Width: {Mwidth} pixels" & vbCrLf & $"Height: {Mheight} pixels"
                Catch
                    DeleteUnindexedFile(Dfilename)
                    Exit Sub
                End Try

                Dim dateInfo = GetPhotoMonthAndYear(Dfilename)
                If dateInfo.Item1 > 0 AndAlso dateInfo.Item2 > 0 Then
                    TxtMonth.Text = CStr(dateInfo.Item1)
                    txtYear.Text = CStr(dateInfo.Item2)
                Else
                    TxtMonth.Text = "0"
                    txtYear.Text = "0"
                End If
                LoadMetadataToUI(Dfilename)
                If txtEvent.Text <> "" Then
                    txtEvent.Visible = True
                    txtEventDetails.Visible = True
                    lblEvent.Visible = True
                End If
            ElseIf itype = "2" Then
                picBox.Visible = False

                With pl
                    .Width = lpw
                    .Height = (lph \ 2)
                    .Dock = DockStyle.Top
                    .Visible = True
                End With

                With BtnRestart
                    .Text = "Restart"
                    .BackColor = Color.LightBlue
                    .ForeColor = Color.DarkBlue
                    .Font = New Font("Arial", 12, FontStyle.Bold)
                    .Size = New Size(220, 40)
                    .Location = New Point(lpw \ 2 - 110, lph \ 2)
                    .Visible = False
                    .AutoSize = True
                End With


                lbldimen.Text = $"Width: {Mwidth} pixels" & vbCrLf & vbCrLf & $"Height: {Mheight} pixels"
                BtnRestart.Visible = True
                rhp.Controls.Add(BtnRestart)
                rhp.Controls.Add(pl)
                Dim t As New Timer()
                AddHandler t.Tick, Sub()
                                       t.Stop()
                                       Playvideo(Dfilename)
                                   End Sub
                t.Interval = 100 ' Small delay to let the control fully initialize
                t.Start()
                Playvideo(DDir & SFileName)
                Dim eventName As String = ""
                Dim eventDetails As String = ""
            Else


            End If

        End Using
    End Sub
    Private Sub FillListview()
        lvNames.Items.Clear()
        Dim qryfixname As String = "select ID,neName,neRelation  from NameEvent where ID= @Name "
        If NameArray Is Nothing Then Exit Sub
        connection = Manager.GetConnection()

        For I = 0 To NameArray.Count - 1
            Try
                If Not String.IsNullOrWhiteSpace(NameArray(I)) Then
                    Dim command1 As New SQLiteCommand(qryfixname, connection)
                    command1.Parameters.AddWithValue("@Name", NameArray(I))

                    Dim reader1 As SQLiteDataReader = command1.ExecuteReader()
                    reader1.Read()
                    Dim item1 As New ListViewItem(reader1("neName").ToString)
                    item1.SubItems.Add(reader1("neRelation").ToString)
                    lvNames.Items.Add(item1)
                    reader1.Close()
                End If
            Catch ex As Exception
                'MessageBox.Show(ex.Message)
            End Try
            Tposition.Text = CStr(NameArray.Count)
        Next


        connection.Close()
    End Sub
    Private Sub btnAdd_click(sender As Object, e As EventArgs)
        Tposition.Visible = True

        cbNamesOnFile.Visible = True
        'cbNamesOnFile.DroppedDown = True
        lblPosition.Visible = True
        FillNamesAvail()
        cbNamesOnFile.Focus()
    End Sub
    Private Sub FillNamesAvail()

        dt = FillNames()
        RemoveHandler cbNamesOnFile.SelectionChangeCommitted, AddressOf NameCombo_SelectionChangeCommitted

        cbNamesOnFile.DataSource = dt
        cbNamesOnFile.DisplayMember = "Names"
        cbNamesOnFile.ValueMember = "ID"
        cbNamesOnFile.Visible = True

        lhp.Controls.Add(cbNamesOnFile)
        lhp.Controls.Add(lblPosition)
        lhp.Controls.Add(Tposition)
        If Not IsNumeric(Tposition.Text) Then Tposition.Text = "1"
        cbNamesOnFile.SelectedIndex = -1
        AddHandler cbNamesOnFile.SelectionChangeCommitted, AddressOf NameCombo_SelectionChangeCommitted

    End Sub
    Private Sub NameCombo_SelectionChangeCommitted(sender As Object, e As EventArgs)

        ' Cast the sender to a ComboBox
        Dim comboBox As System.Windows.Forms.ComboBox = CType(sender, System.Windows.Forms.ComboBox)
        Dim selectedRow As DataRowView = CType(comboBox.SelectedItem, DataRowView)
        If selectedRow Is Nothing Then Exit Sub
        Dim selectedPerson As String = selectedRow("ID").ToString()
        Dim insertIndex As Integer
        If comboBox.SelectedItem IsNot Nothing Then
            Try
                insertIndex = CInt(Tposition.Text)

            Catch
                MessageBox.Show("Position has to be an Integer  ")
                Exit Sub
            End Try
            NameArray = ModifyPeopleList(String.Join(",", NameArray), insertIndex, 1, selectedPerson).Split(","c).ToList
            FillListview()


            'cbNamesOnFile.Visible = False
            cbNamesOnFile.SelectedIndex = -1
            'connection.Close()
            Tposition.Text = CStr(NameArray.Count + 1)
        Else
            MessageBox.Show("No selection made.")
        End If
    End Sub

    Private Sub btnNewPerson_Click(sender As Object, e As EventArgs)
        New1 = New NewName(SFileName, "AddPhoto")
        AddHandler New1.FormClosed, AddressOf SubformClosed
        New1.TopLevel = False
        lhp.Controls.Add(New1)
        New1.BringToFront()
        New1.Show()
    End Sub

    Private Sub btnNext_click(sender As Object, e As EventArgs)
        offset += 1
        If offset > TotalCount - 1 Then
            btnNext.Enabled = False
            offset -= 1
        End If
        If offset > 0 Then btnPrior.Enabled = True
        lvNames.Items.Clear()

        ProcessNextImage()
    End Sub

    Private Sub btnPrior_click(sender As Object, e As EventArgs)
        offset -= 1
        If offset < 0 Then
            btnPrior.Enabled = False
            offset = 0
        End If
        If offset < TotalCount - 1 Then
            btnNext.Enabled = True
        End If
        lvNames.Items.Clear()

        ProcessNextImage()
    End Sub
    Private Sub btnSavePhoto_click(sender As Object, e As EventArgs)
        Dim plist As String = createPeoplelist()
        Dim connection As SQLiteConnection = Manager.GetConnection()


        Using connection


            Dim transaction As SQLiteTransaction = connection.BeginTransaction()

            Try
                Dim command As New SQLiteCommand() With {.Connection = connection, .Transaction = transaction}

                ' Create Picture Record
                InsertPictureRecord(command, plist)
                ' 🔎 Save thumbnail from DB for visual verification

                InsertNamePhotoRecords(command, plist)

                ' Handle Event Type Record
                If Etype <> "No" Then InsertEventRecord(command)

                ' Remove from unindexedFiles
                command.CommandText = "DELETE FROM unindexedFiles WHERE uiFilename=@Filename"
                command.Parameters("@Filename").Value = DDir & SFileName
                command.ExecuteNonQuery()

                transaction.Commit()
            Catch ex As Exception
                transaction.Rollback()
                MessageBox.Show("Save Date Transaction rolled back. Error: " & ex.Message)
            End Try
        End Using

        Dim jsonMetadata As String = BuildJsonFromControls(txtEvent, txtEventDetails, TxtMonth, txtYear, txtDescription, lvNames)
        WriteJsonMetadataToMediaFile(DDir & SFileName, jsonMetadata)

        ' Cleanup UI
        ResetUI()
    End Sub

    ' Encapsulated SQL Methods
    Private Sub InsertPictureRecord(command As SQLiteCommand, plist As String)
        command.CommandText = "INSERT INTO Pictures ([Pfilename], [PfileDirectory], [PDescription], [PHeight], [PWidth], 
        [PPeopleList], [PMonth], [PYear], [PSoundFile], [PDateEntered], [PType], [PLastModifiedDate], 
        [PReviewed], [PTime], [PThumbnail], [PNameCount]) 
        VALUES (@Pfilename, @PfileDirectory,@PDescription, @PHeight, @PWidth, @PPeopleList, @PMonth, @PYear, 
        @PSoundFile, @PDateEntered, @PType, @PLastModifiedDate, @PReviewed, @PTime, @PThumbnail, @PNameCount)"

        With command.Parameters
            .AddWithValue("@Pfilename", SFileName)
            .AddWithValue("@PfileDirectory", FileDirectory)
            .AddWithValue("@PDescription", txtDescription.Text)
            .AddWithValue("@PHeight", Mheight)
            .AddWithValue("@PWidth", Mwidth)
            .AddWithValue("@PPeopleList", plist)
            .AddWithValue("@PMonth", TxtMonth.Text)
            .AddWithValue("@PYear", txtYear.Text)
            .AddWithValue("@PSoundFile", "")
            .AddWithValue("@PDateEntered", Today)
            .AddWithValue("@PType", itype)
            .AddWithValue("@PLastModifiedDate", Today)
            .AddWithValue("@PReviewed", 0)
            .AddWithValue("@PTime", Playtime)
            .Add(New SQLiteParameter("@PThumbnail", DbType.Binary) With {.Value = thumb})
            .AddWithValue("@PNameCount", lvNames.Items.Count)
        End With
        command.ExecuteNonQuery()
    End Sub

    Private Sub InsertNamePhotoRecords(command As SQLiteCommand, pList As String)
        Dim pitems As List(Of String) = pList.Split(","c).ToList()
        command.CommandText = "INSERT INTO NamePhoto (npID, npFileName) VALUES (@ID, @Filename)"
        command.Parameters.AddWithValue("@Filename", SFileName)
        command.Parameters.Add("@ID", DbType.Int32)

        For Each pitem As String In pitems
            command.Parameters("@ID").Value = CInt(pitem)
            command.ExecuteNonQuery()
        Next
    End Sub

    Private Sub InsertEventRecord(command As SQLiteCommand)
        command.CommandText = "INSERT INTO NamePhoto (npID, npFileName) VALUES (@ID, @Filename)"
        command.Parameters("@ID").Value = EventID
        command.ExecuteNonQuery()
    End Sub

    Private Sub DeleteUnindexedFile(Filename As String)
        Dim qry As String = "DELETE FROM unindexedFiles WHERE uiFilename=@Filename"
        Using conn As SQLiteConnection = Manager.GetConnection()
            Dim command As New SQLiteCommand(qry, conn)
            command.Parameters.AddWithValue("@Filename", Filename)
            command.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub ResetUI()
        NameArray.Clear()
        lvNames.Items.Clear()
        Tposition.Text = "1"
        ProcessNextImage()
    End Sub

    Private Function createPeoplelist() As String
        Dim plist As String = ""
        Dim id As String
        If lvNames.Items.Count = 0 Then
            plist = "1"
        Else
            For i = 0 To lvNames.Items.Count - 1
                id = GetMemberID(lvNames.Items(i).Text, lvNames.Items(i).SubItems(1).Text)

                If plist = "" Then
                    plist = id
                Else
                    plist = plist & "," & id
                End If
            Next
        End If

        Return plist
    End Function
    Private Sub MenuItemExit_Click(sender As Object, e As EventArgs)
        connection = Manager.GetConnection()
        Dim re As Integer
        Using connection
            Dim d As Date = Today
            Try
                Dim command As New SQLiteCommand()
                command.Connection = connection
                command.CommandText = "Update Unindexedfiles set uiStatus ='N' "
                re = command.ExecuteNonQuery()
            Catch ex As Exception
                MessageBox.Show("Status not saved.  Err: " & ex.Message)
            End Try
        End Using
        Me.Close()
    End Sub


    Private Sub Playvideo(videopath As String)
        ' Ensure the control is created before modifying properties
        EnsurePlayerReady()
        If Not pl.Created Then
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
        If pl IsNot Nothing AndAlso pl.Created AndAlso pl.IsHandleCreated Then
            Try
                pl.URL = videopath
                pl.uiMode = "none"
                pl.Ctlcontrols.play()
            Catch ex As Exception
                MessageBox.Show("Playback error: " & ex.Message)
            End Try
        Else
            MessageBox.Show("Player not ready: handle or control not created.")
        End If
    End Sub
    Private Sub EnsurePlayerReady()
        If pl Is Nothing OrElse Not pl.Created OrElse Not pl.IsHandleCreated Then
            ' If already exists, clean up
            If pl IsNot Nothing Then
                lhp.Controls.Remove(pl)
                pl.Dispose()
            End If

            ' Recreate and set up
            pl = New AxWMPLib.AxWindowsMediaPlayer()
            pl.CreateControl()
            pl.uiMode = "none"
            pl.stretchToFit = True
            pl.Visible = True

            ' Set size/position
            Dim vidHeight As Integer = CInt(0.95 * (screenheight / 2))
            Dim vidWidth As Integer = CInt(vidHeight / 1.6)
            pl.Size = New Size(vidWidth, vidHeight)
            pl.Location = New Point(lhp.Width - vidWidth - 50, 250)

            rhp.Controls.Add(pl)
            pl.BringToFront()
        End If
    End Sub

    Private Sub btnDelete_click(sender As Object, e As EventArgs)
        Dim Response As Integer = MsgBox("Are you sure you want to delete this picture/Video., Deleteing a picture or video cannot be undone.", vbYesNo, "Confirmation")

        If Response = vbNo Then
            Exit Sub
        Else

            Try
                connection = Manager.GetConnection()
                Using connection
                    ' Begin a transaction
                    Dim transaction As SQLiteTransaction = connection.BeginTransaction()

                    Try
                        Dim command As New SQLiteCommand()
                        command.Connection = connection
                        command.Transaction = transaction


                        System.Threading.Thread.Sleep(100)
                        command.CommandText = "Delete from Pictures  WHERE PfileName = @filename"
                        command.Parameters.AddWithValue("@filename", SFileName)
                        command.ExecuteNonQuery()

                        ' Delete all event and people records
                        command.CommandText = "Delete from NamePhoto where  npfilename = @filename"
                        command.ExecuteNonQuery()
                        ' Delete Pictures record
                        If picBox.Image IsNot Nothing Then
                            picBox.Image.Dispose()
                        End If
                        picBox.Image = Nothing
                        'GC.Collect()
                        'GC.WaitForPendingFinalizers()
                        Dim filePath As String = GetDefaultDir() & SFileName ' Delete the image
                        If System.IO.File.Exists(filePath) Then
                            System.IO.File.Delete(filePath)
                        End If

                        command.CommandText = "Delete from Unindexedfiles where uiFilename = @filename"
                        command.Parameters("@filename").Value = SFileName
                        command.ExecuteNonQuery()

                        ' Commit the transaction if all updates succeed
                        transaction.Commit()
                        Console.WriteLine("Transaction committed successfully.")
                    Catch ex As Exception
                        ' Rollback the transaction if an error occurs
                        transaction.Rollback()
                        MessageBox.Show("Delete Name Transaction rolled back. Error: " & ex.Message)
                    End Try


                End Using
                TotalCount -= 1
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub

    Public Sub LoadMetadataToUI(filePath As String)
        Dim metadata As ParsedMetadata = ReadJsonFromMediaFile(filePath)

        If metadata IsNot Nothing Then
            txtEvent.Text = metadata.EventName
            If String.IsNullOrEmpty(metadata.EventName) Then
                Etype = "No"
            Else
                EventID = CInt(GeteventID(metadata.EventName, metadata.EventDetails))
                Etype = "Yes"
            End If
            txtEventDetails.Text = metadata.EventDetails
            TxtMonth.Text = metadata.imMonth.ToString
            txtYear.Text = metadata.imYear.ToString
            txtDescription.Text = metadata.Description
            lvNames.Items.Clear()
            For Each person In metadata.People
                Dim item As New ListViewItem(person.Name)
                item.SubItems.Add(person.Relationship)
                lvNames.Items.Add(item)
            Next
        Else
            ' Do nothing — metadata is absent or unreadable
        End If
    End Sub
    Private Sub btnRestart_click(sender As Object, e As EventArgs)
        If pl.Ctlcontrols.currentPosition = 0 Then
            pl.Ctlcontrols.play()
        End If
    End Sub



    Private Sub btnFixOrient_click(sender As Object, e As EventArgs)
        FixImageOrientation(Dfilename)
        picBox.Image = Image.FromFile(Dfilename)
        Updatethumb(Dfilename)
    End Sub
    Private Sub SubformClosed(sender As Object, e As FormClosedEventArgs)

        Dim selectedPerson As String = Label1.Text
        If Len(selectedPerson) > 3 Then
            Dim i As Integer = Label1.Text.IndexOf("|")
            selectedPerson = Mid(Label1.Text, 1, i)
            Dim insertindex = Mid(Label1.Text, i + 2)
            If NameArray.Count = 0 Then
                NameArray.Add(selectedPerson)
            Else
                NameArray = ModifyPeopleList(String.Join(",", NameArray), CInt(insertindex), 1, selectedPerson).Split(","c).ToList()
            End If
        End If
        FillListview()
    End Sub
    Private Sub lvNames_MouseUp(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Right Then
            Dim index As Integer = lvNames.HitTest(e.Location).Item.Index
            If index >= 0 Then
                lvNames.Items(index).Selected = True ' Highlights the item
                DeletePerson(sender, e)
            End If
        End If
    End Sub


    Private Sub DeletePerson(sender As Object, e As MouseEventArgs)
        Dim selectedname As String
        Dim response As Integer
        Dim newplist As String
        If lvNames.SelectedItems.Count > 0 Then
            Dim selectedItem As ListViewItem = lvNames.SelectedItems(0)
            response = MsgBox("Are you sure you want to delete - " & selectedItem.Text, vbYesNo, "Confirmation")
        End If
        If response = vbNo Then
            Exit Sub
        Else
            selectedname = GetMemberID(lvNames.SelectedItems(0).Text, "")
            newplist = DeleteAPerson(selectedname, SFileName, NameCount)
            NameArray.Remove(CStr(selectedname))
            Debug.WriteLine($"Attempting delete: [{SFileName}]")
            Debug.WriteLine($"Length: {SFileName.Length}")

            connection = Manager.GetConnection()
            Using connection
                Dim Command = New SQLiteCommand("Delete from NamePhoto where npID =@name and npFilename =@filename", connection)
                Command.Parameters.AddWithValue("@name", CInt(selectedname))
                Command.Parameters.AddWithValue("@filename", SFileName)
                Dim Result As Integer = Command.ExecuteNonQuery()
                Console.WriteLine(Result)
            End Using
        End If
        Formchanged = True
        FillListview()
    End Sub


End Class
