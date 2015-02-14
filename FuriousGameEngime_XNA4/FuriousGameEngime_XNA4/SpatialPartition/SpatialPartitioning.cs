using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FuriousGameEngime_XNA4.HelperClasses;
using FuriousGameEngime_XNA4.GameEntities;

namespace FuriousGameEngime_XNA4
{
    class SpatialPartitioning
    {
        internal Dictionary<Vector3, SpatialPartitionCell> collisionCells = new Dictionary<Vector3, SpatialPartitionCell>();

        Game _game;

        /// <summary>
        /// the key
        /// </summary>
        Vector3 _min;

        /// <summary>
        /// the key
        /// </summary>
        Vector3 _max;

        /// <summary>
        /// the size of each cell
        /// </summary>
        internal readonly float cellSize;

        internal SpatialPartitioning(Game game, float cellSize)
        {
            _game = game;

            this.cellSize = cellSize;
        }

        /// <summary>
        /// Adds this entity to the spatial partition universe
        /// </summary>
        /// <param name="entity">which entity to add</param>
        /// <returns>returns the tag (think a coat check)</returns>
        internal SpatialPartitionTag Add(Entity entity)
        {
            //if (entity.collisionType == Entity.CollisionType.OrientedBox)
            //{
            //    BoundingBox box = BoundingBox.CreateFromPoints(entity.OrientedBoundingBox.GetCorners());

            //    _min = KeyFromWorldCoordinate(box.Min);
            //    _max = KeyFromWorldCoordinate(box.Max);
            //}
            //else
            {
                BoundingSphere sphere = entity.BoundingSphere;
                _min = KeyFromWorldCoordinate(Vector3Helper.SubtractValue(sphere.Center, sphere.Radius));
                _max = KeyFromWorldCoordinate(Vector3Helper.AddValue(sphere.Center, sphere.Radius));
            }

            SpatialPartitionTag tag = new SpatialPartitionTag();

            for (int x = (int)_min.X; x <= _max.X; ++x)
            {
                for (int y = (int)_min.Y; y <= _max.Y; ++y)
                {
                    for (int z = (int)_min.Z; z <= _max.Z; ++z)
                    {
                        Vector3 key = new Vector3(x, y, z);
                        SpatialPartitionCell value;
                        if (!collisionCells.TryGetValue(key, out value))
                        {
                            //add a cell at the key
                            value = new SpatialPartitionCell(this, key);
                            collisionCells.Add(key, value);
                        }
                        value.CellObjects.Add(entity);

                        tag.keys.Add(new Vector3(x, y, z));


                        //if (value.CellObjects.Count > 50)
                        //{
                        //    value.Subdivide();
                        //}
                    }
                }
            }
            return tag;
        }

        /// <summary>
        /// removes an entity to the spatial partition universe
        /// </summary>
        /// <param name="entity">the entity to remove</param>
        /// <param name="tag">the tag of the entity (think coat check)</param>
        internal void Remove(Entity entity, SpatialPartitionTag tag)
        {
            for (int i = 0; i < tag.keys.Count; ++i)
            {
                Vector3 key = tag.keys[i];
                collisionCells[key].CellObjects.Remove(entity);

                //check if the cell is empty
                if (collisionCells[key].CellObjects.Count == 0)
                {
                    //it is empty, remove it
                    collisionCells.Remove(tag.keys[i]);
                }
            }
        }

        // Set a minimum key distance and a max, have it go by that determined by cell size.
        /// <summary>
        /// gets the spatial partition key from any given coordinate
        /// </summary>
        /// <param name="pixelCoordinate">where you are in the world</param>
        /// <returns>reurns the spatial partition key</returns>
        internal Vector3 KeyFromWorldCoordinate(Vector3 pixelCoordinate)
        {
            Vector3 divided = new Vector3(pixelCoordinate.X / cellSize, pixelCoordinate.Y / cellSize, pixelCoordinate.Z / cellSize);
            Vector3 ret = new Vector3();
            if (divided.X < 0)
            {
                ret.X = (int)Math.Floor(divided.X);
            }
            else
            {
                ret.X = (int)divided.X;
            }

            if (divided.Y < 0)
            {
                ret.Y = (int)Math.Floor(divided.Y);
            }
            else
            {
                ret.Y = (int)divided.Y;
            }
            if (divided.Z < 0)
            {
                ret.Z = (int)Math.Floor(divided.Z);
            }
            else
            {
                ret.Z = (int)divided.Z;
            }

            //return new Point((int)camera.ToWorldLocation(new Vector2(ret.X, ret.Y)).X, (int)camera.ToWorldLocation(new Vector2(ret.X, ret.Y)).Y);
            return ret;
        }
    }
}
