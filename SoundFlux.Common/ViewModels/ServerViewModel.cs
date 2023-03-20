using Avalonia.Data.Converters;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using SoundFlux.Audio.Device;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    internal class ClientInfo // partial : ObservableObject
    {
        public string? Name { get; set; }

        public string Address
        {
            get => address;
            set
            {
                address = value;
                ip = address.Substring(0, address.IndexOf(':'));
            }
        }

        //private Server server;
        private string address, ip;

        public ClientInfo(Server server, string? name, string address)
        {
            Name = name;
            Address = address;
            //this.server = server;
        }

        //[RelayCommand]
        //private void Kick()
        //    => server.Kick(Address);

        public bool CompareIp(ClientInfo other) => ip == other.ip;
    }

    internal partial class ServerViewModel : ObservableObject
    {
        #region Input device

        [ObservableProperty]
        private ObservableCollection<InputDevice> inputDevices;

        private InputDevice? selectedDevice;
        public InputDevice? SelectedDevice
        {
            get => selectedDevice;
            set
            {
                OnPropertyChanging();
                selectedDevice = value;
                OnPropertyChanged();
                SetCurrentInputFormat();
            }
        }

        public bool IsInputDeviceDropDownOpen
        {
            set
            {
                if (value) Task.Run(RefreshInputDevices);
            }
        }

        private void RefreshInputDevices()
        {
            var prevSelected = SelectedDevice;
            InputDevices = new(InputDevice.List);
            if (prevSelected != null)
                SelectedDevice = inputDevices?.First(dev => prevSelected.Handle == dev.Handle);
        }

        #endregion

        #region Addresses

        [ObservableProperty]
        private ObservableCollection<string> addresses = new();

        private void SetCurrentAddresses(List<string> addrs)
        {
            addresses.Clear();
            foreach (var a in addrs)
                addresses.Add($"{a}:{server.Port}");
        }

        #endregion

        #region Server

        [ObservableProperty]
        private ServerStatus status = ServerStatus.NotStarted;

        [RelayCommand]
        public void Start()
        {
            switch (status)
            {
                case ServerStatus.Started:
                case ServerStatus.Starting:
                    // terminate server
                    Status = ServerStatus.Terminating;
                    server.Stop();
                    addresses.Clear();
                    clients.Clear();
                    Status = ServerStatus.NotStarted;
                    break;
                case ServerStatus.NotStarted:
                    // validate selected device
                    if (selectedDevice == null)
                    {
                        GlobalContext.OnError(Resources.Resources.InputDeviceNotSelectedError);
                        break;
                    }

                    // check network connection
                    var addrs = PlatformUtilities.Instance.GetNetworkInterfaceAddressList();
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
                        if (server.Start(selectedDevice, (int)serverBufferDuration, portInt, ClientCallback,
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

        private string ChannelCountToString(int channels)
        {
            switch (channels)
            {
                case 1: return Resources.Resources.ChannelsConfigMono;
                case 2: return Resources.Resources.ChannelsConfigStereo;
                default: return $"{channels} {Resources.Resources.ChannelCountSuffix}";
            }
        }

        private string FloatSamplesFlagToString(bool floatSamples)
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

        [ObservableProperty]
        private ObservableCollection<ClientInfo> clients = new();

        private bool ClientCallback(bool isConnecting, string clientAddress, string? clientName)
        {
            // cancel client if there is already 3 or more connected clients
            if (clients.Count >= 3)
                return false;

            Dispatcher.UIThread.Post(() =>
            {
                if (isConnecting)
                {
                    ClientInfo newClient = new ClientInfo(server, clientName, clientAddress);
                    for (int i = 0; i < clients.Count; ++i)
                    {
                        if (clients[i].CompareIp(newClient))
                        {
                            clients[i] = newClient;
                            return;
                        }
                    }
                    clients.Add(newClient);
                }
                else
                {
                    var info = clients.FirstOrDefault(c => c.Address == clientAddress);
                    if (info != null)
                        clients.Remove(info);
                }
            });

            return true;
        }

        #endregion

        #region Settings

        private void LoadSettings()
        {
            var sect = SharedSettings.Instance.GetSection("ServerViewModel");

            int selectedDeviceIndex = sect == null ? 0 : sect.GetInt("SelectedDeviceIndex");
            if (selectedDeviceIndex != 0)
                SelectedDevice = inputDevices.FirstOrDefault(dev => dev.Handle == selectedDeviceIndex);
            else
                SelectedDevice = inputDevices.FirstOrDefault(dev => dev.IsDefault);

            if (sect == null) return;

            Port = sect.Get("Port", port);
            ServerBufferDuration = sect.GetDouble("ServerBufferDuration", serverBufferDuration);
            RecordingPollingPeriod = sect.GetDouble("RecordingPollingPeriod", DefaultRecordingPollingPeriod);

            TransmissionChannels = sect.GetInt("TransmissionChannels", 0);
            TransmissionSampleRate = sect.GetInt("TransmissionSampleRate", 0);
            TransmissionBitDepth = sect.GetInt("TransmissionBitDepth", 0);

            if (sect.GetBool("IsStarted"))
                Start();
        }

        private void SaveSettings()
        {
            var sect = SharedSettings.Instance.AddSection("ServerViewModel");
            sect.Add("SelectedDeviceIndex", selectedDevice == null ? 0 : selectedDevice.Handle);
            sect.Add("Port", port);
            sect.Add("ServerBufferDuration", serverBufferDuration);
            sect.Add("RecordingPollingPeriod", RecordingPollingPeriod);
            sect.Add("TransmissionChannels", transmissionChannels);
            sect.Add("TransmissionSampleRate", transmissionSampleRate);
            sect.Add("TransmissionBitDepth", transmissionBitDepth);
            sect.Add("IsStarted", status == ServerStatus.Started);
        }

        #endregion

        private Server server = new Server();

        public ServerViewModel()
        {
            RecordingPollingPeriod = DefaultRecordingPollingPeriod;

            GlobalContext.OnExitEvent += SaveSettings;
            RefreshInputDevices();
            LoadSettings();
        }
    }
}
