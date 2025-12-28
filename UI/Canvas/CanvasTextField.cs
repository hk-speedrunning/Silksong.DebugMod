using HarmonyLib;
using InControl;
using System;
using System.Collections;
using TMProOld;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMod.UI.Canvas;

[HarmonyPatch]
public class CanvasTextField : CanvasText
{
    public static bool AnyFieldFocused { get; private set; }

    private TMP_InputField inputField;

    public event Action<string> OnSubmit;

    protected override bool Interactable => true;

    public CanvasTextField(string name) : base(name) { }

    public override void Build()
    {
        base.Build();

        inputField = gameObject.AddComponent<TMP_InputField>();
        inputField.textComponent = t;
        inputField.transition = Selectable.Transition.None;
        inputField.enabled = false;

        inputField.onSubmit.AddListener(text =>
        {
            Text = text;
            OnSubmit?.Invoke(text);
        });

        inputField.onEndEdit.AddListener(_ =>
        {
            inputField.enabled = false;
            t.text = Text;
            AnyFieldFocused = false;
            InputManager.enabled = true;
        });
    }

    public void Activate()
    {
        if (inputField)
        {
            inputField.text = t.text;
            inputField.enabled = true;
            inputField.Select();
            AnyFieldFocused = true;
            InputManager.enabled = false;

            Color selectionColor = inputField.selectionColor;
            inputField.selectionColor = Color.clear;

            // Move caret to end instead of selecting the entire text
            inputField.StartCoroutine(DeselectRoutine());
            IEnumerator DeselectRoutine()
            {
                yield return null;
                inputField.MoveTextEnd(false);
                inputField.selectionColor = selectionColor;
            }
        }
    }

    public void UpdateDefaultText(string text)
    {
        if (inputField && inputField.isFocused)
        {
            return;
        }

        Text = text;
    }

    [HarmonyPatch(typeof(HollowKnightInputModule), nameof(HollowKnightInputModule.ProcessMove))]
    [HarmonyPrefix]
    private static void Prefix(HollowKnightInputModule __instance, ref bool __state)
    {
        __state = __instance.focusOnMouseHover;
        if (AnyFieldFocused) __instance.focusOnMouseHover = false;
    }

    [HarmonyPatch(typeof(HollowKnightInputModule), nameof(HollowKnightInputModule.ProcessMove))]
    [HarmonyPostfix]
    private static void Postfix(HollowKnightInputModule __instance, ref bool __state)
    {
        __instance.focusOnMouseHover = __state;
    }
}