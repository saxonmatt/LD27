﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LD27
{
    public class Chunk
    {
        public const int X_SIZE = 16, Y_SIZE = 16, Z_SIZE = 32;

        public Voxel[, ,] Voxels = new Voxel[X_SIZE,Y_SIZE,Z_SIZE];

        public VertexPositionNormalColor[] VertexArray;
        public short[] IndexArray;

        VoxelWorld parentWorld;
        public int worldX, worldY, worldZ;

        public BoundingSphere boundingSphere;

        public bool Visible = false;
        public bool Updated = false;

        VertexBuffer vb;
        
        public Chunk(VoxelWorld world, int wx, int wy, int wz, bool createGround)
        {
            parentWorld = world;
            worldX = wx;
            worldY = wy;
            worldZ = wz;

            boundingSphere = new BoundingSphere(new Vector3(worldX * (X_SIZE * Voxel.SIZE), worldY * (Y_SIZE * Voxel.SIZE), worldZ * (Z_SIZE * Voxel.SIZE)) + (new Vector3(X_SIZE * Voxel.SIZE, Y_SIZE * Voxel.SIZE, Z_SIZE * Voxel.SIZE) / 2f), (X_SIZE * Voxel.SIZE));

            if (createGround)
            {
                for (int y = 0; y < Y_SIZE; y++)
                    for (int x = 0; x < X_SIZE; x++)
                    {
                        for (int z = Chunk.Z_SIZE - 1; z >= Chunk.Z_SIZE - 2; z--)
                        {
                            SetVoxel(x, y, z, true, 0, VoxelType.Ground, new Color(0.3f, 0.3f,0.3f), new Color(0.2f,0.2f,0.2f));
                        }
                    }
            }

            
        }

        public void SetVoxel(int x, int y, int z, bool active, byte destruct, VoxelType type, Color top, Color side)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

            Voxels[x, y, z].Active = active;
            Voxels[x, y, z].Type = type;
            Voxels[x, y, z].Destructable = destruct;
            Voxels[x, y, z].TR = top.R;
            Voxels[x, y, z].TG = top.G;
            Voxels[x, y, z].TB = top.B;
            Voxels[x, y, z].SR = side.R;
            Voxels[x, y, z].SG = side.G;
            Voxels[x, y, z].SB = side.B;
            //= new Voxel(active, type, top, side);

            Updated = true;
        }

        public void UpdateMesh()
        {
            Vector3 meshCenter = (new Vector3(X_SIZE, Y_SIZE, Z_SIZE) * Voxel.SIZE) / 2f;
            parentWorld.Vertices.Clear();
            parentWorld.Indexes.Clear();

            for (int z = Z_SIZE - 1; z >= 0; z--)
                for (int y = 0; y < Y_SIZE; y++)
                     for(int x=0;x<X_SIZE;x++)
                    {
                        if (Voxels[x, y, z].Active == false) continue;

                        Vector3 worldOffset = new Vector3(worldX*(X_SIZE*Voxel.SIZE), worldY*(Y_SIZE*Voxel.SIZE),worldZ*(Z_SIZE*Voxel.SIZE)) + ((new Vector3(x, y, z) * Voxel.SIZE));// - meshCenter);

                        if (!IsVoxelAt(x, y, z - 1)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), CalcLighting(x,y,z, new Color(Voxels[x, y, z].TR, Voxels[x, y, z].TG,Voxels[x, y, z].TB)));
                        //if (!IsVoxelAt(x, y, z + 1)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, 1f), CalcLighting(x, y-1, z, new Color(Voxels[x, y, z].TR, Voxels[x, y, z].TG, Voxels[x, y, z].TB)));
                        if (!IsVoxelAt(x - 1, y, z)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-1f, 0f, 0f), CalcLighting(x - 1, y, z, new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB)));
                        if (!IsVoxelAt(x + 1, y, z)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(1f, 0f, 0f), CalcLighting(x + 1, y, z, new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB)));
                        if (!IsVoxelAt(x, y + 1, z)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 1f, 0f), CalcLighting(x, y + 1, z, new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB)));
                        //if (!IsVoxelAt(x, y - 1, z)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), CalcLighting(x, y - 1, z, new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB))); 
                    }

            VertexArray = parentWorld.Vertices.ToArray();
            IndexArray = new short[parentWorld.Indexes.Count];

            for (int ind = 0; ind < parentWorld.Indexes.Count / 6; ind++)
            {
                for (int i = 0; i < 6; i++)
                {
                    IndexArray[(ind * 6) + i] = (short)(parentWorld.Indexes[(ind * 6) + i] + (ind * 4));
                }
            }

            parentWorld.Vertices.Clear();
            parentWorld.Indexes.Clear();

            Updated = false;
        }

        bool[] lightingDirs = new bool[9];
        Color CalcLighting(int x, int y, int z, Color currentColor)
        {
            z = z - 1;
            //if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return currentColor;

            Vector3 colVect = currentColor.ToVector3();
            float intensityFactor = 0.1f;
            float light = 1f;

            for (int i = 0; i < 9; i++) lightingDirs[i] = false;

            for (int zz = 0; zz < 8; zz++)
            {
                float intensity = (intensityFactor / 8f) * (8f - (float)zz);
                if ((!lightingDirs[0]) && IsVoxelAt(x, y, z - zz)) { light -= (intensity * 4f); lightingDirs[0] = true; }
                if ((!lightingDirs[0]) && IsVoxelAt(x, y, z - (zz+5))) { light -= intensity; lightingDirs[0] = true; }
                if ((!lightingDirs[0]) && IsVoxelAt(x, y, z - (zz + 10))) { light -= intensity; lightingDirs[0] = true; }
                if ((!lightingDirs[1]) && IsVoxelAt(x - zz, y - zz, z - zz)) { light -= intensity; lightingDirs[1] = true; }
                if ((!lightingDirs[2]) && IsVoxelAt(x, y - zz, z - zz)) { light -= intensity; lightingDirs[2] = true; }
                if ((!lightingDirs[3]) && IsVoxelAt(x + zz, y - zz, z - zz)) { light -= intensity; lightingDirs[3] = true; }
                if ((!lightingDirs[4]) && IsVoxelAt(x - zz, y, z - zz)) { light -= intensity; lightingDirs[4] = true; }
                if ((!lightingDirs[5]) && IsVoxelAt(x + zz, y, z - zz)) { light -= intensity; lightingDirs[5] = true; }
                if ((!lightingDirs[6]) && IsVoxelAt(x - zz, y + zz, z - zz)) { light -= intensity; lightingDirs[6] = true; }
                if ((!lightingDirs[7]) && IsVoxelAt(x, y + zz, z - zz)) { light -= intensity; lightingDirs[7] = true; }
                if ((!lightingDirs[8]) && IsVoxelAt(x + zz, y + zz, z - zz)) { light -= intensity; lightingDirs[8] = true; }
            }

            light = MathHelper.Clamp(light, 0f, 1f);

            return new Color(colVect * light);
        }

        void MakeQuad(Vector3 offset, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Vector3 norm, Color col)
        {
            parentWorld.Vertices.Add(new VertexPositionNormalColor(offset + tl, norm, col));
            parentWorld.Vertices.Add(new VertexPositionNormalColor(offset + tr, norm, col));
            parentWorld.Vertices.Add(new VertexPositionNormalColor(offset + br, norm, col));
            parentWorld.Vertices.Add(new VertexPositionNormalColor(offset + bl, norm, col));
            parentWorld.Indexes.Add(0);
            parentWorld.Indexes.Add(1);
            parentWorld.Indexes.Add(2);
            parentWorld.Indexes.Add(2);
            parentWorld.Indexes.Add(3);
            parentWorld.Indexes.Add(0);
        }

        public bool IsVoxelAt(int x, int y, int z)
        {
            if (x >= 0 && x < X_SIZE && y >= 0 && y < Y_SIZE && z >= 0 && z < Z_SIZE) return Voxels[x, y, z].Active;

            if (x < 0)
                if (worldX == 0) return false;
                else return parentWorld.Chunks[worldX - 1, worldY, worldZ].IsVoxelAt(X_SIZE + x, y, z);

            if (x >= X_SIZE)
                if (worldX >= parentWorld.X_CHUNKS - 1) return false;
                else return parentWorld.Chunks[worldX + 1, worldY, worldZ].IsVoxelAt(x - X_SIZE, y, z);

            if (y < 0)
                if (worldY == 0) return false;
                else return parentWorld.Chunks[worldX, worldY - 1, worldZ].IsVoxelAt(x, Y_SIZE + y, z);

            if (y >= Y_SIZE)
                if (worldY >= parentWorld.Y_CHUNKS - 1) return false;
                else return parentWorld.Chunks[worldX, worldY + 1, worldZ].IsVoxelAt(x, y - Y_SIZE, z);

            if (z < 0)
                if (worldZ == 0) return false;
                else return parentWorld.Chunks[worldX, worldY, worldZ - 1].IsVoxelAt(x, y, Z_SIZE + z);

            if (z >= Z_SIZE)
                if (worldZ >= parentWorld.Z_CHUNKS - 1) return false;
                else return parentWorld.Chunks[worldX, worldY, worldZ + 1].IsVoxelAt(x, y, z-Z_SIZE);

            return false;
        }

        public void ClearMem()
        {
            VertexArray = null;
            IndexArray = null;
            
        }
    }
}
