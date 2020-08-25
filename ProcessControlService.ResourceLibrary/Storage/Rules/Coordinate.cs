// ==================================================
// 文件名：Coordinate.cs
// 创建时间：2020/05/22 9:55
// ==================================================
// 最后修改于：2020/05/22 9:55
// 修改人：jians
// ==================================================

using System;

namespace ProcessControlService.ResourceLibrary.Storage
{
    public class Coordinate
    {
        public int X { get; set; } = -1;

        public int Y { get; set; } = -1;

        public int Z { get; set; } = -1;

        public bool Viable => X != -1;

        public void Init(string strInit)
        {
            var sp = strInit.Split(',');

            if (sp.Length > 0)
            {
                X = Convert.ToInt32(sp[0]);
            }

            if (sp.Length > 1)
            {
                Y = Convert.ToInt32(sp[1]);
            }

            if (sp.Length > 2)
            {
                Z = Convert.ToInt32(sp[2]);
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is Coordinate other)
            {
                return other.X == X && other.Y == Y && other.Z == Z;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// string as X,Y,Z
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{X},{Y},{Z}";
        }
    }
}