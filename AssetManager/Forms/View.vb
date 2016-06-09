﻿Option Explicit On
Option Strict Off
Imports System.ComponentModel
Imports MySql.Data.MySqlClient
Public Class View
    Private Children(0) As Form
    Private bolCheckFields As Boolean
    Private Structure UserInput
        Public strAssetTag As String
        Public strDescription As String
        Public strEqType As String
        Public strSerial As String
        Public strLocation As String
        Public strCurrentUser As String
        Public dtPurchaseDate As String
        Public strReplaceYear As String
        Public strOSVersion As String
    End Structure
    Private OldData As Device_Info
    Public NewData As Device_Info
    Private Sub View_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ToolStrip1.BackColor = colToolBarColor
        ExtendedMethods.DoubleBuffered(DataGridHistory, True)
        ExtendedMethods.DoubleBuffered(TrackingGrid, True)
        'AssetManager.CopyDefaultCellStyles()
        'ClearFields()
    End Sub
    Private Sub GetCurrentValues()
        With OldData
            .strAssetTag = Trim(txtAssetTag_View_REQ.Text)
            .strDescription = Trim(txtDescription_View_REQ.Text)
            .strEqType = GetDBValue(ComboType.EquipType, cmbEquipType_View_REQ.SelectedIndex)
            .strSerial = Trim(txtSerial_View_REQ.Text)
            .strLocation = GetDBValue(ComboType.Location, cmbLocation_View_REQ.SelectedIndex)
            .strCurrentUser = Trim(txtCurUser_View_REQ.Text)
            .dtPurchaseDate = dtPurchaseDate_View_REQ.Value.ToString(strDBDateFormat)
            .strReplaceYear = Trim(txtReplacementYear_View.Text)
            .strOSVersion = GetDBValue(ComboType.OSType, cmbOSVersion_REQ.SelectedIndex)
            .strStatus = GetDBValue(ComboType.StatusType, cmbStatus_REQ.SelectedIndex)
            .bolTrackable = chkTrackable.Checked
        End With
    End Sub
    Public Sub GetNewValues()
        With NewData
            .strAssetTag = Trim(txtAssetTag_View_REQ.Text)
            .strDescription = Trim(txtDescription_View_REQ.Text)
            .strEqType = GetDBValue(ComboType.EquipType, cmbEquipType_View_REQ.SelectedIndex)
            .strSerial = Trim(txtSerial_View_REQ.Text)
            .strLocation = GetDBValue(ComboType.Location, cmbLocation_View_REQ.SelectedIndex)
            .strCurrentUser = Trim(txtCurUser_View_REQ.Text)
            .dtPurchaseDate = dtPurchaseDate_View_REQ.Value.ToString(strDBDateFormat)
            .strReplaceYear = Trim(txtReplacementYear_View.Text)
            .strOSVersion = GetDBValue(ComboType.OSType, cmbOSVersion_REQ.SelectedIndex)
            .strStatus = GetDBValue(ComboType.StatusType, cmbStatus_REQ.SelectedIndex)
            .strNote = UpdateDev.strNewNote
            .bolTrackable = chkTrackable.Checked
        End With
    End Sub
    Private Sub EnableControls()
        Dim c As Control
        For Each c In DeviceInfoBox.Controls
            Select Case True
                Case TypeOf c Is TextBox
                    Dim txt As TextBox = c
                    If txt.Name <> "txtGUID" Then
                        txt.ReadOnly = False
                    End If
                Case TypeOf c Is ComboBox
                    Dim cmb As ComboBox = c
                    cmb.Enabled = True
                Case TypeOf c Is DateTimePicker
                    Dim dtp As DateTimePicker = c
                    dtp.Enabled = True
                Case TypeOf c Is CheckBox
                    c.Enabled = True
                Case TypeOf c Is Label
                    'do nut-zing
            End Select
        Next
        Me.Text = "*View - MODIFYING*"
        ToolStrip1.BackColor = colEditColor
        For Each t As ToolStripItem In ToolStrip1.Items
            If TypeOf t IsNot ToolStripSeparator Then
                t.Visible = False
            Else
                t.Visible = True
            End If
        Next
        cmdAccept_Tool.Visible = True
        cmdCancel_Tool.Visible = True
    End Sub
    Private Sub DisableControls()
        Dim c As Control
        For Each c In DeviceInfoBox.Controls
            Select Case True
                Case TypeOf c Is TextBox
                    Dim txt As TextBox = c
                    txt.ReadOnly = True
                Case TypeOf c Is ComboBox
                    Dim cmb As ComboBox = c
                    cmb.Enabled = False
                Case TypeOf c Is DateTimePicker
                    Dim dtp As DateTimePicker = c
                    dtp.Enabled = False
                Case TypeOf c Is CheckBox
                    c.Enabled = False
                Case TypeOf c Is Label
                    'do nut-zing
            End Select
        Next
        Me.Text = "View"
        ToolStrip1.BackColor = colToolBarColor
        For Each t As ToolStripItem In ToolStrip1.Items
            If TypeOf t IsNot ToolStripSeparator Then
                t.Visible = True
            Else
                t.Visible = False
            End If
        Next
        cmdAccept_Tool.Visible = False
        cmdCancel_Tool.Visible = False
    End Sub
    Public Sub ViewDevice(ByVal DeviceUID As String)
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        Waiting()
        ClearFields()
        RefreshCombos()
        Dim reader As MySqlDataReader
        Dim table As New DataTable
        Dim strQry = "Select * FROM devices, historical WHERE dev_UID = hist_dev_UID And dev_UID = '" & DeviceUID & "' ORDER BY hist_action_datetime DESC"
        Dim cmd As New MySqlCommand(strQry, GlobalConn)
        Try
            reader = cmd.ExecuteReader
            If Not reader.HasRows Then
                reader.Close()
                table.Dispose()
                cmd.Dispose()
                reader.Dispose()
                CurrentDevice = Nothing
                Dim blah = MsgBox("That device was not found!  It may have been deleted.  Re-execute your search.", vbOKOnly + vbExclamation, "Not Found")
                Exit Sub
            End If
            table.Columns.Add("Date", GetType(String))
            table.Columns.Add("Action Type", GetType(String))
            table.Columns.Add("Action User", GetType(String))
            table.Columns.Add("User", GetType(String))
            table.Columns.Add("Asset ID", GetType(String))
            table.Columns.Add("Serial", GetType(String))
            table.Columns.Add("Description", GetType(String))
            table.Columns.Add("Location", GetType(String))
            table.Columns.Add("Purchase Date", GetType(String))
            table.Columns.Add("GUID", GetType(String))
            With reader
                Do While .Read()
                    CollectDeviceInfo(!dev_UID,!dev_description,!dev_location,!dev_cur_user,!dev_serial,!dev_asset_tag,!dev_purchase_date,!dev_replacement_year,!dev_po,!dev_osversion,!dev_eq_type,!dev_status, CBool(!dev_trackable), CBool(!dev_checkedout))
                    txtAssetTag_View_REQ.Text = !dev_asset_tag
                    txtDescription_View_REQ.Text = !dev_description
                    cmbEquipType_View_REQ.SelectedIndex = GetComboIndexFromShort(ComboType.EquipType,!dev_eq_type)
                    txtSerial_View_REQ.Text = !dev_serial
                    cmbLocation_View_REQ.SelectedIndex = GetComboIndexFromShort(ComboType.Location,!dev_location)
                    txtCurUser_View_REQ.Text = !dev_cur_user
                    dtPurchaseDate_View_REQ.Value = !dev_purchase_date
                    txtReplacementYear_View.Text = !dev_replacement_year
                    cmbOSVersion_REQ.SelectedIndex = GetComboIndexFromShort(ComboType.OSType,!dev_osversion)
                    cmbStatus_REQ.SelectedIndex = GetComboIndexFromShort(ComboType.StatusType,!dev_status)
                    txtGUID.Text = !dev_UID
                    chkTrackable.Checked = CBool(!dev_trackable)
                    table.Rows.Add(!hist_action_datetime, GetHumanValue(ComboType.ChangeType,!hist_change_type),!hist_action_user,!hist_cur_user,!hist_asset_tag,!hist_serial,!hist_description, GetHumanValue(ComboType.Location,!hist_location),!hist_purchase_date,!hist_uid)
                Loop
            End With
            reader.Close()
            reader.Dispose()
            DataGridHistory.DataSource = table
            table.Dispose()
            cmd.Dispose()
            DisableControls()
            DataGridHistory.AutoResizeColumns()
            ViewTracking(CurrentDevice.strGUID)
            DoneWaiting()
            Me.Show()
        Catch ex As MySqlException
            DoneWaiting()
            ErrHandle(ex.ErrorCode, ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            reader.Close()
            reader.Dispose()
            table.Dispose()
            cmd.Dispose()
            Exit Sub
        End Try
    End Sub
    Private Sub Waiting()
        Me.Cursor = Cursors.WaitCursor
        StatusBar("Processing...")
    End Sub
    Private Sub DoneWaiting()
        Me.Cursor = Cursors.Default
        StatusBar("Idle...")
    End Sub
    Public Sub StatusBar(Text As String)
        StatusLabel.Text = Text
        'Attachments.StatusLabel.Text = Text
        'Attachments.Refresh()
        Me.Refresh()
    End Sub
    Public Sub ViewTracking(strGUID As String)
        On Error GoTo errs
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        Waiting()
        'Dim ConnID As String = Guid.NewGuid.ToString
        Dim reader As MySqlDataReader
        Dim table As New DataTable
        Dim strQry = "Select * FROM trackable, devices WHERE track_device_uid = dev_UID And track_device_uid = '" & strGUID & "' ORDER BY track_datestamp DESC"
        Dim cmd As New MySqlCommand(strQry, GlobalConn)
        reader = cmd.ExecuteReader
        table.Columns.Add("Date", GetType(String))
        table.Columns.Add("Check Type", GetType(String))
        table.Columns.Add("Check Out User", GetType(String))
        table.Columns.Add("Check In User", GetType(String))
        table.Columns.Add("Check Out", GetType(String))
        table.Columns.Add("Check In", GetType(String))
        table.Columns.Add("Due Back", GetType(String))
        table.Columns.Add("Location", GetType(String))
        table.Columns.Add("GUID", GetType(String))
        Dim i As Integer
        i = 0
        With reader
            Do While .Read()
                If i < 1 Then 'collect most current info
                    CurrentDevice.Tracking.strCheckOutTime = IIf(IsDBNull(!track_checkout_time), "",!track_checkout_time)
                    CurrentDevice.Tracking.strCheckInTime = IIf(IsDBNull(!track_checkin_time), "",!track_checkin_time)
                    CurrentDevice.Tracking.strUseLocation = !track_use_location
                    CurrentDevice.Tracking.strCheckOutUser = !track_checkout_user
                    CurrentDevice.Tracking.strCheckInUser = IIf(IsDBNull(!track_checkin_user), "",!track_checkin_user)
                    CurrentDevice.Tracking.strDueBackTime = !track_dueback_date
                    CurrentDevice.Tracking.strUseReason = !track_notes
                End If
                table.Rows.Add(!track_datestamp,!track_check_type,!track_checkout_user,!track_checkin_user,!track_checkout_time,!track_checkin_time,!track_dueback_date,!track_use_location,!track_uid)
                i += 1
            Loop
        End With
        reader.Close()
        'CloseConnection(ConnID)
        TrackingGrid.DataSource = table
        TrackingGrid.AutoResizeColumns()
        'GetCurrentTracking(CurrentDevice.strGUID)
        DisableSorting(TrackingGrid)
        FillTrackingBox()
        SetTracking(CurrentDevice.bolTrackable, CurrentDevice.Tracking.bolCheckedOut)
        TrackingGrid.Columns("Check Type").DefaultCellStyle.Font = New Font(DataGridHistory.Font, FontStyle.Bold)
        DoneWaiting()
        Exit Sub
errs:
        If ErrHandle(Err.Number, Err.Description, System.Reflection.MethodInfo.GetCurrentMethod().Name) Then
            DoneWaiting()
            Resume Next
        Else
            EndProgram()
        End If
    End Sub
    Private Sub DisableSorting(Grid As DataGridView)
        Dim c As DataGridViewColumn
        For Each c In Grid.Columns
            c.SortMode = DataGridViewColumnSortMode.NotSortable
        Next
    End Sub
    Private Sub FillTrackingBox()
        If CBool(CurrentDevice.Tracking.bolCheckedOut) Then
            txtCheckOut.BackColor = colCheckOut
            txtCheckLocation.Text = CurrentDevice.Tracking.strUseLocation
            lblCheckTime.Text = "CheckOut Time:"
            txtCheckTime.Text = CurrentDevice.Tracking.strCheckOutTime
            lblCheckUser.Text = "CheckOut User:"
            txtCheckUser.Text = CurrentDevice.Tracking.strCheckOutUser
            lblDueBack.Visible = True
            txtDueBack.Visible = True
            txtDueBack.Text = CurrentDevice.Tracking.strDueBackTime
        Else
            txtCheckOut.BackColor = colCheckIn
            txtCheckLocation.Text = GetHumanValue(ComboType.Location, CurrentDevice.strLocation)
            lblCheckTime.Text = "CheckIn Time:"
            txtCheckTime.Text = CurrentDevice.Tracking.strCheckInTime
            lblCheckUser.Text = "CheckIn User:"
            txtCheckUser.Text = CurrentDevice.Tracking.strCheckInUser
            lblDueBack.Visible = False
            txtDueBack.Visible = False
        End If
        txtCheckOut.Text = IIf(CurrentDevice.Tracking.bolCheckedOut, "Checked Out", "Checked In")
    End Sub
    Public Sub SetTracking(bolEnabled As Boolean, bolCheckedOut As Boolean)
        If bolEnabled Then
            'TrackingTab.Visible = True
            'TrackingTab.Enabled = True
            TrackingToolStripMenuItem.Visible = True
            If Not TabControl1.TabPages.Contains(TrackingTab) Then TabControl1.TabPages.Insert(1, TrackingTab)
            AssetManager.CopyDefaultCellStyles()
            TrackingBox.Visible = True
            CheckOutMenu.Visible = Not bolCheckedOut
            CheckInMenu.Visible = bolCheckedOut
            TrackingTool.Visible = bolEnabled
            CheckOutTool.Visible = Not bolCheckedOut
            CheckInTool.Visible = bolCheckedOut
        Else
            'TrackingTab.Enabled = False
            'TrackingTab.Visible = False
            TrackingTool.Visible = bolEnabled
            TrackingToolStripMenuItem.Visible = False
            TabControl1.TabPages.Remove(TrackingTab)
            AssetManager.CopyDefaultCellStyles()
            TrackingBox.Visible = False
        End If
    End Sub
    Private Sub ModifyDevice()
        GetCurrentValues()
        EnableControls()
    End Sub
    Private Sub ClearFields()
        Dim c As Control
        For Each c In DeviceInfoBox.Controls
            If TypeOf c Is TextBox Then
                Dim txt As TextBox = c
                txt.Text = ""
            End If
            If TypeOf c Is ComboBox Then
                Dim cmb As ComboBox = c
                cmb.SelectedIndex = -1
            End If
        Next
    End Sub
    Private Function CheckFields() As Boolean
        Dim bolMissingField As Boolean
        bolMissingField = False
        Dim c As Control
        For Each c In DeviceInfoBox.Controls
            Select Case True
                Case TypeOf c Is TextBox
                    If c.Name.Contains("REQ") Then
                        If Trim(c.Text) = "" Then
                            bolMissingField = True
                            c.BackColor = colMissingField
                        Else
                            c.BackColor = Color.Empty
                        End If
                    End If
                Case TypeOf c Is ComboBox
                    Dim cmb As ComboBox = c
                    If cmb.Name.Contains("REQ") Then
                        If cmb.SelectedIndex = -1 Then
                            bolMissingField = True
                            cmb.BackColor = colMissingField
                        Else
                            cmb.BackColor = Color.Empty
                        End If
                    End If
            End Select
        Next
        Return Not bolMissingField 'if fields are missing return false to trigger a message if needed
    End Function
    Private Sub ResetBackColors()
        Dim c As Control
        For Each c In DeviceInfoBox.Controls
            ' c.BackColor = Color.Empty
            Select Case True
                Case TypeOf c Is TextBox
                    c.BackColor = Color.Empty
                Case TypeOf c Is ComboBox
                    c.BackColor = Color.Empty
            End Select
        Next
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs)
        EnableControls()
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs)
        DisableControls()
    End Sub
    Private Sub EditToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EditToolStripMenuItem.Click
        If Not CheckForAdmin() Then Exit Sub
        ModifyDevice()
    End Sub
    Private Sub cmdUpdate_Click(sender As Object, e As EventArgs)
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        If Not CheckFields() Then
            Dim blah = MsgBox("Some required fields are missing.  Please fill in all highlighted fields.", vbOKOnly + vbExclamation, "Missing Data")
            bolCheckFields = True
            Exit Sub
        End If
        UpdateDev.cmbUpdate_ChangeType.SelectedIndex = -1
        UpdateDev.cmbUpdate_ChangeType.Enabled = True
        UpdateDev.Show()
    End Sub
    Private Sub View_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Me.Hide()
        Attachments.Dispose()
        Tracking.Dispose()
        CloseChildren()
        'Dim f As Form
        'For Each f In 
    End Sub
    Private Sub DataGridHistory_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridHistory.CellDoubleClick
        NewEntryView(DataGridHistory.Item(GetColIndex(DataGridHistory, "GUID"), DataGridHistory.CurrentRow.Index).Value)
    End Sub
    Private Sub NewEntryView(GUID As String)
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        Dim NewEntry As New View_Entry
        Waiting()
        AddChild(NewEntry)
        NewEntry.ViewEntry(GUID)
        NewEntry.Show()
        DoneWaiting()
    End Sub
    Private Sub NewTrackingView(GUID As String)
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        Dim NewTracking As New View_Tracking
        Waiting()
        AddChild(NewTracking)
        NewTracking.ViewTrackingEntry(GUID)
        NewTracking.Show()
        DoneWaiting()
    End Sub
    Private Sub AddChild(form As Form)
        Children(UBound(Children)) = form
        ReDim Preserve Children(UBound(Children) + 1)
    End Sub
    Public Sub CloseChildren()
        On Error Resume Next
        For i As Integer = 0 To UBound(Children)
            Children(i).Dispose()
        Next
        ReDim Children(0)
    End Sub
    Private Sub AddNoteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AddNoteToolStripMenuItem.Click
        If Not CheckForAdmin() Then Exit Sub
        UpdateDev.cmbUpdate_ChangeType.SelectedIndex = GetComboIndexFromShort(ComboType.ChangeType, "NOTE")
        UpdateDev.cmbUpdate_ChangeType.Enabled = False
        UpdateDev.Show()
    End Sub
    Private Sub RefreshCombos()
        FillEquipTypeCombo()
        FillLocationCombo()
        FillOSTypeCombo()
        FillStatusTypeCombo()
    End Sub
    Private Sub FillEquipTypeCombo()
        Dim i As Integer
        cmbEquipType_View_REQ.Items.Clear()
        cmbEquipType_View_REQ.Text = ""
        For i = 0 To UBound(EquipType)
            cmbEquipType_View_REQ.Items.Insert(i, EquipType(i).strLong)
        Next
    End Sub
    Private Sub FillLocationCombo()
        Dim i As Integer
        cmbLocation_View_REQ.Items.Clear()
        cmbLocation_View_REQ.Text = ""
        For i = 0 To UBound(Locations)
            cmbLocation_View_REQ.Items.Insert(i, Locations(i).strLong)
        Next
    End Sub
    Private Sub FillOSTypeCombo()
        Dim i As Integer
        cmbOSVersion_REQ.Items.Clear()
        cmbOSVersion_REQ.Text = ""
        For i = 0 To UBound(OSType)
            cmbOSVersion_REQ.Items.Insert(i, OSType(i).strLong)
        Next
    End Sub
    Private Sub FillStatusTypeCombo()
        Dim i As Integer
        cmbStatus_REQ.Items.Clear()
        cmbStatus_REQ.Text = ""
        For i = 0 To UBound(StatusType)
            cmbStatus_REQ.Items.Insert(i, StatusType(i).strLong)
        Next
    End Sub
    Private Sub DeleteDeviceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteDeviceToolStripMenuItem.Click
        If Not CheckForAdmin() Then Exit Sub
        Dim blah = MsgBox("Are you absolutely sure?  This cannot be undone and will delete all histrical data.", vbYesNo + vbCritical, "WARNING")
        If blah = vbYes Then
            Dim rows As Integer
            rows = DeleteDevice(CurrentDevice.strGUID)
            If rows > 0 Then
                Dim blah2 = MsgBox("Device deleted successfully.", vbOKOnly + vbInformation, "Device Deleted")
                CurrentDevice = Nothing
                Me.Hide()
            Else
                Logger("*****DELETION ERROR******: " & CurrentDevice.strGUID)
                Dim blah2 = MsgBox("Failed to delete device succesfully!  Please let Bobby Lovell know about this.", vbOKOnly + vbCritical, "Delete Failed")
                CurrentDevice = Nothing
                Me.Hide()
            End If
        Else
            Exit Sub
        End If
    End Sub
    Private Sub txtAssetTag_View_REQ_TextChanged(sender As Object, e As EventArgs) Handles txtAssetTag_View_REQ.TextChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub txtDescription_View_REQ_TextChanged(sender As Object, e As EventArgs) Handles txtDescription_View_REQ.TextChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub cmbEquipType_View_REQ_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbEquipType_View_REQ.SelectedIndexChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub txtSerial_View_REQ_TextChanged(sender As Object, e As EventArgs) Handles txtSerial_View_REQ.TextChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub cmbLocation_View_REQ_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbLocation_View_REQ.SelectedIndexChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub cmbOSVersion_REQ_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbOSVersion_REQ.SelectedIndexChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub cmbStatus_REQ_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbStatus_REQ.SelectedIndexChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub txtCurUser_View_REQ_TextChanged(sender As Object, e As EventArgs) Handles txtCurUser_View_REQ.TextChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub dtPurchaseDate_View_REQ_ValueChanged(sender As Object, e As EventArgs) Handles dtPurchaseDate_View_REQ.ValueChanged
        If bolCheckFields Then CheckFields()
    End Sub
    Private Sub cmdCancel_Click(sender As Object, e As EventArgs)
        bolCheckFields = False
        DisableControls()
        ResetBackColors()
        'ClearFields()
        Me.Refresh()
        ViewDevice(CurrentDevice.strGUID)
    End Sub
    Private Sub DeleteEntryToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteEntryToolStripMenuItem.Click
        If Not CheckForAdmin() Then Exit Sub
        Dim strGUID As String = DataGridHistory.Item(GetColIndex(DataGridHistory, "GUID"), DataGridHistory.CurrentRow.Index).Value
        Dim Info As Device_Info = GetEntryInfo(strGUID)
        Dim blah = MsgBox("Are you absolutely sure?  This cannot be undone!" & vbCrLf & vbCrLf & "Entry info: " & Info.Historical.dtActionDateTime & " - " & Info.Historical.strChangeType & " - " & strGUID, vbYesNo + vbCritical, "WARNING")
        If blah = vbYes Then
            Dim blah2 = MsgBox(DeleteEntry(strGUID) & " rows affected.", vbOKOnly + vbInformation, "Deletion Results")
            ViewDevice(CurrentDevice.strGUID)
            'Me.Hide()
        Else
            Exit Sub
        End If
        'DeleteEntry(DataGridHistory.Item(GetColIndex(DataGridHistory, "GUID"), DataGridHistory.CurrentRow.Index).Value)
    End Sub
    Private Sub DataGridHistory_CellMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridHistory.CellMouseDown
        If e.Button = MouseButtons.Right Then
            DataGridHistory.CurrentCell = DataGridHistory(e.ColumnIndex, e.RowIndex) 'DataGridHistory.RowIndex
        End If
    End Sub
    Private Sub CheckInMenu_Click(sender As Object, e As EventArgs) Handles CheckInMenu.Click
        Waiting()
        Tracking.SetupTracking()
        Tracking.Show()
        DoneWaiting()
    End Sub
    Private Sub CheckOutMenu_Click(sender As Object, e As EventArgs) Handles CheckOutMenu.Click
        Waiting()
        Tracking.SetupTracking()
        Tracking.Show()
        DoneWaiting()
    End Sub
    Private Sub TrackingGrid_RowPrePaint(sender As Object, e As DataGridViewRowPrePaintEventArgs) Handles TrackingGrid.RowPrePaint
        Dim Mod3 As Single = 0.75
        Dim c1 As Color = colHighlightBlue 'highlight color
        TrackingGrid.Rows(e.RowIndex).DefaultCellStyle.ForeColor = Color.Black
        TrackingGrid.Rows(e.RowIndex).Cells(GetColIndex(TrackingGrid, "Check Type")).Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        If TrackingGrid.Rows(e.RowIndex).Cells(GetColIndex(TrackingGrid, "Check Type")).Value = strCheckIn Then
            TrackingGrid.Rows(e.RowIndex).DefaultCellStyle.BackColor = colCheckIn
            Dim c2 As Color = Color.FromArgb(colCheckIn.R, colCheckIn.G, colCheckIn.B)
            Dim BlendColor As Color
            BlendColor = Color.FromArgb((CInt(c1.A) + CInt(c2.A)) / 2,
                                                (CInt(c1.R) + CInt(c2.R)) / 2,
                                                (CInt(c1.G) + CInt(c2.G)) / 2,
                                                (CInt(c1.B) + CInt(c2.B)) / 2)
            TrackingGrid.Rows(e.RowIndex).DefaultCellStyle.SelectionBackColor = BlendColor
        ElseIf TrackingGrid.Rows(e.RowIndex).Cells(GetColIndex(TrackingGrid, "Check Type")).Value = strCheckOut Then
            TrackingGrid.Rows(e.RowIndex).DefaultCellStyle.BackColor = colCheckOut
            Dim c2 As Color = Color.FromArgb(colCheckOut.R, colCheckOut.G, colCheckOut.B)
            Dim BlendColor As Color
            BlendColor = Color.FromArgb((CInt(c1.A) + CInt(c2.A)) / 2,
                                                (CInt(c1.R) + CInt(c2.R)) / 2,
                                                (CInt(c1.G) + CInt(c2.G)) / 2,
                                                (CInt(c1.B) + CInt(c2.B)) / 2)
            TrackingGrid.Rows(e.RowIndex).DefaultCellStyle.SelectionBackColor = BlendColor
        End If
    End Sub
    Private Sub Button1_Click_1(sender As Object, e As EventArgs)
        TrackingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnMode.ColumnHeader
        TrackingGrid.AutoResizeColumns()
    End Sub
    Private Sub TrackingGrid_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles TrackingGrid.CellDoubleClick
        NewTrackingView(TrackingGrid.Item(GetColIndex(TrackingGrid, "GUID"), TrackingGrid.CurrentRow.Index).Value)
    End Sub
    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        If Not CheckForAccess("modify") Then Exit Sub
        'If Not CheckForAdmin() Then Exit Sub
        ModifyDevice()
    End Sub
    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        If Not CheckForAccess("modify") Then Exit Sub
        'If Not CheckForAdmin() Then Exit Sub
        UpdateDev.cmbUpdate_ChangeType.SelectedIndex = GetComboIndexFromShort(ComboType.ChangeType, "NOTE")
        UpdateDev.cmbUpdate_ChangeType.Enabled = False
        UpdateDev.Show()
    End Sub
    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        If Not CheckForAccess("delete") Then Exit Sub
        'If Not CheckForAdmin() Then Exit Sub
        Dim blah = MsgBox("Are you absolutely sure?  This cannot be undone and will delete all histrical data, tracking and attachments.", vbYesNo + vbCritical, "WARNING")
        If blah = vbYes Then
            ' Dim rows As Integer
            'rows = DeleteDevice(CurrentDevice.strGUID)
            If DeleteDevice(CurrentDevice.strGUID) Then
                Dim blah2 = MsgBox("Device deleted successfully.", vbOKOnly + vbInformation, "Device Deleted")
                CurrentDevice = Nothing
                Me.Hide()
            Else
                Logger("*****DELETION ERROR******: " & CurrentDevice.strGUID)
                Dim blah2 = MsgBox("Failed to delete device succesfully!  Please let Bobby Lovell know about this.", vbOKOnly + vbCritical, "Delete Failed")
                CurrentDevice = Nothing
                Me.Hide()
            End If
        Else
            Exit Sub
        End If
    End Sub
    Private Sub CheckInTool_Click(sender As Object, e As EventArgs) Handles CheckInTool.Click
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        If Not CheckForAccess("track") Then Exit Sub
        Waiting()
        Tracking.SetupTracking()
        AddChild(Tracking)
        Tracking.Show()
        DoneWaiting()
    End Sub
    Private Sub CheckOutTool_Click(sender As Object, e As EventArgs) Handles CheckOutTool.Click
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        If Not CheckForAccess("track") Then Exit Sub
        Waiting()
        Tracking.SetupTracking()
        AddChild(Tracking)
        Tracking.Show()
        DoneWaiting()
    End Sub
    Private Sub AttachmentTool_Click(sender As Object, e As EventArgs) Handles AttachmentTool.Click
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        If Not CheckForAccess("view_attach") Then Exit Sub
        'If Not CheckForAdmin() Then Exit Sub
        Attachments.FillDeviceInfo()
        AddChild(Attachments)
        Attachments.ListAttachments(CurrentDevice.strGUID)
        'Attachments.Show()
        'Attachments.Activate()
    End Sub
    Private DefGridBC As Color, DefGridSelCol As Color, bolGridFilling As Boolean = False
    Private Sub HighlightCurrentRow(Row As Integer)
        On Error Resume Next
        If Not bolGridFilling Then
            DefGridBC = TrackingGrid.Rows(Row).DefaultCellStyle.BackColor
            DefGridSelCol = TrackingGrid.Rows(Row).DefaultCellStyle.SelectionBackColor
            Dim BackColor As Color = DefGridBC
            Dim SelectColor As Color = DefGridSelCol
            Dim Mod1 As Integer = 3
            Dim Mod2 As Integer = 4
            Dim Mod3 As Single = 0.6 '0.75
            Dim c1 As Color = colHighlightColor 'highlight color
            If Row > -1 Then
                For Each cell As DataGridViewCell In TrackingGrid.Rows(Row).Cells
                    'cell.Style.SelectionBackColor = Color.FromArgb(SelectColor.R * Mod3, SelectColor.G * Mod3, SelectColor.B * Mod3)
                    'cell.Style.BackColor = Color.FromArgb(BackColor.R * Mod3, BackColor.G * Mod3, BackColor.B * Mod3)
                    Dim c2 As Color = Color.FromArgb(SelectColor.R, SelectColor.G, SelectColor.B)
                    Dim BlendColor As Color
                    BlendColor = Color.FromArgb((CInt(c1.A) + CInt(c2.A)) / 2,
                                                (CInt(c1.R) + CInt(c2.R)) / 2,
                                                (CInt(c1.G) + CInt(c2.G)) / 2,
                                                (CInt(c1.B) + CInt(c2.B)) / 2)
                    cell.Style.SelectionBackColor = BlendColor
                    c2 = Color.FromArgb(BackColor.R, BackColor.G, BackColor.B)
                    BlendColor = Color.FromArgb((CInt(c1.A) + CInt(c2.A)) / 2,
                                                (CInt(c1.R) + CInt(c2.R)) / 2,
                                                (CInt(c1.G) + CInt(c2.G)) / 2,
                                                (CInt(c1.B) + CInt(c2.B)) / 2)
                    cell.Style.BackColor = BlendColor
                Next
            End If
        End If
    End Sub
    Public Sub CancelModify()
        bolCheckFields = False
        DisableControls()
        ResetBackColors()
        'ClearFields()
        Me.Refresh()
        ViewDevice(CurrentDevice.strGUID)
    End Sub
    Private Sub cmbEquipType_View_REQ_DropDown(sender As Object, e As EventArgs) Handles cmbEquipType_View_REQ.DropDown
        AdjustComboBoxWidth(sender, e)
    End Sub
    Private Sub cmbLocation_View_REQ_DropDown(sender As Object, e As EventArgs) Handles cmbLocation_View_REQ.DropDown
        AdjustComboBoxWidth(sender, e)
    End Sub
    Private Sub cmbOSVersion_REQ_DropDown(sender As Object, e As EventArgs) Handles cmbOSVersion_REQ.DropDown
        AdjustComboBoxWidth(sender, e)
    End Sub
    Private Sub cmbStatus_REQ_DropDown(sender As Object, e As EventArgs) Handles cmbStatus_REQ.DropDown
        AdjustComboBoxWidth(sender, e)
    End Sub
    Private Sub TabControl1_Click(sender As Object, e As EventArgs) Handles TabControl1.Click
    End Sub
    Private Sub TrackingTool_Click(sender As Object, e As EventArgs) Handles TrackingTool.Click
    End Sub
    Private Sub cmdAccept_Tool_Click(sender As Object, e As EventArgs) Handles cmdAccept_Tool.Click
        If Not ConnectionReady() Then
            ConnectionNotReady()
            Exit Sub
        End If
        If Not CheckFields() Then
            Dim blah = MsgBox("Some required fields are missing.  Please fill in all highlighted fields.", vbOKOnly + vbExclamation, "Missing Data")
            bolCheckFields = True
            Exit Sub
        End If
        DisableControls()
        UpdateDev.cmbUpdate_ChangeType.SelectedIndex = -1
        UpdateDev.cmbUpdate_ChangeType.Enabled = True
        UpdateDev.txtUpdate_Note.Clear()
        UpdateDev.Show()
    End Sub
    Private Sub cmdCancel_Tool_Click(sender As Object, e As EventArgs) Handles cmdCancel_Tool.Click
        CancelModify()
    End Sub
    Private Sub TabControl1_GotFocus(sender As Object, e As EventArgs) Handles TabControl1.GotFocus
    End Sub
    Private Sub TabControl1_MouseDown(sender As Object, e As MouseEventArgs) Handles TabControl1.MouseDown
        TrackingGrid.Refresh()
    End Sub
End Class