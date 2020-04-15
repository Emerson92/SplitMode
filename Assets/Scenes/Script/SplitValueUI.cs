using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplitValueUI : MonoBehaviour
{

    public Slider SplitSlider;

    public Text SplitValueText;

    // Start is called before the first frame update
    void Start()
    {
        SplitSlider.onValueChanged.AddListener(OnSliderValueChange);
    }

    private void OnSliderValueChange(float value)
    {
        SplitValueText.text = value.ToString("f2");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
