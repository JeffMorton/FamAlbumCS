Imports System.Data.SQLite

Module fmmenus
    Private ReadOnly menuStrip As New MenuStrip()
    Dim Namesselected() As String = Nothing
    Dim Manager As New ConnectionManager(GetConnectionString())

    Public Function fmenus() As MenuStrip
        Dim mainmenustrip As New MenuStrip()

        ' Exit
        Dim menuItemExit As New ToolStripMenuItem("Exit")
        AddHandler menuItemExit.Click, AddressOf ExitApp
        mainmenustrip.Items.Add(menuItemExit)

        ' Select People
        Dim menuItemSelectPeople As New ToolStripMenuItem("Select People")
        AddHandler menuItemSelectPeople.Click, AddressOf MenuItemPeople_Click
        mainmenustrip.Items.Add(menuItemSelectPeople)

        ' Select Event
        Dim menuItemSelectEvent As New ToolStripMenuItem("Select Event")
        AddHandler menuItemSelectEvent.Click, AddressOf MenuItemEvent_Click
        mainmenustrip.Items.Add(menuItemSelectEvent)

        ' Pictures with no names
        Dim menuItemNoname As New ToolStripMenuItem("Pictures with no names listed")
        AddHandler menuItemNoname.Click, AddressOf MenuitemNoname_Click
        mainmenustrip.Items.Add(menuItemNoname)

        ' Add Pictures submenu
        Dim menuItemAdd As New ToolStripMenuItem("Add Images")
        Dim FindPhotos As New ToolStripMenuItem("Find Images in All Folder")
        'Dim FolderPhotos As New ToolStripMenuItem("Select Folder for New Images")
        Dim NewEvent As New ToolStripMenuItem("Event - New")
        Dim OldEvent As New ToolStripMenuItem("Event")
        Dim NoEvent As New ToolStripMenuItem("No Event")

        AddHandler FindPhotos.Click, AddressOf FindPhotos_click
        'AddHandler FolderPhotos.Click, AddressOf FindFilesinFolder_click
        AddHandler NewEvent.Click, AddressOf NewEvent_click
        AddHandler OldEvent.Click, AddressOf OldEvent_click
        AddHandler NoEvent.Click, AddressOf NoEvent_click
        'add folder folders if needed
        menuItemAdd.DropDownItems.AddRange({FindPhotos, NewEvent, OldEvent, NoEvent})
        mainmenustrip.Items.Add(menuItemAdd)

        ' Utilities
        Dim menuitemUtilities As New ToolStripMenuItem("Utilites")

        Dim EventManager As New ToolStripMenuItem("Event Manager")
        Dim Namemanager As New ToolStripMenuItem("Name Manager")
        Dim menuItemCheck As New ToolStripMenuItem("Check Integrity")
        Dim menuitemBackup As New ToolStripMenuItem("Backup")
        Dim menuitemRestore As New ToolStripMenuItem("Restore")

        AddHandler EventManager.Click, AddressOf EvManage_click
        AddHandler Namemanager.Click, AddressOf nmManage_click
        AddHandler menuItemCheck.Click, AddressOf MenuitemCheck_Click
        AddHandler menuitemBackup.Click, AddressOf menuitemBackup_click
        AddHandler menuitemRestore.Click, AddressOf menuitemRestore_click

        menuitemUtilities.DropDownItems.AddRange({EventManager, Namemanager, menuItemCheck, menuitemBackup, menuitemRestore})
        mainmenustrip.Items.Add(menuitemUtilities)

        ' Pending Images
        Dim pendingCount As Integer = GetPendingImageCount()
        Dim pendingMenuItem As New ToolStripMenuItem($"{pendingCount} images waiting to be added")
        mainmenustrip.Items.Add(pendingMenuItem)

        ' Set font
        For Each menuItem As ToolStripMenuItem In mainmenustrip.Items
            menuItem.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        Next

        Return mainmenustrip
    End Function

    Public Sub EvManage_click(sender As Object, e As EventArgs)
        Dim ev As New EventManagment()
        ev.Show()
    End Sub
    Public Sub nmManage_click(sender As Object, e As EventArgs)
        Dim ev As New NameEditor()
        ev.Show()
    End Sub

    Public Sub NewEvent_click(sender As Object, e As EventArgs)
        Dim ad As New Select_Event
        Dim Etype As String = "New"
        ad.Etype = Etype
        ad.Show()
    End Sub
    Public Sub OldEvent_click(sender As Object, e As EventArgs)
        Dim ad As New Select_Event
        Dim Etype As String = "Old"
        ad.Etype = Etype
        ad.Show()
    End Sub
    Public Sub NoEvent_click(sender As Object, e As EventArgs)
        Dim ad As New AddPhoto
        Dim Etype As String = "No"
        ad.Etype = Etype
        ad.Show()
    End Sub
    Public Sub FindPhotos_click(sender As Object, e As EventArgs)
        RunUnindexedFileSearchWithSplash()
    End Sub

    Public Sub MenuitemCheck_Click(sender As Object, e As EventArgs)
        Dim defaultDir As String = GetDefaultDir()
        Dim connection As SQLiteConnection = Manager.GetConnection()
        VerifyPictureFilesExist(defaultDir)
        CleanPpeoplelistAndUpdateCount(connection)
    End Sub
    Public Sub MenuitemNoname_Click(sender As Object, e As EventArgs)
        ReDim Namesselected(5)
        Namesselected(0) = "NP"
        Namesselected(1) = "99999"
        Namesselected(2) = "99999"
        Namesselected(3) = "99999"
        Namesselected(4) = "99999"
        Namesselected(5) = "99999"
        Dim thumbForm As New Sthumb()
        thumbForm.NamesSelected = Namesselected
        thumbForm.Show()

    End Sub
    Private Sub MenuItemEvent_Click(sender As Object, e As EventArgs)
        Dim evnt As New Select_Event()
        evnt.Etype = "Ign"
        evnt.Show()
    End Sub
    Public Sub MenuItemPeople_Click(sender As Object, e As EventArgs)
        Dim pp As New Start
        pp.Show()
    End Sub
    Public Sub ExitApp(sender As Object, e As EventArgs)
        Dim ctrl As Control = TryCast(sender, Control)
        If ctrl IsNot Nothing Then
            Dim frm As Form = ctrl.FindForm()
            If frm IsNot Nothing Then
                frm.Close()
            End If
        End If
    End Sub
    Private Sub menuitemBackup_click(sener As Object, e As EventArgs)
        Dim connectionString = GetConnectionString()
        Dim filename As String = "FamAlbum" & DateTime.Now.ToString("MMddyyyy") & ".bak"
        Dim BackupPath As String = GetBackupPath() & filename
        BackupSQLiteDatabase()
    End Sub
    Private Sub menuitemRestore_click(sender As Object, e As EventArgs)
        RestoreSQLiteDatabase()
    End Sub

End Module
