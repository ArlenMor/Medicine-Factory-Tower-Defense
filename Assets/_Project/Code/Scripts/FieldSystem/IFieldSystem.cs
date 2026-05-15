using _Project.Code.Scripts.BattleField;
using UnityEngine;

public interface IFieldSystem
{
    public int Width { get; }
    public int Height { get; }
    public float CellSize { get; }

    void ShowHighlight();
    void HideHighlight();
    void Reset();
    bool CanPlaceMulti(int originX, int originY, int width, int height);
    bool TryPlaceMulti(int originX, int originY, int width, int height, IFieldPlaceable placeable, Vector2 pivotOffset);
    
    Vector3 GetMultiCellWorldPosition(int originX, int originY, int width, int height, Vector2 pivotOffset);
    Vector3 GridToWorld(int x, int y);
    bool WorldToGrid(Vector3 worldPos, out int x, out int y);
}