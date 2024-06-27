using UnityEngine;


// kolekcja przydatnych, ogólnych metod
public static class GameUtils
{
    public static void ConvertToVector2(Vector3 vector, out Vector2 outcome)
    {
        outcome.x = vector.x;
        outcome.y = vector.z;
    }

    public static Vector3 RandomPlanePositionCircle(Vector3 basePosition, float range)
    {
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(0f, range);

        float angleRad = Mathf.Deg2Rad * randomAngle;

        float x = Mathf.Sin(angleRad) * randomDistance + basePosition.x;
        float z = Mathf.Cos(angleRad) * randomDistance + basePosition.z;
        float y = basePosition.y;

        return new Vector3(x, y, z);
    }

    public static Vector3 RandomPlanePositionCircle(Vector3 basePosition, float min, float max)
    {
        float randomAngle = Random.Range(0f, 360f); // Angle in degrees (0 to 360)
        float randomDistance = Random.Range(min, max); // Distance within the radius

        // Convert angle to radians for calculations
        float angleRad = Mathf.Deg2Rad * randomAngle;

        // Calculate random position coordinates based on angle, radius, and world point
        float x = Mathf.Sin(angleRad) * randomDistance + basePosition.x;
        float z = Mathf.Cos(angleRad) * randomDistance + basePosition.z;
        float y = basePosition.y; // Assuming a 2D circle, set z to world point's z

        return new Vector3(x, y, z);
    }

    public static Vector3 RandomPlanePosition(Vector3 basePosition, float range)
    {
        float randomX = UnityEngine.Random.Range(-range, range);
        float randomZ = UnityEngine.Random.Range(-range, range);

        Vector3 randomOffset = new Vector3(randomX, basePosition.y, randomZ);

        Vector3 randomWorldPosition = basePosition + randomOffset;

        return randomWorldPosition;
    }



    //public static float CalculateRelativeVelcoity(Vector3 obj1PrevPos, Vector3 obj1CurPos, Vector3 obj2PrevPos, Vector3 obj2CurPos)
    //{
    //    Vector3 posDelta1 = obj1CurPos - obj1PrevPos;
    //    Vector3 posDelta2 = obj2CurPos - obj2PrevPos;

    //    Vector3 relativeVelocity = posDelta1 - posDelta2;
    //    return relativeVelocity.magnitude / Time.deltaTime;
    //}

    public static Vector3 CalculateVelocity(Vector3 prevPos, Vector3 currPos)
    {
        Vector3 velocity = prevPos - currPos;
        return velocity / Time.deltaTime;
    }

    //public static void DrawCircle(GameObject obj, float radius, Transform point)
    //{
    //    LineRenderer lineRenderer;
    //    int segments = 50;

    //    if (!obj.TryGetComponent<LineRenderer>(out lineRenderer))
    //        lineRenderer = obj.AddComponent<LineRenderer>();

    //    if (lineRenderer)
    //    {
    //        lineRenderer.positionCount = 51;
    //        lineRenderer.startWidth = 0.1f;
    //        lineRenderer.endWidth = 0.1f;
    //        lineRenderer.useWorldSpace = true;

    //        float x;
    //        float z;
    //        float angle = 0f;

    //        // Loop through all segments and set their position based on world point and angle/radius
    //        for (int i = 0; i <= segments; i++)
    //        {
    //            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius + point.position.x; // Add world point x
    //            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius + point.position.z; // Add world point y
    //            lineRenderer.SetPosition(i, new Vector3(x, 0, z)); // Assuming a 2D circle
    //            angle += 360f / segments;
    //        }
    //    }
    //}

    public static void DrawCircle(LineRenderer lineRenderer, float radius, float width, Vector3 point)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        int segments = 50;
        lineRenderer.positionCount = 51;
        lineRenderer.useWorldSpace = true;

        float x;
        float z;
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius + point.x;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius + point.z;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
            angle += 360f / segments;
        }
    }

    public static float CalculateForwardAngle(Transform pos, Vector3 target)
    {
        return Vector3.Angle(target - pos.position, pos.forward);
    }

    public static LayerMask SubtractLayerMasks(LayerMask a, LayerMask b)
    {
        return a & ~b;
    }

    public static LayerMask AddLayerMasks(LayerMask a, LayerMask b)
    {
        return a | b;
    }

    public static bool CheckIfContainsLayer(LayerMask layerMask, int layer)
    {
        return (layerMask & (1 << layer)) != 0;
    }

    public static bool CheckIfContainsLayer(LayerMask baseMask, LayerMask checkMask)
    {
        return (baseMask & checkMask) == checkMask;
    }
}
