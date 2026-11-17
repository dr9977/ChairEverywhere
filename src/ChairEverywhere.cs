
using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;

public class ChairEverywhere : Script
{
    private Prop chair;

    private readonly Model chairModel =
        new Model("prop_chair_01a");

    private bool enabled = true;
    private bool f7WasDown = false;

    private bool wasInVehicle = false;
    private bool vehicleStateInitialized = false;
    private bool chairRequestedAfterExit = false;

    private const float ChairDistance = 1.35f;

    public ChairEverywhere()
    {
        Interval = 100;
        Tick += OnTick;
        Aborted += OnAborted;
    }

    private void OnTick(object sender, EventArgs e)
    {
        bool f7Down = Game.IsKeyPressed(Keys.F7);

        if (f7Down && !f7WasDown)
        {
            enabled = !enabled;

            if (!enabled)
                DeleteChair();
        }

        f7WasDown = f7Down;

        Ped player = Game.Player.Character;

        if (player == null ||
            !player.Exists() ||
            player.IsDead)
        {
            DeleteChair();
            chairRequestedAfterExit = false;
            return;
        }

        bool inVehicle = player.IsInVehicle();

        if (!vehicleStateInitialized)
        {
            wasInVehicle = inVehicle;
            vehicleStateInitialized = true;
        }
        else if (inVehicle)
        {
            wasInVehicle = true;
            chairRequestedAfterExit = false;
            DeleteChair();
            return;
        }
        else if (wasInVehicle)
        {
            wasInVehicle = false;
            chairRequestedAfterExit = true;
        }

        if (!enabled || !chairRequestedAfterExit)
        {
            DeleteChair();
            return;
        }

        if (chair != null && chair.Exists())
            return;

        if (!chairModel.IsLoaded)
        {
            chairModel.Request(1000);
            return;
        }

        // تحديد مكان الكرسي بجانب اللاعب مرة واحدة
        Vector3 target =
            player.Position +
            player.RightVector * ChairDistance;

        // الحصول على ارتفاع الأرض دون تعديل ارتفاع المجسم
        float groundHeight = World.GetGroundHeight(target);

        if (groundHeight > -1000f)
            target.Z = groundHeight;

        // إنشاء الكرسي في الموقع المحدد دون ربطه باللاعب
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
            chair.Delete();

        chair = null;
    }

    private void OnAborted(object sender, EventArgs e)
    {
        DeleteChair();

        if (chairModel.IsLoaded)
            chairModel.MarkAsNoLongerNeeded();
    }
}
