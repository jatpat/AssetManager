﻿Module DBFunctions
    Public Sub RefreshLocalDBCache()
        Try
            StartTimer()
            BuildingCache = True
            Using conn As New SQLite_Comms(False)
                conn.RefreshSQLCache()
            End Using
            BuildingCache = False
            StopTimer()
        Catch ex As Exception
            BuildingCache = False
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
        End Try
    End Sub

    Public Async Function RefreshLocalDBCacheAsync() As Task(Of Boolean)
        Try
            StartTimer()
            Dim Done As Boolean = False
            BuildingCache = True
            Done = Await Task.Run(Function()
                                          Try
                                              Using conn As New SQLite_Comms(False)
                                                  conn.RefreshSQLCache()
                                              End Using
                                              Return True
                                          Catch ex As Exception
                                              BuildingCache = False
                                              Return False
                                              ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
                                          End Try
                                      End Function)
            BuildingCache = False
            StopTimer()
            Return Done
        Catch ex As Exception
            BuildingCache = False
            ErrHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod())
        End Try
    End Function
    Public Async Function VerifyLocalCacheHash() As Task(Of Boolean)
        Dim HashesMatch As Boolean = False
        HashesMatch = Await Task.Run(Function()
                                         Dim LocalHashes As New List(Of String)
                                         Using SQLiteComms As New SQLite_Comms
                                             LocalHashes = SQLiteComms.LocalTableHashList
                                             Return SQLiteComms.CompareTableHashes(LocalHashes, RemoteTableHashes)
                                         End Using
                                     End Function)
        Return HashesMatch
    End Function

    Public Async Function VerifyLocalCacheAsync() As Task(Of Boolean)
        Dim HashesMatch As Boolean = False
        HashesMatch = Await Task.Run(Function()
                                         Try
                                             Dim LocalHashes As New List(Of String)
                                             Using SQLiteComms As New SQLite_Comms
                                                 If SQLiteComms.GetSchemaVersion > 0 Then
                                                     LocalHashes = SQLiteComms.LocalTableHashList
                                                     Return SQLiteComms.CompareTableHashes(LocalHashes, RemoteTableHashes)
                                                 Else
                                                     Return False
                                                 End If
                                             End Using
                                         Catch ex As Exception
                                             Return False
                                         End Try
                                     End Function)
        Return HashesMatch
    End Function
    Public Function VerifyLocalCache(Optional CheckHashes As Boolean = True) As Boolean
        Try
            Using SQLiteComms As New SQLite_Comms
                If SQLiteComms.GetSchemaVersion > 0 Then
                    If CheckHashes Then
                        Dim LocalHashes = SQLiteComms.LocalTableHashList
                        Return SQLiteComms.CompareTableHashes(LocalHashes, RemoteTableHashes)
                    Else
                        Return True
                    End If
                Else
                    Return False
                End If
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function
End Module