using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class PlanCreation
{
    private static float _voxelSize = 0.5f;
    private static int _resolutionSide = 256;
    private static int _floorHeight = 25; //floorheight in voxel units, not in meter
    private static int _floorCutHeight = 12;


    public static void CreatePlanFromTiles(List<Tile> tiles)
    {
        // get the bounds of all the transforms
        Bounds boundary = new Bounds();
        foreach (Collider collider in tiles.Where(t=>t.Collider!= null).Select(t => t.Collider))
        {
            var tBounds = collider.bounds;
            boundary.Encapsulate(tBounds);
        }

        int gridXImages = Mathf.CeilToInt(boundary.size.x / (_voxelSize * _resolutionSide));
        int gridX = gridXImages * _resolutionSide;

        int gridZImages = Mathf.CeilToInt(boundary.size.z / (_voxelSize * _resolutionSide));
        int gridZ = gridZImages * _resolutionSide;



        int gridY = Mathf.CeilToInt(boundary.size.y / _voxelSize);
        int floors = Mathf.CeilToInt((float)gridY / _floorHeight);

        Vector3 origin = boundary.min;

        Texture2D[,,] textures = new Texture2D[gridXImages, floors, gridZImages];

        for (int y = 0; y < floors; y++)
        {
            for (int x = 0; x < gridXImages; x++)
            {
                for (int z = 0; z < gridZImages; z++)
                {

                    textures[x, y, z] = GenerateImage(origin, x, z, y, tiles);
                }
            }
        }
    }

    private static Texture2D GenerateImage(Vector3 origin, int gridImageX, int gridImageZ, int floor, List<Tile> tiles)
    {
        GameObject goDaddy = new GameObject();
        
        Texture2D texture = new Texture2D(_resolutionSide, _resolutionSide);
        for (int x = 0; x < _resolutionSide; x++)
        {
            for (int z = 0; z < _resolutionSide; z++)
            {
                Vector3 position = origin +
                    (_voxelSize * _resolutionSide * new Vector3(gridImageX, 0, gridImageZ) //image origin
                    + new Vector3(x * _voxelSize, ((floor-1) * _floorHeight + _floorCutHeight) * _voxelSize, z * _voxelSize)); //exact position and floor


                //Check if the position is within the building
                int counter = tiles.Count;
                bool isInside = false;
                while (!isInside && --counter >= 0)
                {
                    if (Util.PointInsideCollider(position, tiles[counter].Collider))
                        isInside = true;


                }
                if (isInside)
                {
                    texture.SetPixel(x, z, Color.black);
                   
                }
                else
                    texture.SetPixel(x, z, Color.white);

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = position;
                go.transform.localScale = Vector3.one * _voxelSize;
                go.transform.parent = goDaddy.transform;
            }
        }

        texture.Apply();

        ImageReadWrite.SaveImage(texture, $"p2pOutput/{gridImageX},{floor},{gridImageZ}");
        return texture;
    }

}
