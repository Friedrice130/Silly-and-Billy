using UnityEngine;
 
[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;
    private float spriteWidth;

    void Start()
    {
        // Get the width of the sprite being used by this layer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteWidth = spriteRenderer.sprite.bounds.size.x * transform.localScale.x;
        }
        else
        {
            Debug.LogError("ParallaxLayer needs a SpriteRenderer with an assigned Sprite to calculate width for looping!");
        }
    }
 
    public void Move(float delta)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= delta * parallaxFactor;

        if (delta > 0 && newPos.x < -spriteWidth)
        {
            // Reposition it one sprite width to the right
            newPos.x += spriteWidth;
        }
        else if (delta < 0 && newPos.x > 0)
        {

        }
        
        float totalLayerWidth = 2f * spriteWidth; 

        if (newPos.x < -spriteWidth)
        {
            newPos.x += totalLayerWidth; 
        }
        else if (newPos.x > spriteWidth)
        {
            newPos.x -= totalLayerWidth;
        }
 
        transform.localPosition = newPos;
    }
 
}