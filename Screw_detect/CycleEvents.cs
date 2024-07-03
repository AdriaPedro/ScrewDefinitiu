using Prism.Events;

namespace CCyberPick.Models
{
    public class CycleEvents
    {
        /// <summary>
        /// Object is an HImage
        /// </summary>
        public class UpdateImage: PubSubEvent<object>
        {
        }

        public class UpdateCurrentRecipe : PubSubEvent<string>
        {
        }
        public class UpdateCurrentEnviroment : PubSubEvent<string>
        {
        }

        public class UpdateCicleTime : PubSubEvent<int>
        {
        }
    }
}
