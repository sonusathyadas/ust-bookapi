namespace BookWebApi.DTOs;
public class BookUpdateDto
{
    public string Title { get; set; }
    public string Author { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Language { get; set; }
    public string Genre { get; set; }
}