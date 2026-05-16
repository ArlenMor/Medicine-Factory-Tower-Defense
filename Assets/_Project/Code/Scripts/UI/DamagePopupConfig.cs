using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    [CreateAssetMenu(fileName = "DamagePopupConfig", menuName = "Configs/DamagePopupConfig")]
    public class DamagePopupConfig : ScriptableObject
    {
        public float FontSize = 4f;
        public float Lifetime = 0.8f;
        public float FloatSpeed = 1.5f;
        public Color Color = Color.red;
        public Vector3 Offset = new Vector3(0f, 0.5f, 0f);
        public int SortingOrder = 200;
        public TMP_FontAsset Font; //установить шрифт
        public Material FontMaterial; //установить материал для шрифта (например, с эффектом обводки)


        private static DamagePopupConfig _default;
        public static DamagePopupConfig Default => _default ??= CreateInstance<DamagePopupConfig>();
    }
}
