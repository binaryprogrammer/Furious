using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuriousGameEngime_XNA4
{
    public static class Constants
    {
        //player speed

        //default entity health

        //default entity state

        //default gravity

        //default player spawn position

        //level we are going to loading?

        //window title

        
            //#region Terrain Smooting
            //Vector3[,] newPositions = positions;

            //// Smoothing Passes
            //for (int i = 0; i < 3; ++i)
            //{
            //    //Flatten Terrain
            //    for (int z = 0; z < heightfield.Height; ++z)
            //    {
            //        for (int x = 0; x < heightfield.Width; ++x)
            //        {
            //            float verticieCumulitiveHeight = 0;
            //            int adjacentVerticies = 0;

            //            bool validNegativeY = z - 1 > 0;
            //            bool validNegativeX = x - 1 > 0;
            //            bool validPositiveY = z + 1 < heightfield.Height;
            //            bool validPositiveX = x + 1 < heightfield.Width;

            //            //Check the left
            //            if (validNegativeX)
            //            {
            //                //Left
            //                verticieCumulitiveHeight += positions[x - 1, z].Y;
            //                ++adjacentVerticies;

            //                if (validPositiveY)
            //                {
            //                    //Upper Left
            //                    verticieCumulitiveHeight += positions[x - 1, z + 1].Y;
            //                    ++adjacentVerticies;
            //                }
            //                if (validNegativeY)
            //                {
            //                    //Lower Left
            //                    verticieCumulitiveHeight += positions[x - 1, z - 1].Y;
            //                    ++adjacentVerticies;
            //                }
            //            }

            //            //Check the Right
            //            if (validPositiveX)
            //            {
            //                //Right
            //                verticieCumulitiveHeight += positions[x + 1, z].Y;
            //                ++adjacentVerticies;

            //                if (validPositiveY)
            //                {
            //                    //Upper Right
            //                    verticieCumulitiveHeight += positions[x + 1, z + 1].Y;
            //                    ++adjacentVerticies;
            //                }

            //                if (validNegativeY)
            //                {
            //                    //Lower Right
            //                    verticieCumulitiveHeight += positions[x + 1, z - 1].Y;
            //                    ++adjacentVerticies;
            //                }
            //            }

            //            if (validPositiveY)
            //            {
            //                //Up
            //                verticieCumulitiveHeight += positions[x, z + 1].Y;
            //                ++adjacentVerticies;
            //            }

            //            if (validNegativeY)
            //            {
            //                //Bottom
            //                verticieCumulitiveHeight += positions[x, z - 1].Y;
            //                ++adjacentVerticies;
            //            }

            //            //Center position
            //            newPositions[x, z].Y = (verticieCumulitiveHeight / adjacentVerticies);
            //        }
            //    }

            //    for (int z = 0; z < heightfield.Height; ++z)
            //    {
            //        for (int x = 0; x < heightfield.Width; ++x)
            //        {
            //            positions[x, z].Y = newPositions[x, z].Y;
            //        }
            //    }
            //}
            //#endregion
    }
}
