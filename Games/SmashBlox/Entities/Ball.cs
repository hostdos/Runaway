using System;
using System.IO;
using System.Linq;

using GameAPI;
using GameAPI.BudgetBoy;

namespace SmashBlox.Entities
{
    class Ball : Entity
    {
        private Sprite _sprite;

        private Sound _paddleHit;
        private Sound _blockHit;

        public Vector2f Velocity { get; set; }

        public SwatchIndex SwatchIndex
        {
            get { return _sprite.SwatchIndex; }
            set { _sprite.SwatchIndex = value; }
        }
   
        public event EventHandler<EventArgs> PaddleHit;

        public event EventHandler<EventArgs> Bounced;

        protected override void OnLoadAudio(Audio audio)
        {
            base.OnLoadAudio(audio);

            _paddleHit = audio.GetSound("Resources", "bounce2");
            _blockHit = audio.GetSound("Resources", "bounce");
        }

        protected override void OnLoadGraphics(Graphics graphics)
        {
            base.OnLoadGraphics(graphics);
        
            var image = graphics.GetImage("Resources", "ball");
            var swatch = graphics.Palette.FindSwatch(0xffffff, 0xffffff, 0xffffff);

            _sprite = Add(new Sprite(image, swatch), new Vector2i(-2, -2));

            CalculateBounds();
        }
        
        public void Bounce(Vector2f normal)
        {
            Bounce(normal, 1f);
        }

        public void Bounce(Vector2f normal, float scale)
        {
            float dot = Velocity.Dot(normal);

            if (dot >= 0f) return;

            if (Bounced != null) {
                Bounced(this, new EventArgs());
            }

            Velocity -= (1 + scale) * normal * dot;
        }

        public void SaveState(BinaryWriter writer)
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);

            writer.Write(Velocity.X);
            writer.Write(Velocity.Y);
        }

        public void LoadState(BinaryReader reader)
        {
            Position = new Vector2f(reader.ReadSingle(), reader.ReadSingle());
            Velocity = new Vector2f(reader.ReadSingle(), reader.ReadSingle());
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);

            Position += Velocity * (float) dt;

            if (X < 8f) {
                X = 8f; Bounce(Vector2f.UnitX);
                Stage.Audio.Play(_paddleHit, PanValue, 1f, 1f);
            } else if (X > Stage.Graphics.Width - 8f) {
                X = Stage.Graphics.Width - 8f; Bounce(-Vector2f.UnitX);
                Stage.Audio.Play(_paddleHit, PanValue, 1f, 1f);
            }

            if (Y > Stage.Graphics.Height - 20f) {
                Y = Stage.Graphics.Height - 20f; Bounce(-Vector2f.UnitY);
                Stage.Audio.Play(_paddleHit, PanValue, 1f, 1f);
            }

            var paddle = Stage.GetEntities<Paddle>().FirstOrDefault(x => x.Bounds.Intersects(Bounds));
            if (paddle != null) {
                Bounce(Vector2f.UnitY);

                if (PaddleHit != null) {
                    PaddleHit(this, new EventArgs());
                }

                Stage.Audio.Play(_paddleHit, PanValue, 1f, 1f);
            }

            Vector2f normal; int phase;
            if (Stage.GetEntity<BlockGrid>().CheckForCollision(this, out normal, out phase)) {
                if (normal.X > 0) {
                    Bounce(Vector2f.UnitX);
                } else if (normal.X < 0) {
                    Bounce(-Vector2f.UnitX);
                }

                if (normal.Y > 0) {
                    Bounce(Vector2f.UnitY);
                } else if (normal.Y < 0) {
                    Bounce(-Vector2f.UnitY);
                }

                Stage.Audio.Play(_blockHit, PanValue, 1f, 1f - Mathf.Log(phase, 10) / 2f);
            }
        }
    }
}
