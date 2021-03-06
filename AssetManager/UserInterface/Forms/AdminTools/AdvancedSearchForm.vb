﻿Public Class AdvancedSearchForm
    Sub New(parentForm As ExtendedForm)
        InitializeComponent()
        Me.ParentForm = parentForm
        PopulateTableTree()
        Me.Show()
    End Sub

    Private Sub PopulateTableTree()
        Try
            For Each table In GetTables()
                Dim parentNode As New TreeNode(table)
                parentNode.Tag = False
                Dim childAllNode As New TreeNode("*All")
                childAllNode.Tag = True
                childAllNode.Checked = True
                parentNode.Nodes.Add(childAllNode)
                For Each col In GetColumns(table)
                    Dim childNode As New TreeNode(col)
                    childNode.Tag = False
                    childNode.Checked = True
                    parentNode.Nodes.Add(childNode)
                Next
                TableTree.Nodes.Add(parentNode)
            Next
        Catch ex As Exception
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
        End Try
    End Sub

    Private Function GetTables() As List(Of String)
        Dim Tables As New List(Of String)
        Dim Qry = "SHOW TABLES IN " & ServerInfo.CurrentDataBase.ToString
        Using Results As DataTable = DBFactory.GetDatabase.DataTableFromQueryString(Qry)
            For Each row As DataRow In Results.Rows
                Tables.Add(row.Item(Results.Columns(0).ColumnName).ToString)
            Next
        End Using
        Return Tables
    End Function

    Private Function GetColumns(table As String) As List(Of String)
        Dim colList As New List(Of String)
        Dim SQLQry = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '" & ServerInfo.CurrentDataBase.ToString & "' AND TABLE_NAME = '" & table & "'"
        Using results = DBFactory.GetDatabase.DataTableFromQueryString(SQLQry)
            For Each row As DataRow In results.Rows
                colList.Add(row.Item("COLUMN_NAME").ToString)
            Next
        End Using
        Return colList
    End Function

    Private Async Sub StartSearch()
        Try
            SetWaitCursor(True, ParentForm)
            Dim AdvSearch As New AdvancedSearch.Search(Trim(SearchStringTextBox.Text), GetSelectedTables) ' GetSelectedTables.ToArray, GetSelectedColumns.ToArray)
            Dim DisplayGrid As New GridForm(ParentForm, "Advanced Search Results")

            Dim Tables As List(Of DataTable) = Await Task.Run(Function()
                                                                  Return AdvSearch.GetResults
                                                              End Function)

            For Each table In Tables
                DisplayGrid.AddGrid(table.TableName, table.TableName, table)
            Next
            DisplayGrid.Show()
        Catch ex As Exception
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
        Finally
            SetWaitCursor(False, ParentForm)
        End Try

    End Sub

    Private Function GetSelectedTables() As List(Of AdvancedSearch.TableInfo)
        Dim tables As New List(Of AdvancedSearch.TableInfo)
        For Each node As TreeNode In TableTree.Nodes
            If node.Checked Then tables.Add(New AdvancedSearch.TableInfo(node.Text, GetSelectedColumns(node)))
        Next
        Return tables
    End Function

    Private Function GetSelectedColumns(TableNode As TreeNode) As List(Of String)
        Dim columns As New List(Of String)
        For Each childNode As TreeNode In TableNode.Nodes
            If childNode.Index > 0 Then
                If childNode.Checked Then columns.Add(childNode.Text)
            End If
        Next
        Return columns
    End Function

    Private Sub SearchButton_Click(sender As Object, e As EventArgs) Handles SearchButton.Click
        StartSearch()
    End Sub

    Private Sub SearchStringTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles SearchStringTextBox.KeyDown
        If e.KeyCode = Keys.Return Then
            StartSearch()
        End If
    End Sub

    Private Sub TableTree_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles TableTree.AfterCheck
        If e.Action <> TreeViewAction.Unknown Then
            If e.Node.Level > 0 Then
                If e.Node.Index = 0 Then
                    For Each n As TreeNode In e.Node.Parent.Nodes
                        If n.Index > 0 Then
                            n.Checked = e.Node.Checked
                        End If
                    Next
                Else
                    If e.Node.Checked = False And e.Node.Parent.Nodes(0).Checked Then
                        e.Node.Parent.Nodes(0).Checked = False
                    End If
                End If
            End If
        End If
    End Sub

End Class