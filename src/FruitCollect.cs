
using System;
using GTA;
using GTA.Math;
using GTA.Native;

public class FruitCollect : Script
{
    private const int FruitCount = 10;
    private const float CollectDistance = 1.5f;
    private const int RespawnDelaySeconds = 20;
    private const int MoveAfterSeconds = 60;

    private Prop[] fruits = new Prop[FruitCount];
    private DateTime[] respawnTimes = new DateTime[FruitCount];
    private DateTime[] spawnTimes = new DateTime[FruitCount];

    private readonly string[] fruitModels =
    {
        "ng_proc_food_nana1a",
        "ng_proc_food_ornge1a"
    };

    private int collected = 0;
    private Random random = new Random();

    public FruitCollect()
    {
        Interval = 500;
        Tick += OnTick;
        Aborted += OnAborted;
    }

    private void OnTick(object sender, EventArgs e)
    {
        Ped player = Game.Player.Character;

        if (player == null || !player.Exists() || player.IsDead)
            return;

        for (int i = 0; i < FruitCount; i++)
        {
            if (fruits[i] == null || !fruits[i].Exists())
            {
                if (DateTime.UtcNow < respawnTimes[i])
                    continue;

                SpawnFruit(i, player);
                continue;
            }

            // Move uncollected fruit after 60 seconds
            if ((DateTime.UtcNow - spawnTimes[i]).TotalSeconds
                >= MoveAfterSeconds)
            {
                fruits[i].Delete();
                fruits[i] = null;

                SpawnFruit(i, player);
                continue;
            }

            // Automatic collection
            float distance =
                player.Position.DistanceTo(fruits[i].Position);

            if (distance <= CollectDistance)
            {
                fruits[i].Delete();
                fruits[i] = null;

                collected++;

                respawnTimes[i] =
                    DateTime.UtcNow.AddSeconds(RespawnDelaySeconds);

                UI.ShowSubtitle("Fruit collected: " + collected);
            }
        }
    }

    private void SpawnFruit(int index, Ped player)
    {
        string modelName =
            fruitModels[random.Next(fruitModels.Length)];

        Model model = new Model(modelName);

        if (!model.IsInCdImage || !model.IsValid)
            return;

        if (!model.IsLoaded)
            model.Request(1000);

        if (!model.IsLoaded)
            return;

        // Random location around the player
        float angle = (float)(random.NextDouble() * Math.PI * 2);
        float distance = 5f + (float)random.NextDouble() * 20f;

        Vector3 position = new Vector3(
            player.Position.X + (float)Math.Cos(angle) * distance,
            player.Position.Y + (float)Math.Sin(angle) * distance,
            player.Position.Z
        );

        Prop fruit = World.CreateProp(
            model,
            position,
            false,
            true
        );

        if (fruit == null || !fruit.Exists())
        {
            model.MarkAsNoLongerNeeded();
            return;
        }

        Function.Call(
            Hash.SET_ENTITY_COLLISION,
            fruit.Handle,
            false,
            false
        );

        Function.Call(
            Hash.SET_ENTITY_HAS_GRAVITY,
            fruit.Handle,
            false
        );

        Function.Call(
            Hash.SET_ENTITY_DYNAMIC,
            fruit.Handle,
            false
        );

        fruits[index] = fruit;
        spawnTimes[index] = DateTime.UtcNow;

        model.MarkAsNoLongerNeeded();
    }

    private void OnAborted(object sender, EventArgs e)
    {
        for (int i = 0; i < FruitCount; i++)
        {
            if (fruits[i] != null && fruits[i].Exists())
            {
                fruits[i].Delete();
                fruits[i] = null;
            }
        }
    }
}

