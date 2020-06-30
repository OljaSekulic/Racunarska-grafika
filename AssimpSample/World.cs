// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 20.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = -100.0f;   //vrati na -40 !!!

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 9000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private float m_translateStair = 0.0f;
        private float m_scalePoles = 0.0f;


        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public float TranslateStair
        {
            get { return m_translateStair; }
            set
            {
                m_translateStair = value;

            }
        }

        public float ScalePoles
        {
            get { return m_scalePoles; }
            set
            {
                m_scalePoles = value;

            }
        }

        private uint[] m_textures;
        private string[] m_textureFiles = { "..//..//Images//wood.jpg", "..//..//Images//metal.jpg", "..//..//Images//sea.jpg" };
        private enum TextureObjects { Wood = 0, Metal, Sea };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;

            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
           // gl.Enable(OpenGL.GL_DEPTH_TEST); //obavezno vrati ove tri linije !!!!
           // gl.Enable(OpenGL.GL_CULL_FACE);
           // gl.FrontFace(OpenGL.GL_CCW);
           // gl.CullFace(OpenGL.GL_BACK);

            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(0f, 0f, 139f);
            // Model sencenja na flat (konstantno)   
            gl.ShadeModel(OpenGL.GL_FLAT);

            //Ukljucivanje color tracking mehanizma
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            SetupLighting(gl);

            foreach (uint textureId in m_textures) //
            {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureId);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            }

            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
               // image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);


               gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);  // Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);  // Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                //nacin spajanja sa podlogom
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
               // image.UnlockBits(imageData);
                image.Dispose();
            }

            m_scene.LoadScene();
            m_scene.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

          

            gl.PushMatrix();
            gl.Enable(OpenGL.GL_TEXTURE_2D);
           // gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Scale(0.5, 0.5, 0.5);
            gl.Rotate(m_xRotation, 0.0f, 0.0f, 0.0f);
            gl.LookAt(-3, 2, -3, 0, 0, 0, 0, 2, 0);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            

            gl.PushMatrix(); //MORE
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Sea]);
            gl.Color(0.2f, 0.6f, 0.8f);
            gl.Translate(4500.0f, -600.0f, -3000f);
            gl.Rotate(-90f, 0.0f, 0.0f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(1f, 1f);

            gl.Vertex(-10000f, 10000.0f);
            gl.TexCoord(0f, 1f);
            gl.Vertex(3000.0f, 10000.0f);
            gl.TexCoord(0f, 0f);
            gl.Vertex(3000.0f,-10000.0f);
            gl.TexCoord(1f, 0f);
            gl.Vertex(-10000.0f, -10000.0f);
            

            gl.End();

            gl.PopMatrix();

            gl.PushMatrix();         //MOL
            gl.Translate(4800f, 0f, 0f);
            gl.Scale(3000f,100f,400f);
            gl.Color(0.5f, 0.35f, 0.05f);
            Cube cube = new Cube();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Wood]);
           

            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            


            //PRVI STUB 
            gl.PushMatrix();
            gl.Translate(2800f, -1200f + m_scalePoles, 0f);
            gl.Rotate(-90f, 0f, 0f);
            gl.Scale(300f, 400f, 200f);
            Cylinder cil2 = new Cylinder();
            cil2.TopRadius = 1;
            cil2.Height = 6;
            cil2.BaseRadius = 1;
            
            cil2.CreateInContext(gl);
            cil2.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Wood]);
            cil2.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //DRUGI STUB
            gl.PushMatrix();
            gl.Translate(4800f, -1200f + m_scalePoles, 0f);
            gl.Rotate(-90f, 0f, 0f);
            gl.Scale(300f, 400f, 200f);
            Cylinder cil1 = new Cylinder();
            cil1.TopRadius = 1;
            cil1.BaseRadius = 1;
            cil1.Height = 6;
            
            cil1.CreateInContext(gl);
            cil1.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Wood]);
            cil1.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //TRECI STUB
            gl.PushMatrix();
            gl.Translate(6800f, -1200f + m_scalePoles, 0f);
            gl.Rotate(-90f, 0f, 0f);
            gl.Scale(300f, 400f, 200f);
            Cylinder cil3 = new Cylinder();
            cil3.TopRadius = 1;
            cil3.BaseRadius = 1;
            cil3.Height = 6;
          
            cil3.CreateInContext(gl);
            cil3.TextureCoords = true;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Wood]);
            cil3.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render); ;
            gl.PopMatrix();

            gl.PushMatrix();       //STEPENICE
            gl.Color(0.8f, 0.8f, 0.8f);
            gl.Translate(1900f + m_translateStair, 200f, 0f);
            gl.Scale(500f, 100f, 400f);
            
            Cube cube1 = new Cube();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Metal]);
            cube1.Render(gl, RenderMode.Render);

            gl.PopMatrix();

           //TEKST
            
            m_scene.Draw();

            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(1.0f, -3.5f, 0.0f);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            gl.Ortho2D(-16.0f, 16.0f, -13.0f, 12.0f);

            gl.PushMatrix();
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Translate(1.5f, -4.0f, 0.0f);

            gl.DrawText3D("Arial Bold", 14f, 1f, 1f, "Predmet: Racunarska grafika");
            gl.Translate(-12.3f, -1.0f, 0.0f);
            gl.DrawText3D("Arial Bold", 14f, 1f, 1f, "Sk.god: 2019/20.");
            gl.Translate(-7f, -1.0f, 0.0f);
            gl.DrawText3D("Arial Bold", 14f, 1f, 1f, "Ime: Olivera");
            gl.Translate(-5.2f, -1.0f, 0.0f);
            gl.DrawText3D("Arial Bold", 14f, 1f, 1f, "Prezime: Sekulic");
            gl.Translate(-7.1f, -1.0f, 0.0f);
            gl.DrawText3D("Arial Bold", 14f, 1f, 1f, "Sifra zad: 4.2");
            gl.PopMatrix();

           
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (float)m_width / m_height, 1.0f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            
            gl.PopMatrix();

            gl.Flush();
        }

        private void SetupLighting(OpenGL gl)
        {
            float[] light0pos = new float[] { 0.0f, 100.0f, 0.0f, 1.0f };
            float[] light0ambient = new float[] { 1.0f, 1.0f, 0.5f, 1.0f };    //dodala sam da treci parametar bude 0 i da tako bude zuta boja.
                                                                               //to sam dodala i za abient,diffuse i za specular
            float[] light0diffuse = new float[] { 1.0f, 1.0f, 0.5f, 1.0f };
            float[] light0specular = new float[] { 1.0f, 1.0f, 0.5f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
           // gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, light0direction);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);

            gl.Enable(OpenGL.GL_NORMALIZE); //ukljucena automatska normalizacija
        }
        

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 1f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
