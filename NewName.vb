Imports System.Data.SQLite
Imports System.Data.Odbc
Public Class NewName
    Private _parentFormType As String  ' To know which form called us
    Private _sFileName1 As String       ' To store the incoming filename

    Public Sub New(sFileName As String, fromForm As String)
        InitializeComponent()
        _sFileName1 = sFileName
        _parentFormType = fromForm
        ' Maybe display sFileName somewhere in your NewName form
    End Sub
    Public Property Label1 As New Label()

    Dim Manager As New ConnectionManager(GetConnectionString)
    Dim connection As New SQLiteConnection

    Public Property SFileName As String = _sFileName1
    Dim TXTFullName As New TextBox With {
            .Location = New Point(250, 200),
            .Font = New Font("Arial", 12),
            .Width = 600,
            .Height = 30,
            .AutoSize = True
        }

    Dim TXTPosition As New TextBox With {
            .Location = New Point(250, 600),
            .Font = New Font("Arial", 12),
            .Width = 30,
            .Height = 30,
            .AutoSize = True
            }

    Dim TXTRelation As New TextBox With {
            .Location = New Point(250, 250),
            .Font = New Font("Arial", 12),
            .Width = 600,
            .Height = 300,
            .Multiline = True,
            .AutoSize = True
            }
    Private mainForm As Displayinfo
    Public Sub New(parentForm As Displayinfo)
        InitializeComponent()
        mainForm = parentForm
    End Sub
    Dim Description As String
    Private menuStrip As New MenuStrip()
    Dim WithEvents btnAdd As New Button
    Private Sub NewName_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Width = 900
        Me.Height = 1000

        Dim lblTitle As New Label With {
                        .Font = New Font(.Font.FontFamily, 24),
                        .Text = "Please fill in all fields",
                        .Location = New Point(50, 75),
                        .Width = 800,
                        .Height = 60,
                        .TextAlign = ContentAlignment.MiddleCenter
                        }


        Dim menuItemExit As New ToolStripMenuItem("Exit")
        AddHandler menuItemExit.Click, AddressOf MenuItemExit_click
        ' Add the Exit item to the MenuStrip
        menuStrip.Items.Add(menuItemExit)
        Me.MainMenuStrip = menuStrip
        Me.Controls.Add(menuStrip)

        Dim lblFullName As New Label With {
                        .Font = New Font(.Font.FontFamily, 12),
                        .Text = "Full Name:",
                        .Location = New Point(100, 200),
                        .Width = 140,
                        .Height = 30,
                        .TextAlign = ContentAlignment.MiddleRight
                        }

        Dim lblRelation As New Label With {
                        .Font = New Font(.Font.FontFamily, 12),
                        .Text = "Relation:",
                        .Location = New Point(75, 250),
                        .Width = 140,
                        .Height = 30,
                        .TextAlign = ContentAlignment.MiddleRight
                            }

        Dim lblRelation1 As New Label With {
                        .Font = New Font("Arial Novw Light", 10),
                        .Text = "For example, 'Friend of' or 'Relative of'",
                        .Location = New Point(295, 550),
                        .AutoSize = True
                        }

        Dim lblPosition As New Label With {
                        .Font = New Font(.Font.FontFamily, 12),
                        .Text = "Position in list:",
                        .Location = New Point(50, 600),
                        .Width = 190,
                        .Height = 30,
                        .TextAlign = ContentAlignment.MiddleRight
                            }

        Dim lblPosition1 As New Label With {
                        .Font = New Font("Arial Novw Light", 10),
                        .Text = "Please list people from left to right",
                        .Location = New Point(300, 600),
                        .Width = 400,
                        .Height = 30,
                        .TextAlign = ContentAlignment.MiddleLeft
                        }

        With btnAdd
            .Text = "Save"
            .BackColor = Color.LightBlue
            .ForeColor = Color.DarkBlue
            .Font = New Font("Arial", 12, FontStyle.Bold)
            .Size = New Size(350, 50)
            .Location = New Point(190, 750)
        End With

        Me.Controls.Add(lblFullName)
        Me.Controls.Add(TXTFullName)
        Me.Controls.Add(lblRelation)
        Me.Controls.Add(TXTRelation)
        Me.Controls.Add(lblRelation1)
        Me.Controls.Add(btnAdd)
        Me.Controls.Add(lblTitle)
        Me.Controls.Add(lblPosition)
        Me.Controls.Add(TXTPosition)
        Me.Controls.Add(lblPosition1)
        AddHandler btnAdd.Click, AddressOf btnAdd_click
    End Sub
    Private Sub MenuItemExit_click(sender As Object, e As EventArgs)


        Me.Close()
    End Sub
    Private Sub btnAdd_click(sender As Object, e As EventArgs)
        Dim namelist As String = ""
        Dim newKey As String = "0"
        Dim namecount As Integer
        If Not IsNumeric(TXTPosition.Text) Then
            MessageBox.Show("You must indicate where this person in (from left to right in this picture")
        Else
            newKey = AddNewName(TXTFullName.Text, TXTRelation.Text)

            connection = Manager.GetConnection()
            Using connection
                Dim transaction As SQLiteTransaction = connection.BeginTransaction()
                Try

                    ' insert into NamePhoto
                    Dim command2 As New SQLiteCommand("INSERT INTO NamePhoto (npID, npFilename) VALUES (@selectedPerson, @filename1);", connection, transaction)
                    command2.Parameters.AddWithValue("@selectedPerson", newKey)
                    command2.Parameters.AddWithValue("@filename1", _sFileName1)
                    command2.ExecuteNonQuery()

                    transaction.Commit()
                    'MessageBox.Show("Transaction committed successfully.")
                Catch ex As SQLiteException
                    MessageBox.Show("Database error: " & ex.Message)
                    transaction.Rollback()
                Catch ex As Exception
                    MessageBox.Show("An error occurred: " & ex.Message)
                    transaction.Rollback()
                End Try
            End Using
            namelist = GetPPeopleList(_sFileName1, namecount)
            namelist = ModifyPeopleList(namelist, CInt(TXTPosition.Text) - 1, 1, newKey)

        End If
        Try
            If _parentFormType = "DisplayInfo" Then
                Dim parentForm = TryCast(Me.Parent.FindForm(), Displayinfo)
                If parentForm IsNot Nothing Then
                    parentForm.Label1.Text = namelist
                End If
            ElseIf _parentFormType = "AddPhoto" Then
                Dim parentForm = TryCast(Me.Parent.FindForm(), AddPhoto)
                If parentForm IsNot Nothing Then
                    parentForm.Label1.Text = CStr(newKey) & "|" & CStr(TXTPosition.Text)
                End If
            End If
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error updating parent form: " & ex.Message)
        End Try

        Me.Close()
    End Sub

End Class