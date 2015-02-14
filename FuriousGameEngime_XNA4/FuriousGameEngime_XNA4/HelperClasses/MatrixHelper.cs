using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FuriousGameEngime_XNA4.HelperClasses
{
    public static class MatrixHelper
    {
        /// <summary>
        /// Returns the matrix rotation from a direction.
        /// </summary>
        public static Matrix FromDirection(Vector3 direction)
        {
            float angle = (float)Math.Acos(Vector3.Dot(Vector3.Forward, direction));
            Vector3 axis = Vector3.Normalize(Vector3.Cross(Vector3.Forward, direction));
            if (float.IsNaN(axis.X)) axis = Vector3.Left;
            return Matrix.CreateFromAxisAngle(axis, angle);
        }
    }
}
