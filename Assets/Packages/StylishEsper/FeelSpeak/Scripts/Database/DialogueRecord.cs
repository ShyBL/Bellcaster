//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using SQLite;

namespace Esper.FeelSpeak.Database
{
    /// <summary>
    /// Represents a single database dialogue graph record.
    /// </summary>
    public class DialogueRecord
    {
        /// <summary>
        /// The graph ID.
        /// </summary>
        [Column("id")]
        public int id { get; set; }

        /// <summary>
        /// The name of the scriptable object.
        /// </summary>
        [Column("object_name")]
        public string objectName { get; set; }

        /// <summary>
        /// The graph name.
        /// </summary>
        [Column("graph_name")]
        public string graphName { get; set; }
    }
}