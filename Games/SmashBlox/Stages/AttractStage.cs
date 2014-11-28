using System.Collections;
using System.IO;

using GameAPI;
using GameAPI.BudgetBoy;

namespace SmashBlox.Stages
{
    class AttractStage : SmashBloxStage
    {
        private Demo _demo;
        private int _frame;

        private Sprite _title;
        private Sprite _insertCoin;

        public AttractStage(Main game, Demo demo)
            : base(game)
        {
            _demo = demo;
            _demo.IsLooping = true;

            var text = Graphics.GetImage("Resources", "title");

            _title = Add(new Sprite(text, CurrentSwatchIndex), 0);
            _title.Position = (Graphics.Size - _title.Size) / 2 - new Vector2i(0, 8);

            text = Graphics.GetImage("Resources", "insertcoin");
            var swatch = Graphics.Palette.FindSwatch(0xffffff, 0xffffff, 0xffffff);

            _insertCoin = Add(new Sprite(text, swatch), 0);
            _insertCoin.Position = (Graphics.Size - _insertCoin.Size) / 2 - new Vector2i(0, Graphics.Height / 4);

            StartCoroutine(MainCoroutine);
            StartCoroutine(InsertCoinFlash);
            StartCoroutine(EliminateEpileptics);
        }

        public AttractStage(Main game, Demo demo, BinaryReader reader)
            : this(game, demo)
        {
            LoadState(reader);
        }

        protected override void OnSaveState(BinaryWriter writer)
        {
            writer.Write(_frame);
        }

        protected override void OnLoadState(BinaryReader reader)
        {
            _frame = reader.ReadInt32();
        }

        private IEnumerator MainCoroutine()
        {
            yield return FadeInCoroutine(FadeDuration);

            yield return Until(() => Controls.A.JustPressed || Controls.B.JustPressed ||
                Controls.Start.JustPressed || Controls.Select.JustPressed || !Controls.Analog.IsZero);

            var showScores = Controls.B.IsDown;

            yield return FadeOutCoroutine(FadeDuration);

            if (showScores) {
                Game.ShowHighscores();
            } else {
                Game.Start();
            }
        }

        private IEnumerator InsertCoinFlash()
        {
            while (true) {
                _insertCoin.IsVisible = true;
                yield return Delay(0.5);
                _insertCoin.IsVisible = false;
                yield return Delay(0.5);
            }
        }

        private IEnumerator EliminateEpileptics()
        {
            while (true) {
                yield return Delay(1 / 16d);
                NextSwatch();
            }
        }

        protected override void OnSwatchChanged(SwatchIndex swatchIndex)
        {
            _title.SwatchIndex = swatchIndex;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            ++_frame;
        }

        protected override void OnRender()
        {
            _demo.Render(Graphics, Audio, _frame, 0.25f);

            base.OnRender();
        }
    }
}
