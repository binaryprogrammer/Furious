using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FuriousGameEngime_XNA4.GameEntities;

namespace FuriousGameEngime_XNA4
{
    enum TreeSection
    {
        leftLowerTopCell,
        leftUpperTopCell,
        rightLowerTopCell,
        rightUpperTopCell,
        leftLowerBottomCell,
        leftUpperBottomCell,
        rightLowerBottomCell,
        rightUpperBottomCell,
    }

    class SpatialPartitionCell
    {
        SpatialPartitioning _spatialPartitioning;

        #region OctTree
        /// <summary>
        /// Top left <see cref="SpatialPartitionCell"/> child section on the upper half of this <see cref="SpatialPartitionCell"/>.
        /// </summary>
        SpatialPartitionCell _leftUpperTopCell;

        /// <summary>
        /// Top right <see cref="SpatialPartitionCell"/> child section on the upper half of this <see cref="SpatialPartitionCell"/>.
        /// </summary>
        SpatialPartitionCell _rightUpperTopCell;

        /// <summary>
        /// Bottom left <see cref="SpatialPartitionCell"/> child section on the upper half of this <see cref="SpatialPartitionCell"/>.
        /// </summary>
        SpatialPartitionCell _leftUpperBottomCell;

        /// <summary>
        /// Bottom right <see cref="SpatialPartitionCell"/> child section on the upper half of this <see cref="SpatialPartitionCell"/>.
        /// </summary>
        SpatialPartitionCell _rightUpperBottomCell;

        /// <summary>
        /// Top left <see cref="SpatialPartitionCell"/> child section on the lower half of this <see cref="SpatialPartitionCell"/>.
        /// </summary>
        SpatialPartitionCell _leftLowerTopCell;

        /// <summary>
        /// Top right <see cref="SpatialPartitionCell"/> child section on the lower half of this <see cref="SpatialPartitionCell"/>.
        /// </summary>
        SpatialPartitionCell _rightLowerTopCell;

        /// <summary>
        /// Bottom left <see cref="SpatialPartitionCell"/> child section on the lower half of this <see cref="SpatialPartitionCell"/>.
        /// </summary>
        SpatialPartitionCell _leftLowerBottomCell;

        /// <summary>
        /// Bottom right <see cref="SpatialPartitionCell"/> child section on the lower half of this <see cref="SpatialPartitionCell"/>.
        /// </summary>
        SpatialPartitionCell _rightLowerBottomCell;

        /// <summary>
        /// a list of all <see cref="SpatialPartitionCell"/> that branch from this one
        /// </summary>
        SpatialPartitionCell[] _childCells;

        /// <summary>
        /// the parentCells all the way up to the root
        /// </summary>
        List<SpatialPartitionCell> _parentCells = new List<SpatialPartitionCell>();

        /// <summary>
        /// if this <see cref="SpatialPartitionCell"/> doesn't break down farther
        /// </summary>
        bool _isLeaf = true;
        #endregion

        List<Entity> _cellObjects = new List<Entity>();

        internal BoundingBox cellBox = new BoundingBox();

        Vector3 _key;

        float _cellSize;

        /// <summary>
        /// creates root cells
        /// </summary>
        /// <param name="spatialPartitioning"></param>
        /// <param name="key"></param>
        internal SpatialPartitionCell(SpatialPartitioning spatialPartitioning, Vector3 key)
        {
            _spatialPartitioning = spatialPartitioning;
            _key = key;
            _cellSize = spatialPartitioning.cellSize;

            Vector3 min = Vector3.Zero;
            Vector3 max = new Vector3(spatialPartitioning.cellSize, spatialPartitioning.cellSize, spatialPartitioning.cellSize);

            Vector3 offset = new Vector3(key.X, key.Y, key.Z) * new Vector3(_spatialPartitioning.cellSize, _spatialPartitioning.cellSize, _spatialPartitioning.cellSize);

            min += offset;
            max += offset;

            cellBox = new BoundingBox(min, max);
        }

        internal List<Entity> CellObjects
        {
            get
            {
                return _cellObjects;
            }
        }

        /// <summary>
        /// Creates child cells only
        /// </summary>
        /// <param name="spatialPartitioning"></param>
        /// <param name="key"></param>
        /// <param name="cellSize"></param>
        private SpatialPartitionCell(SpatialPartitioning spatialPartitioning, Vector3 key, float cellSize)
        {
            _spatialPartitioning = spatialPartitioning;
            _key = key;

            Vector3 min = Vector3.Zero;
            Vector3 max = new Vector3(cellSize, cellSize, cellSize);

            Vector3 offset = new Vector3(key.X, key.Y, key.Z) * new Vector3(cellSize, cellSize, cellSize);

            min += offset;
            max += offset;

            cellBox = new BoundingBox(min, max);
        }

