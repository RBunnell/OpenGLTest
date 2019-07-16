using System;
using OpenGL;
using Tao.FreeGlut;
using DothisVertexThing;


namespace OpenGLTest
{
    class Program
    {
        private static int width = 1200, height = 720;
        private static ShaderProgram program;
        private static VBO<Vector3> cube;
        private static VBO<Vector3> cubeNormals;
        private static VBO<Vector2> cubeUV;
        private static VBO<int>  cubeElements;
        private static Texture glassTexture;
        private static System.Diagnostics.Stopwatch watch;
        private static float yangle, xangle;
        private static bool lighting = true, autoRotate = true, fullscreen = false, alpha = true;
        private static bool left = false, right = false, up = false, down = false;

        private static DothisVertexThing.DothisVertexThing a;



        static void Main(string[] args)
        {
            System.Diagnostics.Debug.Print("{0}",a.DothisVertexThing1(1));

            //Open an opengl window
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL Testing");

            //provide the Glut callbacks that are necessary fro running this tutorial
            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);
            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutReshapeFunc(OnReshape);

            // enable depth otesting to ensure correct z-ordering
            Gl.Disable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            //Compile the shader program
            program = new ShaderProgram(VertexShader, FragmentShader);

            //set the view and projection matrix, which are static throughout this tutorial
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY));

            program["light_direction"].SetValue(new Vector3(0, 0, 1));
            program["enable_lighting"].SetValue(lighting);

            //load our crate texture
            glassTexture = new Texture("glass.bmp");


            //create cube with vertices and colors
            cube = new VBO<Vector3>(new Vector3[] 
            {
                new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1,1,1),  //top
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1,-1,-1), //bottom
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1,-1,1),   // front
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1,1,-1),   //back
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1,-1,1),   //left
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1,-1,-1)    //right
            });

            cubeUV = new VBO<Vector2>(new Vector2[]
            {
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
                new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)

            });
            cubeNormals = new VBO<Vector3>(new Vector3[]
            {
                new Vector3(0,1,0), new Vector3(0,1,0), new Vector3(0,1,0), new Vector3(0,1,0),
                new Vector3(0,-1,0), new Vector3(0,1,0), new Vector3(0,-1,0), new Vector3(0,-1,0),
                new Vector3(0,0,1), new Vector3(0,0,1), new Vector3(0,0,1), new Vector3(0,0,1),
                new Vector3(0,0,-1), new Vector3(0,0,-1), new Vector3(0,0,-1), new Vector3(0,0,-1), 
                new Vector3(1,0,0), new Vector3(1,0,0), new Vector3(1,0,0), new Vector3(1,0,0),
                new Vector3(-1,0,0), new Vector3(-1,0,0), new Vector3(-1,0,0), new Vector3(-1,0,0)



            });

            cubeElements = new VBO<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, BufferTarget.ElementArrayBuffer);

            watch = System.Diagnostics.Stopwatch.StartNew();

            Glut.glutMainLoop();
        } 

        private static void OnClose()
        {
            // dispose of all of the resources that were created
            cube.Dispose();
            cubeUV.Dispose();
            glassTexture.Dispose();
            cubeNormals.Dispose();
            cubeElements.Dispose();
            program.DisposeChildren = true;
            program.Dispose();
        }

        private static void OnRenderFrame()
        {
            //calculate how much time has elapsed since the last frame
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            //use the deltaTime to adjust the angle of the cube and pyramid
            if (autoRotate)
            {
                yangle += deltaTime / 2;
                xangle += deltaTime;
            }

            if (up) xangle -= deltaTime;
            if (down) xangle += deltaTime;
            if (left) yangle += deltaTime;
            if (right) yangle -= deltaTime;


            //set up the OpenGl viewport and clear both the color and depth bits
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            //use our shader program
            program.Use();
            Gl.BindTexture(glassTexture);


            uint vertexPositionIndex = (uint)Gl.GetAttribLocation(program.ProgramID, "vertexPosition");
            Gl.EnableVertexAttribArray(vertexPositionIndex);

            //enable disable lighting
            program["enable_lighting"].SetValue(lighting);

            //bind the vertex position, colors and elemnets of the cube
            program["model_matrix"].SetValue(Matrix4.CreateRotationY(yangle) * Matrix4.CreateRotationX(xangle));
 //           System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3}",yangle, xangle, up, down);
            Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(cubeNormals, program, "vertexNormal");
            Gl.BindBufferToShaderAttribute(cubeUV, program, "vertexUV");
            Gl.BindBuffer(cubeElements);

            // draw my cube
            Gl.DrawElements(BeginMode.Quads, cubeElements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            Glut.glutSwapBuffers();

        }
        
        private static void OnReshape(int width, int height)
        {
            Program.width = width;
            Program.height = height;
            
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
        }
        private static void OnKeyboardDown(byte key, int x, int y)
        {
            if (key == 27) Glut.glutLeaveMainLoop();
            else if (key == 'a') left = true;
            else if (key == 'w') up = true;
            else if (key == 'd') right = true;
            else if (key == 's') down = true;
        }
        private  static void OnKeyboardUp(byte key, int x , int y)
        {
            if (key == 'l') lighting = !lighting;
            else if (key == ' ') autoRotate = !autoRotate;
            else if (key == 'a') left = false;
            else if (key == 'w') up = false;
            else if (key == 'd') right = false;
            else if (key == 's') down = false;
            else if (key == 'f')
            {
                fullscreen = !fullscreen;
                if (fullscreen) Glut.glutFullScreen();
                else
                {
                    Glut.glutPositionWindow(0, 0);
                    Glut.glutReshapeWindow(1280, 720);

                }
            }
            else if (key == 'b')
            {
                alpha = !alpha;
                if (alpha)
                {
                    Gl.Enable(EnableCap.Blend);
                    Gl.Disable(EnableCap.DepthTest);
                }
                else
                {
                    Gl.Disable(EnableCap.Blend);
                    Gl.Enable(EnableCap.DepthTest);
                }
            }
            }


        private static void OnDisplay()
        {

        }
        public static string VertexShader = @"
#version 130

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;

out vec3 normal;
out vec2 uv;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    normal = normalize((model_matrix * vec4(vertexNormal, 0)).xyz);
    uv = vertexUV;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";
        public static string FragmentShader = @"
#version 130

uniform vec3 light_direction;
uniform sampler2D texture;
uniform bool enable_lighting;

in vec3 normal;
in vec2 uv;

out vec4 fragment;

void main(void)
{
    float diffuse = max(dot(normal, light_direction), 0);
    float ambient = 0.3;
    float lighting = (enable_lighting ? max(diffuse, ambient): 1.0);

    vec4 sample = texture2D(texture, uv);
    fragment = vec4(lighting * texture2D(texture, uv).xyz, 0.5);
}
";
    }
}
