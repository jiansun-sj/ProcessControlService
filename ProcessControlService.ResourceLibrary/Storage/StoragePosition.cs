// ==================================================
// 文件名：StoragePosition.cs
// 创建时间：2020/01/02 13:52
// ==================================================
// 最后修改于：2020/05/21 13:52
// 修改人：jians
// ==================================================

using log4net;

namespace ProcessControlService.ResourceLibrary.Storage
{
    /// <summary>
    ///     存储坐标对象
    ///     Dongmin 20180811
    /// </summary>
    public class StoragePosition
    {
        public const int MaxDiemension = 3; // 目前最大是3维
        private static readonly ILog LOG = LogManager.GetLogger(typeof(StoragePosition));

        private readonly short[] _pos = new short[MaxDiemension]; // 0 - X, 1 - Y, 2 - Z

        public StoragePosition(short PosX)
        {
            DimensionCount = 1;

            _pos[0] = PosX;
        }

        public StoragePosition(short PosX, short PosY)
        {
            DimensionCount = 2;

            _pos[0] = PosX;
            _pos[1] = PosY;
        }

        public StoragePosition(short PosX, short PosY, short PosZ)
        {
            DimensionCount = 3;

            _pos[0] = PosX;
            _pos[1] = PosY;
            _pos[2] = PosZ;
        }

        public short DimensionCount { get; } = -1;

        public short GetDiemensionValue(StoragePositionDimension Diemension)
        {
            if (Diemension == StoragePositionDimension.X)
                return _pos[0];
            if (Diemension == StoragePositionDimension.Y)
                return _pos[1];
            if (Diemension == StoragePositionDimension.Z) return _pos[2];

            return -1;
        }

        public short GetDiemensionValue()
        {
            if (DimensionCount == 1)
                return GetDiemensionValue(StoragePositionDimension.X);
            return -1;
        }
    }

    public enum StoragePositionDimension
    {
        Unknown = 0,
        X = 1,
        Y = 2,
        Z = 3
    }
}