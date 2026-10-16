using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SpanishWordLearner.Models;

[Table("user_progress")]
public class SupabaseProgressRecord : BaseModel
{
    [PrimaryKey("user_id", false)]
    public string UserId { get; set; } = string.Empty;

    [Column("data")]
    public string DataJson { get; set; } = "{}";

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
