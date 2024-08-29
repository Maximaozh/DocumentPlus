using System.ComponentModel.DataAnnotations;

namespace DocumentPlus.Client.Data
{
    public class DocPageParameter
    {
        [Required(ErrorMessage = "Укажите название")]
        public string NameDoc { get; set; }
        public string AuthorDoc { get; set; }
        public string? DescriptionDoc { get; set; }
        public DateTime? CreatedDateDoc { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Укажите срок действия")]
        public DateTime? ExpireDateDoc { get; set; }

        [Required(ErrorMessage = "Укажите путь")]
        //[RegularExpression(@"^root\\.*", ErrorMessage = "Путь должен начинаться с 'root'")]
        public required string PathDoc { get; set; }
        public bool ReadOnlyForm { get; set; }
        public bool ReadOnlyText { get; set; }
    }
}
