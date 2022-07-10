﻿using System;
using AiCup22.Debugging;
using AiCup22.Model;

namespace AiCup22.Custom
{
    public class EndAction:Processable
    {
        private int _lastActivationTick;
        private int _lastDeactivationTick;
        private bool _isActive;

        public int LastActivationTick
        {
            get => _lastActivationTick;
            set => _lastActivationTick = value;
        }

        public int LastDeactivationTick
        {
            get => _lastDeactivationTick;
            set => _lastDeactivationTick = value;
        }

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        public void Activate(int currentGameTick)
        {
            if (!IsActive)
            {
                IsActive = true;
                LastActivationTick = currentGameTick;
            }
        }

        public void Deactivate(int currentGameTick)
        {
            if (IsActive)
            {
                IsActive = false;
                LastDeactivationTick = currentGameTick;
            }
        }

        public virtual UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
           return new UnitOrder();
        }
    }

    public class RunToCenter : EndAction
    {
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            ActionOrder action = new ActionOrder.Aim(false);
            Unit unit = perception.MyUnints[0];
            return new UnitOrder(new Vec2(-unit.Position.X, -unit.Position.Y),
                new Vec2(-unit.Position.X, -unit.Position.Y), action);
        }
    }

    public class RunShootToCenter : EndAction
    {
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            ActionOrder action = new ActionOrder.Aim(false);
            Unit unit = perception.MyUnints[0];
            return new UnitOrder(new Vec2(-unit.Position.X, -unit.Position.Y),
                new Vec2(-unit.Position.X, -unit.Position.Y), action);
        }
    }

    public class RunToCenterRadar : EndAction
    {
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            ActionOrder action = new ActionOrder.Aim(false);
            Unit unit = perception.MyUnints[0];
            return new UnitOrder(new Vec2(-unit.Position.X, -unit.Position.Y),
               new Vec2(-unit.Direction.Y, unit.Direction.X), action);
        }
    }

    public class RunShootToCenterRadar : EndAction
    {
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            ActionOrder action = new ActionOrder.Aim(true);
            Unit unit = perception.MyUnints[0];
            return new UnitOrder(new Vec2(-unit.Position.X, -unit.Position.Y),
               new Vec2(-unit.Direction.Y, unit.Direction.X), action);
        }
    }



    public class StayShootToEnemy : EndAction
    {
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            ActionOrder action = new ActionOrder.Aim(true);
            Unit unit = perception.MyUnints[0];
            Unit target = perception.EnemyUnints[0];
            Vec2 enemy = target.Position.Subtract(unit.Position);
            return new UnitOrder(new Vec2(0, 0),
             enemy, action);
        }
    }

    public class RunToDestination : EndAction
    {
        protected Vec2 destination;
        public override UnitOrder Process(Perception perception,DebugInterface debugInterface)
        {
            Unit unit = perception.MyUnints[0];
            var dir = destination.Subtract(unit.Position).Normalize().Multi(perception.Constants.MaxUnitForwardSpeed);
            return new UnitOrder(dir, dir, null);
        }

        public void SetDestination(Vec2 dest)
        {
            destination = dest;
        }
    }

    public class SteeringRunToDestination : RunToDestination
    {
        public override UnitOrder Process(Perception perception,DebugInterface debugInterface)
        {

            Obstacle? obst = Tools.RaycastObstacle2Point(perception.MyUnints[0].Position,destination,perception.Constants.UnitRadius*2,perception.CloseObstacles.ToArray(),false);
            if (!obst.HasValue || obst.Value.Position.SqrDistance(perception.MyUnints[0].Position)>obst.Value.Radius*obst.Value.Radius*9)

            {
                return base.Process(perception, debugInterface);
            }
            else
            {
                Unit unit = perception.MyUnints[0];
                var dir = destination.Subtract(unit.Position).Normalize().Multi(perception.Constants.MaxUnitForwardSpeed);
                Straight perpS = new Straight();
                perpS.SetByNormalAndPoint(dir,obst.Value.Position);
                var dirS = new Straight(dir,unit.Position);
                var intersectPoint = dirS.GetIntersection(perpS);
                var perpDir = obst.Value.Position.Subtract(intersectPoint.Value);
                var targetPos = obst.Value.Position.Add(perpDir.Normalize().Multi(obst.Value.Radius + 3*perception.Constants.UnitRadius));
                var targetDir = targetPos.Subtract(unit.Position).Normalize().Multi(perception.Constants.MaxUnitForwardSpeed);
                /*Console.WriteLine($"Прямая перпендикулярная цели: {perpS}");
                Console.WriteLine($"Прямая до цели: {dirS}");
                Console.WriteLine($"Точка пересечения: {intersectPoint}");
                Console.WriteLine($"Центр препятствия: {obst.Value.Position}");
                Console.WriteLine($"Перпендикулярный вектор: {perpDir}");
                Console.WriteLine($"Целевая позиция: {targetPos}");
                Console.WriteLine($"Целевой вектор: {targetDir}");*/
                debugInterface.AddRing(intersectPoint.Value,1,0.5,new Color(1,0,0,1));
                debugInterface.AddRing(targetPos,1,0.5,new Color(0,0.5,0.5,1));
                debugInterface.AddSegment(obst.Value.Position,obst.Value.Position.Add(perpDir.Normalize()),0.5,new Color(0,0,1,1));
                debugInterface.AddSegment(unit.Position,destination,0.5,new Color(0,1,0,1));
                debugInterface.AddSegment(obst.Value.Position,targetPos,0.5,new Color(1,0,0,1));
                return new UnitOrder(targetDir, dir, null);
            }
        }
    }
    public class UseShield:EndAction
    {
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            return new UnitOrder(new Vec2(), new Vec2(), new ActionOrder.UseShieldPotion());
        }

    }
    public class PickupLoot:EndAction
    {
        private int pickableLootId;
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            ActionOrder action = new ActionOrder.Pickup(pickableLootId);

            //Unit unit = perception.MyUnints[0];
            return new UnitOrder(new Vec2(), new Vec2(), action);
        }

        public void SetPickableLootId(int id)
        {
            pickableLootId = id;
        }
    }

    public class ShootToPoint:EndAction
    {
        private Vec2 target;
        public ShootToPoint()
        {
            target = new Vec2();
        }
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            ActionOrder action = new ActionOrder.Aim(true);
            Unit unit = perception.MyUnints[0];
            Vec2 enemy = target.Subtract(unit.Position);
            return new UnitOrder(new Vec2(), enemy, action);
        }

        public void SetTarget(Vec2 _target)
        {
            target = _target;
        }
    }
    public class AimingToPoint:EndAction
    {
        private Vec2 target;
        public AimingToPoint()
        {
            target = new Vec2();
        }
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {

            ActionOrder action = new ActionOrder.Aim(false); //Исправить, не знаю будет ли прицеливаться...
            Unit unit = perception.MyUnints[0];
            Vec2 enemy = target.Subtract(unit.Position);
            return new UnitOrder(new Vec2(), enemy, action);
        }

        public void SetTarget(Vec2 _target)
        {
            target = _target;
        }
    }

    public class LookAroundAction : EndAction
    {
        public override UnitOrder Process(Perception perception, DebugInterface debugInterface)
        {
            var unit = perception.MyUnints[0];
            Vec2 dir = new Vec2(-unit.Direction.Y,unit.Direction.X);
            return new UnitOrder(dir,dir,null);
        }
    }

}