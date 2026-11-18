
using System;
using GTA;
using GTA.Math;

public class StoneAboveHead : Script
{
    private Prop stone;

    private readonly Model stoneModel =
        new Model("prop_rock_4_cl_2");

    private const int HeadBone = 31086;

    // المسافة فوق الرأس: 50 سم
    private const float Height = 0.50f;

    public StoneAboveHead()
    {
        Interval = 100;
        Tick += OnTick;
        Aborted += OnAborted;
    }

    private void OnTick(object sender, EventArgs e)
    {
        Ped player = Game.Player.Character;

        if (player == null ||
            !player.Exists() ||
            player.IsDead)
        {
            DeleteStone();
            return;
        }

        if (stone != null && stone.Exists())
            return;

        if (!stoneModel.IsLoaded)
        {
            stoneModel.Request(1000);
            return;
        }

        stone = World.CreateProp(
            stoneModel,
            player.Position,
            Vector3.Zero,
            false,
            true
        );

        if (stone == null || !stone.Exists())
            return;

        stone.IsPersistent = true;

        stone.AttachTo(
            player,
            HeadBone,
            new Vector3(0f, 0f, Height),
            Vector3.Zero
        );
    }

    private void DeleteStone()
    {
        if (stone != null && stone.Exists())
        {
            stone.Detach();
            stone.Delete();
        }

        stone = null;
    }

    private void OnAborted(object sender, EventArgs e)
    {
        DeleteStone();

        if (stoneModel.IsLoaded)
            stoneModel.MarkAsNoLongerNeeded();
    }
}
