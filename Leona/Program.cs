#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common.Data;
using LeagueSharp.Common;
using LeagueSharp;

#endregion


namespace Leona
{
    internal class Program
    {
        public static Menu _root;
        public static Spell _q, _w, _e, _r;
        public static Orbwalking.Orbwalker Orbwalker;
        
        public static List<Spell> _spells = new List<Spell>();

        public static SpellSlot _exhaustSlot;
        
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OutLoading;
        }

        private static void Game_OutLoading(EventArgs args)
        {
            try
            {
            
            Game.PrintChat("Debug Log 1 ");
            Console.WriteLine("Debug Log 1 ");
            
            if (ObjectManager.Player.ChampionName != "Leona")
                return;
            
            Game.PrintChat("Debug Log");
            Console.WriteLine("Debug Log");
            
            #region Spells
            _q = new Spell(SpellSlot.Q, 120f);
            _w = new Spell(SpellSlot.W, 450f);
            _e = new Spell(SpellSlot.E, 875f);
            _e.SetSkillshot(0.25f, 100f, 2000f, false, SkillshotType.SkillshotLine);

            _r = new Spell(SpellSlot.R, 1200f);
            _r.SetSkillshot(1f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            _exhaustSlot = ObjectManager.Player.GetSpellSlot("SummonerExhaust");

            _spells.Add(_q);
            _spells.Add(_w);
            _spells.Add(_e);
            _spells.Add(_r);
            }
            
            catch (ArgumentNullException e)
            {
            Console.WriteLine("{0} First exception caught.", e);
            }
        // Least specific:
            catch (Exception e)
            {
            Console.WriteLine("{0} Second exception caught.", e);
            }
            
            #endregion

            #region Root
            _root = new Menu("Leona", "Leona", true);
            _root.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _root.AddSubMenu(targetSelectorMenu);

            _root.AddSubMenu(new Menu("C Spell Handler", "CSpellsM"));
            _root.SubMenu("CSpellsM").AddItem(new MenuItem("UseQCombo", "Use Q Combo").SetValue(true));
            _root.SubMenu("CSpellsM").AddItem(new MenuItem("UseWCombo", "Use W Combo ").SetValue(true));
            _root.SubMenu("CSpellsM").AddItem(new MenuItem("UseECombo", "Use E Combo").SetValue(true));
            _root.SubMenu("CSpellsM").AddItem(new MenuItem("UseRCombo", "Use R Combo").SetValue(true));
            _root.SubMenu("CSpellsM").AddItem(new MenuItem("UseExhaustCombo", "Use Exhaust").SetValue(true));
            _root.SubMenu("CSpellsM")
                .AddItem(
                    new MenuItem("ComboActive", "Combo!").SetValue(
                        new KeyBind(_root.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            _root.AddSubMenu(new Menu("Drawing Manager", "SharpDrawer"));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("_qRange", "Draw Q Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("_wRange", "Draw W Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("_eRange", "Draw E Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            _root.SubMenu("SharpDrawer").AddItem(new MenuItem("_rRange", "Draw R Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));

            _root.AddSubMenu(new Menu("Misc Manager", "Misc"));
            _root.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));
            _root.SubMenu("InterruptSpells").AddItem(new MenuItem("EQInterrupt", "Use EQ on Target").SetValue(true));
            _root.SubMenu("InterruptSpells").AddItem(new MenuItem("RInterrupt", "Use R on Target").SetValue(true));
            _root.SubMenu("Misc").AddItem(new MenuItem("QUsage", "Auto-Q on GapClosers").SetValue(true));
            #endregion

            Game.OnGameUpdate += Game_Updating;
            Drawing.OnDraw += OnDraw_Drawing;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser -= OnEnemyGapcloser;
        }

        private static void OnDraw_Drawing(EventArgs args)
        {           
            foreach (var spell in _spells)
            {
                var menuItem = _root.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_root.Item("QUsage").GetValue<bool>())
            {
                if (ObjectManager.Player.Distance(gapcloser.Sender) < ObjectManager.Player.AttackRange)
                {
                    _q.Cast();
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                }
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var MenuInstance = _root.Item("InterruptSpells").GetValue<bool>();
            var EQInstance = _root.Item("EQInterrupt").GetValue<bool>();
            var RInstance = _root.Item("RInterrupt").GetValue<bool>();

            if (MenuInstance)
            {
                if (ObjectManager.Player.Distance(sender) < _e.Range || _e.IsReady() || _q.IsReady())
                {
                    if (EQInstance)
                    {
                        _q.Cast();
                        _e.CastIfHitchanceEquals(sender, HitChance.High, true);
                    }
                }
                else if (ObjectManager.Player.Distance(sender) < _r.Range || _r.IsReady())
                {
                    if (RInstance)
                        _r.CastIfHitchanceEquals(sender, HitChance.High, true);
                }
            }
        }
        private static void Game_Updating(EventArgs args)
        {
            if (_root.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                #region Handlers
                var qHandler = _root.Item("UseQCombo").GetValue<bool>();
                var wHandler = _root.Item("UseWCombo").GetValue<bool>();
                var eHandler = _root.Item("UseECombo").GetValue<bool>();
                var rHandler = _root.Item("UseRCombo").GetValue<bool>();
                #endregion
                    
                var target = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                var randuin = ItemData.Randuins_Omen.GetItem();
                if (eHandler && _e.IsReady())
                {
                    if (ObjectManager.Player.Distance(target) < _e.Range)
                    {
                        if(qHandler && _q.IsReady())
                        {
                            _q.Cast();
                            ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            if (randuin.IsOwned() && randuin.IsReady() && randuin.IsInRange(target))
                                randuin.Cast();
                        }
                        _e.CastIfHitchanceEquals(target, HitChance.High, true);

                        if (_root.Item("UseExhaustCombo").GetValue<bool>())
                        {
                            if (_exhaustSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(_exhaustSlot) == SpellState.Ready)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(_exhaustSlot, target);
                            }
                        }
                    }
                    else if (ObjectManager.Player.Distance(target) > _e.Range)
                    {
                        if (ObjectManager.Player.Distance(target) < _r.Range)
                        {
                            if (rHandler && _r.IsReady())
                            { 
                                _r.CastIfHitchanceEquals(target, HitChance.High, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
