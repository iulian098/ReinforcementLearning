using System;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoSingleton<PanelManager>
{
    struct PanelData {
        public string name;
        public Action OnShow;
        public Action OnHide;

        public override bool Equals(object obj) {
            return obj is PanelData data &&
                   name == data.name;
        }

        public override int GetHashCode() {
            return HashCode.Combine(name);
        }
    }

    Stack<PanelData> activePanels = new Stack<PanelData>();

    bool IsPanelOpen => activePanels.Count > 0;

    public Action OnExitPopupShow;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (IsPanelOpen)
                HidePanel();
            else
                OnExitPopupShow?.Invoke();
        }
    }

    public void ShowPanel(string name, Action onShow, Action onHide) {
        PanelData panel = new PanelData() {
            name = name,
            OnShow = onShow,
            OnHide = onHide
        };

        if(activePanels.Contains(panel)) {
            Debug.Log("[PanelManager] Panel already exists " + panel.name);
            return;
        }

        activePanels.Push(panel);
        panel.OnShow?.Invoke();

    }

    public void HidePanel() {
        if (!IsPanelOpen) return;

        PanelData panel = activePanels.Pop();

        panel.OnHide?.Invoke();
    }

    public void HidePanel(string name) {
        Stack<PanelData> panels = new Stack<PanelData>();

        while (activePanels.Count > 0) {
            PanelData pd = activePanels.Pop();
            if (pd.name == name) {
                pd.OnHide?.Invoke();
                break;
            }
            else
                panels.Push(pd);
        }

        while(panels.Count > 0) {
            activePanels.Push(panels.Pop());
        }

    }
}
