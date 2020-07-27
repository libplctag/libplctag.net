﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace libplctag
{
    public class Tag<Marshaller, T> : IDisposable
        where Marshaller : IMarshaller<T>, new()
    {

        Tag _tag;

        IMarshaller<T> _marshaller;

        /// <summary>
        /// Provides a new tag. If the CPU type is LGX, the port type and slot has to be specified.
        /// </summary>
        /// <param name="gateway">IP address of the gateway for this protocol. Could be the IP address of the PLC you want to access.</param>
        /// <param name="path">Required for LGX, Optional for PLC/SLC/MLGX IOI path to access the PLC from the gateway.
        /// <param name="plcType">Allen-Bradley CPU model</param>
        /// <param name="name">The textual name of the tag to access. The name is anything allowed by the protocol. E.g. myDataStruct.rotationTimer.ACC, myDINTArray[42] etc.</param>
        /// <param name="elementCount">elements count: 1- single, n-array.</param>
        /// <param name="millisecondTimeout"></param>
        /// <param name="protocol">Currently only ab_eip supported.</param>
        /// <param name="readCacheMillisecondDuration">Set the amount of time to cache read results</param>
        /// <param name="useConnectedMessaging">Control whether to use connected or unconnected messaging.</param>
        public Tag(IPAddress gateway,
                   string path,
                   PlcType plcType,
                   string name,
                   int millisecondTimeout,
                   int elementCount,
                   Protocol protocol = Protocol.ab_eip,
                   int readCacheMillisecondDuration = default,
                   bool useConnectedMessaging = true)
        {

            _marshaller = new Marshaller()
            {
                PlcType = plcType
            };

            _tag = new Tag(
                gateway,
                path,
                plcType,
                _marshaller.ElementSize,
                name,
                millisecondTimeout,
                elementCount,
                protocol,
                readCacheMillisecondDuration,
                useConnectedMessaging);


            Value = new T[elementCount];
            DecodeAll();
        }

        public Protocol Protocol => _tag.Protocol;
        public IPAddress Gateway => _tag.Gateway;
        public string Path => _tag.Path;
        public PlcType PlcType => _tag.PlcType;
        public int Count => _tag.ElementCount;
        public string Name => _tag.Name;
        public bool UseConnectedMessaging => _tag.UseConnectedMessaging;
        public int ReadCacheMillisecondDuration
        {
            get => _tag.ReadCacheMillisecondDuration;
            set => _tag.ReadCacheMillisecondDuration = value;
        }

        public async Task ReadAsync(int millisecondTimeout, CancellationToken token = default)
        {
            await _tag.ReadAsync(millisecondTimeout, token);
            DecodeAll();
        }

        public async Task ReadAsync(CancellationToken token = default)
        {
            await _tag.ReadAsync(token);
            DecodeAll();
        }

        public void Read(int millisecondTimeout)
        {
            _tag.Read(millisecondTimeout);
            DecodeAll();
        }

        public async Task WriteAsync(int millisecondTimeout, CancellationToken token = default)
        {
            await _tag.ReadAsync(millisecondTimeout, token);
            DecodeAll();
        }

        public async Task WriteAsync(CancellationToken token = default)
        {
            await _tag.ReadAsync(token);
            DecodeAll();
        }

        public void Write(int millisecondTimeout)
        {
            EncodeAll();
            _tag.Write(millisecondTimeout);
        }

        void DecodeAll()
        {
            int offset = 0;
            for (int ii = 0; ii < Value.Length; ii++)
            {
                Value[ii] = _marshaller.Decode(_tag, offset, out int elementSize);
                offset += elementSize;
            }
        }

        void EncodeAll()
        {
            int offset = 0;
            for (int ii = 0; ii < Value.Length; ii++)
            {
                _marshaller.Encode(_tag, offset, out int elementSize, Value[ii]);
                offset += elementSize;
            }
        }

        public Status GetStatus() => _tag.GetStatus();

        public void Dispose() => _tag.Dispose();

        ~Tag()
        {
            Dispose();
        }

        public T[] Value { get; set; }

    }
}