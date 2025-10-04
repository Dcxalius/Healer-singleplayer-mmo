# Folder Summary: Project 1/Camera
## Namespaces Detected
- Project_1.Camera: Feature completeness unknown
## Files
- AbsoluteScreenPosition.cs — Namespace: Project_1.Camera — Feature completeness: Unknown
- Camera.cs — Namespace: Project_1.Camera — Feature completeness: Unknown
- CameraMover.cs — Namespace: Project_1.Camera — Feature completeness: Unknown
- CameraSettings.cs — Namespace: Project_1.Camera — Feature completeness: Unknown
- RelativeScreenPosition.cs — Namespace: Project_1.Camera — Feature completeness: Unknown
- WorldSpace.cs — Namespace: Project_1.Camera — Feature completeness: Unknown

## Class Feature Usage
### AbsoluteScreenPosition.cs (Project_1.Camera.AbsoluteScreenPosition)
- Fields: `position`.
- Properties: `UpCardinal`, `DownCardinal`, `LeftCardinal`, `RightCardinal`, `Zero`, `X`, `Y`, `OnlyX`, `OnlyY`.
- Methods and operators: constructor overloads for `(int)`, `(int, int)`, and `Point`; static `FromRelativeScreenPosition`; static and instance `ToRelativeScreenPos` overloads; static and instance `ToRelativeScreenPosition` overloads; static and instance `ToWorldSpace`; implicit/explicit conversions with `Point`; unary `+`; binary `+`, `-`, `*`, `/` overloads for `AbsoluteScreenPosition` and scalar combinations; equality operators; overrides of `GetHashCode`, `Equals(object)`, `Equals(AbsoluteScreenPosition)`, `ToPoint`, `ToVector2`, and `ToString`.
- External interactions: `Camera.ScreenRectangle.Width`, `Camera.ScreenRectangle.Height`, `Camera.WorldRectangle.Location`, `Camera.WorldRectangle`, `Camera.Scale`, `RelativeScreenPosition.FromAbsoluteScreenPosition(...)` (both overloads), `WorldSpace` constructors, `Point.Zero`, `Point.ToVector2`, `Point` arithmetic, and `position` struct methods `GetHashCode`, `ToVector2`, `ToString`.

### Camera.cs (Project_1.Camera.Camera)
- Fields: `scale`, `minScale`, `maxScale`, `cameraSettings`, `cameraMover`, `devScreenBorder`.
- Properties: `ScreenRectangle`, `CurrentCameraSetting`, `FullScreen`, `WorldRectangle`, `CentrePointInScreenSpace`, `WindowSize`, `WindowSizeAsPoint`, `Scale`, `Zoom`, `CentreInWorldSpace`.
- Methods: static constructor; `Update`; save/load helpers `ImportSettings`, `ExportSettings`, `SavePosition`, `LoadPosition`; zoom handlers `Scroll`, `ZoomIn`, `ZoomOut`; `BindCamera`; `SetWindowSize`; `WorldRectToScreenRect`; frame-boundary checks `ScreenspaceBoundsCheck`, `WorldspaceBoundsCheck(Rectangle)`, `WorldspaceBoundsCheck(Vector2)`; `MinimapDraw`.
- External interactions: `WindowSize.ToPoint()`, `WindowSize.ToVector2()`, `WindowSize.X`, `WindowSize.Y`, `cameraSettings.FollowSetting`, `cameraSettings.Fullscreen`, `cameraSettings.WindowSize`, `cameraSettings.SetCamera()`, `cameraMover.CentreInWorldSpace`, `cameraMover.Move()`, `cameraMover.bindingRectangle`, `cameraMover.maxCircleCameraMove`, `cameraMover.BindCamera(...)`, `WorldRectangle.Location`, `WorldRectangle.Size`, `WorldSpace` arithmetic (scaling, subtraction, addition), `File.Exists`, `File.ReadAllText`, `SaveManager.CameraSettings`, `SaveManager.DefaultCameraSettings`, `SaveManager.ImportData<T>()`, `SaveManager.ExportData(...)`, `Debug.Assert(...)`, `aSave.CameraPosition`, `ScrollEvent.Up`, `ScrollEvent.Down`, `ScrollEvent.Steps`, `DebugManager.Print(...)`, `Math.Max`, `Rectangle` construction plus `.Intersects`/`.Contains`, `Point.Zero`, `Vector2.ToPoint()`, `Vector2.ToVector2()`, `TileManager.TileSize`, `UI.UIElements.Minimap.minimapDot.Draw(...)`, `MovingObject` binding, `Save` data usage, and `Color.White`.

