using System.Collections;
using System.Collections.Generic;
using GameAPI;
using GameAPI.BudgetBoy;
namespace Games.Runaway
{
    public class GameStage : Stage
    {
        // called when this stage is created
        public GameStage(Main game) : base(game)
        {
            Debug.Log("GameStage created");
            Player player = Add(new Player(), 0);
            player.Position = new Vector2f(40f, 40f);

        }
        // called when this stage is entered
        protected override void OnEnter()
        {
            Debug.Log("GameStage entered");
            Graphics.SetClearColor(Game.Swatches.ClearColor);
            StartCoroutine(SpawnBullets);
        }
        // called each tick
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
        // called when this stage is rendered
        protected override void OnRender()
        {
            base.OnRender();
        }

        IEnumerator SpawnBullets()
        {
            while(true)
            {
                // wait
                yield return Delay(0.5f);
                // spawn bullet
                float PADDING = 10.0f;
                Vector2f bulletPos = new Vector2f(Mathf.Random(PADDING, Graphics.Width - PADDING), Graphics.Height + PADDING);
                Bullet bullet = Add(new Bullet(), 1);
                bullet.Position = bulletPos;
            }
        }

    }
}
