using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }
    public TileCell cell { get; private set; }
    public bool locked { get; set; }

    private Image background;
    private TextMeshProUGUI text;

    private void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state)
    {
        this.state = state;

        background.color = state.backgroundColor;
        text.color = state.textColor;
        text.text = state.number.ToString();
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null) {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        transform.position = cell.transform.position;
        transform.localScale = Vector3.zero;

        StartCoroutine(SpawnAnimation());
    }

    private IEnumerator SpawnAnimation()
    {
        const float upTime = 0.15f;
        const float downTime = 0.1f;
        Vector3 original = Vector3.one;
        Vector3 overshoot = original * 1.2f;

        float t = 0f;
        while (t < upTime)
        {
            float f = t / upTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, overshoot, f);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < downTime)
        {
            float f = t / downTime;
            transform.localScale = Vector3.Lerp(overshoot, original, f);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = original;
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null) {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        StartCoroutine(Animate(cell.transform.position, false));
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null) {
            this.cell.tile = null;
        }

        this.cell = null;
        cell.tile.locked = true;

        StartCoroutine(Animate(cell.transform.position, true));
    }

    public void PlayMergeAnimation()
    {
        StartCoroutine(MergePulse());
    }

    private IEnumerator MergePulse()
    {
        float duration = 0.15f;
        Vector3 original = transform.localScale;
        Vector3 peak = original * 1.2f;

        // Scale up
        float t = 0f;
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(original, peak, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        // Scale down
        t = 0f;
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(peak, original, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = original;
    }

    private IEnumerator Animate(Vector3 to, bool merging)
    {
        float elapsed = 0f;
        float duration = 0.1f;

        Vector3 from = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;

        if (merging) {
            Destroy(gameObject);
        }
    }

}