### CameraMover.cs (Project_1.Camera.CameraMover)
- Fields: `centreInWorldSpace`, `cameraMoveBorderSize`, `velocity`, `momentum`, `baseSpeed`, `drag`, `boundObject`, `bindingRectangle`, `maxCircleCameraMove`.
- Properties: `CentreInWorldSpace`.
- Methods: constructor; `ApplyMouseVelocity`; `Move`; `BindCamera`; `CheckForSpacePress`; `MoveRectangleSoftBound`; `CheckIfCameraTriesToLeavePlayer`; `CalculateIntersection`; `LengthToCollisionFromFirstVector`; `GetClosestRectangleCorner`; `GetRectangleRays`; `MoveCircleSoftBound`; `MoveHardBound`; `MoveFree`; `StayWithinCircleBind`; `ApplyMovementToCamera`.
- External interactions: `WindowSize.X`, `WindowSize.Y`, `CurrentCameraSetting`, `CameraSettings.Follow` enum values (`Free`, `CircleSoftBound`, `RectangleSoftBound`, `Hardbound`), `InputManager.GetMousePosRelative()`, `RelativeScreenPosition.X`, `RelativeScreenPosition.Y`, `InputManager.GetMousePosAbsolute()`, `CentrePointInScreenSpace`, `Vector2.Normalize(...)`, `Vector2.Length()`, `Vector2.Zero`, `TimeManager.SecondsSinceLastFrame`, `InputManager.GetPress(...)`, `Microsoft.Xna.Framework.Input.Keys.Space`, `MovingObject.FeetPosition`, `bindingRectangle.Location`, `bindingRectangle.Size.ToVector2()`, `bindingRectangle.Contains(...)`, `bindingRectangle.Bottom`, `bindingRectangle.Top`, `bindingRectangle.Right`, `bindingRectangle.Left`, `Math.Abs`, `DebugManager.Print(...)`, `Debug.Assert(...)`, `WorldSpace.Zero`, and `WorldSpace` arithmetic/conversions.

### CameraSettings.cs (Project_1.Camera.CameraSettings)
- Fields: `follow`, `windowSize`, `fullscreen`.
- Properties: `FollowSetting`, `WindowSize`, `Fullscreen`.
- Methods: enumerations `Follow` and `WindowType`; constructor with `[JsonConstructor]`; `SetCamera`.
- External interactions: `GraphicsManager.SetWindowSize(...)` invoked during property setters and `SetCamera`.

### RelativeScreenPosition.cs (Project_1.Camera.RelativeScreenPosition)
- Fields: `position`.
- Properties: `Zero`, `One`, `OnlyX`, `OnlyY`, `X`, `Y`.
- Methods and operators: constructor overloads (parameterless, `Vector2`, `float`, `float,float`); static helpers `GetSquareFromX`/`GetSquareFromY` (with and without parent size); `TransformToAbsoluteRect`; static `FromAbsoluteScreenPosition`; static and instance `ToAbsoluteScreenPosition`/`ToAbsoluteScreenPos` overloads; implicit/explicit conversions with `Vector2`; arithmetic operators `+`, `-`, `*`, `/` for relative/same-type and scalar; equality operators; overrides of `GetHashCode`, `Equals(object)`, `Equals(RelativeScreenPosition)`, `ToPoint`, `ToVector2`, `ToString`; `Assert`.
- External interactions: `Camera.ScreenRectangle.Size`, `Camera.ScreenRectangle.Width`, `Camera.ScreenRectangle.Height`, `Camera.WindowSize`, `Camera.WindowSize.X`, `Camera.WindowSize.Y`, `AbsoluteScreenPosition.FromRelativeScreenPosition(...)` (both overloads), `AbsoluteScreenPosition` constructors, `Point` and `Rectangle` creation, `Vector2` math, and tuple deconstruction via `position.Deconstruct` in `Assert`.

### WorldSpace.cs (Project_1.Camera.WorldSpace)
- Fields: `position`.
- Properties: `Zero`, `OnlyX`, `OnlyY`, `X`, `Y`.
- Methods and operators: constructor overloads (parameterless, `Point`, `Vector2`, `float`, `float,float`); `DistanceTo`; `ToAbsoltueScreenPosition`; static `FromRelativeScreenSpace`; static `FromAbsoluteScreenSpace`; implicit/explicit conversions with `Vector2`; unary `+`; binary `+`, `-`, `*`, `/` overloads with `WorldSpace` and scalar; equality operators; overrides of `GetHashCode`, `Equals(object)`, `Equals(WorldSpace)`, `ToPoint`, `ToVector2`, `ToString`; `Normalize`; static `Normalize(WorldSpace)`.
- External interactions: `Camera.CentreInWorldSpace`, `Camera.Scale`, `Camera.WindowSize.ToVector2()`, `Camera.CentrePointInScreenSpace`, `Camera.Zoom`, `AbsoluteScreenPosition` constructors, `Vector2.Normalize(...)`, `Math.Floor`, and `WorldSpace` arithmetic helpers on `Vector2`.
