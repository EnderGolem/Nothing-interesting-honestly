﻿using System;
using System.Collections.Generic;
using System.Linq;
using AiCup22.Model;

namespace AiCup22.Custom
{
    public static class Tools
    {
        /// <summary>
        /// Возвращает расстояние от точки до края текущей зоны
        /// Если позиция находится за зоной, то расстояние считается отрицательным
        /// </summary>
        public static double CurrentZoneDistance(Zone zone, Vec2 position)
        {
            return zone.CurrentRadius - zone.CurrentCenter.Distance(position);
        }
        /// <summary>
        /// Возвращает первое препятствие на прямой из стартовой точки к конечной, если оно существует
        /// И null в противном случае
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="obstacles">Некий набор укрытий который мы просматриваем желательно не все укрытия на карте</param>
        /// <param name="ignoreLowObstacles">Следует ли игнорировать при просмотре
        /// простреливаемые укрытия
        /// </param>
        /// <returns></returns>
        public static Obstacle? RaycastObstacle(Vec2 startPoint, Vec2 endPoint, Obstacle[] obstacles, bool ignoreLowObstacles)
        {
            List<Obstacle> hitObstacles = new List<Obstacle>();

            for (int i = 0; i < obstacles.Length; i++)
            {
                if(ignoreLowObstacles && obstacles[i].CanShootThrough)
                    continue;
                Vec2 sp = startPoint;
                Vec2 ep = endPoint;
                sp = sp.Subtract(obstacles[i].Position);
                ep = ep.Subtract(obstacles[i].Position);

                Vec2 d = ep.Subtract(sp);
                
                double a = d.X*d.X + d.Y*d.Y;
                double b = 2*(sp.X*d.X + sp.Y*d.Y);
                double c = sp.X*sp.X + sp.Y*sp.Y - obstacles[i].Radius*obstacles[i].Radius;

                bool flag;
                if (-b < 0)
                    flag = (c < 0);
                else if (-b < (2*a))
                 flag = ((4*a*c - b*b) < 0);
                else
                flag = (a+b+c < 0);

                if (flag)
                {
                    hitObstacles.Add(obstacles[i]);
                }
            }

            if (hitObstacles.Count == 0)
            {
                return null;
            }
            else
            {
                return hitObstacles.OrderBy((Obstacle o) => startPoint.SqrDistance(o.Position)).First();
            }
        }
        /// <summary>
        /// Обнаружение препятствий по 2-м точкам
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="width">Ширина юнита</param>
        /// <param name="obstacles"></param>
        /// <param name="ignoreLowObstacles"></param>
        /// <returns></returns>
        public static Obstacle? RaycastObstacle2Point(Vec2 startPoint, Vec2 endPoint,double width, Obstacle[] obstacles,
            bool ignoreLowObstacles)
        {
            Straight s = new Straight();
            var normal = endPoint.Subtract(startPoint);
            s.SetByNormalAndPoint(normal, startPoint);
            var directive = s.GetDirective().Normalize();
            var point1 = startPoint.Add(directive.Multi(width / 2));
            var point2 = startPoint.Add(directive.Multi(-width / 2));
            var o = RaycastObstacle(point1, endPoint, obstacles, ignoreLowObstacles);
            if (o.HasValue)
            {
                return o;
            }
            o = RaycastObstacle(point2, endPoint, obstacles, ignoreLowObstacles);
            return o;
        }
        /// <summary>
        /// Определяет принадлежит ли точка полю обзора
        /// </summary>
        /// <param name="point"></param>
        /// <param name="startPos"></param>
        /// <param name="viewDistance"></param>
        /// <param name="viewAngle"></param>
        /// <returns></returns>
        public static bool BelongConeOfVision(Vec2 point,Vec2 startPos,Vec2 viewDirection, double viewDistance, double viewAngle)
        {
            if (!BelongDirection(point,startPos,viewDirection,viewAngle))
            {
                return false;
            }

            return startPos.Distance(point)<viewDistance;
        }

        public static bool BelongDirection(Vec2 point,Vec2 startPos,Vec2 viewDirection, double viewAngle)
        {
            var pointAngle = AngleToPoint(startPos, point);
            var viewDirAngle = AngleToPoint(new Vec2(0, 0), viewDirection);

            return IsInside(viewDirAngle, viewAngle, pointAngle);
        }

        public static double AngleToPoint(Vec2 yourPosition, Vec2 point)
        {
            return (Math.Atan2(point.Y - yourPosition.Y,
                point.X - yourPosition.X) * (180 / Math.PI));
        }
        
        public static double AngleDiff(double angle1, double angle2)
        {
            return ((((angle1 - angle2) % 360) + 540) % 360) - 180;
        }

        public static bool IsInside(double rayAngle, double alpha, double pointAngle)
        {
            return (AngleDiff(pointAngle, rayAngle - alpha) > 0 && AngleDiff(pointAngle, rayAngle + alpha) < 0);
        }
    }
}
