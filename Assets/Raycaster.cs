using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Raycaster : MonoBehaviour
{
    #region Camera
        // Main Camera
        public Camera Camera;
        // Camera position
        [SerializeField] private Vector3 cameraPosition = new Vector3(0, 4f, 5.5f);
    #endregion

    #region
    public GameObject plane;
    [SerializeField] private Vector3 planePosition = new Vector3(0, 5f, -2f);
    [SerializeField] private Vector3 planeRotation = new Vector3(90f, 0, 0);

    [SerializeField] private float width = 640f;
    [SerializeField] private float height = 480f;
    #endregion

    #region Light
        // Point Light
        public Light pointLight;
        // Light position
        [SerializeField] Vector3 lightPosition = new Vector3(0, 7.5f, 3f);
    #endregion

    #region Spheres
        // Number of spheres in the scene
        [SerializeField] private const int NUMBER_SPHERES = 20;

    // Sphere radio ranges
        private float sphereRadio;
        [SerializeField] private float minRadio = 0.1f;
        [SerializeField] private float maxRadio = 0.35f;

        // Sphere Position Y ranges
        [SerializeField] private float minPositionX = -2.0f;
        [SerializeField] private float maxPositionX = 2.0f;
        // Sphere Position Y ranges
        [SerializeField] private float minPositionY = 2.0f;
        [SerializeField] private float maxPositionY = 6.0f;
        // Sphere Position Z ranges
        [SerializeField] private float minPositionZ = 8.0f;
        [SerializeField] private float maxPositionZ = 10.0f;
    #endregion

    #region Ilumination
        // Ambient light intensity
        [SerializeField] private Vector3 Ia = new Vector3(0.7f, 0.7f, 0.7f);
        // Diffuse light intensity
        [SerializeField] private Vector3 Id = new Vector3(0.8f, 0.8f, 1f);
        // Specular light intensity
        [SerializeField] private Vector3 Is = new Vector3(1f, 1f, 1f);
    #endregion

    #region Constants
        // Diffuse constant ranges
        private float kdr, kdg, kdb;
        [SerializeField] private float kdMin = 0.5f;
        [SerializeField] private float kdMax = 1.0f;

        // Ambient constant 
        [SerializeField] private Vector3 ka /* = (kdr/5.0, kdg/5.0, kdb/5.0)*/;
        // Specular constants
        [SerializeField] private Vector3 ks /* = (kdr/3.0, kdg/3.0, kdb/3.0)*/;
        // Alpha ranges
        [SerializeField] private float minAlpha = 500f;
        [SerializeField] private float maxAlpha = 600f;
    #endregion

    #region Raycaster
        private Vector3 NC, NTL, NTR, NBL, NBR;
        private Vector3 pixelPos;
        private float pixelCenterW, pixelCenterH;
        private float NEARheight, NEARwidth;
    #endregion


    private void Start()
    {
        #region Plane Modifications
            plane.transform.localPosition = planePosition;
            plane.transform.Rotate(planeRotation);
            plane.transform.localScale = Vector3.one;
        #endregion

        #region Camera Modifications
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(0f, 0f, 0f);
            Camera.fieldOfView = 65f;
            Camera.nearClipPlane = 1f;
            Camera.farClipPlane = 20f;
            Camera.transform.localPosition = cameraPosition;
            Camera.transform.localRotation = Quaternion.identity;
            Camera.transform.localScale = Vector3.one;
        #endregion

        #region Light Modifications
            pointLight.transform.localPosition = lightPosition;
            pointLight.intensity = Id.magnitude;
            pointLight.color = new Color(Id.x, Id.y, Id.z);

            RenderSettings.ambientLight = new Color(Ia.x, Ia.y, Id.z);
        #endregion

        #region Spheres Generator
        for (int i = 0; i < NUMBER_SPHERES; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            sphereRadio = Random.Range(minRadio, maxRadio);
            sphere.transform.localScale *= sphereRadio * 2;
            sphere.transform.localPosition = new Vector3(Random.Range(minPositionX, maxPositionX), Random.Range(minPositionY, maxPositionY), Random.Range(minPositionZ, maxPositionZ));

            sphere.GetComponent<Renderer>().material.shader = Shader.Find("Legacy Shaders/Specular");
            kdr = Random.Range(kdMin, kdMax);
            kdg = Random.Range(kdMin, kdMax);
            kdb = Random.Range(kdMin, kdMax);
            sphere.GetComponent<Renderer>().material.SetColor("_Color", new Color(kdr, kdg, kdb, Random.Range(minAlpha / 1000f, maxAlpha / 1000f))); // Diffuse
            sphere.GetComponent<Renderer>().material.SetColor("_Emission", new Color(kdr / 5.0f, kdg / 5.0f, kdb / 5.0f, Random.Range(minAlpha / 1000f, maxAlpha / 1000f))); // Ambient
            sphere.GetComponent<Renderer>().material.SetColor("_SpecColor", new Color(kdr / 3.0f, kdg / 3.0f, kdb / 3.0f, Random.Range(minAlpha / 1000f, maxAlpha / 1000f))); // Specular
        }
        #endregion

        #region Raycaster Calculus
            NC = cameraPosition + Camera.transform.forward * Camera.nearClipPlane;
        GameObject sphNC = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphNC.GetComponent<Renderer>().material.color = Color.red;
        sphNC.transform.localPosition = NC;
        sphNC.transform.localScale = new Vector3(.1f, .1f, .1f);

        float angleFOV = Camera.fieldOfView * Mathf.Deg2Rad;
        NEARheight = 2 * Mathf.Tan(angleFOV / 2) * Camera.nearClipPlane;
        NEARwidth = NEARheight * Camera.aspect;

        NTL = NC + Camera.transform.up * NEARheight / 2 + (-Camera.transform.right) * NEARwidth / 2;
        GameObject sphNTL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphNTL.GetComponent<Renderer>().material.color = Color.red;
        sphNTL.transform.localPosition = NTL;
        sphNTL.transform.localScale = new Vector3(.1f, .1f, .1f);

        Debug.Log(1 / 640f);

        // Texels
        pixelCenterW = (NEARwidth / 640f) / 2;
        pixelCenterH = (NEARheight / 480f) / 2;

        //pixelPos = NTL + Camera.transform.right * 1 * (1 / 640f) - Camera.transform.up * 1 * (1 / 480f) + Camera.transform.right * pixelCenterW - Camera.transform.up * pixelCenterH;
        #endregion
    }

    private void Update()
    {
        for (int i = 0; i <= width; i++) {
            for (int j = 0; j <= height; j++)
            {
                Vector3 pixelPos = pixelPosCalc(i, j);
                //Debug.DrawLine(Camera.transform.position, pixelPos, Color.green); //
            }
        }
    }

    public Vector3 pixelPosCalc(int i, int j)
    {
        pixelPos = NTL + Camera.transform.right * i * (NEARwidth / width) - Camera.transform.up * j * (NEARheight / height) + Camera.transform.right * pixelCenterW - Camera.transform.up * pixelCenterH;
        return pixelPos;
    }

}

// MaterialSetColor para cambio de color por medio de especular, modificacion de shader de Legacyrenderer/Specular
// Shader.Find(LegacyShaderSpecular) para cambiar el shader del material