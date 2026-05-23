using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TMPShimmerEffect : MonoBehaviour
{
    [SerializeField] private Color _baseColor = Color.white;
    [SerializeField] private Color _shimmerColor = Color.cyan;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _width = 0.3f;
    [SerializeField] private float _pause = 2f;

    private TMP_Text _text;
    private TMP_TextInfo _textInfo;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        _text.ForceMeshUpdate();
        _textInfo = _text.textInfo;

        int charCount = _textInfo.characterCount;
        if (charCount == 0) return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        for (int i = 0; i < charCount; i++)
        {
            if (!_textInfo.characterInfo[i].isVisible) continue;
            var ch = _textInfo.characterInfo[i];
            if (ch.bottomLeft.x < minX) minX = ch.bottomLeft.x;
            if (ch.topRight.x > maxX) maxX = ch.topRight.x;
        }

        float textWidth = maxX - minX;
        if (textWidth <= 0) return;

        float shimmerRadius = textWidth * _width;
        float shimmerTravel = textWidth + shimmerRadius * 2f;
        float shimmerDuration = shimmerTravel / _speed;
        float totalDuration = shimmerDuration + Mathf.Max(0f, _pause);
        float elapsed = Mathf.Repeat(Time.time, totalDuration);

        bool isAnimating = elapsed < shimmerDuration;

        if (isAnimating)
        {
            float t = elapsed / shimmerDuration;
            float shimmerCenter = Mathf.Lerp(minX - shimmerRadius, maxX + shimmerRadius, t);

            for (int i = 0; i < charCount; i++)
            {
                if (!_textInfo.characterInfo[i].isVisible) continue;

                int matIndex = _textInfo.characterInfo[i].materialReferenceIndex;
                int vertIndex = _textInfo.characterInfo[i].vertexIndex;
                Color32[] colors = _textInfo.meshInfo[matIndex].colors32;

                float charCenter = (_textInfo.characterInfo[i].bottomLeft.x + _textInfo.characterInfo[i].topRight.x) * 0.5f;
                float dist = Mathf.Abs(charCenter - shimmerCenter);
                float lerp = Mathf.Clamp01(1f - dist / shimmerRadius);
                lerp = Mathf.SmoothStep(0f, 1f, lerp);

                Color32 finalColor = Color.Lerp(_baseColor, _shimmerColor, lerp);

                colors[vertIndex + 0] = finalColor;
                colors[vertIndex + 1] = finalColor;
                colors[vertIndex + 2] = finalColor;
                colors[vertIndex + 3] = finalColor;
            }
        }
        else
        {
            Color32 baseColor32 = _baseColor;

            for (int i = 0; i < charCount; i++)
            {
                if (!_textInfo.characterInfo[i].isVisible) continue;

                int matIndex = _textInfo.characterInfo[i].materialReferenceIndex;
                int vertIndex = _textInfo.characterInfo[i].vertexIndex;
                Color32[] colors = _textInfo.meshInfo[matIndex].colors32;

                colors[vertIndex + 0] = baseColor32;
                colors[vertIndex + 1] = baseColor32;
                colors[vertIndex + 2] = baseColor32;
                colors[vertIndex + 3] = baseColor32;
            }
        }

        for (int i = 0; i < _textInfo.meshInfo.Length; i++)
        {
            _textInfo.meshInfo[i].mesh.colors32 = _textInfo.meshInfo[i].colors32;
            _text.UpdateGeometry(_textInfo.meshInfo[i].mesh, i);
        }
    }
}
