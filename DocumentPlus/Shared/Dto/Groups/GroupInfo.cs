﻿namespace DocumentPlus.Shared.Dto.Groups
{
    public class GroupInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