        internal void Subdivide()
        {
            _cellSize /= 2;

            Vector3 newKey = _key + new Vector3(.5f, .5f, .5f) / (_parentCells.Count + 1);

            _leftLowerTopCell = new SpatialPartitionCell(_spatialPartitioning, new Vector3(_key.X, newKey.Y, _key.Z), _cellSize);
            _leftUpperTopCell = new SpatialPartitionCell(_spatialPartitioning, new Vector3(_key.X, _key.Y, _key.Z), _cellSize);
            _rightLowerTopCell = new SpatialPartitionCell(_spatialPartitioning, new Vector3(newKey.X, newKey.Y, _key.Z), _cellSize);
            _rightUpperTopCell = new SpatialPartitionCell(_spatialPartitioning, new Vector3(newKey.X, _key.Y, _key.Z), _cellSize);
            _leftLowerBottomCell = new SpatialPartitionCell(_spatialPartitioning, new Vector3(_key.X, newKey.Y, newKey.Z), _cellSize);
            _leftUpperBottomCell = new SpatialPartitionCell(_spatialPartitioning, new Vector3(_key.X, _key.Y, newKey.Z), _cellSize);
            _rightLowerBottomCell = new SpatialPartitionCell(_spatialPartitioning, new Vector3(newKey.X, newKey.Y, newKey.Z), _cellSize);
            _rightUpperBottomCell = new SpatialPartitionCell(_spatialPartitioning, new Vector3(newKey.X, _key.Y, newKey.Z), _cellSize);

            _childCells = new SpatialPartitionCell[8];

            _leftLowerTopCell._parentCells.Add(this);
            _leftUpperTopCell._parentCells.Add(this);
            _rightLowerTopCell._parentCells.Add(this);
            _rightUpperTopCell._parentCells.Add(this);
            _leftLowerBottomCell._parentCells.Add(this);
            _leftUpperBottomCell._parentCells.Add(this);
            _rightLowerBottomCell._parentCells.Add(this);
            _rightUpperBottomCell._parentCells.Add(this);

            //_spatialPartitioning.collisionCells.Remove(_leftUpperTopCell._key);
            _spatialPartitioning.collisionCells.Add(_leftLowerTopCell._key, _leftLowerTopCell);
            _spatialPartitioning.collisionCells.Add(_leftUpperTopCell._key, _leftUpperTopCell);
            _spatialPartitioning.collisionCells.Add(_rightLowerTopCell._key, _rightLowerTopCell);
            _spatialPartitioning.collisionCells.Add(_rightUpperTopCell._key, _rightUpperTopCell);
            _spatialPartitioning.collisionCells.Add(_leftLowerBottomCell._key, _leftLowerBottomCell);
            _spatialPartitioning.collisionCells.Add(_leftUpperBottomCell._key, _leftUpperBottomCell);
            _spatialPartitioning.collisionCells.Add(_rightLowerBottomCell._key, _rightLowerBottomCell);
            _spatialPartitioning.collisionCells.Add(_rightUpperBottomCell._key, _rightUpperBottomCell);

            _childCells[(int)TreeSection.leftLowerBottomCell] = _leftLowerBottomCell;
            _childCells[(int)TreeSection.leftLowerTopCell] = _leftLowerTopCell;
            _childCells[(int)TreeSection.leftUpperBottomCell] = _leftUpperBottomCell;
            _childCells[(int)TreeSection.leftUpperTopCell] = _leftUpperTopCell;
            _childCells[(int)TreeSection.rightLowerBottomCell] = _rightLowerBottomCell;
            _childCells[(int)TreeSection.rightLowerTopCell] = _rightLowerTopCell;
            _childCells[(int)TreeSection.rightUpperBottomCell] = _rightUpperBottomCell;
            _childCells[(int)TreeSection.rightUpperTopCell] = _rightUpperTopCell;

            ReassignObjects();
        }

        void ReassignObjects()
        {
            //add all cell objects to the new cells based on it's brach key.
        }

        /// <summary>
        /// gets the spatial partition key from any given coordinate
        /// </summary>
        /// <param name="pixelCoordinate">where you are in the world</param>
        /// <returns>reurns the spatial partition key</returns>
        internal Vector3 BranchKeyFromWorldCoordinate(Vector3 pixelCoordinate)
        {
            Vector3 divided = new Vector3(pixelCoordinate.X / _cellSize, pixelCoordinate.Y / _cellSize, pixelCoordinate.Z / _cellSize);
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
