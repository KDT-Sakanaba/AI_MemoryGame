using UnityEngine;
using MemoryGame.GameLogic;

/// <summary>3D カード（Cube メッシュ）。Flip は 180 度回転で表現。裏/表/確定を色で区別。</summary>
public sealed class CardView : MonoBehaviour
{
    private static readonly Color BackColor = new Color(0.20f, 0.30f, 0.55f);   // 裏
    private static readonly Color FaceColor = new Color(0.90f, 0.85f, 0.55f);   // 表
    private static readonly Color MatchedColor = new Color(0.45f, 0.75f, 0.45f); // 確定

    public CardModel Model { get; set; }

    private Renderer rend;
    private TextMesh label;
    private float targetAngle;

    public void Init(CardModel model)
    {
        Model = model;
        rend = GetComponent<Renderer>();
        if (label == null)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.51f, 0f); // 天面の上
            go.transform.localRotation = Quaternion.Euler(-90f, 180f, 0f); // 上を向き、Flip(180°y回転)後も読める向き
            label = go.AddComponent<TextMesh>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.characterSize = 0.25f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.black;
        }
        targetAngle = 0f;
        transform.localEulerAngles = Vector3.zero;
        ApplyColor();
    }

    public void OnModelStateChanged(CardModel model) => ApplyColor();

    private void ApplyColor()
    {
        if (rend == null) return;
        rend.material.color = Model.State switch
        {
            CardState.FaceDown => BackColor,
            CardState.FaceUp => FaceColor,
            CardState.Matched => MatchedColor,
            _ => BackColor
        };
        targetAngle = Model.State == CardState.FaceDown ? 0f : 180f;

        // 数字は表（FaceUp/Matched）のときだけ見える
        if (label != null)
        {
            label.text = Model.Number.ToString();
            label.gameObject.SetActive(Model.State != CardState.FaceDown);
        }
    }

    private void Update()
    {
        var e = transform.localEulerAngles;
        e.y = Mathf.MoveTowardsAngle(e.y, targetAngle, 360f * Time.deltaTime);
        transform.localEulerAngles = e;
    }
}
