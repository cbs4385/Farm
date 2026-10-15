using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // A titled group that opens and closes when its header is chosen (the look of the Help tree). The body is built once and only shown or hidden, so what it holds
    // (sliders, a rebind in progress) keeps its state; which groups are open lives in the caller's set.
    public static class CollapsibleSection
    {
        public const string HeaderPrefix = "Section_";

        public static RectTransform Create(Transform parent, string id, string title, HashSet<string> open, out Button header)
        {
            var body = UiKit.VStack(parent, "Body_" + id, 10f, 4);
            var bodyRect = (RectTransform)body.transform;
            var isOpen = open.Contains(id);
            header = UiKit.MakeButton(parent, Text(title, isOpen), null, 100f, 38f);
            header.name = HeaderPrefix + id;
            UiKit.Size(header.gameObject, -1f, 38f);
            header.transform.SetSiblingIndex(bodyRect.GetSiblingIndex());          // the header goes above its body
            var button = header;
            header.onClick.AddListener(() =>
            {
                var now = !bodyRect.gameObject.activeSelf;
                bodyRect.gameObject.SetActive(now);
                if (now) open.Add(id); else open.Remove(id);
                UiKit.SetButtonText(button, Text(title, now));
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)parent);
            });
            bodyRect.gameObject.SetActive(isOpen);
            return bodyRect;
        }

        static string Text(string title, bool open) => (open ? "v  " : ">  ") + title;
    }
}
