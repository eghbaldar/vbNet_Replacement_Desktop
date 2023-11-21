Imports System.Net
Imports System.IO.Compression
Imports System.IO
Imports System.Diagnostics
Imports System.ComponentModel
Imports System.Data.SqlClient

Public Class Form1

    Dim path As String
    Dim pathExtract As String = "C:\iranfilmport-temporary-replacement\"
    Dim copyDestination As String
    Dim filenameFromDataBase As String
    Dim directoryUnzipPath As String
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        checkDownloadedFile()
        '''''''''''''''''''''''''''''''''
        Dim pr As New Process
        For Each Process In pr.GetProcesses(My.Computer.Name)
            If Process.ProcessName = "IFP" Then
                Process.Kill()
                Exit For
            End If
        Next
        '''''''''''''''''''''''''''''''''
        Dim _uri As New Uri(GetLink())
        filenameFromDataBase = _uri.Segments(_uri.Segments.Length - 1)
        directoryUnzipPath = filenameFromDataBase.Split(".")(0)
        '''''''''''''''''''''''''''''''''
        path = "C:\iranfilmport-temporary-replacement\" + filenameFromDataBase
        copyDestination = readExePath()
        UnZip()
    End Sub

    Private Sub checkDownloadedFile()
        Dim di As New IO.DirectoryInfo(pathExtract)
        If Not di.Exists Then
            MessageBox.Show("The file is not downloaded yet!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End
        End If
    End Sub

    Dim sqlconn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("iranfilmportConnectionString").ConnectionString)
    Public Function GetLink() As String
        Try
            If sqlconn.State = ConnectionState.Open Then sqlconn.Close()
            sqlconn.Open()
            Dim sqlcom As New SqlCommand("select [winapp] from tbl_setting", sqlconn)
            Return sqlcom.ExecuteScalar
            sqlconn.Close()
        Catch ex As Exception
        Finally
            sqlconn.Close()
        End Try
    End Function

    Function readExePath() As String

        Dim line As String
        Dim FilePath As String = "C:\iranfilmport-temporary-replacement\" & "exePath.txt"
        ' Create new StreamReader instance with Using block.
        Using reader As StreamReader = New StreamReader(FilePath)
            ' Read one line from file
            line = reader.ReadLine
        End Using
        lblPathApplication.Text = line
        Return line

    End Function

    Private Sub UnZip()
        Call RunProcessing()
    End Sub

    'Private Function setLoading(sourcepath As String) As Integer
    '    Return IO.Directory.GetFiles(sourcepath, "*.*", SearchOption.AllDirectories).Length
    'End Function

    Sub RunProcessing()
        BackgroundWorker.WorkerSupportsCancellation = True
        BackgroundWorker.RunWorkerAsync()
        If BackgroundWorker.IsBusy Then
            BackgroundWorker.CancelAsync()
        End If
    End Sub

    Private Sub BackgroundWorker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles BackgroundWorker.ProgressChanged
        ProgressBar.Value = e.ProgressPercentage
    End Sub

    Private Sub BackgroundWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker.DoWork
        Threading.Thread.Sleep(2000)
        Try
            'unziping ....
            Dim zip As ZipArchive
            Try
                zip = ZipFile.OpenRead(path)
            Catch ex As Exception
                Windows.Forms.MessageBox.Show("Error : " & ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.[Error])
            Finally
                zip.Dispose()
            End Try
            Try
                ZipFile.ExtractToDirectory(path, pathExtract)
            Catch ex As Exception
                Windows.Forms.MessageBox.Show("Error : " & ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.[Error])
            Finally
            End Try
            'Get Directories and Files
            Dim sourcepath As String = pathExtract + directoryUnzipPath
            'setLoading(sourcepath)
            Dim DestPath As String = copyDestination
            'Delete the original files and directories
            For Each file As IO.FileInfo In New IO.DirectoryInfo(DestPath).GetFiles
                System.IO.File.Delete(DestPath & "\" & file.Name)
            Next
            For Each directory As IO.DirectoryInfo In New IO.DirectoryInfo(DestPath).GetDirectories
                If directory.Name.ToLower <> "replacement" Then
                    For Each file As IO.FileInfo In New IO.DirectoryInfo(DestPath & "\" & directory.Name).GetFiles
                        System.IO.File.Delete(DestPath & "\" & directory.Name & "\" & file.Name)
                    Next
                    System.IO.Directory.Delete(DestPath & "\" & directory.Name)
                End If
            Next
            'Copying
            For Each _directory As IO.DirectoryInfo In New DirectoryInfo(sourcepath).GetDirectories
                If _directory.Name.ToLower <> "replacement" Then
                    IO.Directory.CreateDirectory(DestPath & "\" & _directory.Name)
                    For Each file As IO.FileInfo In _directory.GetFiles
                        file.CopyTo(DestPath & "\" & _directory.Name & "\" & file.Name)
                    Next
                End If
            Next
            For Each file As IO.FileInfo In New IO.DirectoryInfo(sourcepath).GetFiles
                file.CopyTo(DestPath & "\" & file.Name)
            Next
            'Delete the temporary files and directories
            For Each d In Directory.GetDirectories(pathExtract)
                Directory.Delete(d, True)
            Next
            For Each f In Directory.GetFiles(pathExtract) 'Finish removing also the files in the root folder
                File.Delete(f)
            Next
            System.IO.Directory.Delete(pathExtract, True) 'delete the main folder
            'close automatically application
            'System.Threading.Thread.Sleep(5000)
            Process.Start(copyDestination & "/IFP.exe")
            End

        Catch ex As Exception
            Windows.Forms.MessageBox.Show("Error : " & vbCrLf & "There is a problem with updating, you'll be closed automatically" & vbCrLf & ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.[Error])
            End
        End Try
    End Sub

End Class
