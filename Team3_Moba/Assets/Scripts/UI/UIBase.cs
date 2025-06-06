using System;
using UnityEngine;

public class UIBaseData 
{
    public Action OnOpen;
    public Action OnClose;
}

public class UIBase : MonoBehaviour
{
    protected Action OnOpen;
    protected Action OnClose;

    public virtual void Initialize(Transform anchor)
    {
        this.OnOpen = null;
        this.OnClose = null;

        transform.SetParent(anchor);

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.offsetMin = Vector3.zero;
        rectTransform.offsetMax = Vector3.zero;
    }

    public virtual void SetInfo(UIBaseData uidata)
    {
        OnOpen = uidata.OnOpen;
        OnClose = uidata.OnClose;
    }

    public virtual void OpenUI()
    {
        OnOpen?.Invoke();
        OnOpen = null;
    }

    public virtual void CloseUI(bool isCloseAll = false)
    {
        if(isCloseAll == false)
        {
            OnClose?.Invoke();
        }
        OnClose = null;
        UIManager.Instance.CloseUI(this);
    }

    public virtual void OnClickCloseButton()
    {
        //TODO : 사운드매니저 버튼
        CloseUI();
    }
}
