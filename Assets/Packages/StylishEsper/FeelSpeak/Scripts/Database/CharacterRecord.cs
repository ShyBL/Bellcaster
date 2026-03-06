//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using SQLite;

namespace Esper.FeelSpeak.Database
{
    /// <summary>
    /// Represents a single character record.
    /// </summary>
    public class CharacterRecord
    {
        /// <summary>
        /// The character ID.
        /// </summary>
        [Column("id")]
        public int id { get; set; }

        /// <summary>
        /// The name of the scriptable object.
        /// </summary>
        [Column("object_name")]
        public string objectName { get; set; }

        /// <summary>
        /// The character name.
        /// </summary>
        [Column("character_name")]
        public string characterName { get; set; }
    }
}