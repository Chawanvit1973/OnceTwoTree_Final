using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnceTwoTree_game1
{
    class Camera
    {
        public Matrix Tranform;
        Vector2 centre;

        public Camera()
        {

        }

        public void Update(Vector2 position)
        {
            centre = new Vector2(position.X - 200, position.Y - 250);
            Tranform = Matrix.CreateScale(new Vector3(1, 1, 0)) *
                Matrix.CreateTranslation(new Vector3(-centre.X, -centre.Y, 0));
        }
    }
}
