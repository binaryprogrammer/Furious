using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FuriousGameEngime_XNA4.Screens;
using Microsoft.Xna.Framework.Content;

namespace FuriousGameEngime_XNA4.Enviornment
{
    class Water
    {
        public struct VertexMultitextured
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public Vector3 Tangent;
            public Vector3 BiNormal;

            public static int SizeInBytes = (3 + 3 + 2 + 3 + 3) * 4;
            public static VertexDeclaration VertexDeclaration = new VertexDeclaration
             (
                 new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                 new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                 new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                 new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                 new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)
             );

        }

        GameScreen _game;

        VertexBuffer _vertexBuffer;
        IndexBuffer _indexBuffer;
        VertexMultitextured[] _vertices;
        int _height = 128;
        int _width = 128;

        Vector3 _position;
        Vector3 _scale;
        Quaternion _rotation;

        Effect _effect;

        Vector3 _basePosition;

        float _bumpHeight = 0.5f;
        Vector2 _textureScale = new Vector2(4, 4);
        Vector2 _bumpSpeed = new Vector2(0, .05f);
        float _fresnelBias = .025f;
        float _fresnelPower = 1.0f;
        float _hdrMultiplier = 1.0f;
        Color _deepWaterColor = Color.Black;
        Color _shallowWaterColor = Color.SkyBlue;
        Color _reflectionColor = Color.White;
        float _reflectionAmount = 0.5f;
        float _waterAmount = 0f;
        float _waveAmplitude = 0.5f;
        float _waveFrequency = 0.1f;

        #region Getters and Setters
        public Vector3 Position
        {
            get { return _basePosition; }
            set { _basePosition = value; }
        }

        /// <summary>
        /// Height of water bump texture.
        /// Min 0.0 Max 2.0 Default = .5
        /// </summary>
        public float BumpHeight
        {
            get { return _bumpHeight; }
            set { _bumpHeight = value; }
        }
        /// <summary>
        /// Scale of bump texture.
        /// </summary>
        public Vector2 TextureScale
        {
            get { return _textureScale; }
            set { _textureScale = value; }
        }
        /// <summary>
        /// Velocity of water flow
        /// </summary>
        public Vector2 BumpSpeed
        {
            get { return _bumpSpeed; }
            set { _bumpSpeed = value; }
        }
        /// <summary>
        /// Min 0.0 Max 1.0 Default = .025
        /// </summary>
        public float FresnelBias
        {
            get { return _fresnelBias; }
            set { _fresnelBias = value; }
        }
        /// <summary>
        /// Min 0.0 Max 10.0 Default = 1.0;
        /// </summary>
        public float FresnelPower
        {
            get { return FresnelPower; }
            set { _fresnelPower = value; }
        }
        /// <summary>
        /// Min = 0.0 Max = 100 Default = 1.0
        /// </summary>
        public float HDRMultiplier
        {
            get { return _hdrMultiplier; }
            set { _hdrMultiplier = value; }
        }
        /// <summary>
        /// Color of deep water Default = Black;
        /// </summary>
        public Color DeepWaterColor
        {
            get { return _deepWaterColor; }
            set { _deepWaterColor = value; }
        }
        /// <summary>
        /// Color of shallow water Default = SkyBlue
        /// </summary>
        public Color ShallowWaterColor
        {
            get { return _shallowWaterColor; }
            set { _shallowWaterColor = value; }
        }
        /// <summary>
        /// Default = White
        /// </summary>
        public Color ReflectionColor
        {
            get { return _reflectionColor; }
            set { _reflectionColor = value; }
        }
        /// <summary>
        /// Min = 0.0 Max = 2.0 Default = .5
        /// </summary>
        public float ReflectionAmount
        {
            get { return _reflectionAmount; }
            set { _reflectionAmount = value; }
        }
        /// <summary>
        /// Amount of water color to use.
        /// Min = 0 Max = 2 Default = 0;
        /// </summary>
        public float WaterAmount
        {
            get { return _waterAmount; }
            set { _waterAmount = value; }
        }
        /// <summary>
        /// Min = 0.0 Max = 10 Defatult = 0.5
        /// </summary>
        public float WaveAmplitude
        {
            get { return _waveAmplitude; }
            set { _waveAmplitude = value; }
        }
        /// <summary>
        /// Min = 0 Max = 1 Default .1
        /// </summary>
        public float WaveFrequency
        {
            get { return _waveFrequency; }
            set { _waveFrequency = value; }
        }

        /// <summary>
        /// Default 128
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        /// <summary>
        /// Default 128
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }
        #endregion

        public Water(GameScreen game)
        {
            _game = game;

            _scale = new Vector3(100, 0, 100);
            _rotation = new Quaternion(0, 0, 0, 1);
        }

        public void SetDefault()
        {
            _bumpHeight = 0.5f;
            _textureScale = new Vector2(4, 4);
            _bumpSpeed = new Vector2(0, .05f);
            _fresnelBias = .025f;
            _fresnelPower = 1.0f;
            _hdrMultiplier = 1.0f;
            _deepWaterColor = Color.Black;
            _shallowWaterColor = Color.SkyBlue;
            _reflectionColor = Color.White;
            _reflectionAmount = 0.5f;
            _waterAmount = 0f;
            _waveAmplitude = 0.5f;
            _waveFrequency = 0.1f;
        }

        internal void LoadContent(ContentManager Content)
        {
            _effect = Content.Load<Effect>("Effects/Water");

            //_effect.Parameters["tEnvMap"].SetValue(_game.sceneRenderTarget);
            _effect.Parameters["tEnvMap"].SetValue(Content.Load<Texture2D>("Map//water"));
            _effect.Parameters["tNormalMap"].SetValue(Content.Load<Texture2D>("Map//water"));

            _position = new Vector3(_basePosition.X - (_width / 2), _basePosition.Y, _basePosition.Z - (_height / 2));

            // Vertices
            _vertices = new VertexMultitextured[_width * _height];

            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                {
                    _vertices[x + y * _width].Position = new Vector3(y, 0, x);
                    _vertices[x + y * _width].Normal = new Vector3(0, -1, 0);
                    _vertices[x + y * _width].TextureCoordinate.X = (float)x / 30.0f;
                    _vertices[x + y * _width].TextureCoordinate.Y = (float)y / 30.0f;
                }

            // Calc Tangent and Bi Normals.
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                {
                    // Tangent Data.
                    if (x != 0 && x < _width - 1)
                        _vertices[x + y * _width].Tangent = _vertices[x - 1 + y * _width].Position - _vertices[x + 1 + y * _width].Position;
                    else
                        if (x == 0)
                            _vertices[x + y * _width].Tangent = _vertices[x + y * _width].Position - _vertices[x + 1 + y * _width].Position;
                        else
                            _vertices[x + y * _width].Tangent = _vertices[x - 1 + y * _width].Position - _vertices[x + y * _width].Position;

                    // Bi Normal Data.
                    if (y != 0 && y < _height - 1)
                        _vertices[x + y * _width].BiNormal = _vertices[x + (y - 1) * _width].Position - _vertices[x + (y + 1) * _width].Position;
                    else
                        if (y == 0)
                            _vertices[x + y * _width].BiNormal = _vertices[x + y * _width].Position - _vertices[x + (y + 1) * _width].Position;
                        else
                            _vertices[x + y * _width].BiNormal = _vertices[x + (y - 1) * _width].Position - _vertices[x + y * _width].Position;
                }


            _vertexBuffer = new VertexBuffer(_game.ScreenManager.GraphicsDevice, VertexMultitextured.VertexDeclaration, VertexMultitextured.SizeInBytes * _width * _height, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_vertices);

            short[] terrainIndices = new short[(_width - 1) * (_height - 1) * 6];
            for (short x = 0; x < _width - 1; x++)
            {
                for (short y = 0; y < _height - 1; y++)
                {
                    terrainIndices[(x + y * (_width - 1)) * 6] = (short)((x + 1) + (y + 1) * _width);
                    terrainIndices[(x + y * (_width - 1)) * 6 + 1] = (short)((x + 1) + y * _width);
                    terrainIndices[(x + y * (_width - 1)) * 6 + 2] = (short)(x + y * _width);

                    terrainIndices[(x + y * (_width - 1)) * 6 + 3] = (short)((x + 1) + (y + 1) * _width);
                    terrainIndices[(x + y * (_width - 1)) * 6 + 4] = (short)(x + y * _width);
                    terrainIndices[(x + y * (_width - 1)) * 6 + 5] = (short)(x + (y + 1) * _width);
                }
            }

            _indexBuffer = new IndexBuffer(_game.ScreenManager.GraphicsDevice, typeof(short), (_width - 1) * (_height - 1) * 6, BufferUsage.WriteOnly);
            _indexBuffer.SetData(terrainIndices);
        }

        //public override void Draw(GameTime gameTime)
        internal void Draw()
        {
            Matrix World = Matrix.CreateScale(_scale) * Matrix.CreateFromQuaternion(_rotation) * Matrix.CreateTranslation(_position);

            Matrix WVP;
            Matrix WV;
            Matrix viewI;


            WVP = World * _game.camera.View * _game.camera.Projection;
            WV = World * _game.camera.View;
            viewI = Matrix.Invert(_game.camera.View);

            _effect.Parameters["matWorldViewProj"].SetValue(WVP);
            _effect.Parameters["matWorld"].SetValue(World);
            _effect.Parameters["matWorldView"].SetValue(WV);
            _effect.Parameters["matViewI"].SetValue(Matrix.Identity * Matrix.CreateTranslation(10000, 0, -10000));

            //_effect.Parameters["fBumpHeight"].SetValue(_bumpHeight);
            //_effect.Parameters["vTextureScale"].SetValue(_textureScale);
            //_effect.Parameters["vBumpSpeed"].SetValue(_bumpSpeed);
            //_effect.Parameters["fFresnelBias"].SetValue(_fresnelBias);
            //_effect.Parameters["fFresnelPower"].SetValue(_fresnelPower);
            //_effect.Parameters["fHDRMultiplier"].SetValue(_hdrMultiplier);
            //_effect.Parameters["vDeepColor"].SetValue(_deepWaterColor.ToVector4());
            //_effect.Parameters["vShallowColor"].SetValue(_shallowWaterColor.ToVector4());
            //_effect.Parameters["vReflectionColor"].SetValue(_reflectionColor.ToVector4());
            //_effect.Parameters["fReflectionAmount"].SetValue(_reflectionAmount);
            //_effect.Parameters["fWaterAmount"].SetValue(_waterAmount);
            //_effect.Parameters["fWaveAmp"].SetValue(_waveAmplitude);
            //_effect.Parameters["fWaveFreq"].SetValue(_waveFrequency);

            _game.ScreenManager.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            _game.ScreenManager.GraphicsDevice.Indices = _indexBuffer;

            for (int p = 0; p < _effect.CurrentTechnique.Passes.Count; p++)
            {
                _effect.CurrentTechnique.Passes[p].Apply();
                _game.ScreenManager.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _width * _height, 0, (_width - 1) * (_height - 1) * 2);
            }

            //add the verticies of the water to our poly count
            //_game.editor.polygonCount += _vertexBuffer.VertexCount;
        }

        internal void Update(GameTime gameTime)
        {
            _effect.Parameters["fTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
        }
    }
}
