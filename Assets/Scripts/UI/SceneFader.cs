using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    public float FadeDuration = 1f;
    public FadeType CurrentFadeType;

    private int _fadeAmount = Shader.PropertyToID("_FadeAmount");

    private int _useShutters = Shader.PropertyToID("_UseShutters");
    private int _useRadialWipe = Shader.PropertyToID("_UseRadialWipe");
    private int _usePlainBlack = Shader.PropertyToID("_UsePlainBlack");
    private int _useGoop = Shader.PropertyToID("_UseGoop");

    private int? _lastEffect;

    private Image _image;
    private Material _material;

    public enum FadeType
    {
        Shutters,
        RadialWipe,
        PlainBlack,
        Goop
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _image = GetComponent<Image>();

        Material mat = _image.material;
        _image.material = new Material(mat);
        _material = _image.material;

        _lastEffect = _useShutters;

        if (SceneLoader.IsLoadingNewScene)
        {
            _material.SetFloat(_fadeAmount, 1f); // fully black
        }
        else
        {
            _material.SetFloat(_fadeAmount, 0f); // start transparent   
        }
    }

    public void StartFadeInAfterLoad(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        
        _material.SetFloat(_fadeAmount, 1f); 
        
        StartFadeIn(); 
    }

    public IEnumerator StartTransition(FadeType fadeType, Action onMidpointAction)
    {
        ChangeFadeEffect(fadeType);
        
        yield return StartCoroutine(HandleFade(1f, 0f)); 

        onMidpointAction?.Invoke();

        yield return null; 

        yield return StartCoroutine(HandleFade(0f, 1f)); 
    }

    public void FadeOut(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        StartFadeOut();
    }

    public void FadeIn(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        StartFadeIn();
    }

    public void ChangeFadeEffect(FadeType fadeType)
    {
        if (_lastEffect.HasValue)
        {
            _material.SetFloat(_lastEffect.Value, 0f);
        }

        switch (fadeType)
        {
            case FadeType.Shutters:

                SwitchEffect(_useShutters);
                break;

            case FadeType.RadialWipe:

                SwitchEffect(_useRadialWipe);
                break;

            case FadeType.PlainBlack:

                SwitchEffect(_usePlainBlack);
                break;

            case FadeType.Goop:

                SwitchEffect(_useGoop);
                break;
        }
    }

    private void SwitchEffect(int effectToTurnOn)
    {
        _material.SetFloat(effectToTurnOn, 1f);
        _lastEffect = effectToTurnOn;
    }

    private void StartFadeOut()
    {
        _material.SetFloat(_fadeAmount, 0f);

        StartCoroutine(HandleFade(1f, 0f));

        // _material.DOFade(1f, _fadeAmount, FadeDuration)
        //     .SetEase(AssemblyIsEditorAssembly.InOutSine);
    }

    private void StartFadeIn()
    {
        StartCoroutine(HandleFade(0f, 1f));
    }
    
    public IEnumerator HandleFade(float targetAmount, float startAmount)
    {
        float elapsedTime = 0f;
        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(startAmount, targetAmount, (elapsedTime / FadeDuration));
            _material.SetFloat(_fadeAmount, lerpedAmount);

            yield return null;
        }

        _material.SetFloat(_fadeAmount, targetAmount);
    }
}
