using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace Hecarim
{
    class Program
    {
        public static Menu Menu;
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;

        public static SpellSlot Ignite;
        public static SpellSlot Smite;

        public static int index;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Hecarim") return;
            Notifications.AddNotification("Seth : Hecarim - Loaded", 2000);

            Ignite = ObjectManager.Player.GetSpellSlot("summonerdot");
            Smite = ObjectManager.Player.GetSpellSlot("summonersmite");

            Q = new Spell(SpellSlot.Q, 350);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 1350);

            R.SetSkillshot(0.5f, 200f, 1200f, false, SkillshotType.SkillshotLine);

            Menu = new Menu("Hecarim", "HecarimMenu", true);

            var Orb = new Menu("Pantheon : Orbwalker", "Orbwalker");
            {
                Orbwalker = new Orbwalking.Orbwalker(Orb);
                Menu.AddSubMenu(Orb);
            }

            var TargetS = new Menu("Hecarim : Selector", "TargetSelector");
            {
                TargetSelector.AddToMenu(TargetS);
                Menu.AddSubMenu(TargetS);
            }

            var Spells = new Menu("Pantheon : Spells", "SpellMenu");
            {
                Spells.AddItem(new MenuItem("Combo", "Hecarim : Combo"));

                Spells.SubMenu("Combo").AddItem(new MenuItem("QCombo", "Use Q").SetValue(true));
                Spells.SubMenu("Combo").AddItem(new MenuItem("WCombo", "Use W").SetValue(true));
                Spells.SubMenu("Combo").AddItem(new MenuItem("ECombo", "Use E").SetValue(true));
                Spells.SubMenu("Combo").AddItem(new MenuItem("RCombo", "Use R").SetValue(true));

                Spells.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));

                Spells.AddItem(new MenuItem("Harass", "Hecarim : Harass"));

                Spells.SubMenu("Harass").AddItem(new MenuItem("QHarass", "Always use Q").SetValue(true));

                Spells.AddItem(new MenuItem("Farm", "Hecarim : Farm"));

                Spells.SubMenu("Farm").AddItem(new MenuItem("QFarm", "Use Q").SetValue(new StringList(new[] { "Last Hit", "LaneClear" }, 0)));
                Spells.SubMenu("Farm").AddItem(new MenuItem("WFarm", "Use W").SetValue(true));
                Menu.AddSubMenu(Spells);
            }

            var KS = new Menu("Hecarim : KS Mode", "KSMenu");
            {
                KS.AddItem(new MenuItem("QKS", "Use Q").SetValue(true));
                KS.AddItem(new MenuItem("WKS", "Use W").SetValue(true));
            }

            var Miscs = new Menu("Pantheon : Misc", "MiscMenu");
            {
                Miscs.AddItem(new MenuItem("Interrupter", "Use E to Interrupt").SetValue(true));
                Miscs.AddItem(new MenuItem("InterrupterR", "Use R to Interrupt").SetValue(false));
                Miscs.AddItem(new MenuItem("UseR", "AutoR if enemies >=").SetValue(new Slider(2, 1, 5)));
                Miscs.AddItem(new MenuItem("UseW", "AutoW if HP %").SetValue(new Slider(45, 1, 100)));
                Miscs.AddItem(new MenuItem("AutoSmite", "Smite [ Use Oracle ] WIP!"));

                var Item = new Menu("Pantheon : Items", "ItemsMenu");
                {
                    Item.AddItem(new MenuItem("Targeted", "Targeted"));
                    Menu.SubMenu("Targeted").AddItem(new MenuItem("3153", "Blade of the Ruined King").SetValue(true));
                    Menu.SubMenu("Targeted").AddItem(new MenuItem("3144", "Bilgewater Cutlass").SetValue(true));

                    Item.AddItem(new MenuItem("AOE", "AOE"));
                    Menu.SubMenu("AOE").AddItem(new MenuItem("3143", "Randuin's Omen").SetValue(true));
                    Menu.SubMenu("AOE").AddItem(new MenuItem("3074", "Ravenous Hydra").SetValue(true));
                    Menu.SubMenu("AOE").AddItem(new MenuItem("3077", "Tiamat").SetValue(true));
                    Menu.AddSubMenu(Item);
                }
            }

            var Drawinge = new Menu("Pantheon : Draw", "DrawMenu");
            {
                Drawinge.AddItem(new MenuItem("DrawQ", "Q Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));
                Drawinge.AddItem(new MenuItem("DrawW", "W Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));
                Drawinge.AddItem(new MenuItem("DrawE", "E Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));
                Drawinge.AddItem(new MenuItem("DrawR", "R Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 58, 90, 179))));

                Drawinge.AddItem(new MenuItem("DrawQM", "Q Mode").SetValue(true));
                Menu.AddSubMenu(Drawinge);
            }

            Menu.AddToMainMenu();

            // Ncess stuff
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (ObjectManager.Player.Distance(sender) <= 400 && E.IsReady() && Menu.Item("Interrupter").GetValue<bool>())
            {
                E.Cast();
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
            }
            else if (Menu.Item("InterrupterR").GetValue<bool>() && R.IsReady() && sender.IsValidTarget(R.Range))
            {
                R.CastIfHitchanceEquals(sender, HitChance.Medium, true);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.Item("DrawQ").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Menu.Item("DrawQ").GetValue<Circle>().Color);
            }
            if (Menu.Item("DrawW").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Menu.Item("DrawW").GetValue<Circle>().Color);
            }
            if (Menu.Item("DrawE").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Menu.Item("DrawE").GetValue<Circle>().Color);
            }

            if (Menu.Item("DrawR").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Menu.Item("DrawR").GetValue<Circle>().Color);
            }

            if (Menu.Item("DrawQM").GetValue<bool>())
            {
                var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
                if (index == 0)
                {
                    Drawing.DrawText(wts[0] - 20, wts[1], System.Drawing.Color.White, "Q Mode: LastHit");
                }
                else if (index == 1)
                {
                    Drawing.DrawText(wts[0] - 20, wts[1], System.Drawing.Color.White, "Q Mode: LaneClear");
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(800f, TargetSelector.DamageType.Physical);
            index = Menu.Item("QFarm").GetValue<StringList>().SelectedIndex;
            if (Orbwalker.ActiveMode.ToString() == "Combo")
            {
                if (Menu.Item("ECombo").GetValue<bool>())
                {
                    if (ObjectManager.Player.Distance(target) > R.Range && E.IsReady())
                    {
                        E.Cast();
                    }
                    else if (R.IsReady() && Menu.Item("RCombo").GetValue<bool>())
                    {
                        R.CastIfHitchanceEquals(target, HitChance.Medium, true);
                    }
                }

                if (Menu.Item("QCombo").GetValue<bool>())
                {
                    if (Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        Q.Cast();
                    }
                }

                if (Menu.Item("3153").GetValue<bool>())
                {
                    var BladeRuined = ItemData.Blade_of_the_Ruined_King.GetItem();
                    if (BladeRuined.IsOwned() && BladeRuined.IsReady() && BladeRuined.IsInRange(target))
                    {
                        BladeRuined.Cast(target);
                    }
                }

                if (Menu.Item("3144").GetValue<bool>())
                {
                    var BilgeCut = ItemData.Bilgewater_Cutlass.GetItem();
                    if (BilgeCut.IsOwned() && BilgeCut.IsReady() && BilgeCut.IsInRange(target))
                    {
                        BilgeCut.Cast(target);
                    }
                }

                if (Menu.Item("3143").GetValue<bool>())
                {
                    var Randuin = ItemData.Randuins_Omen.GetItem();
                    if (Randuin.IsOwned() && Randuin.IsReady() && Randuin.IsInRange(target))
                    {
                        Randuin.Cast();
                    }
                }

                if (Menu.Item("3074").GetValue<bool>())
                {
                    var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
                    if (Hydra.IsOwned() && Hydra.IsReady() && Hydra.IsInRange(target))
                    {
                        Hydra.Cast();
                    }
                }

                if (Menu.Item("3077").GetValue<bool>())
                {
                    var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
                    if (Tiamat.IsOwned() && Tiamat.IsReady() && Tiamat.IsInRange(target))
                    {
                        Tiamat.Cast();
                    }
                }

                if (Menu.Item("UseIgnite").GetValue<bool>())
                {
                    if (Ignite != SpellSlot.Unknown &&
                    ObjectManager.Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Ignite, target);
                    }
                }

                if (Menu.Item("WCombo").GetValue<bool>())
                {
                    if (W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        W.Cast();                       
                    }
                }
            }
            
            if (Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                var minions = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly);
                if (index == 1)
                {
                    foreach(var minion in minions)
                    {
                        if (minion.IsValidTarget(Q.Range))
                        {
                            Q.Cast();
                        }
                    }
                }

                if (W.IsReady() && Menu.Item("WFarm").GetValue<bool>())
                {
                    foreach(var minion in minions)
                    {
                        if (minion.IsValidTarget(W.Range) && W.IsReady())
                        {
                            W.Cast();
                        }
                    }
                }
            }
            
            if (Orbwalker.ActiveMode.ToString() == "LastHit")
            {
                var minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                {
                    if (index == 0)
                    {
                        foreach (var minion in minions)
                        {
                            if (minion.IsValidTarget(Q.Range) && Q.IsReady() && minion.Health + 10 <= Q.GetDamage(minion))
                            {
                                Q.Cast();
                            }
                        }
                    }
                }
            }
            
            if (Orbwalker.ActiveMode.ToString() == "Mixed")
            {
                if (Menu.Item("QHarass").GetValue<bool>())
                {
                    if (target.IsValidTarget(Q.Range) && Q.IsReady())
                    {
                        Q.Cast();
                    }
                }
            }

            var Enemies = HeroManager.Enemies;
            var RMin = Menu.Item("UseR").GetValue<Slider>().Value;
            if (Enemies.Count >= RMin)
            {
                if (R.IsReady() && target.IsValidTarget(R.Range))
                {
                    R.CastIfHitchanceEquals(target, HitChance.Medium, true);
                }
            }

            var wHp = Menu.Item("UseW").GetValue<Slider>().Value * 100 / ObjectManager.Player.MaxHealth;
            if (ObjectManager.Player.Health <= wHp)
            {
                W.Cast();
            }

            if(target.IsValidTarget(Q.Range) && Q.IsReady() && Menu.Item("QKS").GetValue<bool>())
            {
                if (target.Health + 10 <= Q.GetDamage(target))
                {
                    Q.Cast();
                }
            }

            if (target.IsValidTarget(W.Range) && Q.IsReady() && Menu.Item("WKS").GetValue<bool>())
            {
                if (target.Health + 10 <= W.GetDamage(target))
                {
                    W.Cast();
                }
            }

            /*
            if (Smite == SpellSlot.Unknown)
            {
                if (Menu.Item("AutoSmite").GetValue<KeyBind>().Active)
                {
                    string[] monsterNames = { "LizardElder", "AncientGolem", "Worm", "Dragon" };
                    var monsters = MinionManager.GetMinions(ObjectManager.Player.Position, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                    var vMonsters = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                    foreach (var vMonster in vMonsters.Where(vMonster => vMonster != null
                                                                      && !vMonster.IsDead
                                                                      && !ObjectManager.Player.IsDead
                                                                      && !ObjectManager.Player.IsStunned
                                                                      && Smite != SpellSlot.Unknown
                                                                      && ObjectManager.Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready)
                                                                      .Where(vMonster => (vMonster.Health < ObjectManager.Player.GetSummonerSpellDamage(vMonster, Damage.SummonerSpell.Smite)) && (monsterNames.Any(name => vMonster.BaseSkinName.StartsWith(name)))))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Smite, vMonster);
                    }
                }
            }
            */
        }
    }
}
