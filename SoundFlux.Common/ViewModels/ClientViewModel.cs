using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using SoundFlux.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

    internal partial class ClientViewModel : ObservableObject, INotifyDataErrorInfo
    {
        #region Output device

        [ObservableProperty]
        private Dictionary<int, DeviceInfo> outputDevices;

        public KeyValuePair<int, DeviceInfo> SelectedOutputDevice
        {
            get => selectedOutputDevice;
            set
            {
                OnPropertyChanging();
                selectedOutputDevice = value;
                client.NextOutputDeviceIndex = selectedOutputDevice.Key;
                OnPropertyChanged();
            }
        }
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
                var list = AudioDeviceEnumerator.OutputDevices;
                int idx = list.ContainsKey(SelectedOutputDevice.Key) ?
                    SelectedOutputDevice.Key : AudioDeviceEnumerator.DefaultOutputDeviceIndex;

                Dispatcher.UIThread.Post(() =>
                {
                    OutputDevices = list;
                    SelectedOutputDevice = new(idx, list[idx]);
                });
            }
        }

        #endregion

        #region Connection

        public string? ServerAddress
        {
            get => serverAddress;
            set
            {
                if (SetProperty(ref serverAddress, value) && flagValidateServerAddress)
                    ValidateServerAddress();
            }
        }
        private string? serverAddress;

        [ObservableProperty]
        private ClientStatus status = ClientStatus.NotConnected;

        private Client client;

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
                    if (!ValidateServerAddress())
                        break;

                    // check network connection
                    //if (ServiceRegistry.NetHelper.NetworkInterfaceAddressList.Count == 0)
                    //{
                    //    ServiceRegistry.ErrorHandler.Error(Resources.Resources.NetworkNotConnectedError);
                    //    break;
                    //}

                    Status = ClientStatus.Connecting;

                    try
                    {
                        if (client.Start(ServerAddress!, ServiceRegistry.NetHelper.DeviceName, () =>
                            {
                                Task.Run(() =>
                                {
                                    if (Status == ClientStatus.Connected)
                                    {
                                        Status = ClientStatus.NotConnected;
                                        client.Stop();
                                        Connect();
                                    }
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
                            ServiceRegistry.ErrorHandler.Error(string.Format(
                                Resources.Resources.BassErrorFormat, e.Message));
                    }
                    catch (Exception e)
                    {
                        ServiceRegistry.ErrorHandler.Error(string.Format(
                            Resources.Resources.ExceptionFormat, e.Message));
                    }

                    // return back unconnected status on connection error
                    Status = ClientStatus.NotConnected;
                    break;
            }
        }

        #endregion

        #region Volume

        [RelayCommand]
        private void Mute() => IsMuted = !IsMuted;

        public bool IsMuted
        {
            get => client.IsMuted;
            set
            {
                if (client.IsMuted != value)
                {
                    OnPropertyChanging();
                    client.IsMuted = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Volume
        {
            get => client.Volume * 100.0;
            set
            {
                OnPropertyChanging();
                if (IsMuted) IsMuted = false;
                client.Volume = value / 100.0;
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

        public double NetworkBufferDuration
        {
            get => Client.NetworkBufferDuration;
            set
            {
                OnPropertyChanging();
                Client.NetworkBufferDuration = (int)value;
                OnPropertyChanged();
            }
        }

        public double PlaybackBufferDuration
        {
            get => Client.PlaybackBufferDuration;
            set
            {
                OnPropertyChanging();
                Client.PlaybackBufferDuration = (int)value;
                OnPropertyChanged();
            }
        }

        [RelayCommand]
        private void ResetAdvancedSettings()
        {
            ConnectTimeOut = Client.DefaultConnectTimeOut / 1000.0;
            NetworkBufferDuration = Client.DefaultNetworkBufferDuration;
            PlaybackBufferDuration = Client.DefaultPlaybackBufferDuration;
        }

        #endregion

        #region Settings

        private void LoadSettings()
        {
            ServerAddress = ServiceRegistry.SettingsManager.Get("Client", "ServerAddress", null);
            if (!string.IsNullOrEmpty(ServerAddress))
                ConnectAsync();
        }

        public void SaveSettings()
        {
            ServiceRegistry.SettingsManager.Set("Client", "ServerAddress", ServerAddress != null &&
                (Status == ClientStatus.Connected || Status == ClientStatus.Connecting)
                ? ServerAddress : string.Empty);
        }

        #endregion

        #region Validation

        private bool flagValidateServerAddress = false;
        private bool ValidateServerAddress()
        {
            bool f = flagValidateServerAddress;

            if (ServerAddress == null || !Utils.ValidateIpv4WithPort(ServerAddress))
                flagValidateServerAddress = true;
            else if (flagValidateServerAddress)
                flagValidateServerAddress = false;

            if (f != flagValidateServerAddress)
                ErrorsChanged?.Invoke(this, new(nameof(ServerAddress)));

            return !flagValidateServerAddress;
        }

        public bool HasErrors => throw new NotImplementedException();

        public IEnumerable GetErrors(string? propertyName)
        {
            switch (propertyName)
            {
                case nameof(ServerAddress):
                    if (flagValidateServerAddress)
                        return new[] { string.Format(
                            Resources.Resources.InvalidServerAddressError, ServerAddress ?? "") };
                    break;
            }
            return null!;
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        #endregion

#pragma warning disable CS8618
        public ClientViewModel(Client client)
        {
            this.client = client;
            OutputDevices = AudioDeviceEnumerator.OutputDevices;
            LoadSettings();

            if (OutputDevices.TryGetValue(client.NextOutputDeviceIndex, out var info))
                SelectedOutputDevice = new(client.NextOutputDeviceIndex, info);
        }
#pragma warning restore CS8618
    }
}
