using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

namespace Corki
{
    class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R1, R2;
        public static Menu Config;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {

            if (ObjectManager.Player.BaseSkinName != "Corki")
                return;

            Q = new Spell(SpellSlot.Q, 825f);
            Q.SetSkillshot(0.35f, 250f, 1500f, false, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 800f);
            E = new Spell(SpellSlot.E, 600f);
            E.SetSkillshot(0f, (float)(45 * Math.PI / 180), 1500, false, SkillshotType.SkillshotCone);

            R1 = new Spell(SpellSlot.R, 1300f);
            R1.SetSkillshot(0.2f, 40f, 2000f, true, SkillshotType.SkillshotLine);

            R2 = new Spell(SpellSlot.R, 1500f);
            R2.SetSkillshot(0.2f, 40f, 2000f, true, SkillshotType.SkillshotLine);

            // Menu Constructor
            Config = new Menu("Corki", "corki", true);
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Drawing", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("DrawQ", "{Q}")).SetValue(true);
            Config.SubMenu("drawing").AddItem(new MenuItem("DrawW", "{W}")).SetValue(true);
            Config.SubMenu("drawing").AddItem(new MenuItem("DrawE", "{E}")).SetValue(true);
            Config.SubMenu("drawing").AddItem(new MenuItem("DrawR", "{R}")).SetValue(true);

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(1300f, TargetSelector.DamageType.Physical);
            if (Config.Item("DrawQ").GetValue<bool>() == true)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan);
            }
            if (Config.Item("DrawW").GetValue<bool>() == true)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Cyan);
            }
            if (Config.Item("DrawE").GetValue<bool>() == true)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Cyan);
            }
            if (Config.Item("DrawR").GetValue<bool>() == true)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R1.Range, System.Drawing.Color.Cyan);
            }

            Render.Circle.DrawCircle(target.Position, 100, System.Drawing.Color.White);
            // Drawing.DrawText();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.HasBuff("corkimissilebarragecounterbig"))
            {
                R1.Range = R2.Range;
            }
            else
            {
                R1.Range = R1.Range;
            }

            var ManaControl = 100;
            var target = TargetSelector.GetTarget(1300f, TargetSelector.DamageType.Physical);
            
            if (Orbwalker.ActiveMode.ToString() == "Combo")
            {               
                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    Q.CastIfHitchanceEquals(target, HitChance.Medium, true);
                    Console.WriteLine("Casted Q");
                }
                if (target.IsValidTarget(ObjectManager.Player.AttackRange) && E.IsReady())
                {
                    E.Cast(target, true);
                    Console.WriteLine("Casted E");
                }
                if (ObjectManager.Player.Mana >= ManaControl)
                {
                    if (ObjectManager.Player.Distance(target) < 600 && R1.IsReady())
                    {
                        R1.CastIfHitchanceEquals(target, HitChance.Medium, true);
                        Console.WriteLine("Casted R Medium");
                    }
                    else if (ObjectManager.Player.Distance(target) > 600 && R1.IsReady())
                    {
                        R1.CastIfHitchanceEquals(target, HitChance.High, true);
                        Console.WriteLine("Casted R High");
                    }
                }                
            }
            
            if (target.HasBuffOfType(BuffType.Blind) ||
                target.HasBuffOfType(BuffType.Charm) ||
                target.HasBuffOfType(BuffType.Fear) ||
                target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Slow) ||
                target.HasBuffOfType(BuffType.Stun) ||
                target.HasBuffOfType(BuffType.Taunt) ||
                target.HasBuffOfType(BuffType.Suppression))
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    Q.CastIfHitchanceEquals(target, HitChance.Medium, true);
                }
                else if (target.IsValidTarget(R1.Range) && R1.IsReady() && ObjectManager.Player.Distance(target) < 600)
                {
                    R1.CastIfHitchanceEquals(target, HitChance.Medium, true);
                }
                else if (target.IsValidTarget(R1.Range) && R1.IsReady() && ObjectManager.Player.Distance(target) > 600)
                {
                    R1.CastIfHitchanceEquals(target, HitChance.High, true);
                }
            }
            
            if (Orbwalker.ActiveMode.ToString() == "Mixed" || Orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                if (minions.Count > 1 && Q.IsReady())
                {
                    var minion = minions[0];
                    if (minion.IsValidTarget(Q.Range))
                        Q.Cast(minion, false); 
                }
                else if (minions.Count > 0 && E.IsReady())
                {
                    var minion = minions[0];
                    if (target.IsValidTarget(E.Range))
                        E.Cast();
                }
                else if (minions.Count > 2 && R1.IsReady() && ObjectManager.Player.Mana >= ManaControl)
                {
                    var minion = minions[0];
                    if (minion.IsValidTarget(R1.Range))
                        R1.CastIfHitchanceEquals(minion, HitChance.Medium, false);
                }

                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                    Q.Cast(target);
                else if(target.IsValidTarget(R1.Range) && R1.IsReady() && ObjectManager.Player.Mana >= ManaControl)
                {
                    if (ObjectManager.Player.Distance(target) > 600)
                        R1.CastIfHitchanceEquals(target, HitChance.High, true);
                    else if (ObjectManager.Player.Distance(target) < 600)
                        R1.CastIfHitchanceEquals(target, HitChance.Medium, false);
                }
            }
            
            if (Orbwalker.ActiveMode.ToString() == "LastHit")
            {
                if (target.IsValidTarget(R1.Range) && R1.IsReady() && ObjectManager.Player.Mana >= ManaControl)
                {
                    if (ObjectManager.Player.Distance(target) > 600)
                    {
                        R1.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                    else if (ObjectManager.Player.Distance(target) < 600)
                    {
                        R1.CastIfHitchanceEquals(target, HitChance.Medium, true);
                    }
                }
            }
            
            foreach(var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                var rdmg = R1.GetDamage(hero);
                var qdmg = Q.GetDamage(hero);

                float predictedHealthQ = HealthPrediction.GetHealthPrediction(hero, (int)(Q.Delay + (ObjectManager.Player.Distance(target.ServerPosition) / Q.Speed) * 1000));
                float predictedHealthR = HealthPrediction.GetHealthPrediction(hero, (int)(R1.Delay + (ObjectManager.Player.Distance(target.ServerPosition) / R1.Speed) * 1000));

                if (rdmg > predictedHealthR)
                {
                    R1.CastIfHitchanceEquals(hero, HitChance.High, true);
                }
                else if (qdmg > predictedHealthQ)
                {
                    Q.Cast(hero, true);
                }
            }        
        }
    }
}
