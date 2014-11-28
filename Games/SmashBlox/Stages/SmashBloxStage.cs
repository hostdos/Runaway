using System.Collections;
using System.Collections.Generic;
using System.Linq;

using GameAPI;
using GameAPI.BudgetBoy;

using SmashBlox.Entities;

namespace SmashBlox.Stages
{
    class SmashBloxStage : Stage
    {
        public new Main Game { get { return (Main) base.Game; } }

        public const float FadeDuration = 0.25f;

        private readonly SwatchIndex[] _swatches;
        private int _swatchIndex;

        private Tilemap _fadeTiles;

        private readonly List<Particle> _particles;
        
        public SwatchIndex CurrentSwatchIndex { get { return _swatches[_swatchIndex]; } }

        public SmashBloxStage(Main game)
            : base(game)
        {
            _swatchIndex = 0;

            _swatches = new int[] {
                0x0000FC, 0x0078F8, 0x3CBCFC, 0x0078F8, 0x0000FC,
                0x940084, 0xD800CC, 0xF878F8, 0xD800CC, 0x940084,
                0xA81000, 0xF83800, 0xF87858, 0xF83800, 0xA81000,
                0x503000, 0xAC7C00, 0xF8B800, 0xAC7C00, 0x503000,
                0x007800, 0x00B800, 0xB8F818, 0x00B800, 0x007800
            }.Select(x => Graphics.Palette.FindSwatch(x, x, x)).ToArray();

            _particles = new List<Particle>();
        }

        protected virtual void OnSwatchChanged(SwatchIndex swatchIndex) { }

        protected void NextSwatch()
        {
            _swatchIndex = (_swatchIndex + 1) % _swatches.Length;

            OnSwatchChanged(CurrentSwatchIndex);
        }

        private int _curFadeVal;
        private void SetFadeTiles(float val)
        {
            if (_fadeTiles == null) {
                var tileSize = new Vector2i(40, 40);

                _fadeTiles = Add(new Tilemap(tileSize, Graphics.Size / tileSize), int.MaxValue);
            }

            int iVal = Mathf.RoundToInt(Mathf.Clamp(val, 0f, 1f) * 6f);

            if (iVal == _curFadeVal) return;
            _curFadeVal = iVal;

            var image = Graphics.GetImage("Resources", "transition", iVal.ToString());
            var swatch = Graphics.Palette.FindSwatch(0x000000, 0x000000, 0x000000);

            for (int r = 0; r < _fadeTiles.Rows; ++r) {
                for (int c = 0; c < _fadeTiles.Columns; ++c) {
                    _fadeTiles.SetTile(c, r, image, swatch);
                }
            }
        }

        public void AddParticle(Vector2f origin, Vector2f velocity, double lifetime)
        {
            _particles.Add(new Particle { Position = origin, Velocity = velocity, Lifetime = lifetime });
        }

        protected IEnumerator FadeInCoroutine(float duration)
        {
            SetFadeTiles(1f);

            int frames = Mathf.RoundToInt(duration * Game.UpdateRate);
            for (int i = frames; i >= 0; --i) {
                yield return null;
                SetFadeTiles(i / (float) frames);
            }
        }

        protected IEnumerator FadeOutCoroutine(float duration)
        {
            SetFadeTiles(0f);

            int frames = Mathf.RoundToInt(duration * Game.UpdateRate);
            for (int i = 0; i <= frames; ++i) {
                yield return null;
                SetFadeTiles(i / (float) frames);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            for (int i = _particles.Count - 1; i >= 0; --i) {
                var particle = _particles[i];

                particle.Update(Timestep);

                if (particle.ShouldRemove) {
                    _particles.RemoveAt(i);
                }
            }
        }

        protected override void OnRender()
        {
            base.OnRender();

            Graphics.DrawPoints(SwatchIndex.White, 1, _particles.Select(x => (Vector2i) x.Position).ToArray());
        }
    }
}
