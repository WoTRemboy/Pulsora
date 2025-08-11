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

    // Prevent concurrent animations that can leave the tile at a wrong scale
    private Coroutine moveRoutine;
    private Coroutine scaleRoutine;

    private static readonly Vector3 kOne = Vector3.one;

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
        if (this.cell != null) this.cell.tile = null;

        this.cell = cell;
        this.cell.tile = this;

        // Stop any running animations before starting a new one
        StopMoveRoutine();
        StopScaleRoutine();

        transform.position = cell.transform.position;
        transform.localScale = Vector3.zero;
        scaleRoutine = StartCoroutine(SpawnAnimation());
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null) this.cell.tile = null;

        this.cell = cell;
        this.cell.tile = this;

        StopMoveRoutine();
        moveRoutine = StartCoroutine(AnimateMove(cell.transform.position, false));
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null) this.cell.tile = null;

        this.cell = null;
        cell.tile.locked = true;

        StopMoveRoutine();
        moveRoutine = StartCoroutine(AnimateMove(cell.transform.position, true));
    }

    public void PlayMergeAnimation()
    {
        // Ensure only one scale animation runs at a time
        StopScaleRoutine();
        scaleRoutine = StartCoroutine(MergePulse());
    }

    private void StopMoveRoutine()
    {
        if (moveRoutine != null) { StopCoroutine(moveRoutine); moveRoutine = null; }
    }

    private void StopScaleRoutine()
    {
        if (scaleRoutine != null) { StopCoroutine(scaleRoutine); scaleRoutine = null; }
    }

    private IEnumerator SpawnAnimation()
    {
        const float upTime = 0.15f;
        const float downTime = 0.10f;

        Vector3 overshoot = kOne * 1.2f;

        float t = 0f;
        while (t < upTime)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, overshoot, t / upTime);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < downTime)
        {
            transform.localScale = Vector3.Lerp(overshoot, kOne, t / downTime);
            t += Time.deltaTime;
            yield return null;
        }

        // Snap to the canonical size to avoid drift
        transform.localScale = kOne;
        scaleRoutine = null;
    }

    private IEnumerator MergePulse()
    {
        float duration = 0.15f;

        // Use the current value only for the upward leg, but always end at 1
        Vector3 start = transform.localScale;
        Vector3 peak = kOne * 1.2f;

        // Scale up
        float t = 0f;
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(start, peak, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        // Scale down
        t = 0f;
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(peak, kOne, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = kOne; // normalize size
        scaleRoutine = null;
    }

    private IEnumerator AnimateMove(Vector3 to, bool merging)
    {
        float elapsed = 0f;
        const float duration = 0.10f;

        Vector3 from = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;

        if (merging)
        {
            // Ensure no scale animation is touching this object before destroying
            StopScaleRoutine();
            Destroy(gameObject);
        }

        moveRoutine = null;
    }
}
