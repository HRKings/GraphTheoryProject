using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Graphs
{
    public class GraphButton
    {
        public readonly string Name;
        public bool IsActive;
        public Rectangle Area;
        public Color Colour;
        public Color Highlight;
        public Color Text;
        public Func<bool> OnPress;

        public GraphButton(string name, int x, int y, Vector2 textSize, Color cor, Color active, Color text)
        {
            this.Name = name;
            this.IsActive = false;
            (float tX, float ty) = textSize;
            this.Area = new Rectangle(x, y - (int)ty, (int)tX+10, (int)ty);
            this.Colour = cor;
            this.Highlight = active;
            this.Text = text;
        }

        public void Toggle()
        {
            this.IsActive = !this.IsActive;
        }
    }
    public static class Utils
    {
        public static Dictionary<string, GraphButton> GraphButtons;
        
        public static void DrawLine(Texture2D pixel, SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color,
            int width = 1)
        {
            var r = new Rectangle((int) begin.X, (int) begin.Y, (int) (end - begin).Length() + width, width);
            var v = Vector2.Normalize(begin - end);
            var angle = (float) Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;

            spriteBatch.Draw(pixel, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
        
        public static void UpdateButtons(Point mousePosition, bool isLeftButtonPressed)
        {
            foreach ((string key, _) in GraphButtons.Where(button => button.Value.Area.Contains(mousePosition)))
            {
                if(!isLeftButtonPressed) continue;
                
                GraphButtons[key].Toggle();
                
                if(GraphButtons[key].OnPress != null)
                    GraphButtons[key].OnPress();
            }
        }

        public static void DrawButtons(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
        {
            foreach (var button in GraphButtons.Values)
            {
                spriteBatch.Draw(pixel, new Vector2(button.Area.X, button.Area.Y), button.Area, button.IsActive ? button.Highlight : button.Colour);
                spriteBatch.DrawString(font, button.Name, new Vector2(button.Area.X+5, button.Area.Y), button.Text);
            }
        }

        public static void DrawLineString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 begin, Vector2 end, Color color, float scale = 1f)
        {
            var v = Vector2.Normalize(begin - end);
            var angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            var spriteEffects = SpriteEffects.None;
            if (begin.Y > end.Y)
            {
                angle = MathHelper.TwoPi - angle;
                spriteEffects = SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
            }

            float posX = ((begin.X + end.X) / 2);
            float posY = ((begin.Y + end.Y) / 2);
            spriteBatch.DrawString(font, text, new Vector2(posX, posY), color, angle, Vector2.Zero, scale,
                spriteEffects, 1f);
        }
    }
}