using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public GUIStyle progress_empty;
    public GUIStyle progress_full;

    //current progress
    public float barDisplay;
    public Vector2 pos = new Vector2(10, 40);
    public Vector2 size = new Vector2(250, 60);

    public Texture2D emptyTex;
    public Texture2D fullTex;

    private void Awake()
    {
        //emptyTex = new Texture2D(1, 1);
        //emptyTex.SetPixel(0, 0, Color.red);
        //emptyTex.wrapMode = TextureWrapMode.Repeat;
        //emptyTex.Apply();
        //progress_empty = new GUIStyle();
        //progress_empty.normal.background = emptyTex;

        //fullTex = new Texture2D(1, 1);
        //fullTex.SetPixel(0, 0, Color.green);
        //fullTex.wrapMode = TextureWrapMode.Repeat;
        //fullTex.Apply();
        //progress_full = new GUIStyle();
        //progress_full.normal.background = fullTex;
    }

    private void Start()
    {
        Slider slider = CalculateSlider();
        StartCoroutine(ActiveSlider(slider));
    }

    IEnumerator ActiveSlider(Slider slider)
    {
        //Tells Unity to wait 1 second
        yield return new WaitForSeconds(0.1f);
        slider.gameObject.SetActive(true);
        yield return null;
    }

    private Vector3 GetSize()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.one);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.size;
    }

    void OnGUI()
    {
        //draw the background:
        //GUI.BeginGroup(new Rect(pos.x, pos.y, size.x, size.y));

        //GUI.Box(new Rect(0, 0, size.x, size.y), GUIContent.none, progress_empty);

        ////draw the filled-in part:
        ////GUI.BeginGroup(new Rect(0, 0, size.x + pos.x, size.y));
        ////GUI.Box(new Rect(0, 0, size.x + pos.x, size.y), GUIContent.none, progress_full);

        //GUI.BeginGroup(new Rect(0, 0, size.x * barDisplay, size.y));
        //GUI.Box(new Rect(0, 0, size.x, size.y), GUIContent.none, progress_full);

        //GUI.EndGroup();
        //GUI.EndGroup();
        /*
        //draw the background:
        GUI.BeginGroup(new Rect(pos.x, pos.y, size.x, size.y), emptyTex, progress_empty);

        GUI.Box(new Rect(pos.x, pos.y, size.x, size.y), fullTex, progress_full);

        //draw the filled-in part:
        GUI.BeginGroup(new Rect(0, 0, size.x * barDisplay, size.y));
        GUI.Box(new Rect(0, 0, size.x, size.y), fullTex, progress_full);

        GUI.EndGroup();
        GUI.EndGroup();
        */
    }

    private Slider CalculateSlider()
    {
        Transform slider = gameObject.transform.Find("Canvas/Slider");
        Slider uiSlider = slider.gameObject.GetComponent<Slider>();

        slider.position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        uiSlider.value = uiSlider.maxValue * gameObject.GetComponent<Monster>().Health / gameObject.GetComponent<Monster>().DefaultHealth;
        return uiSlider;
    }


    // Update is called once per frame
    void Update()
    {
        //Vector3 objectSize = Vector3.Scale(transform.localScale, GetSize());
        //barDisplay = gameObject.GetComponent<Monster>().Health / gameObject.GetComponent<Monster>().DefaultHealth;
        ////size =  new Vector2(objectSize.x / barDisplay, objectSize.y/ barDisplay);
        //size =  new Vector2(30f, 10f);
        //pos = Camera.main.WorldToScreenPoint(new Vector2(gameObject.transform.position.x - 0.5f, gameObject.transform.position.z));
    }

    private void FixedUpdate()
    {
        CalculateSlider();
    }
}
