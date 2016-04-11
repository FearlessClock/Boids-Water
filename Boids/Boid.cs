using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using OpenTK.Input;

namespace Boids
{
    class Boid
    {
        //Simulation time
        static public int time = 0;
        Random rand;
        public Vector2 position;
        public Vector2 direction;   //Unit vector
        public double visionRadius;
        double visionRadiusPower;

        int alligence;

        public double seperationRadius;
        double seperationRadiusPower;

        int screenSizeX;
        int screenSizeY;

        //Weight system to make a creature more likely to follow a certain rule
        double wSeperation;
        double wCohesion;
        double wAlignement;
        double wRandomness;

        int health;
        List<Message> messageList = new List<Message>();
        List<Boid> target;
        int coolDown = 0;
        public bool isAlive = true;
        public Boid(int alligence, Vector2 pos, Vector2 dir, double visRadius, double sepRadius, int scrX, int scrY, Random random, double wSep, double wCoh, double wAli, double wRnd)
        {
            rand = random;
            this.alligence = alligence;
            position = pos;
            direction = dir;
            visionRadius = visRadius;
            visionRadiusPower = visionRadius * visionRadius;
            seperationRadius = sepRadius;
            seperationRadiusPower = seperationRadius * seperationRadius;

            screenSizeX = scrX;
            screenSizeY = scrY;

            wAlignement = wAli;
            wCohesion = wCoh;
            wSeperation = wSep;
            wRandomness = wRnd;

            health = 100;
            target = new List<Boid>();
        }

        public void Update(Boid[] boids, Vector2 lastClickPosition, Vector2 pointer)
        {
            //Add the 3 rules
            //Seperation
            //Cohesion
            //Alignement
            //Add another rule, Follow the mouse
            Vector2 mouseFollow = new Vector2(0, 0);
            //MouseState ms = Mouse.GetCursorState();
           /* Vector2 ms = lastClickPosition;
            mouseFollow = ms - position;//new Vector2((ms.X - Camera.cameraPos.X ) - position.X, (Camera.cameraPos.X - (ms.Y-767)) - position.Y);
            mouseFollow.NormalizeFast();
            mouseFollow = Vector2.Multiply(mouseFollow, 1.2F);*/
            //Seperation and Alignement and cohesion vectors
            Vector2 sep = new Vector2(0, 0);
            Vector2 ali = new Vector2(0, 0);
            Vector2 coh = new Vector2(0, 0);
            //Vairables to make it attack
            int attackTargetsCount = 0;
            Vector2 att = new Vector2(0, 0);

            Vector2 swarm = new Vector2(0, 0);

            int numberOfNeighbors = 0;
            int sepNeightbors = 0;
            //Get a list of all the boids in the vicinity
            foreach (Boid b in boids)
            {
                double dis = DistanceBetween(b);
                //Seperation
                if (dis < visionRadiusPower)
                {
                    Vector2 vec = new Vector2(0, b.position.Y - this.position.Y);
                    
                    //sep += vec;
                    sepNeightbors++;
                }
            }
            //Seperation averagin
            //sep = Vector2.Divide(sep, sepNeightbors);
            //if (sep.X != 0 && sep.Y != 0)
            //sep.NormalizeFast();
            ali = new Vector2(0, 300 - position.Y);
            if (!ali.Equals(Vector2.Zero)) ;
                //ali.NormalizeFast();
            //ali *= 3;

            Console.WriteLine(ali.Y);
            //Apply the weights
            //ApplyWeights(ali, coh, sep, randomDir);
            Vector2 dir = new Vector2(0, 0);
            dir += ali; //Add the 3 rules together;

            if(!(dir.X == 0 && dir.Y == 0))
                dir.Normalize();        //normalize it
            //dir = Vector2.Divide(dir, 1);//Scale it so that it isn't over powering
            direction += dir;           //Add the new scaled force to the direction of the boid
            Console.WriteLine(direction.ToString() + dir.ToString());
            if (!(direction.X == 0 && direction.Y == 0)) 
                direction.Normalize();      //Normalize it
            //direction = Vector2.Multiply(direction, 2);
            //Console.WriteLine(direction.Y);
            position += direction;      //Move it
                                        //Make sure it doesn't leave the screen
            if (position.X < -5) position.X = screenSizeX + 5;
            if (position.X > screenSizeX + 5) position.X = -5;

            if (position.Y < -5) position.Y = screenSizeY + 5;
            if (position.Y > screenSizeY + 5) position.Y = -5;

            if (coolDown > 0)
                coolDown--;

            //DealWithMessages();
        }
        

