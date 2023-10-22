using Godot;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

public partial class meteor :  Area3D
{

    [Export]
    float WINDOW_SIZE_Y = 15;

    [Export]
	Vector2 SPEED = new Vector2(10, 15);

    [Export]
    private int health = 3;


    int dir = -1;

    public int Health { get => health; set => health = value; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        if (GD.Randf() < 0.5)
        {
            dir = -1;
        }
        else
        {
            dir = 1;
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        Vector3 new_position = Position;
        new_position.X -= SPEED.X * (float)delta;
        new_position.Y -= SPEED.Y * (float)delta * dir;
        Position = new_position;

        RotateZ(0.5f * (float)delta);
        //rotation += 0.01

        if (Position.Y > WINDOW_SIZE_Y)
        {
            dir = 1;
        }
        if (Position.Y < -WINDOW_SIZE_Y)
        {
            dir = -1;
        }
    }



    public async void _on_area_entered(Area3D area) 
    {
        if (Health == 0) return;


        if (area.IsInGroup("player"))
        {
            if (Health == 0) return;
            Health -= 1;

            if (Health < 1)
            {
                await DestruirMeteor();
            }
            else
            {
                PlayHitSound();
            }

        }

        if (area.IsInGroup("enemyColision"))
        {
            if (Health == 0) return;
            Health -= 1;
            
            if (Health < 1)
            {
                await DestruirMeteor();
            }
            else
            {
                PlayHitSound();
            }

        }

        if (area.IsInGroup("ball"))
        {
            if (Health == 0) return;
            Health -= 1;

            area.GetParent().QueueFree();//destruyo bala no tiene trigger propio
            
            if(Health < 1) 
            {
                await DestruirMeteor();
            }
            else
            {
                PlayHitSound();
            }

        }

        if (area.IsInGroup("meteor"))
        {
            await DestruirMeteor();
        }



    }

    private async Task DestruirMeteor()
    {
        Health = 0;//aseguro que sea cero

        (GetTree().GetFirstNodeInGroup("explosionSound") as AudioStreamPlayer).Play();
        GetNode<GpuParticles3D>("GPUParticles3D").Emitting = true;
        GetNode<GpuParticles3D>("GPUParticles3Dfire").Emitting = true;
        GetNode<MeshInstance3D>("asterioid").Visible = false;

        //await ToSignal(GetTree().CreateTimer(0.2f), "timeout");//detengo por 0.3 segundo por la explosion
        GetNode<CollisionShape3D>("triggercollision").QueueFree();

        await ToSignal(GetTree().CreateTimer(2f), "timeout");//detengo por 1 segundo
        QueueFree();
    }

    private void PlayHitSound()
    {
        (GetTree().GetFirstNodeInGroup("hitSound") as AudioStreamPlayer).Play();
    }


    public async void _on_visible_on_screen_notifier_3d_screen_exited()
    {
        GD.Print("salio el asteroide");
        await ToSignal(GetTree().CreateTimer(1f), "timeout");//detengo por 0.2 por la explosion
        QueueFree();
    }



}
