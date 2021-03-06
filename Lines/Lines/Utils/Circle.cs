using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Lines.States;

namespace Lines.Utils {
    public class Circle {
        private static Texture2D texture = CreateTexture(300);
        private static Texture2D bigTexture = CreateTexture(900);

        public Vector2 position { get; private set; }
        private Color color;
        private int player;
        private bool isBig;

        private float animationOffset;
        private float animationTime;
        private float radius;

        private List<Circle> connections;

        public Circle(float i, float j, int player, bool isBig) {
            this.position = new Vector2(i, j);
            this.player = player;
            this.color = Constants.COLORS[player];
            this.isBig = isBig;

            this.animationTime = 0f;
            this.radius = 0f;

            this.animationOffset = 0f;

            this.connections = new List<Circle>();
        }

        public bool IsConnected(Circle b) {
            return connections.Contains(b);
        }

        public bool Connect(Circle b) {
            if (!IsConnected(b)) {
                connections.Add(b);
                b.Connect(this);
                return true;
            }

            return false;
        }

        public void OpenAnimation(GameTime gameTime) {
            if (animationTime < Constants.OPEN_ANIMATION_TIME) {
                animationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                float scale = (float)Math.Sin(Math.PI / 4f * 3f * animationTime / Constants.OPEN_ANIMATION_TIME) * 1.5f;
                radius = Constants.CIRCLE_RADIUS * scale;
            } else {
                radius = Constants.CIRCLE_RADIUS;
            }
            radius *= isBig ? 3 : 1;
        }

        public void ResetAnimation() {
            this.animationTime = 0f;
        }

        public void CloseAnimation(GameTime gameTime) {
            if (animationTime <= Constants.CLOSE_ANIMATION_TIME - Constants.LINE_ANIMATION_TIME) {
                animationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                float c = player == Constants.FIRST_PLAYER ? position.Y : position.X;

                float t = (Constants.CLOSE_ANIMATION_LAST_CIRCLE_START / Constants.FIELD_SIZE) * c;
                if (animationTime < t) {
                    return;
                }

                float end = (Constants.CLOSE_ANIMATION_TIME - Constants.LINE_ANIMATION_TIME) / 2 - Constants.CLOSE_ANIMATION_LAST_CIRCLE_START;

                animationOffset = Constants.CloseAnimate(animationTime- t, 0, 1, end);
            }
        }

        public static Vector2 ToField(Vector2 v) {
            float fieldSize = Constants.ToScreenMin(1 - Constants.FIELD_OFFSET * 2);

            Vector2 offset = new Vector2 {
                X = (LinesGame.instance.GetScreenWidth() - fieldSize) / 2,
                Y = (LinesGame.instance.GetScreenHeight() - fieldSize) / 2
            };

            Vector2 step = new Vector2(fieldSize / (Constants.FIELD_WIDTH + 1.5f));

            return (v - GetCenterPosition(Vector2.Zero, 0)) / step;
        }

        public static Vector2 FromField(Vector2 v) {
            float fieldSize = Constants.ToScreenMin(1 - Constants.FIELD_OFFSET * 2);

            Vector2 offset = new Vector2 {
                X = (LinesGame.instance.GetScreenWidth() - fieldSize) / 2,
                Y = (LinesGame.instance.GetScreenHeight() - fieldSize) / 2
            };

            Vector2 step = new Vector2(fieldSize / (Constants.FIELD_WIDTH + 1.5f));

            return (v * step) + GetCenterPosition(Vector2.Zero, 0);
        }

        public void Draw(SpriteBatch batch) {
            Texture2D texture = isBig ? bigTexture : Circle.texture;

            Vector2 animationOffset = Vector2.Zero;
            float screenOffset = Constants.ToScreenMax(this.animationOffset);

            if (isBig) {
                if (player == Constants.FIRST_PLAYER) {
                    animationOffset.X = position.X > 0 ? screenOffset : -screenOffset;
                } else {
                    animationOffset.Y = position.Y > 0 ? screenOffset : -screenOffset;
                }
            } else { 
                if (player == Constants.FIRST_PLAYER) {
                    animationOffset.X = screenOffset;
                } else {
                    animationOffset.Y = screenOffset;
                }
            }

            Vector2 center = GetCenterPosition(this) + animationOffset;
            float r = Constants.ToScreenMin(radius);

            Vector2 scale = new Vector2(r / texture.Width);
            Vector2 origin = new Vector2(texture.Width / 2);

            batch.Draw(texture, center, null, null, origin, 0, scale, color);
        }