        private void DealWithMessages()
        {
            for (int i = 0; i < messageList.Count; i++)
            {
                if (messageList[i].CallTime != time)
                    if (messageList[i].GetMessage == MessageType.Attack)
                    {
                        health -= messageList[i].Value;
                        if (health < 1)
                        {
                            isAlive = false;
                            health = 0;
                        }
                        messageList.RemoveAt(i);
                    }
            }
        }

        private void ApplyWeights(Vector2 ali, Vector2 coh, Vector2 sep, Vector2 rnd)
        {
            ali = Vector2.Multiply(ali, (float)wAlignement);
            coh = Vector2.Multiply(coh, (float)wCohesion);
            sep = Vector2.Multiply(sep, (float)wSeperation);
            rnd = Vector2.Multiply(rnd, (float)wRandomness);
        }

        double DistanceBetween(Boid B)
        {
            return Math.Pow(B.position.X - position.X, 2) + Math.Pow(B.position.Y - position.Y, 2);
        }

        public void Draw(ref Buffer buf)
        {
            //Create a list to stock the new vertices
            List<Vertex> vertBuffer = new List<Vertex>();
            List<uint> indexBuffer = new List<uint>();
            //Fill them with the old data
            vertBuffer.AddRange(buf.vertBuffer);
            indexBuffer.AddRange(buf.indexBuffer);
            //Get the triangle positions
            Vector2 tri1 = position;
            Vector2 tri2 = position - direction * 10 - new Vector2(-direction.Y, direction.X).Normalized() * 10;
            Vector2 tri3 = position - direction * 10 - new Vector2(direction.Y, -direction.X).Normalized() * 10;
            //Place the new points into the lists
            vertBuffer.Add(new Vertex(tri1, new Vector2(0, 0)) { Color = alligence == 0 ? Color.FromArgb(health, 255, 0) : Color.FromArgb(255, 0, health )} );
            vertBuffer.Add(new Vertex(tri2, new Vector2(0, 1)) { Color = alligence == 0 ? Color.FromArgb(health, 255, 0) : Color.FromArgb(255, 0, health) });
            vertBuffer.Add(new Vertex(tri3, new Vector2(1, 0)) { Color = alligence == 0 ? Color.FromArgb(health, 255, 0) : Color.FromArgb(255, 0, health) });

            indexBuffer.Add((uint)buf.vertBuffer.Length);
            indexBuffer.Add((uint)buf.vertBuffer.Length+1);
            indexBuffer.Add((uint)buf.vertBuffer.Length+2);

            //Put the lists back into the buffer
            buf.vertBuffer = vertBuffer.ToArray<Vertex>();
            buf.indexBuffer = indexBuffer.ToArray<uint>();
        }

        public void DebugDraw(ref Buffer buf)
        {
            //Create a list to stock the new vertices
            List<Vertex> vertBuffer = new List<Vertex>();
            List<uint> indexBuffer = new List<uint>();
            //Fill them with the old data
            vertBuffer.AddRange(buf.vertBuffer);
            indexBuffer.AddRange(buf.indexBuffer);
            //Get the triangle positions
            //Place the new points into the lists
            uint counter = (uint)vertBuffer.Count;
            for (int i = 0; i < 360; i += 360 / 3)
            {
                vertBuffer.Add(new Vertex(new Vector2(position.X + (float)(visionRadius* Math.Cos(MathHelper.DegreesToRadians(i))), position.Y + (float)(visionRadius* Math.Sin(MathHelper.DegreesToRadians(i)))), new Vector2(0, 0)) { Color = Color.Red });
                indexBuffer.Add(counter);
                counter++;
            }

            

            //Put the lists back into the buffer
            buf.vertBuffer = vertBuffer.ToArray<Vertex>();
            buf.indexBuffer = indexBuffer.ToArray<uint>();
        }

        public override string ToString()
        {
            return " Ali:" + alligence + " Pos:" + position.ToString() + " Dir:" + direction.ToString();
        }
    }
}
