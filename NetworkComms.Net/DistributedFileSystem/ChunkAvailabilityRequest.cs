﻿// 
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet;
using ProtoBuf;
using NetworkCommsDotNet.Tools;

namespace DistributedFileSystem
{
    /// <summary>
    /// Used to classify the different types of ChunkAvailabilityReply in response to a ChunkAvailabilityRequest
    /// </summary>
    public enum ChunkReplyState : byte
    {
        /// <summary>
        /// Specifies that data will be included.
        /// </summary>
        DataIncluded,

        /// <summary>
        /// The item or requested chunk is not available
        /// </summary>
        ItemOrChunkNotAvailable,

        /// <summary>
        /// The contacted peer is currently busy, please try again later.
        /// </summary>
        PeerBusy
    }

    /// <summary>
    /// Wrapper used for requesting a chunk
    /// </summary>
    [ProtoContract]
    public class ChunkAvailabilityRequest
    {
        /// <summary>
        /// The checksum of the item being requested
        /// </summary>
        [ProtoMember(1)]
        public string ItemCheckSum { get; private set; }

        /// <summary>
        /// The index of the requested chunk
        /// </summary>
        [ProtoMember(2)]
        public byte ChunkIndex { get; private set; }

        /// <summary>
        /// The index of the request. Each request made by the DFS increments the request counter.
        /// </summary>
        [ProtoMember(3)]
        public long RequestNumIndex { get; private set; }

        /// <summary>
        /// The time this request was created
        /// </summary>
        public DateTime RequestCreationTime { get; private set; }

        /// <summary>
        /// The peer contacted for this request
        /// </summary>
        public ConnectionInfo PeerConnectionInfo { get; private set; }

        /// <summary>
        /// We are currently processing incoming data for this request.
        /// </summary>
        public bool RequestIncoming { get; set; }

        /// <summary>
        /// We have received data and this request is complete.
        /// </summary>
        public bool RequestComplete { get; set; }

        private ChunkAvailabilityRequest() { }

        /// <summary>
        /// Instantiate a new ChunkAvailabilityRequest
        /// </summary>
        /// <param name="itemCheckSum">The checksum of the DFS item</param>
        /// <param name="chunkIndex">The index of the requested chunk</param>
        /// <param name="peerConnectionInfo">The peer contacted for this request</param>
        /// <param name="requestNumIndex">The index of this chunk request</param>
        public ChunkAvailabilityRequest(string itemCheckSum, byte chunkIndex, ConnectionInfo peerConnectionInfo, long requestNumIndex)
        {
            this.ItemCheckSum = itemCheckSum;
            this.ChunkIndex = chunkIndex;
            this.RequestCreationTime = DateTime.Now;
            this.PeerConnectionInfo = peerConnectionInfo;
            this.RequestIncoming = false;
            this.RequestComplete = false;
            this.RequestNumIndex = requestNumIndex;
        }
    }

    /// <summary>
    /// A wrapper used to reply to a ChunkAvailabilityRequest
    /// </summary>
    [ProtoContract]
    public class ChunkAvailabilityReply
    {
        /// <summary>
        /// The checksum of the item being requested
        /// </summary>
        [ProtoMember(1)]
        public string ItemCheckSum { get; private set; }

        /// <summary>
        /// The index of the requested chunk
        /// </summary>
        [ProtoMember(2)]
        public byte ChunkIndex { get; private set; }

        /// <summary>
        /// The state of this reply
        /// </summary>
        [ProtoMember(3)]
        public ChunkReplyState ReplyState { get; private set; }

        /// <summary>
        /// The packet identifier used to send the chunk data
        /// </summary>
        [ProtoMember(4)]
        public string PacketIdentifier { get; private set; }

        /// <summary>
        /// The network identifier of the peer that generated this ChunkAvailabilityReply
        /// </summary>
        [ProtoMember(5)]
        public string SourceNetworkIdentifier { get; private set; }

        /// <summary>
        /// The connectionInfo of the peer that generated this ChunkAvailabilityReply
        /// </summary>
        public ConnectionInfo SourceConnectionInfo { get; private set; }

        /// <summary>
        /// The requested data
        /// </summary>
        public byte[] ChunkData { get; private set; }

        /// <summary>
        /// True once ChunkData has been set
        /// </summary>
        public bool ChunkDataSet { get; private set; }

