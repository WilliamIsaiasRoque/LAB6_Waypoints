using UnityEngine;

/// <summary>Builds a small, editable waypoint race scene from Unity primitives.</summary>
[ExecuteAlways]
public class CarCircuitBuilder : MonoBehaviour
{
    private static readonly Vector3[] TrackPoints =
    {
        new Vector3(-14f, 0.12f, 14f), new Vector3(-4f, 0.12f, 14f),
        new Vector3(0f, 0.12f, 18f), new Vector3(4f, 0.12f, 14f),
        new Vector3(14f, 0.12f, 14f), new Vector3(18f, 0.12f, 10f),
        new Vector3(18f, 0.12f, -10f), new Vector3(14f, 0.12f, -14f),
        new Vector3(-14f, 0.12f, -14f), new Vector3(-18f, 0.12f, -10f),
        new Vector3(-18f, 0.12f, 10f), new Vector3(-14f, 0.12f, 14f)
    };

    private void OnEnable()
    {
        if (!Application.isPlaying && transform.childCount == 0)
            BuildCircuit();
    }

    [ContextMenu("Build Race Circuit")]
    private void BuildCircuit()
    {
        if (transform.childCount > 0) return;

        Material grass = MakeMaterial(new Color(0.25f, 0.48f, 0.29f));
        Material asphalt = MakeMaterial(new Color(0.20f, 0.22f, 0.24f));
        Material waypointMat = MakeMaterial(new Color(1f, 0.75f, 0.12f));

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Grass";
        ground.transform.SetParent(transform, false);
        ground.transform.position = new Vector3(0f, -0.02f, 0f);
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
        ground.GetComponent<Renderer>().sharedMaterial = grass;

        GameObject[] waypoints = new GameObject[TrackPoints.Length];
        for (int i = 0; i < TrackPoints.Length; i++)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Waypoint " + (i + 1);
            marker.transform.SetParent(transform, false);
            marker.transform.position = TrackPoints[i] + Vector3.up * 0.1f;
            marker.transform.localScale = Vector3.one * 0.45f;
            marker.GetComponent<Renderer>().sharedMaterial = waypointMat;
            waypoints[i] = marker;

            Vector3 next = TrackPoints[(i + 1) % TrackPoints.Length];
            Vector3 direction = next - TrackPoints[i];
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road " + (i + 1);
            road.transform.SetParent(transform, false);
            road.transform.position = (TrackPoints[i] + next) * 0.5f;
            road.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            road.transform.localScale = new Vector3(8f, 0.16f, direction.magnitude + 4f);
            road.GetComponent<Renderer>().sharedMaterial = asphalt;
        }

        Color[] colors = { new Color(0.12f, 0.42f, 0.92f), new Color(0.88f, 0.16f, 0.14f),
                           new Color(0.98f, 0.72f, 0.08f), new Color(0.15f, 0.62f, 0.28f) };
        string[] names = { "Blue Car", "Red Car", "Yellow Car", "Green Car" };
        float[] speeds = { 8f, 10f, 9f, 7.5f };
        int[] starts = { 0, 3, 6, 9 };
        Material dark = MakeMaterial(new Color(0.08f, 0.09f, 0.1f));
        Material glass = MakeMaterial(new Color(0.20f, 0.30f, 0.35f));

        for (int carIndex = 0; carIndex < 4; carIndex++)
        {
            int start = starts[carIndex];
            GameObject car = new GameObject(names[carIndex]);
            car.transform.SetParent(transform, false);
            car.transform.position = TrackPoints[start];
            car.transform.rotation = Quaternion.LookRotation(
                TrackPoints[(start + 1) % TrackPoints.Length] - TrackPoints[start], Vector3.up);

            AddCarPart(car.transform, "Body", PrimitiveType.Cube,
                new Vector3(0f, 0.48f, 0f), new Vector3(1.5f, 0.6f, 2.3f),
                MakeMaterial(colors[carIndex]));
            AddCarPart(car.transform, "Cabin", PrimitiveType.Cube,
                new Vector3(0f, 0.92f, -0.15f), new Vector3(1.08f, 0.42f, 1.12f), glass);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    GameObject wheel = AddCarPart(car.transform, "Wheel", PrimitiveType.Cylinder,
                        new Vector3(side * 0.72f, 0.23f, axle * 0.68f),
                        new Vector3(0.40f, 0.20f, 0.40f), dark);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
            }

            FollowWP follower = car.AddComponent<FollowWP>();
            follower.speed = speeds[carIndex];
            follower.rotSpeed = 4f;
            follower.lookAhead = 10f;
            follower.waypoints = new GameObject[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
                follower.waypoints[i] = waypoints[(start + i) % waypoints.Length];
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.transform.position = new Vector3(0f, 45f, -34f);
            camera.transform.rotation = Quaternion.Euler(52f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 27f;
        }
    }

    private static GameObject AddCarPart(Transform parent, string partName, PrimitiveType type,
        Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        return part;
    }

    private static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
