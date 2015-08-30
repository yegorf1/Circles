﻿using Lidgren.Network;
using Lines.Network;
using Lines.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Lines.Network {
    public class LinesServer {
        private NetPeerConfiguration config;
        private NetServer server;
        private bool serverWorking;

        private Action<string> log;

        public List<Lobbie> Lobbies;

        public LinesServer() {
            config = NetworkGlobals.GetConfig();
            config.Port = NetworkGlobals.Port;

            server = new NetServer(config);
            Lobbies = new List<Lobbie>();
        }

        public void Start(Action<string> log) {
            this.log = log;

            server.Start();
            serverWorking = true;

            log("Server started at 0.0.0.0:" + NetworkGlobals.Port);
            while (serverWorking) {
                Update();
            }
        }

        private void Update() {
            NetIncomingMessage inc;
            while ((inc = server.ReadMessage()) != null) {
                switch (inc.MessageType) {
                    case NetIncomingMessageType.DiscoveryRequest:
                        log("Discovery request from " + inc.SenderEndPoint);

                        NetOutgoingMessage response = server.CreateMessage();
                        server.SendDiscoveryResponse(response, inc.SenderEndPoint);
                        break;

                    case NetIncomingMessageType.ConnectionApproval:
                        NetConnection conn = inc.SenderConnection;

                        log("Someone trying to connect from " + inc.SenderEndPoint);
                        if (Lobbies.Count == 0 || Lobbies[Lobbies.Count - 1].IsFull()) {
                            Lobbies.Add(new Lobbie(log, server));
                        }

                        log("Connected to " + Lobbies[Lobbies.Count - 1].GetHash());
                        log("Now we have " + (server.ConnectionsCount + 1) + " players");
                        
                        conn.Approve();
                        Lobbies[Lobbies.Count - 1].Connect(conn);
                        break;

                    case NetIncomingMessageType.Data:
                        ((Lobbie)inc.SenderConnection.Tag).WorkWithData(inc);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus s = (NetConnectionStatus)inc.ReadByte();
                        if (s == NetConnectionStatus.Disconnected) {
                            ((Lobbie)inc.SenderConnection.Tag).PlayerDisconnect();
                        }
                        break;
                }
            }

            foreach (Lobbie l in Lobbies) {
                l.Update();
            }
        }
    }
}
