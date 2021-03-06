﻿Imports System.Environment
Imports System.Deployment.Application

Module Paths

    'Application paths
    Public ReadOnly strAppDir As String = GetFolderPath(SpecialFolder.ApplicationData) & "\AssetManager\"

    Public Const strLogName As String = "log.log"
    Public ReadOnly strLogPath As String = strAppDir & strLogName
    Public ReadOnly DownloadPath As String = strAppDir & "temp\"

    'SQLite DB paths

    Public ReadOnly strSQLiteDBName As String = "cache" & IIf(Not ApplicationDeployment.IsNetworkDeployed, "_DEBUG", "").ToString & ".db"

    Public ReadOnly strSQLitePath As String = strAppDir & "SQLiteCache\" & strSQLiteDBName
    Public ReadOnly strSQLiteDir As String = strAppDir & "SQLiteCache\"

    'Gatekeeper package paths
    Public Const GKInstallDir As String = "C:\PSi\Gatekeeper"

    Public Const GKPackFileName As String = "GatekeeperPack.gz"
    Public Const GKPackHashName As String = "hash.md5"
    Public ReadOnly GKPackFileFDir As String = strAppDir & "GKUpdateFiles\PackFile\"
    Public ReadOnly GKPackFileFullPath As String = GKPackFileFDir & GKPackFileName
    Public ReadOnly GKExtractDir As String = strAppDir & "GKUpdateFiles\Gatekeeper\"
    Public Const GKRemotePackFileDir As String = "\\core.co.fairfield.oh.us\dfs1\fcdd\files\Information Technology\Software\Other\GatekeeperPackFile\"
    Public Const GKRemotePackFilePath As String = GKRemotePackFileDir & GKPackFileName

End Module