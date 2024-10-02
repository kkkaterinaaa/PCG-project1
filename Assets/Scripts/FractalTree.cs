using UnityEngine;

public class FractalTree : MonoBehaviour
{
    public int maxDepth = 6;             // recursion depth
    public float branchLength = 5f;      // Length of the initial branch
    public float angleVariance = 50f;    // Random variation in branch angles
    public float lengthMultiplier = 0.6f;// Factor by which each branch's length decreases
    public float startWidth = 0.1f;      // Starting width of the trunk
    public float endWidth = 0.01f;       // Ending width of the thinnest branches
    Color startColor = new Color(0.1f, 0.1f, 0.1f);
    Color endColor = new Color(0.6f, 0.1f, 0.1f);
    
    void Start()
    {
        Vector2 startPosition = transform.position;
        GenerateFractalTree(startPosition, branchLength, 90f, maxDepth);
    }

    // Recursively generates the fractal tree
    void GenerateFractalTree(Vector2 startPosition, float length, float angle, int currentDepth)
    {
        if (currentDepth == 0) return;

        // Calculate end position of branch using angle and length
        Vector2 endPosition = startPosition + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * length;

        // Create and draw the current branch
        CreateLine(startPosition, endPosition, length, currentDepth);

        // Randomly decide how many branches to generate from this branch
        int numBranches = Random.Range(3, 5);
        for (int i = 0; i < numBranches; i++)
        {
            // Randomize the angle and length for the next branches
            float newAngle = angle + Random.Range(-angleVariance, angleVariance);
            float newLength = (length * lengthMultiplier) * Random.Range(0.7f, 1.2f);
            GenerateFractalTree(endPosition, newLength, newAngle, currentDepth - 1);
        }
    }
    
    void CreateLine(Vector2 start, Vector2 end, float length, int currentDepth)
    {
        GameObject line = new GameObject("Line");
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Calculate the width of the branch based on its recursion depth
        float width = Mathf.Max(Mathf.Lerp(endWidth, startWidth, (float)currentDepth / maxDepth), 0.02f);
        lr.startWidth = width;
        lr.endWidth = width * 0.5f;

        // Calculate the color of the branch based on its recursion depth
        Color currentColor = Color.Lerp(endColor, startColor, (float)currentDepth / maxDepth);
        lr.startColor = currentColor;
        lr.endColor = currentColor;
        
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.useWorldSpace = true;
    }
}
