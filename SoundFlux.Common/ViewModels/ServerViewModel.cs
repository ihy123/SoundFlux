using Avalonia.Data.Converters;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoundFlux.ViewModels
{
    internal enum ServerStatus
    {
        Started,
        NotStarted,
        Terminating,
        Starting
    }

    internal partial class ServerViewModel : ObservableObject
    {
        #region Input device

        [ObservableProperty]
        private Dictionary<int, DeviceInfo> inputDevices;

        [ObservableProperty]
        private KeyValuePair<int, DeviceInfo> selectedInputDevice;

        public bool IsInputDeviceDropDownOpen
        {
            set
            {
                if (value) Task.Run(RefreshInputDevicesAsync);
            }
        }

        private readonly object refreshDevicesMutex = new();
        private void RefreshInputDevicesAsync()
        {
            lock (refreshDevicesMutex)
            {
                var list = Server.InputDevices;
                int idx = list.TryGetValue(SelectedInputDevice.Key, out DeviceInfo val) ?
                    SelectedInputDevice.Key : Server.DefaultInputDeviceIndex;

                Dispatcher.UIThread.Post(() =>
                {
                    InputDevices = list;
                    SelectedInputDevice = new(idx, list[idx]);
                });
            }
        }

        #endregion

        #region Addresses

        [ObservableProperty]
        private List<string>? addresses = null;

        private void SetCurrentAddresses(List<string> addrs)
        {
            for (int i = 0; i < addrs.Count; ++i)
                addrs[i] = $"{addrs[i]}:{server.CurrentPort}";
            Addresses = addrs;
        }

        #endregion

        #region Server

        [ObservableProperty]
        private ServerStatus status = ServerStatus.NotStarted;

        [RelayCommand]
        private void StartAsync() => Task.Run(Start);

        private void Start()
        {
            switch (Status)
            {
                case ServerStatus.Started:
                case ServerStatus.Starting:
                    // terminate server
                    Status = ServerStatus.Terminating;
                    server.Stop();
                    clients.Clear();
                    Addresses = null;
                    Status = ServerStatus.NotStarted;
                    break;
                case ServerStatus.NotStarted:
                    // validate selected device
                    //if (InputDevices == null || SelectedDeviceIndex < 0 ||
                    //    SelectedDeviceIndex >= InputDevices.Count)
                    //{
                    //    GlobalEvents.OnError(Resources.Resources.InputDeviceNotSelectedError);
                    //    break;
                    //}

                    // check network connection
                    var addrs = PlatformUtilities.Instance.NetworkInterfaceAddressList;
                    if (addrs.Count == 0)
                        GlobalEvents.OnError(Resources.Resources.NetworkNotConnectedError);

                    // validate port
                    string currentPort = Port;
                    if (Utils.TryParsePort(currentPort) == -1)
                        Port = currentPort = "0";

                    Status = ServerStatus.Starting;

                    try
                    {
                        if (server.Start(SelectedInputDevice.Key, (int)ServerBufferDuration,
                            currentPort, (int)RecordingPollingPeriod, ClientCallback,
                            TransmissionChannels, TransmissionSampleRate, TransmissionBitDepth != 16))
                        {
                            Status = ServerStatus.Started;
                            SetCurrentAddresses(addrs);
                            SetCurrentInputFormat();
                            return;
                        }
                    }
                    catch (BassException e)
                    {
                        GlobalEvents.OnError(string.Format(Resources.Resources.BassErrorFormat, e.Message));
                    }

                    // return back unconnected status on connection error
                    Status = ServerStatus.NotStarted;
                    break;
            }
            SetCurrentInputFormat();
        }

        #endregion

        #region Advanced settings

        [ObservableProperty]
        private string port = "0";

        [ObservableProperty]
        private string? currentInputFormat = null;

        [ObservableProperty]
        private double serverBufferDuration = DefaultServerBufferDuration;
        public const double DefaultServerBufferDuration = 2000;

        [ObservableProperty]
        private double recordingPollingPeriod = DefaultRecordingPollingPeriod;
        public const double DefaultRecordingPollingPeriod = 30;

        [ObservableProperty]
        private int transmissionChannels;

        public static readonly IValueConverter TransmissionChannelsConverter =
            new FuncValueConverter<int, string>(c =>
            {
                if (c == 1) return Resources.Resources.ChannelsConfigMono;
                else if (c == 2) return Resources.Resources.ChannelsConfigStereo;
                return Resources.Resources.Default;
            });

        [ObservableProperty]
        private int transmissionSampleRate;

        public static readonly IValueConverter TransmissionSampleRateConverter =
            new FuncValueConverter<int, string>(s =>
            {
                if (s == 0) return Resources.Resources.Default;
                return $"{s} {Resources.Resources.HzSuffix}";
            });

        [ObservableProperty]
        private int transmissionBitDepth;

        public static readonly IValueConverter TransmissionBitDepthConverter =
            new FuncValueConverter<int, string>(s =>
            {
                if (s == 0) return Resources.Resources.Default;
                return $"{s} {Resources.Resources.BitSuffix}";
            });

        private void SetCurrentInputFormat()
        {
            if (Status != ServerStatus.Started)
            {
                CurrentInputFormat = null;
                return;
            }

            server.GetSamplesInfo(out int streamChannels,
                out int streamSampleRate, out bool streamFloatSamples);

            string channels = ChannelCountToString(streamChannels);
            string sampleRate = $"{streamSampleRate} {Resources.Resources.HzSuffix}";
            string bitDepth = FloatSamplesFlagToString(streamFloatSamples);

            CurrentInputFormat = $"{channels}, {sampleRate}, {bitDepth}";
        }

        private static string ChannelCountToString(int channels)
        {
            switch (channels)
            {
                case 1: return Resources.Resources.ChannelsConfigMono;
                case 2: return Resources.Resources.ChannelsConfigStereo;
                default: return $"{channels} {Resources.Resources.ChannelCountSuffix}";
            }
        }

        private static string FloatSamplesFlagToString(bool floatSamples)
            => (floatSamples ? "32 " : "16 ") + Resources.Resources.BitSuffix;

        [RelayCommand]
        private void ResetAdvancedSettings()
        {
            ServerBufferDuration = DefaultServerBufferDuration;
            RecordingPollingPeriod = DefaultRecordingPollingPeriod;
            TransmissionChannels = TransmissionSampleRate = TransmissionBitDepth = 0;
            Port = "0";
        }

        #endregion

        #region Clients

        private Dictionary<string, string?> clients = new();

        private bool ClientCallback(bool isConnecting, string clientAddress, string? clientName)
        {
            // cancel client if there is already 3 or more connected clients
            if (clients.Count >= 3)
                return false;

            if (isConnecting)
                clients.Add(clientAddress, clientName);
            else
                clients.Remove(clientAddress);

            return true;
        }

        #endregion

        #region Settings

        private void LoadSettings()
        {
            var sect = SharedSettings.Instance.GetSection("ServerViewModel");
            if (sect == null) return;

            int selectedDeviceIndex = sect.GetInt("SelectedDeviceIndex", Server.DefaultInputDeviceIndex);
            int idx = InputDevices.ContainsKey(selectedDeviceIndex) ?
                selectedDeviceIndex : Server.DefaultInputDeviceIndex;
            SelectedInputDevice = new(idx, InputDevices[idx]);

            Port = sect.Get("Port", Port);
            ServerBufferDuration = sect.GetDouble("ServerBufferDuration", ServerBufferDuration);
            RecordingPollingPeriod = sect.GetDouble("RecordingPollingPeriod", DefaultRecordingPollingPeriod);

            TransmissionChannels = sect.GetInt("TransmissionChannels", 0);
            TransmissionSampleRate = sect.GetInt("TransmissionSampleRate", 0);
            TransmissionBitDepth = sect.GetInt("TransmissionBitDepth", 0);

            if (sect.GetBool("IsStarted"))
                StartAsync();
        }

        private void SaveSettings()
        {
            var sect = SharedSettings.Instance.AddSection("ServerViewModel");
            sect.Add("SelectedDeviceIndex", SelectedInputDevice.Key);
            sect.Add("Port", Port);
            sect.Add("ServerBufferDuration", ServerBufferDuration);
            sect.Add("RecordingPollingPeriod", RecordingPollingPeriod);
            sect.Add("TransmissionChannels", TransmissionChannels);
            sect.Add("TransmissionSampleRate", TransmissionSampleRate);
            sect.Add("TransmissionBitDepth", TransmissionBitDepth);
            sect.Add("IsStarted", Status == ServerStatus.Started);
        }

        #endregion

        private Server server = new();

        public ServerViewModel()
        {
            GlobalEvents.OnExitEvent += SaveSettings;
            InputDevices = Server.InputDevices;
            LoadSettings();
        }
    }
}
