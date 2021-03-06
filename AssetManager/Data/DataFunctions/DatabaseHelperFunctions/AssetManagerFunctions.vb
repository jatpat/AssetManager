﻿Public Class AssetManagerFunctions

#Region "Methods"

    Public Sub AddNewEmp(empInfo As MunisEmployeeStruct)
        Try
            If Not IsEmployeeInDB(empInfo.Number) Then
                Dim UID As String = Guid.NewGuid.ToString
                Dim InsertEmployeeParams As New List(Of DBParameter)
                InsertEmployeeParams.Add(New DBParameter(EmployeesCols.Name, empInfo.Name))
                InsertEmployeeParams.Add(New DBParameter(EmployeesCols.Number, empInfo.Number))
                InsertEmployeeParams.Add(New DBParameter(EmployeesCols.UID, UID))
                DBFactory.GetDatabase.InsertFromParameters(EmployeesCols.TableName, InsertEmployeeParams)
            End If
        Catch ex As Exception
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
        End Try
    End Sub





    Public Function DeleteMasterSqlEntry(sqlGUID As String, type As EntryType) As Boolean
        Try
            Dim DeleteQuery As String = ""
            Select Case type
                Case EntryType.Device
                    DeleteQuery = "DELETE FROM " & DevicesCols.TableName & " WHERE " & DevicesCols.DeviceUID & "='" & sqlGUID & "'"
                Case EntryType.Sibi
                    DeleteQuery = "DELETE FROM " & SibiRequestCols.TableName & " WHERE " & SibiRequestCols.UID & "='" & sqlGUID & "'"
            End Select
            If DBFactory.GetDatabase.ExecuteQuery(DeleteQuery) > 0 Then
                Return True
            End If
            Return False
        Catch ex As Exception
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
            Return False
        End Try
    End Function

    Public Function DeleteFtpAndSql(sqlGUID As String, type As EntryType) As Boolean
        Try
            If FTPFunc.HasFtpFolder(sqlGUID) Then
                If FTPFunc.DeleteFtpFolder(sqlGUID) Then Return DeleteMasterSqlEntry(sqlGUID, type) ' if has attachments, delete ftp directory, then delete the sql records.
            Else
                Return DeleteMasterSqlEntry(sqlGUID, type) 'delete sql records
            End If
        Catch ex As Exception
            Return ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
        End Try
        Return False
    End Function

    Public Function DeleteSqlAttachment(attachment As Attachment) As Integer
        Try
            Dim AttachmentFolderID = GetSqlValue(attachment.AttachTable.TableName, attachment.AttachTable.FileUID, attachment.FileUID, attachment.AttachTable.FKey)
            'Delete FTP Attachment
            If FTPFunc.DeleteFtpAttachment(attachment.FileUID, AttachmentFolderID) Then
                'delete SQL entry
                Dim SQLDeleteQry = "DELETE FROM " & attachment.AttachTable.TableName & " WHERE " & attachment.AttachTable.FileUID & "='" & attachment.FileUID & "'"
                Return DBFactory.GetDatabase.ExecuteQuery(SQLDeleteQry)
            End If
            Return -1
        Catch ex As Exception
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
            Return -1
        End Try
    End Function

    Public Function DeviceExists(assetTag As String, serial As String) As Boolean
        Dim bolAsset As Boolean
        Dim bolSerial As Boolean
        If assetTag = "NA" Then 'Allow NA value because users do not always have an Asset Tag for new devices.
            bolAsset = False
        Else
            Dim CheckAsset As String = GetSqlValue(DevicesCols.TableName, DevicesCols.AssetTag, assetTag, DevicesCols.AssetTag)
            If CheckAsset <> "" Then
                bolAsset = True
            Else
                bolAsset = False
            End If
        End If

        Dim CheckSerial As String = GetSqlValue(DevicesCols.TableName, DevicesCols.Serial, serial, DevicesCols.Serial)
        If CheckSerial <> "" Then
            bolSerial = True
        Else
            bolSerial = False
        End If
        If bolSerial Or bolAsset Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function DevicesBySupervisor(parentForm As ExtendedForm) As DataTable
        Try
            Dim SupInfo As MunisEmployeeStruct
            Using NewMunisSearch As New MunisUserForm(parentForm)
                If NewMunisSearch.DialogResult = DialogResult.Yes Then
                    SetWaitCursor(True, parentForm)
                    SupInfo = NewMunisSearch.EmployeeInfo
                    Using DeviceList As New DataTable, EmpList As DataTable = MunisFunc.ListOfEmpsBySup(SupInfo.Number)
                        For Each r As DataRow In EmpList.Rows
                            Dim strQRY As String = "SELECT * FROM " & DevicesCols.TableName & " WHERE " & DevicesCols.MunisEmpNum & "='" & r.Item("a_employee_number").ToString & "'"
                            Using tmpTable As DataTable = DBFactory.GetDatabase.DataTableFromQueryString(strQRY)
                                DeviceList.Merge(tmpTable)
                            End Using
                        Next
                        Return DeviceList
                    End Using
                Else
                    Return Nothing
                End If
            End Using
        Finally
            SetWaitCursor(False, parentForm)
        End Try
    End Function

    Public Function IsEmployeeInDB(empNum As String) As Boolean
        Dim EmpName As String = GetSqlValue(EmployeesCols.TableName, EmployeesCols.Number, empNum, EmployeesCols.Name)
        If EmpName <> "" Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function FindDeviceFromAssetOrSerial(searchVal As String, type As FindDevType) As DeviceObject
        Try
            If type = FindDevType.AssetTag Then
                Dim Params As New List(Of DBQueryParameter)
                Params.Add(New DBQueryParameter(DevicesCols.AssetTag, searchVal, True))
                Return New DeviceObject(DBFactory.GetDatabase.DataTableFromParameters("SELECT * FROM " & DevicesCols.TableName & " WHERE ", Params))
            ElseIf type = FindDevType.Serial Then
                Dim Params As New List(Of DBQueryParameter)
                Params.Add(New DBQueryParameter(DevicesCols.Serial, searchVal, True))
                Return New DeviceObject(DBFactory.GetDatabase.DataTableFromParameters("SELECT * FROM " & DevicesCols.TableName & " WHERE ", Params))
            End If
            Return Nothing
        Catch ex As Exception
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
            Return Nothing
        End Try
    End Function

    Public Function GetDeviceInfoFromGUID(deviceGUID As String) As DeviceObject
        Return New DeviceObject(DBFactory.GetDatabase.DataTableFromQueryString("SELECT * FROM " & DevicesCols.TableName & " WHERE " & DevicesCols.DeviceUID & "='" & deviceGUID & "'"))
    End Function

    Public Function GetMunisCodeFromAssetCode(assetCode As String) As String
        Return GetSqlValue("munis_codes", "asset_man_code", assetCode, "munis_code")
    End Function

    Public Function GetSqlValue(table As String, fieldIn As String, valueIn As String, fieldOut As String) As String
        Dim sqlQRY As String = "SELECT " & fieldOut & " FROM " & table & " WHERE "
        Dim Params As New List(Of DBQueryParameter)
        Params.Add(New DBQueryParameter(fieldIn, valueIn, True))
        Dim Result = DBFactory.GetDatabase.ExecuteScalarFromCommand(DBFactory.GetDatabase.GetCommandFromParams(sqlQRY, Params))
        If Result IsNot Nothing Then
            Return Result.ToString
        Else
            Return ""
        End If
    End Function

    Public Function GetAttachmentCount(attachFolderUID As String, attachTable As AttachmentsBaseCols) As Integer
        Try
            Return CInt(GetSqlValue(attachTable.TableName, attachTable.FKey, attachFolderUID, "COUNT(*)"))
        Catch ex As Exception
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
            Return 0
        End Try
    End Function

    Public Function UpdateSqlValue(table As String, fieldIn As String, valueIn As String, idField As String, idValue As String) As Integer
        Return DBFactory.GetDatabase.UpdateValue(table, fieldIn, valueIn, idField, idValue)
    End Function

#End Region

End Class