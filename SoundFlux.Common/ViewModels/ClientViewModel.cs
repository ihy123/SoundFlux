using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using SoundFlux.Audio.Device;
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
        private List<OutputDevice>? outputDevices;

        [ObservableProperty]
        private int selectedDeviceIndex = OutputDevice.DefaultHandle;

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
                var list = OutputDevice.List;
                int handle = Utils.HandleFromDeviceIndex(outputDevices,
                    selectedDeviceIndex, OutputDevice.DefaultHandle);
                int newIdx = Utils.DeviceIndexFromHandle(
                    list, handle, OutputDevice.DefaultHandle);
                Dispatcher.UIThread.Post(() =>
                {
                    OutputDevices = list;
                    SelectedDeviceIndex = newIdx;
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
            switch (status)
            {
                case ClientStatus.Connecting:
                case ClientStatus.Connected:
                    Status = ClientStatus.Disconnecting;
                    client.Stop();
                    Status = ClientStatus.NotConnected;
                    break;
                case ClientStatus.NotConnected:
                    // validate selected device
                    if (outputDevices == null || selectedDeviceIndex < 0 ||
                        selectedDeviceIndex >= outputDevices.Count)
                    {
                        GlobalContext.OnError(Resources.Resources.OutputDeviceNotSelectedError);
                        break;
                    }

                    // validate server address
                    if (serverAddress == null || !Utils.ValidateIpv4WithPort(serverAddress))
                    {
                        GlobalContext.OnError(string.Format(Resources.Resources.InvalidServerAddressError,
                            serverAddress ?? ""));
                        break;
                    }

                    // check network connection
                    if (PlatformUtilities.Instance.NetworkInterfaceAddressList.Count == 0)
                    {
                        GlobalContext.OnError(Resources.Resources.NetworkNotConnectedError);
                        break;
                    }

                    Status = ClientStatus.Connecting;

                    try
                    {
                        if (client.Start(outputDevices[selectedDeviceIndex],
                            serverAddress, PlatformUtilities.Instance.DeviceName,
                            (int)networkBufferDuration, (int)playbackBufferDuration, () =>
                            {
                                Task.Run(() =>
                                {
                                    client.Stop();
                                    Status = ClientStatus.NotConnected;
                                    Connect();
                                });
                            }))
                        {
                            Status = ClientStatus.Connected;
                            return;
                        }
                    }
                    catch (BassException e)
                    {
                        if (e.ErrorCode != Errors.FileOpen && e.ErrorCode != Errors.Timeout)
                            GlobalContext.OnError(string.Format(Resources.Resources.BassErrorFormat, e.Message));
                    }
                    catch (Exception e)
                    {
                        GlobalContext.OnError(string.Format(Resources.Resources.ExceptionFormat, e.Message));
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
                client.Volume = volume / 100.0;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Advanced settings

        public double ConnectTimeOut
        {
            get => client.ConnectTimeOut / 1000;
            set
            {
                OnPropertyChanging();
                client.ConnectTimeOut = 1000 * (int)value;
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

            SelectedDeviceIndex = Utils.DeviceIndexFromHandle(outputDevices,
                sect.GetInt("SelectedDeviceHandle", OutputDevice.DefaultHandle), selectedDeviceIndex);
            Volume = sect.GetDouble("Volume", volume);
            IsMuted = sect.GetBool("IsMuted", isMuted);
            ConnectTimeOut = sect.GetDouble("ConnectTimeOut", ConnectTimeOut);
            NetworkBufferDuration = sect.GetDouble("NetworkBufferDuration", networkBufferDuration);
            PlaybackBufferDuration = sect.GetDouble("PlaybackBufferDuration", playbackBufferDuration);

            ServerAddress = sect.Get("ServerAddress");
            if (!string.IsNullOrEmpty(ServerAddress))
                ConnectAsync();
        }

        private void SaveSettings()
        {
            var sect = SharedSettings.Instance.AddSection("ClientViewModel");
            sect.Add("SelectedDeviceHandle", Utils.HandleFromDeviceIndex(
                outputDevices, selectedDeviceIndex, OutputDevice.DefaultHandle));
            sect.Add("IsMuted", isMuted);
            sect.Add("Volume", volume);
            sect.Add("ConnectTimeOut", ConnectTimeOut);
            sect.Add("NetworkBufferDuration", networkBufferDuration);
            sect.Add("PlaybackBufferDuration", playbackBufferDuration);
            sect.Add("ServerAddress",
                serverAddress != null && (status == ClientStatus.Connected || status == ClientStatus.Connecting)
                ? serverAddress : string.Empty);
        }

        #endregion

        private Client client = new();

        public ClientViewModel()
        {
            GlobalContext.OnExitEvent += SaveSettings;
            OutputDevices = OutputDevice.List;
            LoadSettings();
        }
    }
}