        public static Vector2 GetPosition(Vector2 screenPoint, int player) {
            float fieldSize = Constants.ToScreenMin(1 - Constants.FIELD_OFFSET * 2);

            Vector2 offset = new Vector2
            {
                X = (LinesGame.instance.GetScreenWidth() - fieldSize) / 2,
                Y = (LinesGame.instance.GetScreenHeight() - fieldSize) / 2
            };

            Vector2 step = new Vector2(fieldSize / (Constants.FIELD_WIDTH + 1.5f));
            if (player == Constants.FIRST_PLAYER) {
                offset.Y += step.Y / 2;
            } else {
                offset.X += step.X / 2;
            }

            screenPoint -= offset - step / 2;
            screenPoint /= step;

            return new Vector2((int)(screenPoint.X - 1.5f), (int)(screenPoint.Y - 1.5f));
        }

        public static Vector2 GetCenterPosition(Circle circle) {
            return GetCenterPosition(circle.position, circle.player);
        }

        public static Vector2 GetCenterPosition(Vector2 position, int player) {
            float fieldSize = Constants.ToScreenMin(1 - Constants.FIELD_OFFSET * 2);

            Vector2 offset = new Vector2
            {
                X = (LinesGame.instance.GetScreenWidth() - fieldSize) / 2,
                Y = (LinesGame.instance.GetScreenHeight() - fieldSize) / 2
            };

            Vector2 step = new Vector2(fieldSize / (Constants.FIELD_WIDTH + 1.5f));
            if (player == Constants.FIRST_PLAYER) {
                offset.Y += step.Y / 2;
            } else {
                offset.X += step.X / 2;
            }

            return offset + (new Vector2(1.5f, 1.5f) + position) * step;
        }

        private static Texture2D CreateTexture(int radius) {
            if (LinesGame.instance == null) { // Server
                return null;
            }

            Texture2D texture = new Texture2D(LinesGame.instance.GraphicsDevice, radius, radius);
            Color[] colorData = new Color[radius * radius];

            float diam = radius / 2f;
            float diamsq = diam * diam;

            // DON'T change this. It will broke antialiasing. Don't event try
            float someProcent = diamsq / 2;

            for (int x = 0; x < radius; x++) {
                for (int y = 0; y < radius; y++) {
                    int index = x * radius + y;
                    Vector2 pos = new Vector2(x - diam, y - diam);
                    if (pos.LengthSquared() <= diamsq - someProcent) {
                        colorData[index] = Color.White;
                    } else if (pos.LengthSquared() <= diamsq) {
                        colorData[index] = Color.White;

                        byte scale = (byte)(255 - AliasingFunction(pos.LengthSquared() - someProcent, 0, 255, someProcent));

                        colorData[index].A = scale;
                    } else {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }

        private static float AliasingFunction(float t, float b, float c, float d) {
            if ((t /= d / 2) < 1) {
                return c / 2 * t * t * t * t + b;
            }

            return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
        }

        private bool IsLast() {
            if (player == Constants.FIRST_PLAYER) {
                return position.X + 1.5f > Constants.FIELD_WIDTH;
            } else {
                return position.Y + 1.5f > Constants.FIELD_HEIGHT;
            }
        }

        public static bool CheckWon(Field currentField, int currentTurn) {
            LinkedList<Circle> q = new LinkedList<Circle>();
            bool[,] was = new bool[Constants.FIELD_WIDTH, Constants.FIELD_HEIGHT];

            for (int i = 0; i < Constants.FIELD_WIDTH; i++) {
                for (int j = 0; j < Constants.FIELD_HEIGHT; j++) {
                    was[i, j] = false;
                }

                if (i > 0) {
                    if (currentTurn == Constants.FIRST_PLAYER) {
                        q.AddLast(currentField[0, i - 1]);
                    } else {
                        q.AddLast(currentField[Constants.FIELD_WIDTH - 1 - i, 0]);
                    }
                }
            }

            while (q.Count > 0) {
                Circle c = q.First.Value;
                q.RemoveFirst();
                if (was[(int)c.position.X, (int)c.position.Y]) {
                    continue;
                } else {
                    was[(int)c.position.X, (int)c.position.Y] = true;
                }

                if (c.IsLast()) {
                    return true;
                }

                foreach (Circle n in c.connections) {
                    q.AddLast(n);
                }
            }

            return false;
        }

        public static bool CheckWon() {
            return CheckWon(GameState.instance.CurrentField, GameState.CurrentTurn);
        }
    }
}
