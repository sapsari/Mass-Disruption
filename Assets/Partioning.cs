using UnityEngine;
using System.Collections.Generic;

namespace SpatialPartitionPattern
{
    public interface IGridItem
    {
        IGridItem PreviousItem { get; set; }
        IGridItem NextItem { get; set; }
        Vector2 Pos2 { get; }
    }

    public class Grid
    {
        //Need this to convert from world coordinate position to cell position
        int cellSize;

        //This is the actual grid, where a soldier is in each cell
        //Each individual soldier links to other soldiers in the same cell
        IGridItem[,] cells;


        //Init the grid
        public Grid(int mapWidth, int cellSize)
        {
            this.cellSize = cellSize;

            int numberOfCells = mapWidth / cellSize;

            cells = new IGridItem[numberOfCells, numberOfCells];
        }


        //Add a unity to the grid
        public void Add(IGridItem item)
        {
            //Determine which grid cell the soldier is in
            int cellX = (int)(item.Pos2.x / cellSize);
            int cellZ = (int)(item.Pos2.y / cellSize);

            //Add the soldier to the front of the list for the cell it's in
            item.PreviousItem = null;
            item.NextItem = cells[cellX, cellZ];

            //Associate this cell with this soldier
            cells[cellX, cellZ] = item;

            if (item.NextItem != null)
            {
                //Set this soldier to be the previous soldier of the next soldier of this soldier (linked lists ftw)
                item.NextItem.PreviousItem = item;
            }
        }

        IEnumerable<IGridItem> GetCloseEnemiesAux(IGridItem item)
        {
            //Determine which grid cell the friendly soldier is in
            int cellX = (int)(item.Pos2.x / cellSize);
            int cellZ = (int)(item.Pos2.y / cellSize);

            return GetCloseEnemiesAux(cellX, cellZ);
        }

        //IEnumerable<IGridItem> GetCloseEnemiesAux(IGridItem item)
        IEnumerable<IGridItem> GetCloseEnemiesAux(int cellX, int cellZ)
        {
            //Determine which grid cell the friendly soldier is in
            //int cellX = (int)(item.Pos2.x / cellSize);
            //int cellZ = (int)(item.Pos2.y / cellSize);

            //Get the first enemy in grid
            IGridItem enemy = cells[cellX, cellZ];

            //Find the closest soldier of all in the linked list
            IGridItem closestSoldier = null;

            float bestDistSqr = Mathf.Infinity;

            //Loop through the linked list
            while (enemy != null)
            {
                //The distance sqr between the soldier and this enemy
                float distSqr = 0;//**-- (enemy.Pos2 - item.Pos2).sqrMagnitude;

                //If this distance is better than the previous best distance, then we have found an enemy that's closer
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;

                    closestSoldier = enemy;
                }

                yield return enemy;

                //Get the next enemy in the list
                enemy = enemy.NextItem;
            }
        }

        //Get the closest enemy from the grid
        public IGridItem FindClosestEnemy(IGridItem item)
        {
            //Determine which grid cell the friendly soldier is in
            int cellX = (int)(item.Pos2.x / cellSize);
            int cellZ = (int)(item.Pos2.y / cellSize);

            //Get the first enemy in grid
            IGridItem enemy = cells[cellX, cellZ];

            //Find the closest soldier of all in the linked list
            IGridItem closestSoldier = null;

            float bestDistSqr = Mathf.Infinity;

            //Loop through the linked list
            while (enemy != null)
            {
                //The distance sqr between the soldier and this enemy
                float distSqr = (enemy.Pos2 - item.Pos2).sqrMagnitude;

                //If this distance is better than the previous best distance, then we have found an enemy that's closer
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;

                    closestSoldier = enemy;
                }

                //Get the next enemy in the list
                enemy = enemy.NextItem;
            }

            return closestSoldier;
        }


        //A soldier in the grid has moved, so see if we need to update in which grid the soldier is
        public void Move(IGridItem item, Vector3 oldPos)
        {
            //See which cell it was in 
            int oldCellX = (int)(oldPos.x / cellSize);
            int oldCellZ = (int)(oldPos.z / cellSize);

            //See which cell it is in now
            int cellX = (int)(item.Pos2.x / cellSize);
            int cellZ = (int)(item.Pos2.y / cellSize);

            //If it didn't change cell, we are done
            if (oldCellX == cellX && oldCellZ == cellZ)
            {
                return;
            }

            //Unlink it from the list of its old cell
            if (item.PreviousItem != null)
            {
                item.PreviousItem.NextItem = item.NextItem;
            }

            if (item.NextItem != null)
            {
                item.NextItem.PreviousItem = item.PreviousItem;
            }

            //If it's the head of a list, remove it
            if (cells[oldCellX, oldCellZ] == item)
            {
                cells[oldCellX, oldCellZ] = item.NextItem;
            }

            //Add it bacl to the grid at its new cell
            Add(item);
        }
    }
}