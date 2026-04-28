using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using Project_1.UI.UIElements.SelectBoxes;
using System;
using System.Collections.Generic;

namespace Project_1.UI.OptionMenu
{
    internal class ScreenSizeSelect : Box
    {
        readonly InputBox widthInput;
        readonly InputBox heightInput;
        readonly Button applyButton;
        readonly FullscreenResolutionSelect fullscreenSelect;
        readonly Label borderlessLabel;

        Point originalSize;
        bool revertQueued;
        CameraSettings.WindowType lastMode;

        public ScreenSizeSelect(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : base(aParent, UITexture.Null, aPos, aSize)
        {
            RelativeScreenPosition fieldSize = new RelativeScreenPosition(0.36f, 1f);
            RelativeScreenPosition heightPos = new RelativeScreenPosition(0.38f, 0f);
            RelativeScreenPosition applyPos = new RelativeScreenPosition(0.76f, 0f);
            RelativeScreenPosition applySize = new RelativeScreenPosition(0.24f, 1f);

            widthInput = new InputBox(
                this,
                "Width",
                Size,
                new[] { InputBox.ValidInputs.Digits },
                Color.Black,
                "",
                Color.White,
                false,
                Color.Black,
                Color.Black,
                RelativeScreenPosition.Zero,
                fieldSize);

            heightInput = new InputBox(
                this,
                "Height",
                Size,
                new[] { InputBox.ValidInputs.Digits },
                Color.Black,
                "",
                Color.White,
                false,
                Color.Black,
                Color.Black,
                heightPos,
                fieldSize);

            applyButton = new Button(
                this,
                new List<Action> { ApplyResolution },
                applyPos,
                applySize,
                Color.Beige,
                "Apply",
                Color.Black);

            widthInput.SetEnter(new List<Action> { ApplyResolution });
            heightInput.SetEnter(new List<Action> { ApplyResolution });

            fullscreenSelect = new FullscreenResolutionSelect(
                this,
                RelativeScreenPosition.Zero,
                RelativeScreenPosition.One,
                ApplyResolution);

            borderlessLabel = new Label(
                this,
                RelativeScreenPosition.Zero,
                RelativeScreenPosition.One,
                Label.TextAllignment.CentreLeft,
                Color.Black,
                aText: "Uses monitor resolution");

            lastMode = Camera.Camera.FullScreen;
            SyncInputsToCurrentSize();
            UpdateModeLayout(force: true);
        }

        void ApplyResolution()
        {
            if (!int.TryParse(widthInput.Input, out int width) || !int.TryParse(heightInput.Input, out int height))
            {
                DebugManager.Print("Resolution input requires two whole numbers.");
                return;
            }

            if (width <= 0 || height <= 0)
            {
                DebugManager.Print("Resolution input must be greater than zero.");
                return;
            }

            if (!revertQueued)
            {
                originalSize = Camera.Camera.WindowSizeAsPoint;
                OptionManager.AddActionToDoAtExitOfOptionMenu(
                    () => Camera.Camera.WindowSizeAsPoint = originalSize,
                    Camera.Camera.ExportSettings);
                revertQueued = true;
            }
            else
            {
                OptionManager.ChangesMade = true;
            }

            Camera.Camera.SetWindowSize(width, height);
            SyncInputsToCurrentSize();
            fullscreenSelect.SyncToCurrentSize();
        }

        void ApplyResolution(Point size)
        {
            if (!revertQueued)
            {
                originalSize = Camera.Camera.WindowSizeAsPoint;
                OptionManager.AddActionToDoAtExitOfOptionMenu(
                    () => Camera.Camera.WindowSizeAsPoint = originalSize,
                    Camera.Camera.ExportSettings);
                revertQueued = true;
            }
            else
            {
                OptionManager.ChangesMade = true;
            }

            Camera.Camera.SetWindowSize(size.X, size.Y);
            SyncInputsToCurrentSize();
            fullscreenSelect.SyncToCurrentSize();
        }

        void SyncInputsToCurrentSize()
        {
            Point currentSize = Camera.Camera.WindowSizeAsPoint;
            widthInput.Input = currentSize.X.ToString();
            heightInput.Input = currentSize.Y.ToString();
        }

        public override void Update()
        {
            UpdateModeLayout(force: false);
            base.Update();
        }

        void UpdateModeLayout(bool force)
        {
            CameraSettings.WindowType mode = Camera.Camera.FullScreen;
            if (!force && mode == lastMode) return;

            lastMode = mode;
            SyncInputsToCurrentSize();
            fullscreenSelect.SyncToCurrentSize();

            bool showWindowedInputs = mode == CameraSettings.WindowType.Windowed;
            bool showFullscreenList = mode == CameraSettings.WindowType.Fullscreen;
            bool showBorderlessInfo = mode == CameraSettings.WindowType.Borderless;

            widthInput.Visible = showWindowedInputs;
            heightInput.Visible = showWindowedInputs;
            applyButton.Visible = showWindowedInputs;
            fullscreenSelect.Visible = showFullscreenList;
            borderlessLabel.Visible = showBorderlessInfo;
        }

        sealed class FullscreenResolutionSelect : SelectBox
        {
            readonly Action<Point> applyResolution;
            Point[] supportedSizes = Array.Empty<Point>();

            public FullscreenResolutionSelect(UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Action<Point> aApplyResolution)
                : base(aParent, new UITexture("WhiteBackground", Color.White), -1, aPos, aSize)
            {
                applyResolution = aApplyResolution;
                ReloadValues();
                displayValue = new SelectBoxValueDisplay(this, RelativeScreenPosition.Zero, RelativeScreenPosition.One, string.Empty);
                SyncToCurrentSize();
            }

            public void SyncToCurrentSize()
            {
                ReloadValues();
                Point currentSize = Camera.Camera.WindowSizeAsPoint;
                selectedValue = FindSelectedIndex(currentSize);
                displayValue.SetToNewText($"{currentSize.X}, {currentSize.Y}");
            }

            protected override void ActionWhenSelected(int aSelectedValue)
            {
                base.ActionWhenSelected(aSelectedValue);
                applyResolution(supportedSizes[aSelectedValue]);
            }

            void ReloadValues()
            {
                supportedSizes = GraphicsManager.GetSupportedFullscreenSizes();
                string[] displayValues = new string[supportedSizes.Length];
                for (int i = 0; i < supportedSizes.Length; i++)
                {
                    displayValues[i] = $"{supportedSizes[i].X}, {supportedSizes[i].Y}";
                }

                values = SelectBoxValueOption.CreateArray(this, allValues, displayValues);
                allValues.RemoveAllScrollableElements();
                allValues.AddScrollableElements(values);
            }

            static int FindSelectedIndex(Point currentSize)
            {
                Point[] sizes = GraphicsManager.GetSupportedFullscreenSizes();
                for (int i = 0; i < sizes.Length; i++)
                {
                    if (sizes[i] == currentSize)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }
}
