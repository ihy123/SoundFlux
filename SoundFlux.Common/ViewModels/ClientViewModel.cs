using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using SoundFlux.Audio.Device;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private OutputDevice? selectedDevice = null;

        public bool IsOutputDeviceDropDownOpen
        {
            set
            {
                if (value) Task.Run(RefreshOutputDevices);
            }
        }

        private void RefreshOutputDevices()
        {
            OutputDevices = OutputDevice.List;
            if (selectedDevice != null)
                SelectedDevice = outputDevices?.FirstOrDefault(dev => selectedDevice.Handle == dev.Handle);
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
                    if (selectedDevice == null)
                    {
                        GlobalContext.OnError(Resources.Resources.OutputDeviceNotSelectedError);
                        break;
                    }

                    // validate server address
                    if (serverAddress == null || !Utils.ValidateIpv4WithPort(serverAddress))
                    {
                        GlobalContext.OnError(string.Format(Resources.Resources.InvalidServerAddressError,
                            serverAddress == null ? "" : serverAddress));
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
                        if (client.Start(selectedDevice, serverAddress, PlatformUtilities.Instance.DeviceName,
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

            int selectedDeviceIndex = sect == null ? 0 : sect.GetInt("SelectedDeviceIndex");
            if (selectedDeviceIndex != 0)
                SelectedDevice = outputDevices?.FirstOrDefault(dev => dev.Handle == selectedDeviceIndex);
            else
                SelectedDevice = outputDevices?.FirstOrDefault(dev => dev.IsDefault);

            if (sect == null) return;

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
            sect.Add("SelectedDeviceIndex", selectedDevice == null ? 0 : selectedDevice.Handle);
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

        private Client client = new Client();

        public ClientViewModel()
        {
            GlobalContext.OnExitEvent += SaveSettings;
            RefreshOutputDevices();
            LoadSettings();
        }
    }
}
