using ManagedBass;
using ManagedBass.Enc;
using System.Collections.Generic;
using System.Text;

namespace SoundFlux.Audio.DSP
{
    internal delegate bool EncoderServerCallback(bool isConnecting, string clientAddress, string[]? httpHeaders);

    internal class EncoderServer
    {
        public int Port { get; private set; }

        private EncoderServerCallback? callback;
        private EncodeClientProcedure? clientProc;
        private Encoder encoder;

        public EncoderServer(Encoder encoder, string port, int bufferDurationMs,
            int burstBytes, EncoderServerCallback? callback)
        {
            this.encoder = encoder;
            this.callback = callback;
            if (callback == null)
                clientProc = null;
            else
            {
                clientProc = (a, connecting, addr, hdrs, u) =>
                {
                    if (hdrs == 0)
                        return this.callback!(connecting, addr, null);

                    List<string> headers = new List<string>();

                    unsafe
                    {
                        bool isNull = false, prevIsNull = false;
                        byte* ptr = (byte*)hdrs;
                        for (int i = 0; ; ++i)
                        {
                            isNull = ptr[i] == 0;
                            if (isNull)
                            {
                                if (prevIsNull)
                                    break;
                                headers.Add(Encoding.ASCII.GetString(ptr, i));
                                ptr += i + 1;
                                i = 0;
                            }
                            prevIsNull = isNull;
                        }
                    }

                    return this.callback!(connecting, addr, headers.ToArray());
                };
            }

            int bufSize = (int)Bass.ChannelSeconds2Bytes(encoder.Stream.Handle, bufferDurationMs / 1000.0);
            if (bufSize == -1)
                throw new BassException();

            Port = BassEnc.ServerInit(encoder.Handle, port, bufSize, burstBytes, 0, clientProc, 0);
            if (Port == 0)
                throw new BassException();
        }

        public bool Kick(string clientAddress)
            => 0 != BassEnc.ServerKick(encoder.Handle, clientAddress);
    }
}
