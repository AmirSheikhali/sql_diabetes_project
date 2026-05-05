using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

public class AddEntryModel : PageModel
{
    private readonly IConfiguration _config;

    public AddEntryModel(IConfiguration config)
    {
        _config = config;
    }

    [BindProperty] public int UserId { get; set; }
    [BindProperty] public int BG { get; set; }
    [BindProperty] public int Carbs { get; set; }
    [BindProperty] public double Insulin { get; set; }
    [BindProperty] public string Note { get; set; } = "";

    public IActionResult OnPost()
    {
        string connStr = _config.GetConnectionString("DefaultConnection");

        using var conn = new MySqlConnection(connStr);
        conn.Open();

        // Insert BG
        string bgQuery = "INSERT INTO BloodGlucose (user_id, bg_level, bg_time, note) VALUES (@uid, @bg, NOW(), @note)";
        using var cmd = new MySqlCommand(bgQuery, conn);
        cmd.Parameters.AddWithValue("@uid", UserId);
        cmd.Parameters.AddWithValue("@bg", BG);
        cmd.Parameters.AddWithValue("@note", Note);
        cmd.ExecuteNonQuery();

        int bgId = (int)cmd.LastInsertedId;

        // Insert carbs
        string carbQuery = "INSERT INTO CarbIntake (user_id, carbs) VALUES (@uid, @carbs)";
        using var carbCmd = new MySqlCommand(carbQuery, conn);
        carbCmd.Parameters.AddWithValue("@uid", UserId);
        carbCmd.Parameters.AddWithValue("@carbs", Carbs);
        carbCmd.ExecuteNonQuery();

        // Insert insulin
        string insQuery = "INSERT INTO Insulin (bg_id, dose) VALUES (@bgid, @dose)";
        using var insCmd = new MySqlCommand(insQuery, conn);
        insCmd.Parameters.AddWithValue("@bgid", bgId);
        insCmd.Parameters.AddWithValue("@dose", Insulin);
        insCmd.ExecuteNonQuery();

        return RedirectToPage("/Index", new { userId = UserId });
    }
}