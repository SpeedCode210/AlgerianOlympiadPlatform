using DatabaseConnector;
using DatabaseConnector.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;

namespace AopWebAdmin.Pages;

public class SubmissionDetails : PageModel
{
    private readonly IMongoDatabase _database;
    
    public SubmissionDetails(IMongoDatabase database)
    {
        _database = database;
    }
    
    [BindProperty(SupportsGet = true)]
    public string RequestedSubmission { get; set; }
    
    [BindProperty(SupportsGet = false)]
    public StudentSubmission Submission { get; set; }
    
    [BindProperty(SupportsGet = false)]
    public Actions Action { get; set; }
    
    public void OnGet()
    {
        Submission = new StudentSubmissionManager(_database).GetStudentSubmissionById(RequestedSubmission);
    }

    public IActionResult? OnPost()
    {
        switch (Action)
        {
            case Actions.Delete:
                return Delete();
        }

        return null;
    }

    private IActionResult Delete()
    {
        new StudentSubmissionManager(_database).DeleteStudentSubmission(RequestedSubmission);
        return Redirect("/Problems/Submissions");
    }

    public enum Actions
    {
        Delete
        
    }
}