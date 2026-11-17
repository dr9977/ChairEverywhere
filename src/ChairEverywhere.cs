
using System;
using GTA;
using GTA.Math;

public class ChairEverywhere : Script
{
    private Prop chair;

    private readonly Model chairModel =
        new Model("prop_chair_01a");

    private bool wasInVehicle = false;
    private bool initialized = false;

    private const float ChairDistance = 1.35f;

    public ChairEverywhere()
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
            DeleteChair();
            initialized = false;
            return;
        }

        bool inVehicle = player.IsInVehicle();

        // تهيئة حالة اللاعب عند بداية تشغيل المود
        if (!initialized)
        {
            wasInVehicle = inVehicle;
            initialized = true;
            return;
        }

        // عند ركوب أي سيارة
        if (inVehicle)
        {
            wasInVehicle = true;
            DeleteChair();
            return;
        }

        // عند النزول من السيارة
        if (wasInVehicle)
        {
            wasInVehicle = false;
            CreateChair(player);
        }
    }

    private void CreateChair(Ped player)
    {
        DeleteChair();

        if (!chairModel.IsLoaded)
        {
            chairModel.Request(1000);

            if (!chairModel.IsLoaded)
                return;
        }

        // تحديد مكان الكرسي بجانب اللاعب مرة واحدة
        Vector3 target =
            player.Position +
            player.RightVector * ChairDistance;

        // تحديد مستوى الأرض
        float groundHeight = World.GetGroundHeight(target);

        if (groundHeight > -1000f)
            target.Z = groundHeight;

        // إنشاء الكرسي دون ربطه باللاعب
        chair = World.CreateProp(
            chairModel,
            target,
            new Vector3(0f, 0f, player.Heading),
            false,
            true
        );

        if (chair != null && chair.Exists())
        {
            chair.IsPersistent = true;
        }
    }

    private void DeleteChair()
    {
        if (chair != null && chair.Exists())
        {
            chair.Delete();
        }

        chair = null;
    }

    private void OnAborted(object sender, EventArgs e)
    {
        DeleteChair();

        if (chairModel.IsLoaded)
        {
            chairModel.MarkAsNoLongerNeeded();
        }
    }
}
