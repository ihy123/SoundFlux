using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoundFlux.ViewModels
{
    internal enum ClientStatus
    {
        NotConnected,
        Connecting,
        Disconnecting,
        Connected
    }

    internal partial class ClientViewModel : ObservableObject
    {
        #region Output device

        [ObservableProperty]
        private Dictionary<int, DeviceInfo> outputDevices;

        [ObservableProperty]
        private KeyValuePair<int, DeviceInfo> selectedOutputDevice;

        public bool IsOutputDeviceDropDownOpen
        {
            set
            {
                if (value) Task.Run(RefreshOutputDevicesAsync);
            }
        }

        private readonly object refreshDevicesMutex = new();
        private void RefreshOutputDevicesAsync()
        {
            lock (refreshDevicesMutex)
            {
                var list = Client.OutputDevices;
                int idx = list.TryGetValue(SelectedOutputDevice.Key, out DeviceInfo val) ?
                    SelectedOutputDevice.Key : Client.DefaultOutputDeviceIndex;

                Dispatcher.UIThread.Post(() =>
                {
                    OutputDevices = list;
                    SelectedOutputDevice = new(idx, list[idx]);
                });
            }
        }

        #endregion

        #region Connection

        [ObservableProperty]
        private string? serverAddress;

        [ObservableProperty]
        private ClientStatus status = ClientStatus.NotConnected;

        [RelayCommand]
        private void ConnectAsync() => Task.Run(Connect);

        private void Connect()
        {
            switch (Status)
            {
                case ClientStatus.Connecting:
                case ClientStatus.Connected:
                    Status = ClientStatus.Disconnecting;
                    client.Stop();
                    Status = ClientStatus.NotConnected;
                    break;
                case ClientStatus.NotConnected:
                    // validate selected device
                    //if (OutputDevices == null)
                    //{
                    //    GlobalEvents.OnError(Resources.Resources.OutputDeviceNotSelectedError);
                    //    break;
                    //}

                    // validate server address
                    if (ServerAddress == null || !Utils.ValidateIpv4WithPort(ServerAddress))
                    {
                        GlobalEvents.OnError(string.Format(Resources.Resources.InvalidServerAddressError,
                            ServerAddress ?? ""));
                        break;
                    }

                    // check network connection
                    if (PlatformUtilities.Instance.NetworkInterfaceAddressList.Count == 0)
                    {
                        GlobalEvents.OnError(Resources.Resources.NetworkNotConnectedError);
                        break;
                    }

                    Status = ClientStatus.Connecting;

                    try
                    {
                        if (client.Start(SelectedOutputDevice.Key,
                            ServerAddress, PlatformUtilities.Instance.DeviceName,
                            (int)NetworkBufferDuration, (int)PlaybackBufferDuration, () =>
                            {
                                Task.Run(() =>
                                {
                                    Status = ClientStatus.NotConnected;
                                    client.Stop();
                                    Connect();
                                });
                            }))
                        {
                            Status = ClientStatus.Connected;
                            client.Volume = IsMuted ? 0 : volume / 100.0;
                            return;
                        }
                    }
                    catch (BassException e)
                    {
                        if (e.ErrorCode != Errors.FileOpen && e.ErrorCode != Errors.Timeout)
                            GlobalEvents.OnError(string.Format(Resources.Resources.BassErrorFormat, e.Message));
                    }
                    catch (Exception e)
                    {
                        GlobalEvents.OnError(string.Format(Resources.Resources.ExceptionFormat, e.Message));
                    }

                    // return back unconnected status on connection error
                    Status = ClientStatus.NotConnected;
                    break;
            }
        }

        #endregion

        #region Volume

        [RelayCommand]
        private void Mute()
        {
            IsMuted = !IsMuted;

            if (Status == ClientStatus.Connected)
                client.Volume = IsMuted ? 0 : volume / 100.0;
        }

        [ObservableProperty]
        private bool isMuted = false;

        private double volume = 100.0;
        public double Volume
        {
            get => volume;
            set
            {
                OnPropertyChanging();
                IsMuted = false;
                volume = value;
                if (Status == ClientStatus.Connected)
                    client.Volume = volume / 100.0;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Advanced settings

        public double ConnectTimeOut
        {
            get => Client.ConnectTimeOut / 1000;
            set
            {
                OnPropertyChanging();
                Client.ConnectTimeOut = 1000 * (int)value;
                OnPropertyChanged();
            }
        }
        public const double DefaultConnectTimeOut = 5;

        [ObservableProperty]
        private double networkBufferDuration = DefaultNetworkBufferDuration;
        public const double DefaultNetworkBufferDuration = 5000;

        [ObservableProperty]
        private double playbackBufferDuration = DefaultPlaybackBufferDuration;
        public const double DefaultPlaybackBufferDuration = 500;

        [RelayCommand]
        private void ResetAdvancedSettings()
        {
            ConnectTimeOut = DefaultConnectTimeOut;
            NetworkBufferDuration = DefaultNetworkBufferDuration;
            PlaybackBufferDuration = DefaultPlaybackBufferDuration;
        }

        #endregion

        #region Settings

        private void LoadSettings()
        {
            var sect = SharedSettings.Instance.GetSection("ClientViewModel");
            if (sect == null) return;

            int selectedDeviceIndex = sect.GetInt("SelectedDeviceIndex", Client.DefaultOutputDeviceIndex);
            int idx = OutputDevices.ContainsKey(selectedDeviceIndex) ?
                selectedDeviceIndex : Client.DefaultOutputDeviceIndex;
            SelectedOutputDevice = new(idx, OutputDevices[idx]);

            Volume = sect.GetDouble("Volume", Volume);
            IsMuted = sect.GetBool("IsMuted", IsMuted);
            ConnectTimeOut = sect.GetDouble("ConnectTimeOut", ConnectTimeOut);
            NetworkBufferDuration = sect.GetDouble("NetworkBufferDuration", NetworkBufferDuration);
            PlaybackBufferDuration = sect.GetDouble("PlaybackBufferDuration", PlaybackBufferDuration);

            ServerAddress = sect.Get("ServerAddress");
            if (!string.IsNullOrEmpty(ServerAddress))
                ConnectAsync();
        }

        private void SaveSettings()
        {
            var sect = SharedSettings.Instance.AddSection("ClientViewModel");
            sect.Add("SelectedDeviceIndex", SelectedOutputDevice.Key);
            sect.Add("IsMuted", IsMuted);
            sect.Add("Volume", Volume);
            sect.Add("ConnectTimeOut", ConnectTimeOut);
            sect.Add("NetworkBufferDuration", NetworkBufferDuration);
            sect.Add("PlaybackBufferDuration", PlaybackBufferDuration);
            sect.Add("ServerAddress",
                ServerAddress != null && (Status == ClientStatus.Connected || Status == ClientStatus.Connecting)
                ? ServerAddress : string.Empty);
        }

        #endregion

        private Client client = new();

        public ClientViewModel()
        {
            GlobalEvents.OnExitEvent += SaveSettings;
            OutputDevices = Client.OutputDevices;
            LoadSettings();
        }
    }
}
