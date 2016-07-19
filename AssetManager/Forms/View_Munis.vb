﻿Public Class View_Munis
    Private Sub LoadMunisInventoryGrid(Device As Device_Info)


        Dim MunisTable As DataTable
        Dim strFields As String = "fama_asset,fama_status,fama_class,fama_subcl,fama_tag,fama_serial,fama_desc,fama_loc,fama_acq_dt,fama_fisc_yr,fama_pur_cost,fama_manuf,fama_model,fama_est_life,fama_repl_dt,fama_purch_memo"
        MunisTable = ReturnMSSQLTable("SELECT TOP 1 " & strFields & " FROM famaster WHERE fama_serial='" & Device.strSerial & "'")
        StartTimer()
        DataGridMunis_Inventory.DataSource = MunisTable
        StopTimer()
    End Sub
    Private Sub LoadMunisRequisitionGrid(Device As Device_Info)
        Try
            Dim strColumns As String = "rg_fiscal_year,a_requisition_no,LineNumber,rg_org,rg_object,rg_dollar_am,a_object_desc,a_org_description,RequisitionId,Quantity,UnitPrice,NetAmount,ItemDescription,SuggestedVendorId,PurchaseOrderNumber,PurchaseOrderDate"
            Dim strQRY As String = "SELECT " & strColumns & " FROM rq_gl_info, RequisitionItems WHERE a_requisition_no='" & Munis_GetReqNumberFromPO(Device.strPO) & "' AND rg_fiscal_year='" & Device.strFiscalYear & "' AND  PurchaseOrderNumber='" & Device.strPO & "' AND rg_line_number = LineNumber"
            'Debug.Print(strQRY)
            Dim results As DataTable

            results = ReturnMSSQLTable(strQRY)

            DataGridMunis_Requisition.DataSource = results
        Catch ex As Exception
            ErrHandleNew(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
        End Try
    End Sub
    Public Sub LoadMunisInfo(Device As Device_Info)
        If Device.strPO <> "" And YearFromDate(Device.dtPurchaseDate) <> "" Then 'if PO and Fiscal yr on record > load data using our records

            Device.strFiscalYear = YearFromDate(Device.dtPurchaseDate)
            LoadMunisInventoryGrid(Device)
            LoadMunisRequisitionGrid(Device)
            Me.Show()
        Else
            If Device.strPO = "" Then
                Dim PO As String = Munis_GetPOFromAsset(Device.strAssetTag)
                If PO <> "" Then Device.strPO = PO 'if some's missing > try to find it by other means
            End If
            If YearFromDate(Device.dtPurchaseDate) = "" Then
                Dim FY As String = Munis_GetFYFromAsset(Device.strAssetTag)
                If FY <> "" Then Device.strFiscalYear = FY



            End If




        End If




    End Sub
    Private Sub View_Munis_Load(sender As Object, e As EventArgs) Handles Me.Load
        ExtendedMethods.DoubleBuffered(DataGridMunis_Inventory, True)
        ExtendedMethods.DoubleBuffered(DataGridMunis_Requisition, True)
        MainForm.CopyDefaultCellStyles()
    End Sub
    Private Sub cmdSearch_Click(sender As Object, e As EventArgs) Handles cmdSearch.Click
        Dim MunisTable As DataTable
        MunisTable = ReturnMSSQLTable("SELECT TOP 10 * FROM famaster WHERE fama_serial='" & Trim(txtSerial.Text) & "'")
        Dim r As DataRow
        For Each r In MunisTable.Rows
            Debug.Print(r.Item("fama_asset"))
        Next
        DataGridMunis_Inventory.DataSource = MunisTable
    End Sub
End Class