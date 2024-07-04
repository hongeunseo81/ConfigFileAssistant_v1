using System;
using System.Collections.Generic;
using System.IO;

using CoPick.Logging;
using CoPick.Robot;
using CoPick.Setting;

namespace CalibrationTool
{
    public class Config
    {
        public Dictionary<string, Dictionary<RobotAttribute, string>> RobotConfigs { get; set; } = new Dictionary<string, Dictionary<RobotAttribute, string>>();
        public Dictionary<string, List<Dictionary<CameraAttribute, string>>> CameraConfigs { get; set; } = new Dictionary<string, List<Dictionary<CameraAttribute, string>>>();
        public Dictionary<string, List<Dictionary<CameraAttribute, string>>> CameraConfigs2 { get; set; } = new Dictionary<string, List<Dictionary<CameraAttribute, string>>>();

        public string Camera { get; set; }
        public string InstallRobot { get; set; }
        public string ScanRobot { get; set; }
        public CalibrationMode Mode { get; set; }
        private string _logPath = "./log";


        public string LogPath
        {
            get
            {
                return _logPath;
            }

            set
            {
                try
                {
                    Path.GetFullPath(value);
                    _logPath = value;
                }
                catch (Exception)
                {
                }
            }
        }

        public LogLevel MinimumFileLogLevel { get; set; }
        public LogLevel MinimumUiLogLevel { get; set; }
        public string Language { get; set; } = "ko-KR";
    }

    public static class DefaultSettingLoader
    {
        public static Dictionary<RobotMaker, Func<Dictionary<RobotAttribute, string>>> Robots = new Dictionary<RobotMaker, Func<Dictionary<RobotAttribute, string>>>()
        {
            [RobotMaker.YASKAWA] = GetYaskawaSettings,
            [RobotMaker.KAWASAKI] = GetKawasakiSettings,
            [RobotMaker.FANUC] = GetFanucSettings,
            [RobotMaker.HYUNDAI] = GetHyundaiSettings,
        };

        public static Dictionary<RobotAttribute, string> GetYaskawaSettings()
        {
            return new Dictionary<RobotAttribute, string>()
            {
                [RobotAttribute.Maker] = RobotMaker.YASKAWA.ToString(),
                [RobotAttribute.Ip] = "",
                [RobotAttribute.Port] = "",
                [RobotAttribute.YrcCoordinateSystem] = "BASE",
                [RobotAttribute.CalibrationRcVar] = "",
                [RobotAttribute.CalibrationVcVar] = "",
                [RobotAttribute.CalibrationFinVar] = "",
                [RobotAttribute.CalibrationJobNames] = "",
                [RobotAttribute.CalibrationUpdateJobNames] = ""
            };
        }

        public static Dictionary<RobotAttribute, string> GetKawasakiSettings()
        {
            return new Dictionary<RobotAttribute, string>()
            {
                [RobotAttribute.Maker] = RobotMaker.KAWASAKI.ToString(),
                [RobotAttribute.Ip] = "",
                [RobotAttribute.Port] = "",
                [RobotAttribute.MaxNumberOfTrials] = "",
                [RobotAttribute.CalibrationRcVar] = "",
                [RobotAttribute.CalibrationVcVar] = "",
                [RobotAttribute.CalibrationFinVar] = "",
                [RobotAttribute.CalibrationJobNames] = "",
                [RobotAttribute.CalibrationUpdateJobNames] = ""
            };
        }

        public static Dictionary<RobotAttribute, string> GetFanucSettings()
        {
            return new Dictionary<RobotAttribute, string>()
            {
                [RobotAttribute.Maker] = RobotMaker.FANUC.ToString(),
                [RobotAttribute.Ip] = "",
                [RobotAttribute.Port] = "",
                [RobotAttribute.UserFrame] = "WORLD",
                [RobotAttribute.CalibrationRcVar] = "",
                [RobotAttribute.CalibrationVcVar] = "",
                [RobotAttribute.CalibrationFinVar] = "",
                [RobotAttribute.CalibrationJobNames] = "",
                [RobotAttribute.CalibrationUpdateJobNames] = ""
            };
        }

        public static Dictionary<RobotAttribute, string> GetHyundaiSettings()
        {
            return new Dictionary<RobotAttribute, string>()
            {
                [RobotAttribute.Maker] = RobotMaker.HYUNDAI.ToString(),
                [RobotAttribute.Ip] = "192.168.178.206",
                [RobotAttribute.ClientIp] = "192.168.178.102",
                [RobotAttribute.HrCoordinateSystem] = "BASE",
                [RobotAttribute.CalibrationRcVar] = "",
                [RobotAttribute.CalibrationVcVar] = "",
                [RobotAttribute.CalibrationFinVar] = "",
                [RobotAttribute.CalibrationJobNames] = "",
                [RobotAttribute.CalibrationUpdateJobNames] = ""
            };
        }

        public static Dictionary<CameraModel, Func<Dictionary<CameraAttribute, string>>> Cameras = new Dictionary<CameraModel, Func<Dictionary<CameraAttribute, string>>>()
        {
            [CameraModel.CoPick3D_250] = GetCoPick3D250Settings,
            [CameraModel.CoPick3D_350] = GetCoPick3D350Settings,
            [CameraModel.Phoxi_M] = GetPhoxiMSettings,
            [CameraModel.Phoxi_S] = GetPhoxiSSettings,
        };

        private static Dictionary<CameraAttribute, string> GetPhoxiSSettings()
        {
            return new Dictionary<CameraAttribute, string>()
            {
                [CameraAttribute.Model] = CameraModel.Phoxi_S.ToString(),
                [CameraAttribute.Serial] = "",
                [CameraAttribute.Ip] = "",
                [CameraAttribute.CalibrationDataPath] = "./"
            };
        }

