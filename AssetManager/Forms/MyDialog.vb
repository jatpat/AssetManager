﻿Imports System.Windows.Forms

Public Class MyDialog
    Public ReadOnly Property ControlValues As List(Of Control)
        Get
            Return MyControls
        End Get
    End Property
    Private MyControls As New List(Of Control)
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub
    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub
    Public Sub AddControl(c As Control)
        MyControls.Add(c)
    End Sub
    Private Sub Dialog1_Load(sender As Object, e As EventArgs) Handles Me.Load
        LoadControls(MyControls)
    End Sub
    Private Sub LoadControls(lstControls As List(Of Control))
        Dim ControlLabel As Label
        For Each ctl As Control In lstControls
            Select Case True
                Case TypeOf ctl Is ComboBox
                    ControlLabel = New Label
                    ControlLabel.AutoSize = True
                    ControlLabel.Padding = New Padding(0, 10, 5, 0)
                    ControlLabel.Text = ctl.Tag
                    Panel.Controls.Add(ControlLabel)
                    Panel.Controls.Add(ctl)
                Case TypeOf ctl Is TextBox
                    ControlLabel = New Label
                    ControlLabel.AutoSize = True
                    ControlLabel.Padding = New Padding(0, 10, 5, 0)
                    ControlLabel.Text = ctl.Tag
                    ctl.Width = 150
                    Panel.Controls.Add(ControlLabel)
                    Panel.Controls.Add(ctl)
                Case TypeOf ctl Is CheckBox
                    Dim chk As CheckBox = ctl
                    chk.AutoSize = True
                    chk.Text = chk.Tag
                    Panel.Controls.Add(chk)

                Case TypeOf ctl Is Label
                    ctl.AutoSize = True
                    ctl.Padding = New Padding(5, 15, 5, 5)
                    ctl.Font = New Font(ctl.Font, FontStyle.Bold)
                    Panel.Controls.Add(ctl)
                Case TypeOf ctl Is RichTextBox
                    Dim smPanel As New Panel
                    ControlLabel = New Label
                    ControlLabel.AutoSize = True
                    ControlLabel.Padding = New Padding(0, 10, 5, 0)
                    ControlLabel.Text = ctl.Tag
                    Panel.Controls.Add(ControlLabel)
                    Panel.Controls.Add(ctl)

            End Select


            'If TypeOf ctl IsNot Label Then
            '    ControlLabel = New Label
            '    ControlLabel.AutoSize = True
            '    ControlLabel.Padding = New Padding(0, 10, 5, 0)
            '    ControlLabel.Text = ctl.Tag
            '    If TypeOf ctl Is TextBox Then ctl.Width = 150
            '    Panel.Controls.Add(ControlLabel)
            '    Panel.Controls.Add(ctl)
            'Else
            '    ctl.AutoSize = True
            '    ctl.Padding = New Padding(5, 15, 5, 5)
            '    ctl.Font = New Font(ctl.Font, FontStyle.Bold)
            '    Panel.Controls.Add(ctl)
            'End If

        Next
    End Sub
    Public Function GetControlValue(ControlName As String) As Object
        For Each ctl As Control In MyControls
            If ctl.Name = ControlName Then
                Select Case True
                    Case TypeOf ctl Is ComboBox
                        Dim cmb As ComboBox = ctl
                        Return cmb.SelectedIndex
                    Case TypeOf ctl Is TextBox
                        Dim txt As TextBox = ctl
                        Return txt.Text
                    Case TypeOf ctl Is CheckBox
                        Dim chk As CheckBox = ctl
                        Return chk.CheckState
                End Select
            End If
        Next
        Return Nothing
    End Function
End Class