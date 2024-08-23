namespace DocumentPlus.Shared.Dto.Access
{
    public class DocumentsNamed
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public override string ToString()
        {
            return Name;

        }
    }
}
