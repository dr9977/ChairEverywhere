
using System;
using GTA;
using GTA.Math;
using GTA.Native;

public class StoneAboveHead : Script
{
    private Prop stone;
    private readonly Model stoneModel =
        new Model("prop_rock_4_cl_2");

    private const int HeadBone = 31086;
    private const float Height = 0.50f;

    public StoneAboveHead()
    {
        Interval = 0;

        Tick += OnTick;
        Aborted += OnAborted;
    }

    private void OnTick(object sender, EventArgs e)
    {
        Ped player = Game.Player.Character;

        if (player == null || !player.Exists() || player.IsDead)
            return;

        if (stone == null || !stone.Exists())
        {
            if (!stoneModel.IsInCdImage || !stoneModel.IsValid)
                return;

            if (!stoneModel.IsLoaded)
                stoneModel.Request(1000);

            if (!stoneModel.IsLoaded)
                return;

            Vector3 initialPosition = player.Position;

            stone = World.CreateProp(
                stoneModel,
                initialPosition,
                false,
                false
            );

            if (stone == null || !stone.Exists())
                return;

            Function.Call(
                Hash.SET_ENTITY_AS_MISSION_ENTITY,
                stone.Handle,
                true,
                true
            );
        }

        // Disable all physical interaction
        Function.Call(
            Hash.SET_ENTITY_COLLISION,
            stone.Handle,
            false,
            false
        );

        Function.Call(
            Hash.SET_ENTITY_HAS_GRAVITY,
            stone.Handle,
            false
        );

        Function.Call(
            Hash.SET_ENTITY_DYNAMIC,
            stone.Handle,
            false
        );

        // Get the exact head bone position
        Vector3 headPosition = Function.Call<Vector3>(
            Hash.GET_PED_BONE_COORDS,
            player.Handle,
            HeadBone,
            0f,
            0f,
            0f
        );

        // Place the stone 50 cm above the head
        Vector3 targetPosition = new Vector3(
            headPosition.X,
            headPosition.Y,
            headPosition.Z + Height
        );

        // Move the stone without physical forces
        Function.Call(
            Hash.SET_ENTITY_COORDS_NO_OFFSET,
            stone.Handle,
            targetPosition.X,
            targetPosition.Y,
            targetPosition.Z,
            false,
            false,
            false
        );

        stone.Heading = player.Heading;
    }

    private void OnAborted(object sender, EventArgs e)
    {
        if (stone != null && stone.Exists())
        {
            stone.Delete();
            stone = null;
        }
    }
}
