using Avalonia.Data.Converters;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using SoundFlux.Audio.Device;
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
        private List<InputDevice>? inputDevices;

        private int selectedDeviceIndex = InputDevice.DefaultHandle;
        public int SelectedDeviceIndex
        {
            get => selectedDeviceIndex;
            set
            {
                SetProperty(ref selectedDeviceIndex, value);
                SetCurrentInputFormat();
            }
        }

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
                var list = InputDevice.List;
                int handle = Utils.HandleFromDeviceIndex(inputDevices,
                    selectedDeviceIndex, InputDevice.DefaultHandle);
                int newIdx = Utils.DeviceIndexFromHandle(
                    list, handle, InputDevice.DefaultHandle);
                Dispatcher.UIThread.Post(() =>
                {
                    InputDevices = list;
                    SelectedDeviceIndex = newIdx;
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
                addrs[i] = $"{addrs[i]}:{server.Port}";
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
            switch (status)
            {
                case ServerStatus.Started:
                case ServerStatus.Starting:
                    // terminate server
                    Status = ServerStatus.Terminating;
                    server.Stop();
                    clients.Clear();
                    addresses = null;
                    Status = ServerStatus.NotStarted;
                    break;
                case ServerStatus.NotStarted:
                    // validate selected device
                    if (inputDevices == null || selectedDeviceIndex < 0 ||
                        selectedDeviceIndex >= inputDevices.Count)
                    {
                        GlobalContext.OnError(Resources.Resources.InputDeviceNotSelectedError);
                        break;
                    }

                    // check network connection
                    var addrs = PlatformUtilities.Instance.NetworkInterfaceAddressList;
                    if (addrs.Count == 0)
                        GlobalContext.OnError(Resources.Resources.NetworkNotConnectedError);

                    // validate port
                    int portInt = Utils.TryParsePort(port);
                    if (portInt == -1)
                    {
                        Port = "0";
                        portInt = 0;
                    }

                    Status = ServerStatus.Starting;

                    try
                    {
                        if (server.Start(inputDevices[selectedDeviceIndex],
                            (int)serverBufferDuration, portInt, ClientCallback,
                            transmissionChannels, transmissionSampleRate, transmissionBitDepth != 16))
                        {
                            Status = ServerStatus.Started;
                            SetCurrentAddresses(addrs);
                            SetCurrentInputFormat();
                            return;
                        }
                    }
                    catch (BassException e)
                    {
                        GlobalContext.OnError(string.Format(Resources.Resources.BassErrorFormat, e.Message));
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

        public double RecordingPollingPeriod
        {
            get => server.RecordingPollingPeriod;
            set
            {
                OnPropertyChanging();
                server.RecordingPollingPeriod = (int)value;
                OnPropertyChanged();
            }
        }
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
            if (server == null || server.Device == null)
            {
                CurrentInputFormat = null;
                return;
            }

            string channels = "---";
            if (server.Device.Channels != 0)
                channels = ChannelCountToString(server.Device.Channels);
            else if (server.Stream != null && server.IsStarted)
                channels = ChannelCountToString(server.Stream.Channels);

            string sampleRate = "---";
            if (server.Device.SampleRate != 0)
                sampleRate = $"{server.Device.SampleRate} {Resources.Resources.HzSuffix}";
            else if (server.Stream != null && server.IsStarted)
                sampleRate = $"{server.Stream.SampleRate} {Resources.Resources.HzSuffix}";

            string bitDepth = (server.Stream != null && server.IsStarted) ?
                FloatSamplesFlagToString(server.Stream.FloatSamples) : "---";

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

            SelectedDeviceIndex = Utils.DeviceIndexFromHandle(inputDevices,
                sect.GetInt("SelectedDeviceHandle", InputDevice.DefaultHandle), selectedDeviceIndex);
            Port = sect.Get("Port", port);
            ServerBufferDuration = sect.GetDouble("ServerBufferDuration", serverBufferDuration);
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
            sect.Add("SelectedDeviceHandle", Utils.HandleFromDeviceIndex(
                inputDevices, selectedDeviceIndex, InputDevice.DefaultHandle));
            sect.Add("Port", port);
            sect.Add("ServerBufferDuration", serverBufferDuration);
            sect.Add("RecordingPollingPeriod", RecordingPollingPeriod);
            sect.Add("TransmissionChannels", transmissionChannels);
            sect.Add("TransmissionSampleRate", transmissionSampleRate);
            sect.Add("TransmissionBitDepth", transmissionBitDepth);
            sect.Add("IsStarted", status == ServerStatus.Started);
        }

        #endregion

        private Server server = new();

        public ServerViewModel()
        {
            RecordingPollingPeriod = DefaultRecordingPollingPeriod;

            GlobalContext.OnExitEvent += SaveSettings;
            InputDevices = InputDevice.List;
            LoadSettings();
        }
    }
}
