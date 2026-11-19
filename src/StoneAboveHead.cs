using System;
using GTA;
using GTA.Math;
using GTA.Native;

public class StoneAboveHead : Script
{
    private Prop stone;

    private readonly Model stoneModel =
        new Model("prop_rock_4_cl_2");

    // ارتفاع الحجر: 3 أمتار فوق الرأس
    private const float Height = 3.0f;

    public StoneAboveHead()
    {
        Interval = 0;
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

        if (!stoneModel.IsLoaded)
        {
            stoneModel.Request(1000);
            return;
        }

        if (stone == null || !stone.Exists())
        {
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
        }

        // تعطيل تصادم الحجر باستخدام Native
        Function.Call(
            Hash.SET_ENTITY_COLLISION,
            stone,
            false,
            false
        );

        // موقع الرأس
        Vector3 headPosition =
            player.Bones[Bone.SkelHead].Position;

        // وضع الحجر على ارتفاع 3 أمتار فوق الرأس
        stone.Position =
            headPosition + new Vector3(0f, 0f, Height);

        // اتجاه الحجر مع الشخصية
        stone.Rotation = new Vector3(
            0f,
            0f,
            player.Heading
        );
    }

    private void DeleteStone()
    {
        if (stone != null && stone.Exists())
        {
            stone.Delete();
        }

        stone = null;
    }

    private void OnAborted(object sender, EventArgs e)
    {
        DeleteStone();

        if (stoneModel.IsLoaded)
        {
            stoneModel.MarkAsNoLongerNeeded();
        }
    }
}
