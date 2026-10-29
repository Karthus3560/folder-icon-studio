using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace FolderIconStudio.Services;

public sealed class SingleInstanceService : IDisposable
{
    private const string MutexName = "FolderIconStudio_SingleInstance_Mutex_v1";
    private const string PipeName = "FolderIconStudio_Pipe_v1";

    private Mutex? _mutex;
    private CancellationTokenSource? _cts;
    private bool _isPrimary;

    public bool TryAcquirePrimary()
    {
        _mutex = new Mutex(true, MutexName, out var createdNew);
        _isPrimary = createdNew;
        if (_isPrimary)
            StartPipeServer();
        return _isPrimary;
    }

    public bool TrySendToPrimary(string[] args)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(3000);
            var payload = JsonSerializer.Serialize(args);
            var bytes = Encoding.UTF8.GetBytes(payload);
            client.Write(bytes, 0, bytes.Length);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void StartPipeServer()
    {
        _cts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    await server.WaitForConnectionAsync(_cts.Token);
                    using var ms = new MemoryStream();
                    await server.CopyToAsync(ms, _cts.Token);
                    var json = Encoding.UTF8.GetString(ms.ToArray());
                    var args = JsonSerializer.Deserialize<string[]>(json) ?? [];
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (System.Windows.Application.Current is App app)
                            app.HandleSecondaryInstance(args);
                    });
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    await Task.Delay(500, _cts.Token);
                }
            }
        }, _cts.Token);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        if (_isPrimary)
            _mutex?.ReleaseMutex();
        _mutex?.Dispose();
    }
}
