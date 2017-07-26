﻿Imports System.ComponentModel

Public Class GridForm

#Region "Fields"

    Private bolGridFilling As Boolean = True
    Private GridList As New List(Of DataGridView)
    Private LastDoubleClickRow As DataGridViewRow
    Private MyParent As MyForm

#End Region

#Region "Constructors"

    Sub New(ParentForm As Form, Optional Title As String = "")
        MyParent = DirectCast(ParentForm, MyForm)
        Me.Tag = ParentForm
        Me.Icon = MyParent.Icon
        Me.GridTheme = MyParent.GridTheme

        ' This call is required by the designer.
        InitializeComponent()
        If Title <> "" Then Me.Text = Title
        ' Add any initialization after the InitializeComponent() call.

        DoubleBufferedTableLayout(GridPanel, True)
        DoubleBufferedPanel(Panel1, True)
        GridPanel.RowStyles.Clear()
    End Sub

#End Region

#Region "Properties"

    Public ReadOnly Property GridCount As Integer
        Get
            Return GridList.Count
        End Get
    End Property

    Public ReadOnly Property SelectedValue As DataGridViewRow
        Get
            Return LastDoubleClickRow
        End Get
    End Property

#End Region

#Region "Methods"

    Public Sub AddGrid(Name As String, Label As String, Data As DataTable)
        Dim NewGrid = GetNewGrid(Name, Label & " (" & Data.Rows.Count.ToString & " rows)")
        FillGrid(NewGrid, Data)
        GridList.Add(NewGrid)
    End Sub

    Private Sub CopySelectedToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopySelectedToolStripMenuItem.Click
        CopySelectedGridData(GetActiveGrid)
    End Sub

    Private Sub DisplayGrids()
        Me.SuspendLayout()
        For Each grid As DataGridView In GridList
            Dim GridBox As New GroupBox
            GridBox.Text = DirectCast(grid.Tag, String)
            GridBox.Dock = DockStyle.Fill
            GridBox.Controls.Add(grid)
            GridPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, GridHeight))
            GridPanel.Controls.Add(GridBox)
        Next
        ResizeGrids()
        bolGridFilling = False
        Me.ResumeLayout()
    End Sub

    Private Sub FillGrid(Grid As DataGridView, Data As DataTable)
        If Data IsNot Nothing Then Grid.DataSource = Data
    End Sub

    Private Function GetActiveGrid() As DataGridView
        If TypeOf Me.ActiveControl Is DataGridView Then
            Return DirectCast(Me.ActiveControl, DataGridView)
        End If
        Return Nothing
    End Function

    Private Function GetNewGrid(Name As String, Label As String) As DataGridView
        Dim NewGrid As New DataGridView
        NewGrid.Name = Name
        NewGrid.Tag = Label
        NewGrid.Dock = DockStyle.Fill
        NewGrid.DefaultCellStyle = GridStyles
        NewGrid.DefaultCellStyle.SelectionBackColor = Me.GridTheme.CellSelectColor
        NewGrid.RowHeadersVisible = False
        NewGrid.EditMode = DataGridViewEditMode.EditProgrammatically
        NewGrid.AllowUserToResizeRows = False
        NewGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        NewGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
        NewGrid.AllowUserToAddRows = False
        NewGrid.AllowUserToDeleteRows = False
        NewGrid.Padding = New Padding(0, 0, 0, 10)
        NewGrid.ContextMenuStrip = PopUpMenu
        AddHandler NewGrid.CellLeave, AddressOf GridLeaveCell
        AddHandler NewGrid.CellEnter, AddressOf GridEnterCell
        AddHandler NewGrid.CellDoubleClick, AddressOf GridDoubleClickCell
        DoubleBufferedDataGrid(NewGrid, True)
        Return NewGrid
    End Function
    Private Sub GridDoubleClickCell(sender As Object, e As EventArgs)
        Dim SenderGrid As DataGridView = DirectCast(sender, DataGridView)
        LastDoubleClickRow = SenderGrid.CurrentRow
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub GridEnterCell(sender As Object, e As DataGridViewCellEventArgs)
        If Not bolGridFilling Then
            HighlightRow(DirectCast(sender, DataGridView), Me.GridTheme, e.RowIndex)
        End If
    End Sub

    Private Sub GridForm_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Not Modal Then Me.Dispose()
    End Sub

    Private Sub GridForm_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Not bolGridFilling Then ResizeGridPanel()
    End Sub

    Private Sub GridForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        DisplayGrids()
    End Sub

    Private Function GridHeight() As Integer
        Dim MinHeight As Integer = 200
        Dim CalcHeight As Integer = CInt((Me.ClientSize.Height - 30) / GridList.Count)
        If CalcHeight < MinHeight Then
            Return MinHeight
        Else
            Return CalcHeight
        End If
    End Function
    Private Sub GridLeaveCell(sender As Object, e As DataGridViewCellEventArgs)
        LeaveRow(DirectCast(sender, DataGridView), Me.GridTheme, e.RowIndex)
    End Sub
    Private Sub ResizeGridPanel()
        Dim NewHeight = GridHeight()
        For Each grid In GridList
            Dim row = GridList.IndexOf(grid)
            GridPanel.RowStyles(row).Height = NewHeight
        Next
    End Sub

    Private Sub ResizeGrids()
        For Each grid In GridList
            For Each c As DataGridViewColumn In grid.Columns
                c.Width = c.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, True)
            Next
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            grid.AllowUserToResizeColumns = True
        Next
    End Sub

    Private Sub SendToNewGridForm_Click(sender As Object, e As EventArgs) Handles SendToNewGridForm.Click
        CopyToGridForm(GetActiveGrid, MyParent)
    End Sub

#End Region

End Class