using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarActionUnit : MonoBehaviour
{
    ActionUnit _host;
    // Start is called before the first frame update
    void Start()
    {
        _host = gameObject.GetComponent<ActionUnit>();
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
    private Slider CalculateSlider()
    {
        Transform slider = gameObject.transform.Find("Canvas/Slider");
        Slider uiSlider = slider.gameObject.GetComponent<Slider>();

        slider.position = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        // ActionUnitData origin = (ActionUnitData)_host.tileUnitData;
        ActionUnitData origin = (ActionUnitData)_host.OriginStatus;
        ActionUnitData current = _host.CurrentStatus;
        uiSlider.value = (current.baseHealth >= 0 ? uiSlider.maxValue * current.baseHealth / origin.baseHealth : uiSlider.minValue);
        
        return uiSlider;
    }

    private void FixedUpdate()
    {
        CalculateSlider();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
