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
    }
}
