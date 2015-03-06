using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;


namespace Pantheon
{
    class Program
    {
        public static Menu Menu;
        public static Spell Spear, Aegis, Heartseeker, Skyfall;
        public static Orbwalking.Orbwalker Orbwalker;

        public static SpellSlot Ignite;
        // public static SpellSlot Smite;

        public static bool IsCasting;

        public static int indexq;
        public static int index;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += PantheonLoad;
        }
        private static void PantheonLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Pantheon") return;

            // Notification
            Notifications.AddNotification("Seth : Pantheon - Loaded", 2000);

            Ignite = ObjectManager.Player.GetSpellSlot("summonerdot");

            Spear = new Spell(SpellSlot.Q, 600);
            Spear.SetTargetted(0.2f, 1700f);

            Aegis = new Spell(SpellSlot.W, 600);
            Aegis.SetTargetted(0.2f, 1700f);

            Heartseeker = new Spell(SpellSlot.E, 400);
            Heartseeker.SetSkillshot(0.25f, 15f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);

            Skyfall = new Spell(SpellSlot.R, 5500);
            // Going to make a logic to this

            Menu = new Menu("Seth : Pantheon", "Pantheon", true);

            var Orb = new Menu("Pantheon : Orbwalker", "Orbwalker");
            {
                Orbwalker = new Orbwalking.Orbwalker(Orb);
                Menu.AddSubMenu(Orb);
            }

            var TargetS = new Menu("Pantheon : Selector", "TargetSelector");
            {
                TargetSelector.AddToMenu(TargetS);
                Menu.AddSubMenu(TargetS);
            }

