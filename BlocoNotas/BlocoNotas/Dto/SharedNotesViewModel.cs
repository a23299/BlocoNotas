using BlocoNotas.Models;

namespace BlocoNotas.Dto;

public class SharedNotesViewModel
{
    public List<NoteShare> SharedByMe { get; set; } = new();
    public List<Note> SharedWithMe { get; set; } = new();
}