        private ChunkAvailabilityReply() { }

        /// <summary>
        /// Create an ChunkAvailabilityReply which will not contain the requested data.
        /// </summary>
        /// <param name="sourceNetworkIdentifier">The network identifier of the source of this ChunkAvailabilityReply</param>
        /// <param name="itemCheckSum">The checksum of the DFS item</param>
        /// <param name="chunkIndex">The chunkIndex of the requested item</param>
        /// <param name="replyState">A suitable reply state</param>
        public ChunkAvailabilityReply(string sourceNetworkIdentifier, string itemCheckSum, byte chunkIndex, ChunkReplyState replyState)
        {
            this.SourceNetworkIdentifier = sourceNetworkIdentifier;
            this.ItemCheckSum = itemCheckSum;
            this.ChunkIndex = chunkIndex;
            this.ReplyState = replyState;
        }

        /// <summary>
        /// Create an ChunkAvailabilityReply which will precede the requested data.
        /// </summary>
        /// <param name="sourceNetworkIdentifier">The network identifier of the source of this ChunkAvailabilityReply</param>
        /// <param name="itemCheckSum">The checksum of the DFS item</param>
        /// <param name="chunkIndex">The chunkIndex of the requested item</param>
        /// <param name="packetIdentifier">The packet identifier used to send the data</param>
        public ChunkAvailabilityReply(ShortGuid sourceNetworkIdentifier, string itemCheckSum, byte chunkIndex, string packetIdentifier)
        {
            this.SourceNetworkIdentifier = sourceNetworkIdentifier;
            this.ItemCheckSum = itemCheckSum;
            this.ChunkIndex = chunkIndex;
            this.PacketIdentifier = packetIdentifier;
            this.ReplyState = ChunkReplyState.DataIncluded;
        }

        /// <summary>
        /// Set the data for this ChunkAvailabilityReply
        /// </summary>
        /// <param name="chunkData">The chunk data</param>
        public void SetChunkData(byte[] chunkData)
        {
            if (chunkData == null) throw new ArgumentNullException("chunkData cannot be null.");

            this.ChunkData = chunkData;
            ChunkDataSet = true;
        }

        /// <summary>
        /// Set the connectionInfo associated with the source of this ChunkAvailabilityReply
        /// </summary>
        /// <param name="info">The ConnectionInfo associated with the source of this ChunkAvailabilityReply</param>
        public void SetSourceConnectionInfo(ConnectionInfo info)
        {
            this.SourceConnectionInfo = info;
        }
    }

    /// <summary>
    /// Temporary storage for chunk data which is awaiting corresponding ChunkAvailabilityReply
    /// </summary>
    class ChunkDataWrapper
    {
        /// <summary>
        /// The packet identifier of the chunk data
        /// </summary>
        public string IncomingPacketIdentifier { get; private set; }

        /// <summary>
        /// The chunk data
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// The time this chunk data was received
        /// </summary>
        public DateTime TimeCreated { get; private set; }

        /// <summary>
        /// The ChunkAvailabilityReply associated with this chunk data
        /// </summary>
        public ChunkAvailabilityReply ChunkAvailabilityReply { get; private set; }

        /// <summary>
        /// Initialise a ChunkDataWrapper when the ChunkAvailabilityReply is received before associated data.
        /// </summary>
        /// <param name="chunkAvailabilityReply">The matching ChunkAvailabilityReply</param>
        public ChunkDataWrapper(ChunkAvailabilityReply chunkAvailabilityReply)
        {
            if (chunkAvailabilityReply == null)
                throw new Exception("Unable to create a ChunkDataWrapper with a null ChunkAvailabilityReply reference.");

            this.ChunkAvailabilityReply = chunkAvailabilityReply;
            this.TimeCreated = DateTime.Now;
        }

        /// <summary>
        /// Initialise a ChunkDataWrapper when the data is received before the associated ChunkAvailabilityReply.
        /// </summary>
        /// <param name="packetIdentifier">The packet identifier of the chunk data</param>
        /// <param name="data">The chunk data</param>
        public ChunkDataWrapper(string packetIdentifier, byte[] data)
        {
            this.IncomingPacketIdentifier = packetIdentifier;
            this.Data = data;
            this.TimeCreated = DateTime.Now;
        }
    }
}
