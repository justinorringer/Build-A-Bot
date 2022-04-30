using UnityEngine;

namespace BuildABot
{
    public static class Extensions
    {
        
        #region Vector2

        /**
         * Rotates this vector by the provided angle in degrees.
         * <param name="vector">The vector to rotate.</param>
         * <param name="angle">The angle in degrees to rotate by.</param>
         * <returns>The rotated vector.</returns>
         */
        public static Vector2 Rotate(this Vector2 vector, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vector;
        }
        
        #endregion
        
        #region char
        
        /**
         * Returns true if this character is an uppercase letter.
         * <param name="c">This char.</param>
         * <returns>True if this character is uppercase.</returns>
         */
        public static bool IsUpper(this char c)
        {
            return c >= 'A' && c <= 'Z';
        }
             
        /**
         * Returns true if this character is a lowercase letter.
         * <param name="c">This char.</param>
         * <returns>True if this character is lowercase.</returns>
         */   
        public static bool IsLower(this char c)
        {
            return c >= 'a' && c <= 'z';
        }

        /**
         * Returns true if this character is a number.
         * <param name="c">This char.</param>
         * <returns>True if this character is a number.</returns>
         */
        public static bool IsNumber(this char c)
        {
            return c >= '0' && c <= '9';
        }

        /**
         * Returns the uppercase version of this character if it is lowercase. Otherwise returns this character.
         * <param name="c">This char.</param>
         * <returns>The uppercase version of this char or this char if it is not lowercase.</returns>
         */
        public static char ToUpper(this char c)
        {
            return IsLower(c) ? (char) (c - 'a' + 'A') : c;
        }

        /**
         * Returns the lowercase version of this character if it is uppercase. Otherwise returns this character.
         * <param name="c">This char.</param>
         * <returns>The lowercase version of this char or this char if it is not uppercase.</returns>
         */
        public static char ToLower(this char c)
        {
            return IsUpper(c) ? (char) (c - 'A' + 'a') : c;
        }
        
        #endregion
        
        #region Color

        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
        
        #endregion
        
    }
}