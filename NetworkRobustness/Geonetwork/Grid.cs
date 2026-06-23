using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkRobustness.Geonetwork
{
    public class Grid
    {
        public float Xo = 0;//Top-left conner X of the fixed grid
        public float Yo = 0;//Top-left conner Y of the fixed grid
        public float Xm = 0;//Bottom-right conner X of the fixed grid
        public float Ym = 0;//Bottom-right conner Y of the fixed grid
        public float R = 0;//The width of a cell. This value is fixed
        /// <summary>
        /// Create a grid 
        /// </summary>
        /// <param name="Xo">Top-left conner X of the fixed grid.</param>
        /// <param name="Yo">Top-left conner Y of the fixed grid.</param>
        /// <param name="Xm">Bottom-right conner X of the fixed grid.</param>
        /// <param name="Ym">Bottom-right conner Y of the fixed grid.</param>
        /// <param name="R">The width of a cell. This value is fixed</param>
        public Grid(float Xo, float Yo, float Xm, float Ym, float R)
        {
            this.Xo = Xo;
            this.Yo = Yo;
            this.Xm = Xm;
            this.Ym = Ym;
            this.R = R;

        }
        /// <summary>
        /// Convert GPS taxi location to the taxi location on the grid
        /// </summary>
        /// <param name="X">Taxi location X</param>
        /// <param name="Y">Taxi location Y</param>
        /// <returns>The integer value with domain = [0..n] indicated the taxi location on the grid</returns>
        public int ConvertGPStoGrid(float X, float Y)
        {
            int m = Convert.ToInt32(Math.Ceiling((Xm - Xo) / R));// The column No. of the grid
            //int n = Convert.ToInt32(Math.Ceiling((Ym - Yo) / R)); // The row No. of the grid 
            int i = Convert.ToInt32(Math.Floor((X - Xo) / R)); // Column position of the taxi on the grid
            int j = Convert.ToInt32(Math.Floor((Y - Yo) / R)); // Row position  of the taxi on the grid
            return j * m + i;

        }
        /// <summary>
        /// Convert the taxi location on the grid to GPS taxi location
        /// </summary>
        /// <param name="iGrid">taxi location on the grid</param>
        /// <param name="X">X-GPS position of the top-left conner of the cell where taxi is</param>
        /// <param name="Y">Y-GPS position of the top-left conner of the cell where taxi is</param>
        /// <param name="size">width and height of the cell where taxi is</param>
        /// <returns>GPS taxi location as a pair of X(First), Y(Second)</returns>
        public void ConvertGridtoGPS(int iGrid, ref float X, ref float Y, ref float size)
        {


            int m = Convert.ToInt32(Math.Ceiling((Xm - Xo) / R));// The column No. of the grid
            //int n = Convert.ToInt32(Math.Ceiling((Ym - Yo) / R)); // The row No. of the grid 

            int i = iGrid % m; // Column position of the taxi on the grid
            int j = iGrid / m; // Row position  of the taxi on the grid

            X = i * R + Xo;
            Y = j * R + Yo;
            size = R;

        }
    }
}
