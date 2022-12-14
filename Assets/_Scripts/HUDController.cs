using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Moonvalk.Animation;
using Moonvalk.Utilities;
using Moonvalk;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    public List<Image> HUDElements;

    protected List<Vector3> _originalHUDScale;
    protected Tween _animationTween;
    protected Spring _animationSpring;
    protected float _tweenValue;
    protected float _springValue;

    protected bool _isHUDDisplayed = false;

    public PauseMenuController PauseMenu;

    // Start is called before the first frame update
    private void Start()
    {
        this._originalHUDScale = new List<Vector3>();
        foreach (Image img in HUDElements)
        {
            this._originalHUDScale.Add(img.transform.localScale);
        }

        this._animationTween = new Tween(() => ref this._tweenValue);
        this._animationTween.Duration(0.5f).Ease(Easing.Cubic.InOut);
        this._animationSpring = new Spring(() => ref this._springValue);
        this._animationSpring.Tension(25f).Dampening(3.5f);

        this._tweenValue = 0f;
        this._springValue = 0f;

        this.updateHUDElements();
        this.PauseMenu.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        this.updateHUDElements();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !this.PauseMenu.isActiveAndEnabled)
        {
            PlayerController.Player1.EnableControl(false);
            this.ShowPauseMenu();
        }
    }

    public void ShowDisplay()
    {
        this._animationTween.To(1f).Start();
        this._animationSpring.To(1f);
    }

    public void ShowPauseMenu()
    {
        this.HideDisplay();
        this.PauseMenu.gameObject.SetActive(true);
        this.PauseMenu.DisplayMenu(true);
    }

    public void HideDisplay()
    {
        this._animationTween.To(0f).Start();
        this._animationSpring.To(0f);
        this._isHUDDisplayed = false;
    }

    protected void updateHUDElements()
    {
        for (int index = 0; index < this.HUDElements.Count; index++)
        {
            Image img = this.HUDElements[index];
            Vector3 scale = this._originalHUDScale[index];
            img.color = new Color(1f, 1f, 1f, this._tweenValue);
            img.transform.localScale = new Vector3(scale.x * this._springValue, scale.y * this._springValue, scale.z * this._springValue);
        }
    }
}
