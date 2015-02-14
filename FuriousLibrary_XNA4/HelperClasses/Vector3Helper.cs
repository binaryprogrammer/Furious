using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FuriousLibrary_XNA4.HelperClasses
{
    static public class Vector3Helper
    {
        /// <summary>
        /// Subtracts a value from each component of a Vector3
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static public Vector3 SubtractValue(Vector3 vector, float value)
        {
            return new Vector3(vector.X - value, vector.Y - value, vector.Z - value);
        }

        /// <summary>
        /// Adds a value from each component of a Vector3
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static public Vector3 AddValue(Vector3 vector, float value)
        {
            return new Vector3(vector.X + value, vector.Y + value, vector.Z + value);
        }

        /// <summary>
        /// Used to return just two components of a Vector.
        /// </summary>
        /// <param name="vector">The vector you want modified</param>
        /// <param name="component">the "X", "Y", or "Z" component</param>
        /// <param name="value">The value you want substituted into that component</param>
        /// <returns></returns>
        static public Vector3 ModifySpecificValueAndReturn(Vector3 vector, string component, float value)
        {
            component = component.ToLower();

            if (component == "x")
            {
                vector.X = value;
            }
            else if (component == "y")
            {
                vector.Y = value;
            }
            else //assume value = "z";
            {
                vector.Z = value;
            }

            return vector;
        }

        /// <summary>
        /// returns the lowest possible value fo X, Y, and Z
        /// </summary>
        public static Vector3 Min 
        {
            get
            {
                return new Vector3(float.MinValue, float.MinValue, float.MinValue);
            }
        }

        /// <summary>
        /// returns the highest possible value for X, Y, and Z
        /// </summary>
        public static Vector3 Max
        {
            get
            {
                return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            }
        }
    }
}
