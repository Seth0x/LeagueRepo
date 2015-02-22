using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common.Data;
using LeagueSharp.Common;
using LeagueSharp;

namespace Volibear
{
    class Program
    {
        public const string ChampionName = "Volibear";

        public static Menu _root;
        public static Spell _q, _w, _e, _r;
        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> _spells = new List<Spell>();

        public static SpellSlot _igniteSlot;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoaded;
        }

        private static void Game_OnGameLoaded(EventArgs args)
        {

            if (ObjectManager.Player.BaseSkinName != ChampionName) return;
            Console.WriteLine("Loaded after champion check!");

            _q = new Spell(SpellSlot.Q, 0f);
            _w = new Spell(SpellSlot.W, 350f);
            _e = new Spell(SpellSlot.E, 425f);
            _r = new Spell(SpellSlot.R, 0f);

            _igniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            _spells.Add(_q);
            _spells.Add(_w);
            _spells.Add(_e);
            _spells.Add(_r);

            _root = new Menu("SethVolibear", "Leona", true);
            _root.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(_root.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _root.AddSubMenu(targetSelectorMenu);

            _root.AddSubMenu(new Menu("Spell Handler", "SpellsM"));
            _root.SubMenu("SpellsM").AddItem(new MenuItem("Combo", "Combo"));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            _root.SubMenu("UseWCombo").AddItem(new MenuItem("EnemyHPW", "Enemy % HP to use W").SetValue(new Slider(100, 0, 100)));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            _root.SubMenu("Combo").AddItem(new MenuItem("UseIgniteCombo", "Use Ignite").SetValue(true));
            _root.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "Combo!").SetValue(
                        new KeyBind(_root.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            _root.SubMenu("SpellsM").AddItem(new MenuItem("Farm", "Farm"));
            _root.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            _root.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
            _root.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
            _root.SubMenu("Farm")
                .AddItem(
                new MenuItem("LaneClearActive", "Farm!").SetValue(
                    new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            _root.AddSubMenu(new Menu("Drawing Manager", "SharpDrawer"));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("DrawW", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("DrawE", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));

            _root.AddSubMenu(new Menu("Misc Manager", "Misc"));
            _root.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));
            _root.SubMenu("InterruptSpells").AddItem(new MenuItem("QInterrupt", "Use Q on Target").SetValue(true));
            _root.SubMenu("Misc").AddItem(new MenuItem("EUsage", "Auto-E on GapClosers").SetValue(true));

            _root.AddToMainMenu();
            Game.PrintChat("<font color='#0066FF'>Seth </font>: Volibear [ Loaded ]");

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            _root.SubMenu("SharpDrawer").AddItem(dmgAfterComboItem);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser -= OnEnemyGapcloser;
        }

        private static float GetComboDamage(Obj_AI_Base hero)
        {
            var fComboDamage = 0d;
            if (_q.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q);
            if (_r.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R);
            if (_w.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.W);
            if (_e.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.E);
            if (_igniteSlot != SpellSlot.Unknown &&
            ObjectManager.Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                fComboDamage += ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_root.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                var cTarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                var randuin = ItemData.Randuins_Omen.GetItem();
                
                if (_root.Item("UseQCombo").GetValue<bool>() && ObjectManager.Player.Distance(cTarget) < 200f || ObjectManager.Player.Distance(cTarget) < ObjectManager.Player.AttackRange && _q.IsReady())
                {
                    _q.Cast();
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, cTarget);
                }                
                if (_root.Item("UseECombo").GetValue<bool>() && ObjectManager.Player.Distance(cTarget) < _e.Range && _e.IsReady())
                { _e.Cast(); }
                if (_igniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                    { ObjectManager.Player.Spellbook.CastSpell(_igniteSlot, cTarget); }
                if (randuin.IsOwned() && randuin.IsReady() && randuin.IsInRange(cTarget))
                { randuin.Cast(); }
                if (_root.Item("UseRCombo").GetValue<bool>() && ObjectManager.Player.Distance(cTarget) < ObjectManager.Player.AttackRange && _r.IsReady())
                { _r.Cast(); }
                if (_root.Item("UseWCombo").GetValue<bool>() && ObjectManager.Player.Distance(cTarget) < _w.Range && _w.IsReady())
                {
                    var wHealth = cTarget.MaxHealth * 100 / _root.Item("EnemyHPW").GetValue<Slider>().Value;
                    if (cTarget.Health >= wHealth && ObjectManager.Player.Distance(cTarget) < _w.Range && _w.IsReady())
                    { _w.CastOnUnit(cTarget); }
                }
            }

            if (_root.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {                
                var minions = MinionManager.GetMinions(ObjectManager.Player.Position, _q.Range, MinionTypes.All, MinionTeam.NotAlly);

                foreach (var minion in minions)
                {
                    if (_root.Item("UseQFarm").GetValue<bool>() && ObjectManager.Player.Distance(minion) < _q.Range && _q.IsReady())
                    {
                        _q.Cast();
                        ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                    }

                    if (_root.Item("UseEFarm").GetValue<bool>() && ObjectManager.Player.Distance(minion) < _e.Range && _e.IsReady())
                    { _e.Cast(); }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_root.Item("DrawW").GetValue<Circle>().Active)
            { Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, _root.Item("DrawW").GetValue<Circle>().Color); }
            if (_root.Item("DrawE").GetValue<Circle>().Active)
            { Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, _root.Item("DrawE").GetValue<Circle>().Color); }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (_root.Item("InterruptSpells").GetValue<bool>())
            {
                if (_root.Item("QInterrupt").GetValue<bool>() && ObjectManager.Player.Distance(sender) < 200f || ObjectManager.Player.Distance(sender) < ObjectManager.Player.AttackRange && _q.IsReady())
                {
                    _q.Cast();
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                }
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_root.Item("EUsage").GetValue<bool>())
            {
                if (ObjectManager.Player.Distance(gapcloser.Sender) < _e.Range && _e.IsReady())
                { _e.Cast(); }
            }
        }
    }
}