        private static Dictionary<CameraAttribute, string> GetPhoxiMSettings()
        {
            return new Dictionary<CameraAttribute, string>()
            {
                [CameraAttribute.Model] = CameraModel.Phoxi_M.ToString(),
                [CameraAttribute.Serial] = "",
                [CameraAttribute.Ip] = "",
                [CameraAttribute.CalibrationDataPath] = "./"
            };
        }

        private static Dictionary<CameraAttribute, string> GetCoPick3D350Settings()
        {
            return new Dictionary<CameraAttribute, string>()
            {
                [CameraAttribute.Model] = CameraModel.CoPick3D_350.ToString(),
                [CameraAttribute.Serial] = "",
                [CameraAttribute.Alias] = "",
                [CameraAttribute.Ip] = "",
                [CameraAttribute.ScanMode] = CameraScanMode.MultiCamera.ToString(),
                [CameraAttribute.OutputResolution] = OutputResolution.W1224xH1024.ToString(),
                [CameraAttribute.IsolationDistance] = "1.0",
                [CameraAttribute.IsolationMinNeighbors] = "10",
                [CameraAttribute.SendNormalMap] = "False",
                [CameraAttribute.TextureExposureMultiplier] = "1",
                [CameraAttribute.TextureExposure1] = "16.0",
                [CameraAttribute.TextureExposure2] = "16.0",
                [CameraAttribute.TextureExposure3] = "16.0",
                [CameraAttribute.TextureGain1] = "5.0",
                [CameraAttribute.TextureGain2] = "5.0",
                [CameraAttribute.TextureGain3] = "5.0",
                [CameraAttribute.PatternExposureMultiplier] = "1",
                [CameraAttribute.PatternExposure1] = "10.0",
                [CameraAttribute.PatternExposure2] = "20.0",
                [CameraAttribute.PatternExposure3] = "30.0",
                [CameraAttribute.PatternGain1] = "3.0",
                [CameraAttribute.PatternGain2] = "3.0",
                [CameraAttribute.PatternGain3] = "3.0",
                [CameraAttribute.DecodeThreshold1] = "1",
                [CameraAttribute.DecodeThreshold2] = "1",
                [CameraAttribute.DecodeThreshold3] = "1",
                [CameraAttribute.NormalEstimationRadius] = "2.0",
                [CameraAttribute.SurfaceSmoothness] = SurfaceSmoothness.Sharp.ToString(),
                [CameraAttribute.StructurePatternType] = StructurePatternType.NormalAndInverted.ToString(),
                [CameraAttribute.LedPower] = "1",
                [CameraAttribute.PatternStrategy] = PatternStrategy.PhaseShiftDouble.ToString(),
                [CameraAttribute.PatternColor] = "3",
                [CameraAttribute.TextureSource] = "2",
                [CameraAttribute.MaxNomalAngle] = "90",
                [CameraAttribute.CalibrationDataPath] = "./"
            };
        }

        private static Dictionary<CameraAttribute, string> GetCoPick3D250Settings()
        {
            return new Dictionary<CameraAttribute, string>()
            {
                [CameraAttribute.Model] = CameraModel.CoPick3D_250.ToString(),
                [CameraAttribute.Serial] = "",
                [CameraAttribute.Alias] = "",
                [CameraAttribute.Ip] = "",
                [CameraAttribute.ScanMode] = CameraScanMode.MultiCamera.ToString(),
                [CameraAttribute.OutputResolution] = OutputResolution.W1224xH1024.ToString(),
                [CameraAttribute.IsolationDistance] = "1.0",
                [CameraAttribute.IsolationMinNeighbors] = "10",
                [CameraAttribute.SendNormalMap] = "False",
                [CameraAttribute.TextureExposureMultiplier] = "1",
                [CameraAttribute.TextureExposure1] = "16.0",
                [CameraAttribute.TextureExposure2] = "16.0",
                [CameraAttribute.TextureExposure3] = "16.0",
                [CameraAttribute.TextureGain1] = "5.0",
                [CameraAttribute.TextureGain2] = "5.0",
                [CameraAttribute.TextureGain3] = "5.0",
                [CameraAttribute.PatternExposureMultiplier] = "1",
                [CameraAttribute.PatternExposure1] = "10.0",
                [CameraAttribute.PatternExposure2] = "20.0",
                [CameraAttribute.PatternExposure3] = "30.0",
                [CameraAttribute.PatternGain1] = "3.0",
                [CameraAttribute.PatternGain2] = "3.0",
                [CameraAttribute.PatternGain3] = "3.0",
                [CameraAttribute.DecodeThreshold1] = "1",
                [CameraAttribute.DecodeThreshold2] = "1",
                [CameraAttribute.DecodeThreshold3] = "1",
                [CameraAttribute.NormalEstimationRadius] = "2.0",
                [CameraAttribute.SurfaceSmoothness] = SurfaceSmoothness.Sharp.ToString(),
                [CameraAttribute.StructurePatternType] = StructurePatternType.NormalAndInverted.ToString(),
                [CameraAttribute.LedPower] = "1",
                [CameraAttribute.PatternStrategy] = PatternStrategy.PhaseShiftDouble.ToString(),
                [CameraAttribute.PatternColor] = PatternColor.Blue.ToString(),
                [CameraAttribute.TextureSource] = TextureSource.Led.ToString(),
                [CameraAttribute.MaxNomalAngle] = "90",
                [CameraAttribute.CalibrationDataPath] = "./"
            };
        }
    }
}