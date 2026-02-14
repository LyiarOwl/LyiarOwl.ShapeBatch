using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LyiarOwl.ShapeBatch
{
    public class ShapeBatch : IDisposable
    {
        readonly GraphicsDevice _device;
        readonly int _maxVertices;
        readonly BasicEffect _effect;
        readonly VertexPositionColor[] _vertices;
        readonly short[] _indices;
        int _vertexCount;
        int _indexCount;
        bool _begin;
        bool _disposed;
        public event Action Disposing;
        public ShapeBatch(GraphicsDevice graphicsDevice, int maxVertices = 1024)
        {
            _device = graphicsDevice;
            _effect = new BasicEffect(_device);
            _maxVertices = maxVertices;
            _vertices = new VertexPositionColor[_maxVertices];
            _indices = new short[_maxVertices * 3];
        }
        private void AddLineVertices(float x1, float y1, float x2, float y2, Color color, float thickness = 1f)
        {
            if (_vertexCount + 4 >= _maxVertices || _indexCount + 6 >= _indices.Length)
                Flush();

            int baseVertex = _vertexCount;

            float edgeX = x2 - x1;
            float edgeY = y2 - y1;
            float normalX = -edgeY;
            float normalY = edgeX;

            float lengthSqrd = normalX * normalX + normalY * normalY;
            if (lengthSqrd < 0.000001f)
                return;

            if (lengthSqrd > 0f)
            {
                float invLen = 1f / (float)Math.Sqrt(lengthSqrd);
                normalX *= invLen;
                normalY *= invLen;
            }

            float halfThickness = thickness * 0.5f;
            normalX *= halfThickness;
            normalY *= halfThickness;

            //2 triangles
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x1 - normalX, y1 - normalY, 0f), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x1 + normalX, y1 + normalY, 0f), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x2 - normalX, y2 - normalY, 0f), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x2 + normalX, y2 + normalY, 0f), color);

            _indices[_indexCount++] = (short)(baseVertex + 0);
            _indices[_indexCount++] = (short)(baseVertex + 1);
            _indices[_indexCount++] = (short)(baseVertex + 2);

            _indices[_indexCount++] = (short)(baseVertex + 1);
            _indices[_indexCount++] = (short)(baseVertex + 3);
            _indices[_indexCount++] = (short)(baseVertex + 2);
        }
        private void AddFilledRectangleVertices(float x, float y, float width, float height, Color color)
        {
            if (_vertexCount + 4 >= _maxVertices || _indexCount + 6 >= _indices.Length)
                Flush();

            int baseVertex = _vertexCount;
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x, y, 0f), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x + width, y, 0f), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x, y + height, 0f), color);
            _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x + width, y + height, 0f), color);

            _indices[_indexCount++] = (short)(baseVertex + 0);
            _indices[_indexCount++] = (short)(baseVertex + 1);
            _indices[_indexCount++] = (short)(baseVertex + 2);

            _indices[_indexCount++] = (short)(baseVertex + 1);
            _indices[_indexCount++] = (short)(baseVertex + 3);
            _indices[_indexCount++] = (short)(baseVertex + 2);
        }
        /// <summary>
        /// Draws a segment from a point A to B.
        /// </summary>
        /// <param name="begin">Where this line starts.</param>
        /// <param name="end">Where this line ends.</param>
        /// <param name="color">Color of this segment.</param>
        /// <param name="thickness">Thickness of this segment.</param>
        public void DrawLine(Vector2 begin, Vector2 end, Color color, float thickness = 1f)
        {
            DrawLine(begin.X, begin.Y, end.X, end.Y, color, thickness);
        }
        /// <summary>
        /// Draws a segment from a point A to B.
        /// </summary>
        /// <param name="x1">X of the position where this line begins.</param>
        /// <param name="y1">Y of the position where this line begins.</param>
        /// <param name="x2">X of the position where this line ends.</param>
        /// <param name="y2">Y of the position where this line ends.</param>
        /// <param name="color">Color of this segment.</param>
        /// <param name="thickness">Thickness of this segment.</param>
        /// <exception cref="InvalidOperationException">Throws an exception if this method be called before Begin.</exception>
        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness = 1f)
        {
            if (!_begin)
                throw new InvalidOperationException("Begin must be called first.");

            AddLineVertices(x1, y1, x2, y2, color, thickness);
        }
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="x">
        /// <para>X position of this rectangle.</para>
        /// <para>The origin is at the top-left corner.</para>
        /// </param>
        /// <param name="y">
        /// <para>Y position of this rectangle.</para>
        /// <para>The origin is at the top-left corner.</para>
        /// </param>
        /// <param name="width">Width of this rectangle.</param>
        /// <param name="height">Height of this rectangle.</param>
        /// <param name="color">Color of this rectangle (fill and border).</param>
        /// <param name="fill">Defines if this rectangle should be filled or not.</param>
        /// <param name="thickness">Thickness of the border (ignored if <c>fill</c> is <c>true</c>).</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void DrawRectangle(float x, float y, float width, float height, Color color, bool fill = false, float thickness = 1f)
        {
            if (!_begin)
                throw new InvalidOperationException("Begin must be called first.");


            if (fill)
            {
                AddFilledRectangleVertices(x, y, width, height, color);
                return;
            }

            float halfThickness = thickness * 0.5f;
            AddLineVertices(x - halfThickness, y, x + width, y, color, thickness);
            AddLineVertices(x + width - halfThickness, y, x + width - halfThickness, y + height, color, thickness);
            AddLineVertices(x, y + height - halfThickness, x + width - halfThickness, y + height - halfThickness, color, thickness);
            AddLineVertices(x, y, x, y + height, color, thickness);
        }
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="rect">
        /// <para>Position and dimensions of this rectangle.</para>
        /// <para>The origin is at the top-left corner.</para>
        /// </param>
        /// <param name="color">Color of this rectangle (fill and border).</param>
        /// <param name="fill">Defines if this rectangle should be filled or not.</param>
        /// <param name="thickness">Thickness of the border (ignored if <c>fill</c> is <c>true</c>).</param>
        public void DrawRectangle(Rectangle rect, Color color, bool fill = false, float thickness = 1f)
        {
            DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, color, fill, thickness);
        }
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="pos">Position of this rectangle.</param>
        /// <param name="size">Size of this rectangle.</param>
        /// <param name="color">Color of this rectangle (fill and border).</param>
        /// <param name="fill">Defines if this rectangle should be filled or not.</param>
        /// <param name="thickness">Thickness of the border (ignored if <c>fill</c> is <c>true</c>).</param>
        public void DrawRectangle(Vector2 pos, Vector2 size, Color color, bool fill = false, float thickness = 1f)
        {
            DrawRectangle(pos.X, pos.Y, size.X, size.Y, color, fill, thickness);
        }
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="pos">Position of this rectangle.</param>
        /// <param name="size">Size of this rectangle.</param>
        /// <param name="color">Color of this rectangle (fill and border).</param>
        /// <param name="fill">Defines if this rectangle should be filled or not.</param>
        /// <param name="thickness">Thickness of the border (ignored if <c>fill</c> is <c>true</c>).</param>
        public void DrawRectangle(Point pos, Point size, Color color, bool fill = false, float thickness = 1f)
        {
            DrawRectangle(pos.X, pos.Y, size.X, size.Y, color, fill, thickness);
        }
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="x">X position of the center from this circle.</param>
        /// <param name="y">Y position of the center from this circle.</param>
        /// <param name="radius">Radius of this circle.</param>
        /// <param name="color">Color of this rectangle (fill and border).</param>
        /// <param name="fill">Defines if this rectangle should be filled or not.</param>
        /// <param name="thickness">Thickness of the border (ignored if <c>fill</c> is <c>true</c>).</param>
        /// <param name="segments">Amount of segments of this circle (the more greater the value more smooth will be
        /// the edges, but also more costly to be processed).
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        public void DrawCircle(float x, float y, float radius, Color color, bool fill = false, float thickness = 1f, int segments = 10)
        {
            if (!_begin)
                throw new InvalidOperationException("Begin must be called first.");

            if (segments < 3)
                segments = 3;

            float increment = 0f;
            int requiredVertices = 0;
            int requiredIndices = 0;
            int baseVertex = 0;
            if (fill)
            {
                increment = MathHelper.TwoPi / segments;

                requiredVertices = segments + 2;
                requiredIndices = segments * 3;


                if (_vertexCount + requiredIndices >= _maxVertices || _indexCount + requiredIndices >= _indices.Length)
                    Flush();

                baseVertex = _vertexCount;

                _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(x, y, 0f), color);

                for (int i = 0; i <= segments; i++)
                {
                    float angle = increment * i;

                    float dirX = MathF.Cos(angle);
                    float dirY = MathF.Sin(angle);

                    float posX = x + dirX * radius;
                    float posY = y + dirY * radius;

                    _vertices[_vertexCount++] = new VertexPositionColor(
                        new Vector3(posX, posY, 0f), color
                    );

                    if (i < segments)
                    {
                        _indices[_indexCount++] = (short)baseVertex;
                        _indices[_indexCount++] = (short)(baseVertex + i + 1);
                        _indices[_indexCount++] = (short)(baseVertex + i + 2);
                    }
                }
                return;
            }

            float halfThickness = thickness * 0.5f;
            increment = MathHelper.TwoPi / segments;

            requiredVertices = (segments + 1) * 2;
            requiredIndices = segments * 6;

            if (_vertexCount + requiredIndices >= _maxVertices || _indexCount + requiredIndices >= _indices.Length)
                Flush();

            baseVertex = _vertexCount;

            for (int i = 0; i <= segments; i++)
            {
                float angle = increment * i;

                float dirX = MathF.Cos(angle);
                float dirY = MathF.Sin(angle);

                float outerX = x + dirX * (radius + halfThickness);
                float outerY = y + dirY * (radius + halfThickness);

                float innerX = x + dirX * (radius - halfThickness);
                float innerY = y + dirY * (radius - halfThickness);

                int currentOuter = _vertexCount;
                int currentInner = _vertexCount + 1;

                _vertices[_vertexCount++] = new VertexPositionColor(
                    new Vector3(outerX, outerY, 0f), color
                );
                _vertices[_vertexCount++] = new VertexPositionColor(
                    new Vector3(innerX, innerY, 0f), color
                );

                // creating indices
                if (i > 0)
                {
                    int prevOuter = currentOuter - 2;
                    int prevInner = currentInner - 2;

                    _indices[_indexCount++] = (short)prevOuter;
                    _indices[_indexCount++] = (short)prevInner;
                    _indices[_indexCount++] = (short)currentOuter;

                    _indices[_indexCount++] = (short)prevInner;
                    _indices[_indexCount++] = (short)currentInner;
                    _indices[_indexCount++] = (short)currentOuter;
                }
            }
        }
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">Position of the center of this circle.</param>
        /// <param name="radius">Radius of this circle.</param>
        /// <param name="color">Color of this rectangle (fill and border).</param>
        /// <param name="fill">Defines if this rectangle should be filled or not.</param>
        /// <param name="thickness">Thickness of the border (ignored if <c>fill</c> is <c>true</c>).</param>
        /// <param name="segments">Amount of segments of this circle (the more greater the value more smooth will be
        /// the edges, but also more costly to be processed).
        /// </param>
        public void DrawCircle(Vector2 center, float radius, Color color, bool fill = false, float thickness = 1f, int segments = 10)
        {
            DrawCircle(center.X, center.Y, radius, color, fill, thickness, segments);
        }
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">Position of the center of this circle.</param>
        /// <param name="radius">Radius of this circle.</param>
        /// <param name="color">Color of this rectangle (fill and border).</param>
        /// <param name="fill">Defines if this rectangle should be filled or not.</param>
        /// <param name="thickness">Thickness of the border (ignored if <c>fill</c> is <c>true</c>).</param>
        /// <param name="segments">Amount of segments of this circle (the more greater the value more smooth will be
        /// the edges, but also more costly to be processed).
        /// </param>
        public void DrawCircle(Point center, float radius, Color color, bool fill = false, float thickness = 1f, int segments = 10)
        {
            DrawCircle(center.X, center.Y, radius, color, fill, thickness, segments);
        }
        /// <summary>
        /// <para>Draws a shape according with an array of points.</para>
        /// <para>This method only handles convex shapes.</para>
        /// </summary>
        /// <param name="points">Vertices of this shape.</param>
        /// <param name="color">Color of this shape</param>
        /// <param name="fill">Whether this shape should be filled or be just stroke.</param>
        /// <param name="thickness">Thickness of the stroke (ignored if <c>fill</c> is <c>true</c>).</param>
        public void DrawPolygon(Vector2[] points, Color color, bool fill = false, float thickness = 1f)
        {
            if (!_begin)
                throw new InvalidOperationException("Begin must be called first.");

            if (points == null || points.Length < 3)
                return;

            if (fill)
            {
                int vertexCount = points.Length;
                int requiredVertices = vertexCount;
                int requiredIndices = (vertexCount - 2) * 3;

                if (_vertexCount + requiredIndices >= _maxVertices || _indexCount + requiredIndices >= _indices.Length)
                {
                    Flush();
                }

                int baseVertex = _vertexCount;

                for (int i = 0; i < vertexCount; i++)
                {
                    _vertices[_vertexCount++] = new VertexPositionColor(new Vector3(points[i], 0f), color);

                    if (i > 0 && i < vertexCount - 1)
                    {
                        _indices[_indexCount++] = (short)baseVertex;
                        _indices[_indexCount++] = (short)(baseVertex + i);
                        _indices[_indexCount++] = (short)(baseVertex + i + 1);
                    }
                }
                return;
            }

            for (int i = 0; i < points.Length; i++)
            {
                float p0X = points[i].X;
                float p0Y = points[i].Y;
                float p1X = points[(i + 1) % points.Length].X;
                float p1Y = points[(i + 1) % points.Length].Y;

                DrawLine(p0X, p0Y, p1X, p1Y, color, thickness);
            }
        }
        public void Flush()
        {
            if (_vertexCount == 0 || _indexCount == 0)
                return;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertexCount,
                    _indices, 0, _indexCount / 3);
            }
            _vertexCount = 0;
            _indexCount = 0;
        }
        /// <summary>
        /// Begins to batch vertex data.
        /// </summary>
        /// <param name="projection">The projection matrix to be used by the default <seealso cref="BasicEffect"/>
        /// being used.</param>
        /// <param name="view">The view matrix to be used by the default <seealso cref="BasicEffect"/>
        /// being used.</param>
        public void Begin(Matrix projection, Matrix view)
        {
            _device.BlendState = BlendState.Opaque;
            _device.DepthStencilState = DepthStencilState.Default;
            _device.RasterizerState = RasterizerState.CullNone;
            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = projection;
            _effect.VertexColorEnabled = true;
            _begin = true;
        }
        /// <summary>
        /// Begins to batch vertex data.
        /// </summary>
        /// <param name="transformMatrix">Despite of the name, by updating this parameter the view
        /// matrix is what will receive this value.</param>
        public void Begin(Matrix? transformMatrix = null)
        {
            Viewport vp = _device.Viewport;
            Matrix proj = Matrix.CreateOrthographicOffCenter(0f, vp.Width, vp.Height, 0f, -1f, 100f);
            Begin(proj, transformMatrix ?? Matrix.Identity);
        }
        /// <summary>
        /// Ends the batching of vertex and flush them to draw.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void End()
        {
            if (!_begin)
                throw new InvalidOperationException("Begin must be called first.");

            Flush();
            _begin = false;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _effect.Dispose();
                if (Disposing != null)
                    Disposing();
            }

            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
