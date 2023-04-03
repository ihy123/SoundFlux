using Avalonia.Data.Converters;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using SoundFlux.Services;
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

        public KeyValuePair<int, DeviceInfo> SelectedInputDevice
        {
            get => selectedInputDevice;
            set
            {
                OnPropertyChanging();
                selectedInputDevice = value;
                server.NextInputDeviceIndex = selectedInputDevice.Key;
                OnPropertyChanged();
            }
        }
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
                var list = AudioDeviceEnumerator.InputDevices;
                int idx = list.ContainsKey(SelectedInputDevice.Key) ?
                    SelectedInputDevice.Key : AudioDeviceEnumerator.DefaultInputDeviceIndex;

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

        private Server server;

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
                    // check network connection
                    //var addrs = ServiceRegistry.NetHelper.NetworkInterfaceAddressList;
                    //if (addrs.Count == 0)
                    //{
                    //    ServiceRegistry.ErrorHandler.Error(Resources.Resources.NetworkNotConnectedError);
                    //    return;
                    //}

                    // validate port
                    if (Utils.TryParsePort(Port) == -1)
                        Port = "0";

                    Status = ServerStatus.Starting;

                    try
                    {
                        if (server.Start(ClientCallback))
                        {
                            Status = ServerStatus.Started;
                            SetCurrentAddresses(ServiceRegistry.NetHelper.NetworkInterfaceAddressList);
                            Port = server.CurrentPort.ToString();
                            break;
                        }
                    }
                    catch (BassException e)
                    {
                        ServiceRegistry.ErrorHandler.Error(string.Format(
                            Resources.Resources.BassErrorFormat, e.Message));
                    }

                    // return back unconnected status on connection error
                    Status = ServerStatus.NotStarted;
                    break;
            }
            SetCurrentInputFormat();
        }

        #endregion

        #region Advanced settings

        public string Port
        {
            get => server.NextPort;
            set
            {
                OnPropertyChanging();
                server.NextPort = value;
                OnPropertyChanged();
            }
        }

        public double ServerBufferDuration
        {
            get => server.ServerBufferDuration;
            set
            {
                OnPropertyChanging();
                server.ServerBufferDuration = value;
                OnPropertyChanged();
            }
        }

        public double RecordingPollingPeriod
        {
            get => server.RecordingPollingPeriod;
            set
            {
                OnPropertyChanging();
                server.RecordingPollingPeriod = value;
                OnPropertyChanged();
            }
        }

        public string? CurrentInputFormat
        {
            get
            {
                if (Status != ServerStatus.Started)
                    return null;

                server.GetSamplesInfo(out int streamChannels,
                    out int streamSampleRate, out bool streamFloatSamples);

                string channels = ChannelCountToString(streamChannels);
                string sampleRate = $"{streamSampleRate} {Resources.Resources.HzSuffix}";
                string bitDepth = FloatSamplesFlagToString(streamFloatSamples);

                return $"{channels}, {sampleRate}, {bitDepth}";
            }
        }

        public int TransmissionChannels
        {
            get => server.TransmissionChannels;
            set
            {
                OnPropertyChanging();
                server.TransmissionChannels = value;
                OnPropertyChanged();
            }
        }

        public static readonly IValueConverter TransmissionChannelsConverter =
            new FuncValueConverter<int, string>(c =>
            {
                if (c == 1) return Resources.Resources.ChannelsConfigMono;
                else if (c == 2) return Resources.Resources.ChannelsConfigStereo;
                return Resources.Resources.Default;
            });

        public int TransmissionSampleRate
        {
            get => server.TransmissionSampleRate;
            set
            {
                OnPropertyChanging();
                server.TransmissionSampleRate = value;
                OnPropertyChanged();
            }
        }

        public static readonly IValueConverter TransmissionSampleRateConverter =
            new FuncValueConverter<int, string>(s =>
            {
                if (s == 0) return Resources.Resources.Default;
                return $"{s} {Resources.Resources.HzSuffix}";
            });

        public int TransmissionBitDepth
        {
            get => transmissionBitDepth;
            set
            {
                OnPropertyChanging();
                transmissionBitDepth = value;
                server.TransmissionFloatSamples = transmissionBitDepth != 16;
                OnPropertyChanged();
            }
        }
        private int transmissionBitDepth;

        public static readonly IValueConverter TransmissionBitDepthConverter =
            new FuncValueConverter<int, string>(s =>
            {
                if (s == 0) return Resources.Resources.Default;
                return $"{s} {Resources.Resources.BitSuffix}";
            });

        private void SetCurrentInputFormat()
            => OnPropertyChanged(nameof(CurrentInputFormat));

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
            ServerBufferDuration = Server.DefaultServerBufferDuration;
            RecordingPollingPeriod = Server.DefaultRecordingPollingPeriod;
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
            var sm = ServiceRegistry.SettingsManager;
            if (sm.Get("Server", "IsStarted", false))
                StartAsync();
        }

        public void SaveSettings()
        {
            var sm = ServiceRegistry.SettingsManager;
            sm.Set("Server", "IsStarted", Status == ServerStatus.Started);
        }

        #endregion

#pragma warning disable CS8618
        public ServerViewModel(Server server)
        {
            this.server = server;
            InputDevices = AudioDeviceEnumerator.InputDevices;
            LoadSettings();

            TransmissionBitDepth = server.TransmissionFloatSamples ? 0 : 16;
            if (InputDevices.TryGetValue(server.NextInputDeviceIndex, out var info))
                SelectedInputDevice = new(server.NextInputDeviceIndex, info);
        }
#pragma warning restore CS8618
    }
}
