using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyMesh : MonoBehaviour
{
    public float Intensity = 1f;
    public float mass = 1f;
    public float stiffness = 1f;
    public float damping = 0.75f;
    
    private Mesh _originalMesh, _meshClone;
    private MeshRenderer _renderer;

    private JellyVertex[] JellyVertices;
    private Vector3[] VerticesArray;
    
    // Start is called before the first frame update
    void Start()
    {
        _originalMesh = GetComponent<MeshFilter>().sharedMesh;
        _meshClone = Instantiate(_originalMesh);

        GetComponent<MeshFilter>().sharedMesh = _meshClone;
        
        _renderer = GetComponent<MeshRenderer>();

        JellyVertices = new JellyVertex[_meshClone.vertices.Length];
        for (int i = 0; i < _meshClone.vertices.Length; i++)
        {
            JellyVertices[i] = new JellyVertex(i, transform.TransformPoint(_meshClone.vertices[i]));
        }
    }

    private void FixedUpdate()
    {
        VerticesArray = _originalMesh.vertices;
        for (int i = 0; i < JellyVertices.Length; i++)
        {
            Vector3 target = transform.TransformPoint(VerticesArray[JellyVertices[i].ID]);
            float intensity = (1 - (_renderer.bounds.max.y - target.y) / _renderer.bounds.size.y) * Intensity;
            
            JellyVertices[i].Shake(target, mass, stiffness, damping);

            target = transform.InverseTransformPoint(JellyVertices[i].Position);
            VerticesArray[JellyVertices[i].ID] = Vector3.Lerp(VerticesArray[JellyVertices[i].ID], target, intensity);
        }

        _meshClone.vertices = VerticesArray;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class JellyVertex
    {
        public int ID;
        public Vector3 Position;
        public Vector3 Velocity, Force;

        public JellyVertex(int _id, Vector3 _pos)
        {
            ID = _id;
            Position = _pos;
        }

        public void Shake(Vector3 target, float m, float s, float d)
        {
            Force = (target - Position) * s;
            Velocity = (Velocity + Force / m) * d;
            Position += Velocity;

            if ((Velocity + Force + Force / 2).magnitude < 0.001f)
                Position = target;
        }
        
    }
    
    
}
