using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Superorganism.ScreenManagement;
using System;
using System.Collections.Generic;

namespace Superorganism.Graphics
{
    public class PixelTextRenderer
    {
        private readonly Game _game;
        private VertexBuffer _vertices;
        private IndexBuffer _indices;
        private BasicEffect _effect;
        private readonly string _text;
        private const int GridSize = 5; // 5x5 grid for each letter
        private const float VoxelSize = 0.1f; // Size of each voxel cube
        private const float LetterSpacing = 0.125f; // Space between letters
        private const int VerticesPerCube = 24;
        private const int IndicesPerCube = 60;
        private float _transitionOffset;

        // Animation parameters with more subtle values
        private float _hoverOffset;
        private const float HoverSpeed = 1.3f;         // Slower up/down movement
        private const float HoverAmplitude = 0.0125f;    // Smaller up/down range
        private float _tiltAngle;         // Maximum rotation angle in radians (about 3 degrees)
        private const float TiltSpeed = 1.15f;          // Speed of tilt animation
        private const float TiltAmplitude = 0.01f;     // Maximum tilt angle in radians (about 2.3 degrees)


        // Define pixel blocks for each letter in a 5x5 grid
        private static readonly Dictionary<char, bool[,]> LetterShapes = new()
        {
            ['A'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, true},
            {true, true, true, true, true},
            {true, false, false, false, true},
            {true, false, false, false, true}
        },
            ['B'] = new[,] {
            {true, true, true, true, false},
            {true, false, false, false, true},
            {true, true, true, true, false},
            {true, false, false, false, true},
            {true, true, true, true, false}
        },
            ['C'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, false},
            {true, false, false, false, false},
            {true, false, false, false, false},
            {true, true, true, true, true}
        },
            ['D'] = new[,] {
            {true, true, true, true, false},
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, true, true, true, false}
        },
            ['E'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, false},
            {true, true, true, true, false},
            {true, false, false, false, false},
            {true, true, true, true, true}
        },
            ['F'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, false},
            {true, true, true, true, false},
            {true, false, false, false, false},
            {true, false, false, false, false}
        },
            ['G'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, false},
            {true, false, true, true, true},
            {true, false, false, false, true},
            {true, true, true, true, true}
        },
            ['H'] = new[,] {
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, true, true, true, true},
            {true, false, false, false, true},
            {true, false, false, false, true}
        },
            ['I'] = new[,] {
            {true, true, true, true, true},
            {false, false, true, false, false},
            {false, false, true, false, false},
            {false, false, true, false, false},
            {true, true, true, true, true}
        },
            ['J'] = new[,] {
            {false, false, false, false, true},
            {false, false, false, false, true},
            {false, false, false, false, true},
            {true, false, false, false, true},
            {true, true, true, true, true}
        },
            ['K'] = new[,] {
            {true, false, false, false, true},
            {true, false, false, true, false},
            {true, true, true, false, false},
            {true, false, false, true, false},
            {true, false, false, false, true}
        },
            ['L'] = new[,] {
            {true, false, false, false, false},
            {true, false, false, false, false},
            {true, false, false, false, false},
            {true, false, false, false, false},
            {true, true, true, true, true}
        },
            ['M'] = new[,] {
            {true, false, false, false, true},
            {true, true, false, true, true},
            {true, false, true, false, true},
            {true, false, false, false, true},
            {true, false, false, false, true}
        },
            ['N'] = new[,] {
            {true, false, false, false, true},
            {true, true, false, false, true},
            {true, false, true, false, true},
            {true, false, false, true, true},
            {true, false, false, false, true}
        },
            ['O'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, true, true, true, true}
        },
            ['P'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, true},
            {true, true, true, true, true},
            {true, false, false, false, false},
            {true, false, false, false, false}
        },
            ['Q'] = new[,] {
            {true, true, true, true, false},
            {true, false, false, true, false},
            {true, false, false, true, false},
            {true, false, false, true, false},
            {true, true, true, true, true}
        },
            ['R'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, true},
            {true, true, true, true, true},
            {true, false, false, true, false},
            {true, false, false, false, true}
        },
            ['S'] = new[,] {
            {true, true, true, true, true},
            {true, false, false, false, false},
            {true, true, true, true, true},
            {false, false, false, false, true},
            {true, true, true, true, true}
        },
            ['T'] = new[,] {
            {true, true, true, true, true},
            {false, false, true, false, false},
            {false, false, true, false, false},
            {false, false, true, false, false},
            {false, false, true, false, false}
        },
            ['U'] = new[,] {
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, true, true, true, true}
        },
            ['V'] = new[,] {
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, false, false, false, true},
            {false, true, false, true, false},
            {false, false, true, false, false}
        },
            ['W'] = new[,] {
            {true, false, false, false, true},
            {true, false, false, false, true},
            {true, false, true, false, true},
            {true, true, false, true, true},
            {true, false, false, false, true}
        },
            ['X'] = new[,] {
            {true, false, false, false, true},
            {false, true, false, true, false},
            {false, false, true, false, false},
            {false, true, false, true, false},
            {true, false, false, false, true}
        },
            ['Y'] = new[,] {
            {true, false, false, false, true},
            {false, true, false, true, false},
            {false, false, true, false, false},
            {false, false, true, false, false},
            {false, false, true, false, false}
        },
            ['Z'] = new[,] {
            {true, true, true, true, true},
            {false, false, false, true, false},
            {false, false, true, false, false},
            {false, true, false, false, false},
            {true, true, true, true, true}
        },
            [' '] = new[,] {
            {false, false, false, false, false},
            {false, false, false, false, false},
            {false, false, false, false, false},
            {false, false, false, false, false},
            {false, false, false, false, false}
        }
        };

        public PixelTextRenderer(Game game, string text)
        {
            _game = game;
            _text = text.ToUpper();
            InitializeGeometry();
            InitializeEffect();
        }

        private void CreateLetterGeometry(int letterIndex, VertexPositionColor[] vertices, short[] indices, ref int vertexCount, ref int indexCount)
        {
            // Guard against index out of range
            if (letterIndex >= _text.Length) return;

            char letter = _text[letterIndex];
            if (!LetterShapes.TryGetValue(letter, out bool[,] shape)) return;

            // Calculate the total width of all letters (including spaces)
            float totalLetterWidth = 0;
            for (int i = 0; i < _text.Length; i++)
            {
                char currentLetter = _text[i];
                if (LetterShapes.ContainsKey(currentLetter))
                {
                    // Add width for each letter (assuming all letters use the full grid width)
                    totalLetterWidth += GridSize * VoxelSize;

                    // Add letter spacing if not the last letter
                    if (i < _text.Length - 1)
                    {
                        totalLetterWidth += LetterSpacing;
                    }
                }
            }

            // Calculate the starting X position for the entire text
            // This ensures the text is centered as a whole
            float startX = -totalLetterWidth / 2f;

            // Calculate position for this specific letter
            float currentX = startX;
            for (int i = 0; i < letterIndex; i++)
            {
                // Move right by letter width and spacing for each previous letter
                currentX += GridSize * VoxelSize + LetterSpacing;
            }

            // Now create the geometry for this letter at the calculated position
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    if (!shape[y, x]) continue;

                    bool hasTop = y > 0 && shape[y - 1, x];
                    bool hasBottom = y < GridSize - 1 && shape[y + 1, x];
                    bool hasLeft = x > 0 && shape[y, x - 1];
                    bool hasRight = x < GridSize - 1 && shape[y, x + 1];

                    // Calculate the position of this voxel
                    float xPos = currentX + x * VoxelSize;
                    float yPos = (GridSize / 2f - y) * VoxelSize;

                    bool[] neighborInfo = [hasTop, hasBottom, hasLeft, hasRight];

                    if (vertexCount + VerticesPerCube > vertices.Length ||
                        indexCount + IndicesPerCube > indices.Length)
                    {
                        return;
                    }

                    CreateVoxel(
                        new Vector3(xPos, yPos, 0),
                        VoxelSize,
                        vertices,
                        indices,
                        vertexCount,
                        indexCount,
                        neighborInfo
                    );

                    vertexCount += VerticesPerCube;
                    indexCount += IndicesPerCube;
                }
            }
        }

        private void CreateVoxel(Vector3 position, float size,
            VertexPositionColor[] vertices, short[] indices,
            int vertexOffset, int indexOffset,
            bool[] neighbors) // [top, bottom, left, right]
        {
            // Bounds checking
            if (vertexOffset + VerticesPerCube > vertices.Length || indexOffset + IndicesPerCube > indices.Length)
            {
                System.Diagnostics.Debug.WriteLine($"Buffer overflow! VO: {vertexOffset}, IO: {indexOffset}");
                return;
            }

            Vector3 halfSize = new(size / 2);
            Color fillColor = Color.White;
            Color outlineColor = Color.Black;
            float outlineThickness = size * 0.55f; // Thickness of the outline

            // Create the main white cube vertices
            vertices[vertexOffset + 0] = new VertexPositionColor(position + new Vector3(-halfSize.X, halfSize.Y, -halfSize.Z), fillColor);
            vertices[vertexOffset + 1] = new VertexPositionColor(position + new Vector3(halfSize.X, halfSize.Y, -halfSize.Z), fillColor);
            vertices[vertexOffset + 2] = new VertexPositionColor(position + new Vector3(-halfSize.X, -halfSize.Y, -halfSize.Z), fillColor);
            vertices[vertexOffset + 3] = new VertexPositionColor(position + new Vector3(halfSize.X, -halfSize.Y, -halfSize.Z), fillColor);
            vertices[vertexOffset + 4] = new VertexPositionColor(position + new Vector3(-halfSize.X, halfSize.Y, halfSize.Z), fillColor);
            vertices[vertexOffset + 5] = new VertexPositionColor(position + new Vector3(halfSize.X, halfSize.Y, halfSize.Z), fillColor);
            vertices[vertexOffset + 6] = new VertexPositionColor(position + new Vector3(-halfSize.X, -halfSize.Y, halfSize.Z), fillColor);
            vertices[vertexOffset + 7] = new VertexPositionColor(position + new Vector3(halfSize.X, -halfSize.Y, halfSize.Z), fillColor);

            int outlineOffset = vertexOffset + 8;
            int currentOutlineVertex = 0;

            // Helper function to add outline edge vertices
            void AddOutlineEdge(Vector3 start, Vector3 end, Vector3 normal)
            {
                Vector3 outlineShift = normal * outlineThickness;

                // Inner vertices (on the cube face)
                vertices[outlineOffset + currentOutlineVertex] = new VertexPositionColor(start, outlineColor);
                vertices[outlineOffset + currentOutlineVertex + 1] = new VertexPositionColor(end, outlineColor);

                // Outer vertices (offset by outline thickness)
                vertices[outlineOffset + currentOutlineVertex + 2] = new VertexPositionColor(start + outlineShift, outlineColor);
                vertices[outlineOffset + currentOutlineVertex + 3] = new VertexPositionColor(end + outlineShift, outlineColor);

                currentOutlineVertex += 4;
            }

            // Create outline vertices for exposed edges
            if (!neighbors[0]) // Top edge
            {
                Vector3 normal = Vector3.Up;
                AddOutlineEdge(
                    position + new Vector3(-halfSize.X, halfSize.Y, 0),
                    position + new Vector3(halfSize.X, halfSize.Y, 0),
                    normal
                );
            }

            if (!neighbors[1]) // Bottom edge
            {
                Vector3 normal = Vector3.Down;
                AddOutlineEdge(
                    position + new Vector3(-halfSize.X, -halfSize.Y, 0),
                    position + new Vector3(halfSize.X, -halfSize.Y, 0),
                    normal
                );
            }

            if (!neighbors[2]) // Left edge
            {
                Vector3 normal = Vector3.Left;
                AddOutlineEdge(
                    position + new Vector3(-halfSize.X, -halfSize.Y, 0),
                    position + new Vector3(-halfSize.X, halfSize.Y, 0),
                    normal
                );
            }

            if (!neighbors[3]) // Right edge
            {
                Vector3 normal = Vector3.Right;
                AddOutlineEdge(
                    position + new Vector3(halfSize.X, -halfSize.Y, 0),
                    position + new Vector3(halfSize.X, halfSize.Y, 0),
                    normal
                );
            }

            // Add indices for the white fill cube
            int[] fillIndices =
            [
                // Front face
                0, 1, 2, 2, 1, 3,
                // Back face
                5, 4, 7, 7, 4, 6,
                // Left face
                4, 0, 6, 6, 0, 2,
                // Right face
                1, 5, 3, 3, 5, 7,
                // Top face
                4, 5, 0, 0, 5, 1,
                // Bottom face
                2, 3, 6, 6, 3, 7
            ];

            for (int i = 0; i < fillIndices.Length; i++)
            {
                indices[indexOffset + i] = (short)(fillIndices[i] + vertexOffset);
            }

            // Add indices for outlines
            int currentIndex = indexOffset + 36;

            // Helper function to add outline quad indices
            void AddOutlineQuad(int baseVertex)
            {
                // First triangle
                indices[currentIndex++] = (short)(baseVertex);
                indices[currentIndex++] = (short)(baseVertex + 1);
                indices[currentIndex++] = (short)(baseVertex + 2);

                // Second triangle
                indices[currentIndex++] = (short)(baseVertex + 1);
                indices[currentIndex++] = (short)(baseVertex + 3);
                indices[currentIndex++] = (short)(baseVertex + 2);
            }

            // Add indices for each outline edge
            int outlineVertexBase = outlineOffset;
            if (!neighbors[0]) // Top edge
            {
                AddOutlineQuad(outlineVertexBase);
                outlineVertexBase += 4;
            }
            if (!neighbors[1]) // Bottom edge
            {
                AddOutlineQuad(outlineVertexBase);
                outlineVertexBase += 4;
            }
            if (!neighbors[2]) // Left edge
            {
                AddOutlineQuad(outlineVertexBase);
                outlineVertexBase += 4;
            }
            if (!neighbors[3]) // Right edge
            {
                AddOutlineQuad(outlineVertexBase);
            }
        }

        private void InitializeGeometry()
        {
            if (string.IsNullOrEmpty(_text)) return;  // Guard against empty text

            // Count total active pixels
            int totalPixels = 0;
            foreach (char letter in _text)
            {
                if (!LetterShapes.ContainsKey(letter)) continue;

                bool[,] shape = LetterShapes[letter];
                for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    if (shape[y, x]) totalPixels++;
            }

            // Calculate buffer sizes
            int vertexCount = totalPixels * VerticesPerCube;
            int indexCount = totalPixels * IndicesPerCube;

            VertexPositionColor[] vertices = new VertexPositionColor[vertexCount];
            short[] indices = new short[indexCount];

            int currentVertexOffset = 0;
            int currentIndexOffset = 0;

            // Generate geometry for each letter
            for (int letterIndex = 0; letterIndex < _text.Length; letterIndex++)
            {
                CreateLetterGeometry(letterIndex, vertices, indices, ref currentVertexOffset, ref currentIndexOffset);
            }

            // Create and set buffers
            if (vertices.Length > 0)
            {
                _vertices = new VertexBuffer(_game.GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.None);
                _vertices.SetData(vertices);

                _indices = new IndexBuffer(_game.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
                _indices.SetData(indices);
            }
        }

        private void InitializeEffect()
        {
            _effect = new BasicEffect(_game.GraphicsDevice);
            _effect.World = Matrix.Identity;
            _effect.View = Matrix.CreateLookAt(
                new Vector3(0, -12, 12),    // Camera position
                new Vector3(0, -11, 0),     // Look-at point
                Vector3.Up
            );

            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                _game.GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                100.0f
            );
            _effect.VertexColorEnabled = true;
        }

        public void Update(GameTime gameTime, float transitionPosition, ScreenState screenState)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            // Calculate hover (up/down) movement
            _hoverOffset = (float)Math.Sin(time * HoverSpeed) * HoverAmplitude;

            // Calculate smooth tilt angle
            _tiltAngle = (float)Math.Sin(time * TiltSpeed) * TiltAmplitude;

            // Calculate transition offset with reduced distance
            float baseTransitionOffset = (float)Math.Pow(transitionPosition, 2);
            switch (screenState)
            {
                case ScreenState.TransitionOn:
                    _transitionOffset = -baseTransitionOffset * 1f; 
                    break;
                case ScreenState.TransitionOff:
                    _transitionOffset = baseTransitionOffset * 1f;
                    break;
                default:
                    _transitionOffset = 0f;
                    break;
            }

            // Create transformation matrices
            Matrix translation = Matrix.CreateTranslation(new Vector3(_transitionOffset, -7 + _hoverOffset, 0));
            Matrix tilt = Matrix.CreateRotationZ(_tiltAngle);

            // Combine transformations
            _effect.World = tilt * translation;
        }

        public void Draw()
        {
            if (_vertices == null || _indices == null) return;  // Guard against null buffers

            GraphicsDevice device = _game.GraphicsDevice;

            RasterizerState oldState = device.RasterizerState;
            RasterizerState rasterizerState = new()
            {
                CullMode = CullMode.None
            };
            device.RasterizerState = rasterizerState;

            _effect.CurrentTechnique.Passes[0].Apply();
            device.SetVertexBuffer(_vertices);
            device.Indices = _indices;
            device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                _indices.IndexCount / 3
            );

            device.RasterizerState = oldState;
        }
    }
}