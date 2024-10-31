using UnityEngine;

public class Sphere
{
    public Vector3 Center {  get; set; }
    public float Radius { get; set; }
    public Vector3 KD { get; set; }
    public Vector3 KA { get; set; }
    public Vector3 KS { get; set; }
    public float Alpha { get; set; }

    public Sphere(Vector3 center, float radius, Vector3 kD, Vector3 kA, Vector3 kS, float alpha)
    {
        Center = center;
        Radius = radius;
        KD = kD;
        KA = kA;
        KS = kS;
        Alpha = alpha;
    }
}
