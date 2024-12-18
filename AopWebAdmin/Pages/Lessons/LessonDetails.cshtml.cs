using DatabaseConnector;
using DatabaseConnector.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;

namespace AopWebAdmin.Pages.Lessons;

public class LessonDetails : PageModel
{
    private readonly IMongoDatabase _database;
    
    [BindProperty(SupportsGet = true)]
    public required string RequestedLesson { get; set; }

    [BindProperty(SupportsGet = false)] 
    public Lesson Lesson { get; set; } = new();
    
    [BindProperty(SupportsGet = false)]
    public Actions Action { get; set; }

    public List<Asset> AvailableAttachments { get; set; } = [];

    public LessonDetails(IMongoDatabase database)
    {
        _database = database;
    }
    
    public void OnGet()
    {
        GetLesson();
        FillAvailableAttachments();
    }

    public IActionResult? OnPost()
    {
        switch (Action)
        {
            case Actions.Update:
                return Update();
            case Actions.Delete:
                return Delete();
        }

        return null;
    }

    private IActionResult Delete()
    {
        new LessonManager(_database).DeleteLesson(RequestedLesson);
        return Redirect("/Lessons");
    }

    private IActionResult? Update()
    {
        if (!ModelState.IsValid || Lesson.Id == "new") return Page();
        
        if (RequestedLesson == Lesson.Id)
        {
            new LessonManager(_database).UpdateLesson(Lesson, RequestedLesson);
            FillAvailableAttachments();
            return null;
        }
        else
        {
            var manager = new LessonManager(_database);
            if (manager.LessonExists(Lesson.Id))
            {
                throw new Exception($"Lesson with id {Lesson.Id} already exists");
            }
            if(RequestedLesson != "new") manager.DeleteLesson(RequestedLesson);
            manager.CreateLesson(Lesson);
            return Redirect("/Lessons/"+Lesson.Id);
        }
    }

    private void GetLesson()
    {
        Lesson = new LessonManager(_database).GetLessonById(RequestedLesson);
    }
    
    private void FillAvailableAttachments()
    {
        if (RequestedLesson == "new")
        {
            AvailableAttachments = new CloudAssetManager(_database).GetAllAssets() ?? throw new InvalidOperationException();
            return;
        }
        
        AvailableAttachments = (new CloudAssetManager(_database).GetAllAssets() ?? throw new InvalidOperationException()).Where(only => !Lesson.Attachments.Contains(only.Id)).ToList();
    }

    public enum Actions
    {
        Delete,
        Update
    }
}