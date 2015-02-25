using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common;
using LeagueSharp;

namespace FarmHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("<font color='#0066FF'>Seth </font>: FarmHelper from eXtravoz [ Loaded ]");

            if (ObjectManager.Player.BaseSkinName != "Annie")
            { new Heros.Annie(); }
            if (ObjectManager.Player.BaseSkinName != "Gangplank")
            { new Heros.Gangplank(); }
            if (ObjectManager.Player.BaseSkinName != "Veigar")
            { new Heros.Veigar(); }
            if (ObjectManager.Player.BaseSkinName != "LeBlanc")
            { new Heros.LeBlanc(); }
            if (ObjectManager.Player.BaseSkinName != "Akali")
            { new Heros.Akali(); }
            if (ObjectManager.Player.BaseSkinName != "Nasus")
            { new Heros.Nasus(); }
            if (ObjectManager.Player.BaseSkinName != "Ryze")
            { new Heros.Ryze(); }
        }
    }
}
