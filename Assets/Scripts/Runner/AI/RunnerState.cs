using System;

namespace Runner.RL {

    public class RunnerState {
        public int XDistance { get; set; }
        public int YDistance { get; set; }
        public ObstacleType ObstacleType { get; set; }

        public override string ToString() {
            return $"{XDistance}|{YDistance}|{ObstacleType}";
        }

        public override bool Equals(object obj) {
            return obj is RunnerState state &&
                   XDistance == state.XDistance &&
                   YDistance == state.YDistance &&
                   ObstacleType == state.ObstacleType;
        }

        public override int GetHashCode() {
            return HashCode.Combine(XDistance, YDistance, ObstacleType);
        }

        public static RunnerState StringToState(string val) {
            string[] state = val.Split('|');

            return new RunnerState() {
                XDistance = int.Parse(state[0]),
                YDistance = int.Parse(state[1]),
                ObstacleType = (ObstacleType)Enum.Parse(typeof(ObstacleType), state[2])
            };
        }
    }

}
