//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using SQLite;

namespace Esper.FeelSpeak.Database
{
    /// <summary>
    /// Represents a single emotion record.
    /// </summary>
    public class EmotionRecord
    {
        /// <summary>
        /// The emotion ID.
        /// </summary>
        [Column("id")]
        public int id { get; set; }

        /// <summary>
        /// The name of the scriptable object.
        /// </summary>
        [Column("object_name")]
        public string objectName { get; set; }

        /// <summary>
        /// The emotion name.
        /// </summary>
        [Column("emotion_name")]
        public string emotionName { get; set; }
    }
}