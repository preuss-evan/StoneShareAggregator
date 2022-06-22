using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class PlanCreation
{
    private static float _voxelSize = 0.5f; // In metres
    private static int _resolutionSide = 256;
    private static int _floorHeight = 4; //floorheight in metres
    private static int _floorCutHeight = 2;

    // Size of the image in pixels
    private static int _imageSize = 32;
    // Lenght of coverage of the real world in metres
    private static float _coverageLenght = 8f;
    private static float _dotSize => _coverageLenght / _imageSize;


    public static void CreatePlanFromTiles(List<Tile> tiles)
    {
        // get the bounds of all the transforms
        Bounds boundary = new Bounds();
        foreach (Collider collider in tiles.Where(t=>t.Collider!= null).Select(t => t.Collider))
        {
            var tBounds = collider.bounds;
            boundary.Encapsulate(tBounds);
        }

        int gridXImages = Mathf.CeilToInt(boundary.size.x / _coverageLenght);
        //int gridX = gridXImages * _resolutionSide;

        int gridZImages = Mathf.CeilToInt(boundary.size.z / _coverageLenght);
        //int gridZ = gridZImages * _resolutionSide;

        int gridYImages = Mathf.CeilToInt(boundary.size.y / _floorHeight);
        //int floors = Mathf.CeilToInt((float)gridYImages / _floorHeight);

        Vector3 boundaryOrigin = boundary.min;

        Texture2D[,,] textures = new Texture2D[gridXImages, gridYImages, gridZImages];

        for (int y = 0; y < gridYImages; y++)
        {
            for (int x = 0; x < gridXImages; x++)
            {
                for (int z = 0; z < gridZImages; z++)
                {
                    var textureOrigin = boundaryOrigin + new Vector3(x * _coverageLenght, y * _floorHeight, z * _coverageLenght);
                    textures[x, y, z] = GenerateImage(textureOrigin, /*x, z, y,*/ tiles);
                }
            }
        }
    }

    private static Texture2D GenerateImage(Vector3 origin, /*int gridImageX, int gridImageZ, int floor,*/ List<Tile> tiles)
    {
        GameObject goDaddy = new GameObject();

        Texture2D texture = new Texture2D(_imageSize, _imageSize);
        for (int x = 0; x < _imageSize; x++)
        {
            for (int z = 0; z < _imageSize; z++)
            {
                Vector3 position = origin + new Vector3(x * _dotSize,_floorCutHeight,z * _dotSize);
                //Vector3 position = origin +
                //    (_voxelSize * _resolutionSide * new Vector3(gridImageX, 0, gridImageZ) //image origin
                //    + new Vector3(x * _voxelSize, ((floor-1) * _floorHeight + _floorCutHeight) * _voxelSize, z * _voxelSize)); //exact position and floor


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
                go.transform.localScale = Vector3.one * _dotSize;
                go.transform.parent = goDaddy.transform;
            }
        }

        texture.Apply();
        var resizedTexture = ImageReadWrite.Resize256(texture, Color.white);
        ImageReadWrite.SaveImage(resizedTexture, $"p2pOutput/{origin.x},{origin.y},{origin.z}");
        return texture;
    }

}
