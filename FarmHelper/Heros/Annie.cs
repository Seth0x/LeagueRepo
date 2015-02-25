using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

namespace FarmHelper.Heros
{
    class Annie
    {
        public static Menu _root;
        public static Spell _q;
        public Annie()
        {
            _q = new Spell(SpellSlot.Q, 625f);
            _q.SetTargetted(0.25f, 1400);
            Game_OnGameLoad();
        }
        private static void Game_OnGameLoad()
        {
            _root = new Menu("FarmHelper", "FarmHelper", true);
            _root.SubMenu("FarmHelper").AddItem(new MenuItem("Enabled", "Enabled || Q").SetValue(true));
            _root.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_root.Item("Enabled").GetValue<bool>() == true)
            {
                var minions = MinionManager.GetMinions(_q.Range, MinionTypes.All, MinionTeam.NotAlly);
                foreach (var m in minions)
                {
                    if (ObjectManager.Player.Distance(m) <= _q.Range && _q.IsReady() && m.Health + 10 <= _q.GetDamage(m))
                    { _q.CastOnUnit(m, true); }
                }
            }
        }
    }
}
