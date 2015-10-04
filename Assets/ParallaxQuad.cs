using UnityEngine;
using System.Collections;

public class ParallaxQuad : MonoBehaviour {


    public float parallax = 1f;    

    
    Camera cam;    
    
    Vector3 localScale = Vector3.one;
    Vector3 baseScale;
    Vector3 pos = Vector3.zero;
    
    Vector2 offset;
    Vector2 baseTextureScale;
    Vector2 textureOffset = Vector2.zero;
    Vector2 textureScale;

    Vector2 baseTextureOffset;
    Vector2 offsetConversion = Vector2.zero;
    
    float baseOrthoSize;
    float baseParallaxScale;
    float baseAspect;
    float parallaxScale;
    float pxlPerUnits;
    float p;
    
    //startup settings
    Vector2 startMainTextureScale;
    Vector2 startMainTextureOffset;
    Vector2 startLocalScale;
    Vector2 startOffsetToCamera;
    Vector2 startCameraPosition;
    float   startScaleAspect;
    float   startCameraOrthoGraphicSize;
    Renderer renderer;

    void Awake() {
        renderer = GetComponent<Renderer>();
    }

    void Start() {
        cam = Camera.main;
        //keep reference to initial settings
        startMainTextureScale = this.GetComponent<Renderer>().material.mainTextureScale;
        startMainTextureOffset = this.GetComponent<Renderer>().material.mainTextureOffset;
        startLocalScale = this.transform.localScale;
        startOffsetToCamera = new Vector2 ((cam.transform.position.x - transform.position.x), (cam.transform.position.y - transform.position.y));
        startCameraPosition = cam.transform.position;
        startCameraOrthoGraphicSize = cam.orthographicSize;
        startScaleAspect = this.transform.localScale.y / this.transform.localScale.x;
        
        pos = cam.transform.position;
        pos.z = transform.position.z;
        
        initialize();
    }
    
    void initialize() {
        p = parallax;        
        
        baseTextureScale = startMainTextureScale;
        baseTextureScale.y /= startScaleAspect;
        
        baseTextureOffset = startMainTextureOffset;
        baseTextureOffset.x += baseTextureScale.x * 0.5f;
        baseTextureOffset.y += baseTextureScale.y * 0.5f * startScaleAspect;

        pxlPerUnits = Screen.height * 0.5f * startLocalScale.x;           
        baseParallaxScale = (1f-p) + (startCameraOrthoGraphicSize  *  p);
        baseTextureScale *= (Screen.height / pxlPerUnits) * (startCameraOrthoGraphicSize/baseParallaxScale);
        baseParallaxScale /= startCameraOrthoGraphicSize;
            
        Vector2 offsetToCamera = startOffsetToCamera;
        offsetToCamera.x -= startCameraPosition.x * p;
        offsetToCamera.y -= startCameraPosition.y * p;
        
        offsetToCamera.x = ((offsetToCamera.x * 0.5f * baseParallaxScale * baseTextureScale.x));
        offsetToCamera.y = ((offsetToCamera.y * 0.5f * baseParallaxScale * baseTextureScale.y));    
        
        baseTextureOffset += offsetToCamera;
        
        offsetConversion.x = baseParallaxScale * baseTextureScale.x * p * 0.5f;
        offsetConversion.y = baseParallaxScale * baseTextureScale.y * p * 0.5f;
    }

    public void Update() {        
        if (p != parallax)
            initialize();

        //fit to screen
        localScale.y = cam.orthographicSize * 2f;
        localScale.x = localScale.y * cam.aspect;
        transform.localScale = localScale;
        
        //scale texture
        parallaxScale = (1f-p) + (cam.orthographicSize * p) * 1f;
        textureScale.x = parallaxScale * baseTextureScale.x * cam.aspect; 
        textureScale.y = parallaxScale * baseTextureScale.y;
        renderer.material.mainTextureScale = textureScale;
        
        //position on camera
        pos.x = cam.transform.position.x;
        pos.y = cam.transform.position.y;
        transform.position = pos;

        //texture offset
        textureOffset.x = baseTextureOffset.x + ((pos.x * offsetConversion.x) - textureScale.x * 0.5f);
        textureOffset.y = baseTextureOffset.y + ((pos.y * offsetConversion.y) - textureScale.y * 0.5f);               
        renderer.material.mainTextureOffset = textureOffset;
    }
}