            var Spells = new Menu("Pantheon : Spells", "SpellMenu");
            {
                Spells.AddItem(new MenuItem("Combo", "Pantheon : Combo"));

                Spells.SubMenu("Combo").AddItem(new MenuItem("QCombo", "Use Q").SetValue(true));
                Spells.SubMenu("Combo").AddItem(new MenuItem("WCombo", "Use W").SetValue(true));
                Spells.SubMenu("Combo").AddItem(new MenuItem("ECombo", "Use E").SetValue(true));
                Spells.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));
                Spells.SubMenu("Combo").AddItem(new MenuItem("ComboMode", "Combo Mode").SetValue(new StringList(new[] { "Q-W-E", "W-Q-E", "W-E-Q" }, 0)));

                Spells.AddItem(new MenuItem("Harass", "Pantheon : Harass"));

                Spells.SubMenu("Harass").AddItem(new MenuItem("QHarass", "Use Q").SetValue(true));
                Spells.SubMenu("Harass").AddItem(new MenuItem("EHarass", "Use E").SetValue(false));

                Spells.AddItem(new MenuItem("Farm", "Pantheon : Farm"));

                Spells.SubMenu("Farm").AddItem(new MenuItem("QFarm", "Use Q").SetValue(new StringList(new[] { "Last Hit", "LaneClear" }, 0)));
                Spells.SubMenu("Farm").AddItem(new MenuItem("EFarm", "Use E").SetValue(true));
                Menu.AddSubMenu(Spells);
            }

            var KS = new Menu("Pantheon : KS Mode", "KSMenu");
            {
                KS.AddItem(new MenuItem("QKS", "Use Q").SetValue(true));
                KS.AddItem(new MenuItem("WKS", "Use W").SetValue(true));
            }

            var Miscs = new Menu("Pantheon : Misc", "MiscMenu");
            {
                Miscs.AddItem(new MenuItem("Interrupter", "Use W to Interrupt").SetValue(true));
                Miscs.AddItem(new MenuItem("WGapCloser", "Use W to GapCloser").SetValue(true));

                var Item = new Menu("Pantheon : Items", "ItemsMenu");
                Item.AddItem(new MenuItem("Targeted", "Targeted"));
                Menu.SubMenu("Targeted").AddItem(new MenuItem("3153", "Blade of the Ruined King").SetValue(true));
                Menu.SubMenu("Targeted").AddItem(new MenuItem("3144", "Bilgewater Cutlass").SetValue(true));

                Item.AddItem(new MenuItem("AOE", "AOE"));
                Menu.SubMenu("AOE").AddItem(new MenuItem("3143", "Randuin's Omen").SetValue(true));
                Menu.SubMenu("AOE").AddItem(new MenuItem("3074", "Ravenous Hydra").SetValue(true));
                Menu.SubMenu("AOE").AddItem(new MenuItem("3077", "Tiamat").SetValue(true));
                Menu.AddSubMenu(Item);
            }

            var Drawinge = new Menu("Pantheon : Draw", "DrawMenu");
            {
                Drawinge.AddItem(new MenuItem("DrawQ", "Q Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));
                Drawinge.AddItem(new MenuItem("DrawW", "W Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));
                Drawinge.AddItem(new MenuItem("DrawE", "E Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));

                Drawinge.AddItem(new MenuItem("DrawCM", "Combo Mode").SetValue(true));
                Menu.AddSubMenu(Drawinge);
            }

            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (Menu.Item("DrawQ").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Spear.Range, Menu.Item("DrawQ").GetValue<Circle>().Color);
            }
            if (Menu.Item("DrawW").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Aegis.Range, Menu.Item("DrawW").GetValue<Circle>().Color);
            }
            if (Menu.Item("DrawE").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Heartseeker.Range, Menu.Item("DrawE").GetValue<Circle>().Color);
            }

            if (Menu.Item("DrawCM").GetValue<bool>())
            {
                var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
                int index = Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex;
                if (index == 0)
                {
                    Drawing.DrawText(wts[0] - 20, wts[1], System.Drawing.Color.White, "Combo Mode: Q-W-E");

                }
                if (index == 1)
                {
                    Drawing.DrawText(wts[0] - 20, wts[1], System.Drawing.Color.White, "Combo Mode: W-Q-E");
                }
                if (index == 2)
                {
                    Drawing.DrawText(wts[0] - 20, wts[1], System.Drawing.Color.White, "Combo Mode: W-E-Q");
                }
            }            

        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Menu.Item("WGapCloser").GetValue<bool>())
            {
                if (Aegis.IsReady() && gapcloser.Sender.IsValidTarget(Aegis.Range))
                {
                    Aegis.CastOnUnit(gapcloser.Sender);
                }
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Menu.Item("Interrupter").GetValue<bool>())
            {
                if (Aegis.IsReady() && sender.IsValidTarget(Aegis.Range))
                {
                    Aegis.CastOnUnit(sender);
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Spear.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(vTarget, SpellSlot.Q);

            if (Heartseeker.IsReady())
                fComboDamage += ObjectManager.Player.GetSpellDamage(vTarget, SpellSlot.E);

            if (Ignite != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready)
                fComboDamage += ObjectManager.Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            index = Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex;
            var t = TargetSelector.GetTarget(Spear.Range, TargetSelector.DamageType.Physical);
            if (Orbwalker.ActiveMode.ToString() == "Combo")
            {
                if (index == 0)
                {
                    if (Menu.Item("3153").GetValue<bool>())
                    {
                        var BladeRuined = ItemData.Blade_of_the_Ruined_King.GetItem();
                        if (BladeRuined.IsOwned() && BladeRuined.IsReady() && BladeRuined.IsInRange(t))
                        { BladeRuined.Cast(t); }
                    }

                    if (Menu.Item("3144").GetValue<bool>())
                    {
                        var BilgeCut = ItemData.Bilgewater_Cutlass.GetItem();
                        if (BilgeCut.IsOwned() && BilgeCut.IsReady() && BilgeCut.IsInRange(t))
                        { BilgeCut.Cast(t); }
                    }

                    if (Menu.Item("3143").GetValue<bool>())
                    {
                        var Randuin = ItemData.Randuins_Omen.GetItem();
                        if (Randuin.IsOwned() && Randuin.IsReady() && Randuin.IsInRange(t))
                        { Randuin.Cast(); } 
                    }

                    if (Menu.Item("3074").GetValue<bool>())
                    {
                        var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
                        if (Hydra.IsOwned() && Hydra.IsReady() && Hydra.IsInRange(t))
                        { Hydra.Cast(); }
                    }

                    if (Menu.Item("3077").GetValue<bool>())
                    {
                        var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
                        if (Tiamat.IsOwned() && Tiamat.IsReady() && Tiamat.IsInRange(t))
                        { Tiamat.Cast(); }
                    }

                    if (Menu.Item("QCombo").GetValue<bool>())
                    {
                        if (Spear.IsReady() && t.IsValidTarget(Spear.Range))
                        {
                            Spear.CastOnUnit(t);
                        }
                    }

                    if (Menu.Item("UseIgnite").GetValue<bool>())
                    {
                        if (Ignite != SpellSlot.Unknown &&
                        ObjectManager.Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(Ignite, t);
                        }
                    }

                    if (Menu.Item("WCombo").GetValue<bool>())
                    {
                        if (Aegis.IsReady() && t.IsValidTarget(Aegis.Range))
                        {
                            Aegis.CastOnUnit(t);
                        }
                    }

                    if (Menu.Item("ECombo").GetValue<bool>())
                    {
                        if (Heartseeker.IsReady() && t.IsValidTarget(Heartseeker.Range))
                        {
                            Heartseeker.Cast(t.Position);
                            IsCasting = true;
                            Utility.DelayAction.Add(700, () => IsCasting = false);
                        }
                    }
                }
                
                if (index == 1)
                {
                    if (Menu.Item("3153").GetValue<bool>())
                    {
                        var BladeRuined = ItemData.Blade_of_the_Ruined_King.GetItem();
                        if (BladeRuined.IsOwned() && BladeRuined.IsReady() && BladeRuined.IsInRange(t))
                        { BladeRuined.Cast(t); }
                    }

                    if (Menu.Item("3144").GetValue<bool>())
                    {
                        var BilgeCut = ItemData.Bilgewater_Cutlass.GetItem();
                        if (BilgeCut.IsOwned() && BilgeCut.IsReady() && BilgeCut.IsInRange(t))
                        { BilgeCut.Cast(t); }
                    }

                    if (Menu.Item("3143").GetValue<bool>())
                    {
                        var Randuin = ItemData.Randuins_Omen.GetItem();
                        if (Randuin.IsOwned() && Randuin.IsReady() && Randuin.IsInRange(t))
                        { Randuin.Cast(); }
                    }

                    if (Menu.Item("3074").GetValue<bool>())
                    {
                        var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
                        if (Hydra.IsOwned() && Hydra.IsReady() && Hydra.IsInRange(t))
                        { Hydra.Cast(); }
                    }

                    if (Menu.Item("3077").GetValue<bool>())
                    {
                        var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
                        if (Tiamat.IsOwned() && Tiamat.IsReady() && Tiamat.IsInRange(t))
                        { Tiamat.Cast(); }
                    }

                    if (Menu.Item("WCombo").GetValue<bool>())
                    {
                        if (Aegis.IsReady() && t.IsValidTarget(Aegis.Range))
                        {
                            Aegis.CastOnUnit(t);
                        }
                    }

                    if (Menu.Item("UseIgnite").GetValue<bool>())
                    {
                        if (Ignite != SpellSlot.Unknown &&
                        ObjectManager.Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(Ignite, t);
                        }
                    }

                    if (Menu.Item("QCombo").GetValue<bool>())
                    {
                        if (Spear.IsReady() && t.IsValidTarget(Spear.Range))
                        {
                            Spear.CastOnUnit(t);
                        }
                    }

                    if (Menu.Item("ECombo").GetValue<bool>())
                    {
                        if (Heartseeker.IsReady() && t.IsValidTarget(Heartseeker.Range))
                        {
                            Heartseeker.Cast(t.Position);
                            IsCasting = true;
                            Utility.DelayAction.Add(700, () => IsCasting = false);
                        }
                    }
                }            
                
                if (index == 2)
                {
                    if (Menu.Item("3153").GetValue<bool>())
                    {
                        var BladeRuined = ItemData.Blade_of_the_Ruined_King.GetItem();
                        if (BladeRuined.IsOwned() && BladeRuined.IsReady() && BladeRuined.IsInRange(t))
                        { BladeRuined.Cast(t); }
                    }

                    if (Menu.Item("3144").GetValue<bool>())
                    {
                        var BilgeCut = ItemData.Bilgewater_Cutlass.GetItem();
                        if (BilgeCut.IsOwned() && BilgeCut.IsReady() && BilgeCut.IsInRange(t))
                        { BilgeCut.Cast(t); }
                    }

                    if (Menu.Item("3143").GetValue<bool>())
                    {
                        var Randuin = ItemData.Randuins_Omen.GetItem();
                        if (Randuin.IsOwned() && Randuin.IsReady() && Randuin.IsInRange(t))
                        { Randuin.Cast(); }
                    }

                    if (Menu.Item("3074").GetValue<bool>())
                    {
                        var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
                        if (Hydra.IsOwned() && Hydra.IsReady() && Hydra.IsInRange(t))
                        { Hydra.Cast(); }
                    }

                    if (Menu.Item("3077").GetValue<bool>())
                    {
                        var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
                        if (Tiamat.IsOwned() && Tiamat.IsReady() && Tiamat.IsInRange(t))
                        { Tiamat.Cast(); }
                    }

                    if (Menu.Item("WCombo").GetValue<bool>())
                    {
                        if (Aegis.IsReady() && t.IsValidTarget(Aegis.Range))
                        {
                            Aegis.CastOnUnit(t);
                        }
                    }

                    if (Menu.Item("ECombo").GetValue<bool>())
                    {
                        if (Heartseeker.IsReady() && t.IsValidTarget(Heartseeker.Range))
                        {
                            Heartseeker.Cast(t.Position);
                            IsCasting = true;
                            Utility.DelayAction.Add(700, () => IsCasting = false);
                        }
                    }

                    if (Menu.Item("UseIgnite").GetValue<bool>())
                    {
                        if (Ignite != SpellSlot.Unknown &&
                        ObjectManager.Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(Ignite, t);
                        }
                    }

                    if (Menu.Item("QCombo").GetValue<bool>())
                    {
                        if (Spear.IsReady() && t.IsValidTarget(Spear.Range))
                        {
                            Spear.CastOnUnit(t);
                        }
                    }
                }            
            }

            indexq = Menu.Item("QFarm").GetValue<StringList>().SelectedIndex;
            if (Orbwalker.ActiveMode.ToString() == "LastHit")
            {
                if (indexq == 0)
                {
                    foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                    {
                        if (minion.Health +10 < Spear.GetDamage(minion) && Spear.IsReady() && minion.IsValidTarget(Spear.Range))
                            { Spear.CastOnUnit(minion); }
                    }
                }                 
            }

            if (Orbwalker.ActiveMode.ToString() == "Mixed" || Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                if (indexq == 1)
                {
                    foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                    {
                        if (minion.IsValidTarget(Spear.Range) && Spear.IsReady())
                        {
                            Spear.CastOnUnit(minion);
                        }                        
                    }
                }

                if (Menu.Item("EFarm").GetValue<bool>())
                {
                    foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                    {
                        if (minion.IsValidTarget(Heartseeker.Range) && Heartseeker.IsReady())
                        {
                            Heartseeker.Cast(minion.Position);
                            IsCasting = true;
                            Utility.DelayAction.Add(700, () => IsCasting = false);
                        }
                    }
                }

                var ta = TargetSelector.GetTarget(Spear.Range, TargetSelector.DamageType.Physical);

                if (Menu.Item("QHarass").GetValue<bool>())
                {
                    if (Spear.IsReady() && ta.IsValidTarget(Spear.Range))
                    {
                        Spear.CastOnUnit(ta);
                    }
                }

                if (Menu.Item("EHarass").GetValue<bool>())
                {
                    if (Heartseeker.IsReady() && ta.IsValidTarget(Heartseeker.Range))
                    {
                        Heartseeker.Cast(ta.Position);
                        IsCasting = true;
                        Utility.DelayAction.Add(700, () => IsCasting = false);
                    }
                }
            }

            if (IsCasting == true)
            {
                var ta = TargetSelector.GetTarget(Spear.Range, TargetSelector.DamageType.Physical);

                Orbwalker.SetMovement(false);
                Orbwalker.SetAttack(false);
                Console.WriteLine("E Casting");

                if (ObjectManager.Player.Distance(ta) > 370 && IsCasting)
                { IsCasting = false; }

                if (ObjectManager.Player.HasBuffOfType(BuffType.Blind) ||
                    ObjectManager.Player.HasBuffOfType(BuffType.Fear) ||
                    ObjectManager.Player.HasBuffOfType(BuffType.Knockup) ||
                    ObjectManager.Player.HasBuffOfType(BuffType.Polymorph) ||
                    ObjectManager.Player.HasBuffOfType(BuffType.Silence) ||
                    ObjectManager.Player.HasBuffOfType(BuffType.Snare) ||
                    ObjectManager.Player.HasBuffOfType(BuffType.Stun))
                {
                    IsCasting = false;
                }
            }
            else if (IsCasting == false)
            {
                Orbwalker.SetMovement(true);
                Orbwalker.SetAttack(true);
            }

            if (Menu.Item("QKS").GetValue<bool>())
            {
                var tks = TargetSelector.GetTarget(1000f, TargetSelector.DamageType.Physical);
                if (Spear.IsReady() && tks.IsValidTarget(Spear.Range) && tks.Health + 10 < Spear.GetDamage(tks))
                {
                    Spear.CastOnUnit(tks);

                }
                else if (ObjectManager.Player.Distance(tks) > Spear.Range && Spear.IsReady() && Aegis.IsReady() && tks.Health + 10 < Spear.GetDamage(tks))
                {
                    var minions = MinionManager.GetMinions(Aegis.Range, MinionTypes.All, MinionTeam.NotAlly);
                    foreach(var minion in minions)
                    {
                        Aegis.CastOnUnit(minion);
                        if (tks.IsValidTarget(Spear.Range) && Spear.IsReady())
                        {
                            Spear.CastOnUnit(tks);
                        }
                    }
                }
            }
            if (Menu.Item("WKS").GetValue<bool>())
            {
                var tks = TargetSelector.GetTarget(1000f, TargetSelector.DamageType.Physical);
                if (Aegis.IsReady() && tks.IsValidTarget(Aegis.Range) && tks.Health + 10 < Aegis.GetDamage(tks))
                {
                    Aegis.CastOnUnit(tks);
                }
            }
        }
    }
}
