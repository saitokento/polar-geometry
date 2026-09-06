using System;
using UnityEngine;

namespace PolarGeometry
{
    [Serializable]
    public struct PolarCoordinate
    {
        public float r;
        public float theta;

        public PolarCoordinate(float r, float theta)
        {
            this.r = r;
            this.theta = theta;
        }

        public Vector2 ToCartesian()
        {
            return new Vector2(
                r * Mathf.Cos(theta),
                r * Mathf.Sin(theta)
            );
        }

        public static PolarCoordinate FromCartesian(Vector2 position)
        {
            return new PolarCoordinate(
                position.magnitude,
                Mathf.Atan2(position.y, position.x)
            );
        }

        public override string ToString()
        {
            return $"(r: {r}, θ: {theta})";
        }
    }
}