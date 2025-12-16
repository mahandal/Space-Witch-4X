using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Utility : MonoBehaviour
{
    // - Tiles

    // Safely check if the coordinates fit in the game grid,
    // and return the tile there if they do.
    // Otherwise, return null.
    public static Tile GetTile(int x, int y)
    {
        // Respect boundaries.
        if (x < 0) return null;
        if (y < 0) return null;
        if (x >= GM.I.gridWidth) return null;
        if (y >= GM.I.gridHeight) return null;

        // Return tile located in the game grid at coordinates (x, y).
        return GM.I.grid[x, y];
    }

    // - Debugging

    // Debug print the contents of a hash set.
    public static void DebugSet<T>(HashSet<T> set)
    {
        Debug.Log("PRINTING SET");

        foreach (T t in set)
        {
            Debug.Log(t);
        }
    }

    // Debug print the contents of a Dictionary
    public static void DebugDic<TKey, TValue>(Dictionary<TKey, TValue> dict, string label = "Dictionary")
    {
        Debug.Log($"{label} contains {dict.Count} items:");
        foreach (KeyValuePair<TKey, TValue> pair in dict)
        {
            Debug.Log($"  - {pair.Key}: {pair.Value}");
        }
    }

    // - Conversions!

    // Convert grid coordinates to world coordinates.
    public static Vector3 GridToWorld(int gridX, int gridY)
    {
        return new Vector3(gridX * Constance.tileSize, gridY * Constance.tileSize, 0);
    }

    // Convert world coordinates to grid coordinates and return the closest tile.
    public static Tile WorldToGrid(Vector3 worldPos)
    {
        // Get x.
        int x = Mathf.RoundToInt(worldPos.x / Constance.tileSize);

        // Get y.
        int y = Mathf.RoundToInt(worldPos.y / Constance.tileSize);

        // - Boundary checking.

        // Min x.
        if (x < 0)
            return null;

        // Max x.
        if (x >= GM.I.gridWidth)
            return null;

        // Min y
        if (y < 0)
            return null;

        // Max y.
        if (y >= GM.I.gridHeight)
            return null;

        // Return tile!
        return GM.I.grid[x, y];
    }

    // Return the distance between two tiles.
    // Note: Distance is rounded to a whole number in this game!
    public static int Distance(Tile start, Tile end)
    {
        // Return arbitrarily massive value for null distances.
        if (start == null || end == null)
            return int.MaxValue;

        // Calculate horizontal difference.
        int horizontalDifference = Mathf.Abs(start.x - end.x);

        // Calculate vertical difference.
        int verticalDifference = Mathf.Abs(start.y - end.y);

        // Add together and return!
        return horizontalDifference + verticalDifference;
    }

    

    // - Image loading

    // Load an image located at the given location into the given image.
    public static void LoadImage(Image image, string fileName)
    {
        // Load file into sprite.
        Sprite sprite = Resources.Load<Sprite>(fileName);

        // Load sprite into image
        image.sprite = sprite;
    }

    // Loads a sprite into a spriteRenderer
    public static void LoadImage(SpriteRenderer spriteRenderer, string fileName)
    {
        Sprite sprite = Resources.Load<Sprite>(fileName);
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }

    // Load a given faction's icon into the given image.
    public static void LoadFactionIcon(Image image, Faction faction)
    {
        // Get file path.
        string filePath = "Faction Icon - " + faction.ToString();

        // Delegate to LoadImage
        LoadImage(image, filePath);
    }

    // Fade an image to a target alpha value over a set duration.
    // Wrapper of a coroutine so you can call it like a normal function.
    public static void FadeImage(Image image, float targetAlpha, float duration = 0.5f)
    {
        GM.I.StartCoroutine(FadeImageCoroutine(image, targetAlpha, duration));
    }

    // Fade an image to a target alpha value.
    // Note: Deactivates objects when they fade to 0 opacity.
    public static IEnumerator FadeImageCoroutine(Image image, float targetAlpha, float duration = 0.5f)
    {
        // Make sure object is active!
        image.gameObject.SetActive(true);

        float elapsed = 0f;
        Color startColor = image.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }
        
        image.color = targetColor;

        // Deactivate object if it is fully faded out.
        if (targetAlpha <= 0f)
            image.gameObject.SetActive(false);
    }

    // - Camera

    // Move the main camera to center on the given tile.
    public static void MoveCamera(Tile tileToFocusOn)
    {
        // Get new position.
        Vector3 newCameraPos = tileToFocusOn.transform.position;

        // Keep camera's Z position.
        newCameraPos.z = Camera.main.transform.position.z; 

        // Set new position.
        Camera.main.transform.position = newCameraPos;
    }

    // Move the main camera to focus on the given ship.
    public static void MoveCamera(Ship shipToFocusOn)
    {
        // Get new position.
        Vector3 newCameraPos = shipToFocusOn.transform.position;

        // Keep camera's z position.
        newCameraPos.z = Camera.main.transform.position.z; 

        // Set new position.
        Camera.main.transform.position = newCameraPos;
    }


    // - Menus

    // Exit the game.
    public static void ExitGame()
    {
        Application.Quit();
    }

    // Reload the game scene.
    public static void ReloadGame()
    {
        SceneManager.LoadScene("Game");
    }
}
