﻿using Lines.Network;
using Lines.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lines.States {
    public class PreMainMenu : PreSelectState {
        private static PreMainMenu instance;

        public PreMainMenu(bool isOpening) :  base(
            Constants.COLORS[Constants.DRAW], 
            LinesGame.GameTitle, 
            LinesGame.LocalGame, 
            LinesGame.MultiplayerGame, 
            OnLocalGame, 
            OnMultiplayerGame, 
            isOpening) { instance = this; }

        public static void OnLocalGame() {
            LinesGame.CurrentState = new OpeningState(true);
        }

        public static void OnMultiplayerGame() {
            LinesGame.CurrentState = new PreSelectState(instance.color, LinesGame.Empty, LinesGame.LANGame, LinesGame.GlobalGame, OnLan, OnGlobal, true);
        }

        public static void OnLan() {
            LinesGame.CurrentState = new PreSelectState(instance.color, LinesGame.Empty, LinesGame.Host, LinesGame.Join, OnHost, OnJoin, true);
        }

        public static void OnHost() {
            if (LinesGame.server == null) {
                LinesGame.server = new LinesServer();
                LinesGame.server.StartInNewThread((s) => { System.Diagnostics.Debug.WriteLine("Server: " + s); });
            }
            OnJoin();
        }

        public static void OnJoin() {
            LinesGame.CurrentState = new WaitForServerState(true);
        }

        public static void OnGlobal() {
            LinesGame.CurrentState = new WaitForServerState(false);
        }
    }
}
