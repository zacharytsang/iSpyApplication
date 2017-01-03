using AForge.Video.Kinect;

namespace iSpyApplication
{
    public class Enums
    {
        public enum AlertMode { Movement, NoMovement };

        public enum PtzCommand
        {
            Center,
            Left,
            Upleft,
            Up,
            UpRight,
            Right,
            DownRight,
            Down,
            DownLeft,
            ZoomIn,
            ZoomOut,
            Stop
        } ;

        public static readonly LedColorOption[] Ledmode =
        {
           LedColorOption.Off, LedColorOption.Green, LedColorOption.Red,
           LedColorOption.Yellow, LedColorOption.BlinkGreen, LedColorOption.BlinkRedYellow
        };

        public enum MatchMode
        {
            IsInList = 0,
            NotInList = 1
        } ;

        public enum EOcrEngineMode: int
        {
            TesseractOnly = 0,
            CubeOnly = 1,
            TesseractCubeCombined = 2,
            Default = 3
        }
    }
}
