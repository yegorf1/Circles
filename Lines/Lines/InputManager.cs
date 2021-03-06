using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Lines.Utils;

namespace Lines {
    public class InputManager {
        public enum MouseButton {
            Left = 0,
            Right
        }

        public delegate void ClickContainer(MouseButton button,Vector2 position);

        public event ClickContainer OnClick;
        public event ClickContainer OnMouseDown;

        public bool IsMouseDown { get; private set; }
        public object Constans { get; private set; }

        private MouseState OldState;
        private TouchCollection OldTouchState;

        private bool IsMobile;
        public PlatformID s;

        public InputManager() {
            this.OldState = new MouseState();
            this.OldTouchState = new TouchCollection();
            this.IsMouseDown = false;
            
            this.IsMobile = LinesGame.IsMobile;
        }

        public void Update() {
			if (IsMobile) {
				UpdateTouches ();
			} else {
				UpdateMouse ();
			}
        }

		private void UpdateMouse() {
			MouseState newState = Mouse.GetState();

			if (IsDown(newState) && !IsDown(OldState)) {
				if (OnMouseDown != null && InBounds()) {
					OnMouseDown(GetButton(newState), GetMousePosition());
				}
				IsMouseDown = true;
			}

			if (!IsDown(newState) && IsMouseDown) {
				if (OnClick != null && InBounds()) {
					OnClick(GetButton(OldState), GetMousePosition());
				}
				IsMouseDown = false;
			}

			OldState = newState;
        }

		private void UpdateTouches() {
			TouchCollection newTouchState = TouchPanel.GetState();

			if (IsTouching(newTouchState) && !IsTouching(OldTouchState)) {
				if (OnMouseDown != null && InBounds()) {
					OnMouseDown(MouseButton.Left, GetTouchPosition(newTouchState));
				}
				IsMouseDown = true;
			}

			if (!IsTouching(newTouchState) && IsTouching(OldTouchState)) {
				if (OnClick != null && InBounds()) {
					OnClick(MouseButton.Left, GetTouchPosition(OldTouchState));
				}
				IsMouseDown = false;
			}

			OldTouchState = newTouchState;
        }

        public bool InBounds() {
            return new Rectangle(0,
                0,
                (int)LinesGame.instance.GetScreenWidth(),
                (int)LinesGame.instance.GetScreenHeight()
            ).Contains(GetMousePosition());
        }

        private Vector2 GetTouchPosition(TouchCollection touchCollection) {
            foreach (TouchLocation t in touchCollection) {
                return t.Position;
            }
            return Vector2.Zero;
        }

        private bool IsTouching (TouchCollection touchCollection) {
			return touchCollection.Count > 0;
		}

        private bool IsDown(MouseState state) {
            return state.LeftButton == ButtonState.Pressed || state.RightButton ==  ButtonState.Pressed;
        }

        private MouseButton GetButton(MouseState state) {
            if (state.LeftButton == ButtonState.Pressed) {
                return MouseButton.Left;
            } else {
                return MouseButton.Right;
            }
        }

        public Vector2 GetMousePosition() {
            if (IsMobile) {
                return GetTouchPosition(OldTouchState);
            } else {
                return new Vector2(OldState.Position.X, OldState.Position.Y);
            }
        }

    }
}