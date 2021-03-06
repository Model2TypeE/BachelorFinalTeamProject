﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadMesh
{
    private Vector3[] vertices;
    public Vector3 Position;
    private int[] triangles;

    public QuadMesh(Vector3[] vertices, int[] triangles)
    {
        this.vertices = vertices;
        this.triangles = triangles;
    }

    public int[] Triangles => triangles;
    public Vector3[] Vertices => vertices;
}
