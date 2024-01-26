using System.Collections;
using UnityEngine;

public class MinimapTile : MonoBehaviour
{
    [SerializeField] float _fadeSpeed = 20f;

    float _targetSize = 10;
    float _currentSize = 0;

    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        while(_currentSize < _targetSize)
        {
            float fadeSize = Mathf.Min(10f, _currentSize + _fadeSpeed * Time.deltaTime);
            _currentSize = fadeSize;

            transform.localScale = new Vector3(_currentSize, _currentSize, 1);
            yield return null;
        }
    }
}
