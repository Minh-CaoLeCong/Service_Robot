Imports Microsoft.Expression.Encoder.Devices
Imports System.Collections.ObjectModel

Public Class MainWindow

    Public Property VideoDevices As Collection(Of EncoderDevice)
    Public Property AudioDevices As Collection(Of EncoderDevice)

    Public Sub New()
        InitializeComponent()

        DataContext = Me

        VideoDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video)
        AudioDevices = EncoderDevices.FindDevices(EncoderDeviceType.Audio)
    End Sub

    Private Sub StartCaptureButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        ' Display webcam video
        Try
            WebcamViewer.StartPreview()
        Catch ex As Microsoft.Expression.Encoder.SystemErrorException
            MessageBox.Show("Device is in use by another application")
        End Try
    End Sub

    Private Sub StopCaptureButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        ' Stop the display of webcam video
        WebcamViewer.StopPreview()
    End Sub

    Private Sub StartRecordingButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        ' Start recording of webcam video
        WebcamViewer.StartRecording()
    End Sub

    Private Sub StopRecordingButton_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        ' Stop recording of webcam video
        WebcamViewer.StopRecording()
    End Sub

    Private Sub TakeSnapshotButton_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        ' Take snapshot of webcam video
        WebcamViewer.TakeSnapshot()
    End Sub
End Class
